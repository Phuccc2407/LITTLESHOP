using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class Customer : MonoBehaviour
{
    private enum CustomerState
    {
        WalkingToCounter,
        WalkingToMachine,
        Playing,
        Leaving
    }

    [SerializeField, Min(0.1f)] private float interactionDistance = 1.25f;
    [SerializeField] private Animator animator;
    [SerializeField] private string walkingParameter = "IsWalking";
    [SerializeField] private string playingParameter = "IsPlaying";

    private CustomerManager manager;
    private NavMeshAgent agent;
    private ArcadeMachine machine;
    private Transform coinCounterPoint;
    private Transform exitPoint;
    private CustomerDoor entranceDoor;
    private CustomerState state;
    private float playTimeRemaining;
    private int coins;

    public void Initialize(
        CustomerManager owner,
        ArcadeMachine target,
        Transform counter,
        Transform exit,
        CustomerDoor door)
    {
        manager = owner;
        machine = target;
        coinCounterPoint = counter;
        exitPoint = exit;
        entranceDoor = door;
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

        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            if (state == CustomerState.Leaving)
            {
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
                BuyCoinsAtCounter();
                state = CustomerState.WalkingToMachine;
                SetDestination(machine.PlayPoint.position);
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
            if (entranceDoor != null)
            {
                entranceDoor.CloseAfterExit();
            }

            DestroyCustomer();
        }
    }

    private void BuyCoinsAtCounter()
    {
        coins = machine == null ? 0 : machine.CoinsPerPlay;
    }

    private void BeginLeaving()
    {
        if (machine != null)
        {
            machine.CancelPlay();
            machine = null;
        }

        state = CustomerState.Leaving;
        agent.isStopped = false;
        SetAnimationState(true, false);
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
