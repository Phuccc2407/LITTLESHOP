using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
public sealed class Customer : MonoBehaviour
{
    private enum CustomerState
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
    private float coinPurchaseTimeRemaining;
    private Animator animationTarget;

    public float Satisfaction { get; private set; }
    public int RequiredCoins { get { return machine == null ? 0 : machine.CoinsPerPlay; } }
    public bool IsWaitingForCoins { get { return state == CustomerState.WaitingForCoins; } }

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
        agent = GetComponent<NavMeshAgent>();
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("Customer must spawn on a baked NavMesh.", this);
            return;
        }

        if (coinCounter != null)
        {
            state = CustomerState.WalkingToCounter;
            coinCounter.RegisterCustomer(this);
        }
        else
        {
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

        if (state != CustomerState.Leaving && state != CustomerState.Playing)
        {
            Satisfaction = Mathf.Max(0f, Satisfaction - satisfactionLossPerSecond * Time.deltaTime);
        }

        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            if (state == CustomerState.Leaving)
            {
                CloseEntranceDoor();
                DestroyCustomer();
            }
            else
            {
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
                    SetDestination(machine.PlayPoint.position);
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

            if (coins <= 0 || !machine.TryStartPlay())
            {
                BeginLeaving();
                return;
            }

            agent.isStopped = true;
            transform.SetPositionAndRotation(
                machine.PlayPoint.position,
                machine.PlayPoint.rotation);
            playTimeRemaining = machine.Definition == null
                ? 5f
                : machine.Definition.PlayDuration;
            state = CustomerState.Playing;
            SetAnimationState(false, true);
            return;
        }

        if (state == CustomerState.Playing)
        {
            if (machine == null)
            {
                BeginLeaving();
                return;
            }

            playTimeRemaining -= Time.deltaTime;
            if (playTimeRemaining <= 0f)
            {
                coins--;
                machine.CompletePlay();
                BeginLeaving();
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

        coins = RequiredCoins;
        machine.RecordCoinRevenue(RequiredCoins);
        state = CustomerState.WalkingToMachine;
        SetDestination(machine.PlayPoint.position);
    }

    public void MoveToCoinQueuePosition(Vector3 position, Quaternion rotation)
    {
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
        BeginLeaving();
    }

    private void BuyCoinsAtCounter()
    {
        coins = RequiredCoins;
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
            machine.CancelPlay();
            machine.ReleaseReservation();
            machine = null;
        }

        state = CustomerState.Leaving;
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

        return machine == null ? transform.position : machine.PlayPoint.position;
    }

    private void SetDestination(Vector3 destination)
    {
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
}
