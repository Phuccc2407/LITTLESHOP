using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
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

    public float Satisfaction { get; private set; }
    public int RequiredCoins { get { return machine == null ? 0 : machine.CoinsPerPlay; } }

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
        state = coinCounterPoint == null
            ? CustomerState.WalkingToMachine
            : CustomerState.WalkingToCounter;

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("Customer must spawn on a baked NavMesh.", this);
            return;
        }

        SetDestination(GetCurrentDestination());
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
                    transform.SetPositionAndRotation(coinCounter.transform.position, coinCounter.transform.rotation);
                    state = CustomerState.WaitingForCoins;
                    coinCounter.RegisterCustomer(this);
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

        agent.isStopped = false;
        agent.SetDestination(position);
        transform.rotation = rotation;
    }

    private void BuyCoinsAtCounter()
    {
        coins = RequiredCoins;
    }

    private void BeginLeaving()
    {
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
        if (animator == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(walkingParameter))
        {
            animator.SetBool(walkingParameter, walking);
        }

        if (!string.IsNullOrEmpty(playingParameter))
        {
            animator.SetBool(playingParameter, playing);
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
