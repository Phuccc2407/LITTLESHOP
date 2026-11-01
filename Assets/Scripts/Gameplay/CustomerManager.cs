using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class CustomerManager : MonoBehaviour
{
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform coinCounterPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CustomerDoor entranceDoor;
    [SerializeField, Min(1)] private int maxCustomers = 3;
    [SerializeField, Min(0.1f)] private float spawnInterval = 10f;
    [SerializeField] private bool spawningEnabled = true;

    private readonly List<Customer> customers = new List<Customer>();
    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        if (spawningEnabled)
        {
            spawnRoutine = StartCoroutine(SpawnLoop());
        }
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;
        if (enabled && isActiveAndEnabled && spawnRoutine == null)
        {
            spawnRoutine = StartCoroutine(SpawnLoop());
        }
        else if (!enabled && spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void SpawnCustomer()
    {
        if (customerPrefab == null || entrancePoint == null || exitPoint == null)
        {
            Debug.LogError("CustomerManager requires a customer prefab, entrance, and exit point.", this);
            return;
        }

        if (customers.Count >= maxCustomers)
        {
            return;
        }

        ArcadeMachine machine = FindAvailableMachine();
        if (machine == null)
        {
            return;
        }

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(entrancePoint.position, out hit, 2f, NavMesh.AllAreas))
        {
            Debug.LogError("Customer entrance is not on a baked NavMesh.", this);
            return;
        }

        if (entranceDoor != null)
        {
            entranceDoor.OpenForEntry();
        }

        Customer customer = Instantiate(customerPrefab, hit.position, entrancePoint.rotation);
        customer.Initialize(this, machine, coinCounterPoint, exitPoint, entranceDoor);
        customers.Add(customer);
    }

    public void NotifyCustomerFinished(Customer customer)
    {
        customers.Remove(customer);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnCustomer();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private ArcadeMachine FindAvailableMachine()
    {
        ArcadeMachine[] machines = FindObjectsOfType<ArcadeMachine>();
        ArcadeMachine closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < machines.Length; i++)
        {
            if (machines[i].State != ArcadeMachineState.Available)
            {
                continue;
            }

            float distance = (machines[i].transform.position - entrancePoint.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closest = machines[i];
                closestDistance = distance;
            }
        }

        return closest;
    }
}
