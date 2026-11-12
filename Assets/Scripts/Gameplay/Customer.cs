using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
public sealed class Customer : MonoBehaviour
{
    public enum CustomerState
    {
        WalkingToCounter,
        WaitingForCoins,
        WalkingToMachine,
        Playing,
        Leaving
    }

    [SerializeField, Min(0.1f)] private float interactionDistance = 1.25f;
    [SerializeField] private Animator animator;
    [SerializeField] private string walkingParameter = "IsWalking";
    [SerializeField] private string playingParameter = "IsPlaying";
    [SerializeField, Range(0f, 100f)] private float startingSatisfaction = 100f;
    [SerializeField, Min(0f)] private float satisfactionLossPerSecond = 1f;
    [SerializeField] private UnityEngine.Object[] skinPrefabs;

    private CustomerManager manager;
    private NavMeshAgent agent;
    private ArcadeMachine machine;
    private Transform coinCounterPoint;
    private CoinCounter coinCounter;
    private Transform exitPoint;
    private CustomerDoor entranceDoor;
    private CustomerState state;
    private float playTimeRemaining;
    private int coins;
    private Animator animationTarget;
    private int desiredPlays;
    private int completedPlays;
    private int requestedCoins;
    private float patience;
    private float waitingTime;
    private float reviewChance;
    private string visitorName;
    private string departureReason = "A fun visit. I will come back!";
    private bool reviewWritten;
    private bool playingMachine;
    private int playPrice;
    private float navigationTime;

    public int CompletedPlays { get { return completedPlays; } }
    public CustomerState State { get { return state; } }
    public string Activity { get { return state.ToString(); } }
    public float PatienceRemaining { get { return Mathf.Max(0f, patience - waitingTime); } }

    public float Satisfaction { get; private set; }
    public int RequiredCoins { get { return requestedCoins; } }
    public bool IsWaitingForCoins
    {
        get { return state == CustomerState.WaitingForCoins && machine != null
            && machine.State == ArcadeMachineState.Available; }
    }

    private void Awake()
    {
        ApplyRandomSkin();

        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CapsuleCollider>();
        }

        collider.center = new Vector3(0f, 0.9f, 0f);
        collider.radius = 0.3f;
        collider.height = 1.8f;
        collider.direction = 1;

        Outline outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 10f;
        outline.enabled = false;
    }

    private void ApplyRandomSkin()
    {
        animationTarget = animator;

        if (skinPrefabs == null || skinPrefabs.Length == 0)
        {
            return;
        }

        UnityEngine.Object skinAsset = skinPrefabs[Random.Range(0, skinPrefabs.Length)];
        GameObject skinPrefab = skinAsset as GameObject;
        if (skinPrefab == null)
        {
            Debug.LogError("Customer skin must be a prefab with a GameObject root.", skinAsset);
            return;
        }

        Renderer[] defaultRenderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < defaultRenderers.Length; i++)
        {
            if (defaultRenderers[i].GetComponentInParent<Canvas>() == null)
            {
                defaultRenderers[i].enabled = false;
            }
        }

        UnityEngine.Object instantiatedSkin = UnityEngine.Object.Instantiate(
            skinAsset,
            transform,
            false);
        GameObject skin = instantiatedSkin as GameObject;
        if (skin == null)
        {
            Debug.LogError("Customer skin prefab must have a GameObject root.", skinPrefab);
            return;
        }

        skin.name = "CustomerSkin";
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localRotation = Quaternion.identity;
        skin.transform.localScale = Vector3.one;

        Animator skinAnimator = skin.GetComponentInChildren<Animator>();
        if (skinAnimator != null && animator != null)
        {
            skinAnimator.runtimeAnimatorController = animator.runtimeAnimatorController;
            if (animator.avatar != null)
            {
                skinAnimator.avatar = animator.avatar;
            }

            animationTarget = skinAnimator;
        }
    }

    public void Initialize(
        CustomerManager owner,
        ArcadeMachine target,
        Transform counter,
        CoinCounter counterInteraction,
        Transform exit,
        CustomerDoor door)
    {
        manager = owner;
        machine = target;
        coinCounterPoint = counter;
        coinCounter = counterInteraction;
        exitPoint = exit;
        entranceDoor = door;
        Satisfaction = startingSatisfaction;
        CustomerProfile profile = owner.PickProfile();
        patience = profile == null ? Random.Range(35f, 80f) : profile.PatienceSeconds;
        desiredPlays = profile == null ? Random.Range(1, 5)
            : Random.Range(Mathf.Max(1, profile.MinPlays), Mathf.Max(profile.MinPlays, profile.MaxPlays) + 1);
        reviewChance = profile == null ? 0.8f : profile.ReviewChance;
        visitorName = profile == null ? "Guest" : profile.DisplayName;
        int budget = profile == null ? Random.Range(6, 16) : Mathf.Max(1, profile.CoinBudget);
        playPrice = machine == null ? 1 : machine.CoinsPerPlay;
        desiredPlays = Mathf.Min(desiredPlays, budget / playPrice);
        requestedCoins = desiredPlays * playPrice;
        agent = GetComponent<NavMeshAgent>();
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("Customer must spawn on a baked NavMesh.", this);
            if (machine != null) machine.ReleaseReservation();
            CloseEntranceDoor();
            DestroyCustomer();
            return;
        }
        agent.autoRepath = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        if (machine == null || desiredPlays == 0)
        {
            Satisfaction = 25f;
            departureReason = machine == null ? "No machines available. Please add more games!" : "The games are too expensive for my budget.";
            BeginLeaving();
            return;
        }
        float sensitivity = profile == null ? 0.6f : profile.PriceSensitivity;
        Satisfaction = Mathf.Clamp(Satisfaction - Mathf.Max(0, playPrice - 1) * sensitivity * 5f, 0f, 100f);

        if (coinCounter != null)
        {
            state = CustomerState.WalkingToCounter;
            coinCounter.RegisterCustomer(this);
        }
        else
        {
            if (coinCounterPoint == null) BuyCoinsAtCounter();
            state = coinCounterPoint == null
                ? CustomerState.WalkingToMachine
                : CustomerState.WalkingToCounter;
            SetDestination(GetCurrentDestination());
        }
    }

    private void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        if (state != CustomerState.Leaving && state != CustomerState.Playing
            && (machine == null || machine.State != ArcadeMachineState.Available))
        {
            Satisfaction = Mathf.Max(0f, Satisfaction - 25f);
            departureReason = "The machine I wanted was no longer available.";
            BeginLeaving();
            return;
        }

        if (state != CustomerState.Leaving && state != CustomerState.Playing)
        {
            Satisfaction = Mathf.Max(0f, Satisfaction - satisfactionLossPerSecond * Time.deltaTime);
        }

        if (state == CustomerState.WalkingToCounter || state == CustomerState.WaitingForCoins)
        {
            waitingTime += Time.deltaTime;
            if (waitingTime >= patience || Satisfaction <= 0f)
            {
                Satisfaction = Mathf.Min(Satisfaction, 25f);
                departureReason = "I waited too long to buy coins.";
                ShopAudio.Play(ShopSound.Unhappy);
                BeginLeaving();
                return;
            }
        }
        if (state != CustomerState.Playing && state != CustomerState.WaitingForCoins)
        {
            navigationTime += Time.deltaTime;
            if (navigationTime > 90f)
            {
                if (state == CustomerState.Leaving) { CloseEntranceDoor(); DestroyCustomer(); }
                else { Satisfaction = 20f; departureReason = "I could not reach the games."; BeginLeaving(); }
                return;
            }
        }

        if (!agent.pathPending && agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            if (state == CustomerState.Leaving)
            {
                CloseEntranceDoor();
                DestroyCustomer();
            }
            else
            {
                Satisfaction = Mathf.Min(Satisfaction, 20f);
                departureReason = "The path to the counter or games was blocked.";
                BeginLeaving();
            }

            return;
        }

        if (state == CustomerState.WalkingToCounter)
        {
            if (HasReachedDestination())
            {
                agent.isStopped = true;
                if (coinCounter != null)
                {
                    state = CustomerState.WaitingForCoins;
                    SetAnimationState(false, false);
                }
                else
                {
                    BuyCoinsAtCounter();
                    state = CustomerState.WalkingToMachine;
                    SetDestination(machine.GetNavigationPosition());
                }
            }

            return;
        }

        if (state == CustomerState.WalkingToMachine)
        {
            if (machine == null)
            {
                BeginLeaving();
                return;
            }

            if (!HasReachedDestination())
            {
                return;
            }

            if (coins < machine.CoinsPerPlay || !machine.TryStartPlay())
            {
                Satisfaction = Mathf.Max(0f, Satisfaction - 25f);
                departureReason = "The machine was unavailable or its price changed.";
                BeginLeaving();
                return;
            }

            agent.isStopped = true;
            playingMachine = true;
            playPrice = machine.CoinsPerPlay;
            coins -= playPrice;
            machine.RecordCoinRevenue(playPrice);
            // Keep the agent on its navigable approach position instead of teleporting into the cabinet.
            transform.rotation = machine.PlayPoint.rotation;
            playTimeRemaining = machine.Definition == null
                ? 5f
                : machine.Definition.PlayDuration;
            playTimeRemaining *= Random.Range(0.85f, 1.2f);
            state = CustomerState.Playing;
            SetAnimationState(false, true);
            return;
        }

        if (state == CustomerState.Playing)
        {
            if (machine == null || machine.State != ArcadeMachineState.Occupied)
            {
                Satisfaction = Mathf.Max(0f, Satisfaction - 25f);
                departureReason = "My game was interrupted.";
                BeginLeaving();
                return;
            }

            playTimeRemaining -= Time.deltaTime;
            if (playTimeRemaining <= 0f)
            {
                machine.CompletePlay();
                playingMachine = false;
                completedPlays++;
                Satisfaction = Mathf.Clamp(Satisfaction + 12f - (100 - machine.Condition) * 0.08f, 0f, 100f);
                machine = null;
                if (completedPlays < desiredPlays && coins > 0 && manager.IsShopOpen)
                    machine = manager.ReserveMachine(coins);
                if (machine != null)
                {
                    state = CustomerState.WalkingToMachine;
                    SetDestination(machine.GetNavigationPosition());
                }
                else
                {
                    if (completedPlays < desiredPlays) departureReason = "Had fun, but there were not enough available games.";
                    BeginLeaving();
                }
            }

            return;
        }

        if (state == CustomerState.Leaving && HasReachedDestination())
        {
            CloseEntranceDoor();
            DestroyCustomer();
        }
    }

    private void CloseEntranceDoor()
    {
        if (entranceDoor != null)
        {
            entranceDoor.CloseAfterExit();
            entranceDoor = null;
        }
    }

    public void ReceiveCoins(int amount)
    {
        if (state != CustomerState.WaitingForCoins || amount < RequiredCoins)
        {
            return;
        }

        coins = amount;
        state = CustomerState.WalkingToMachine;
        SetDestination(machine.GetNavigationPosition());
    }

    public void MoveToCoinQueuePosition(Vector3 position, Quaternion rotation)
    {
        if (agent == null || !agent.isOnNavMesh) return;
        if (state != CustomerState.WaitingForCoins && state != CustomerState.WalkingToCounter)
        {
            return;
        }

        float arrivalDistance = Mathf.Max(agent.stoppingDistance, interactionDistance);
        bool alreadyAtPosition = (transform.position - position).sqrMagnitude
            <= arrivalDistance * arrivalDistance;
        if (state == CustomerState.WaitingForCoins && alreadyAtPosition)
        {
            agent.isStopped = true;
            transform.rotation = rotation;
            SetAnimationState(false, false);
            return;
        }

        if (state == CustomerState.WaitingForCoins)
        {
            state = CustomerState.WalkingToCounter;
        }

        agent.isStopped = false;
        agent.SetDestination(position);
        transform.rotation = rotation;
        SetAnimationState(true, false);
    }

    public void RequestLeave()
    {
        if (completedPlays < desiredPlays)
        {
            departureReason = "The shop closed before I finished my visit.";
            Satisfaction = Mathf.Max(0f, Satisfaction - 15f);
        }
        BeginLeaving();
    }

    public void FinishDay()
    {
        RequestLeave();
        CloseEntranceDoor();
        DestroyCustomer();
    }

    private void BuyCoinsAtCounter()
    {
        coins = RequiredCoins;
        if (EconomyManager.Instance != null) EconomyManager.Instance.AddCoinRevenue(coins);
    }

    private void BeginLeaving()
    {
        if (state == CustomerState.Leaving)
        {
            return;
        }

        if (coinCounter != null)
        {
            coinCounter.UnregisterCustomer(this);
        }

        if (machine != null)
        {
            if (playingMachine) machine.CancelPlay();
            playingMachine = false;
            machine.ReleaseReservation();
            machine = null;
        }

        state = CustomerState.Leaving;
        navigationTime = 0f;
        if (!reviewWritten)
        {
            reviewWritten = true;
            if (departureReason == "A fun visit. I will come back!" && Satisfaction < 60f)
                departureReason = "The games were fun, but service, prices or maintenance could be better.";
            if (Random.value <= reviewChance)
                CustomerReviews.Add(visitorName, Satisfaction, completedPlays, departureReason);
        }
        agent.isStopped = false;
        SetAnimationState(true, false);
        if (exitPoint == null)
        {
            CloseEntranceDoor();
            DestroyCustomer();
            return;
        }

        SetDestination(exitPoint.position);
    }

    private Vector3 GetCurrentDestination()
    {
        if (state == CustomerState.WalkingToCounter && coinCounterPoint != null)
        {
            return coinCounterPoint.position;
        }

        return machine == null ? transform.position : machine.GetNavigationPosition();
    }

    private void SetDestination(Vector3 destination)
    {
        navigationTime = 0f;
        agent.isStopped = false;
        agent.SetDestination(destination);
        SetAnimationState(true, false);
    }

    private void SetAnimationState(bool walking, bool playing)
    {
        Animator target = animationTarget == null ? animator : animationTarget;
        if (target == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(walkingParameter))
        {
            target.SetBool(walkingParameter, walking);
        }

        if (!string.IsNullOrEmpty(playingParameter))
        {
            target.SetBool(playingParameter, playing);
        }
    }

    private bool HasReachedDestination()
    {
        if (agent.pathPending)
        {
            return false;
        }

        return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, interactionDistance);
    }

    private void DestroyCustomer()
    {
        if (manager != null)
        {
            manager.NotifyCustomerFinished(this);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (coinCounter != null) coinCounter.UnregisterCustomer(this);
        if (machine != null)
        {
            if (playingMachine) machine.CancelPlay();
            machine.ReleaseReservation();
        }
        if (manager != null) manager.NotifyCustomerFinished(this);
    }
}
