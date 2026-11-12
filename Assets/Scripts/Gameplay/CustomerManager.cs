using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class CustomerManager : MonoBehaviour
{
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform coinCounterPoint;
    [SerializeField] private CoinCounter coinCounter;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CustomerDoor entranceDoor;
    [SerializeField] private ShopDayCycle shopDayCycle;
    [SerializeField, Min(1)] private int maxCustomers = 3;
    [SerializeField, Min(0.1f)] private float spawnInterval = 10f;
    [SerializeField] private bool spawningEnabled = true;
    [SerializeField] private CustomerProfile[] customerProfiles;
    [SerializeField, Range(0f, 0.9f)] private float arrivalVariation = 0.5f;

    public bool IsShopOpen { get { return shopDayCycle == null || shopDayCycle.State == ShopDayState.Open; } }
    public CustomerProfile PickProfile()
    {
        return customerProfiles == null || customerProfiles.Length == 0 ? null
            : customerProfiles[Random.Range(0, customerProfiles.Length)];
    }

    private readonly List<Customer> customers = new List<Customer>();
    private Coroutine spawnRoutine;
    public event System.Action<Customer> CustomerSpawned;
    public event System.Action<Customer> CustomerFinished;

    public int CurrentCustomerCount { get { return customers.Count; } }
    public int CustomersSpawnedToday { get; private set; }
    public int CustomersServedToday { get; private set; }
    public float AverageSatisfaction
    {
        get
        {
            if (customers.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < customers.Count; i++)
            {
                total += customers[i].Satisfaction;
            }

            return total / customers.Count;
        }
    }

    private void OnEnable()
    {
        if (coinCounterPoint != null && coinCounter == null)
        {
            Debug.LogError("CustomerManager has a Coin Counter Point but no Coin Counter assigned.", this);
        }

        if (shopDayCycle != null)
        {
            shopDayCycle.DayChanged += OnDayChanged;
        }

        if (spawningEnabled)
        {
            spawnRoutine = StartCoroutine(SpawnLoop());
        }
    }

    private void OnDisable()
    {
        if (shopDayCycle != null)
        {
            shopDayCycle.DayChanged -= OnDayChanged;
        }

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
        if (GameSession.IsRestoring) return;
        if (shopDayCycle != null && shopDayCycle.State != ShopDayState.Open)
        {
            return;
        }

        if (customerPrefab == null || entrancePoint == null || exitPoint == null)
        {
            Debug.LogError("CustomerManager requires a customer prefab, entrance, and exit point.", this);
            return;
        }

        if (customers.Count >= maxCustomers)
        {
            return;
        }

        ArcadeMachine machine = ReserveMachine();

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(entrancePoint.position, out hit, 2f, NavMesh.AllAreas))
        {
            if (machine != null) machine.ReleaseReservation();
            Debug.LogError("Customer entrance is not on a baked NavMesh.", this);
            return;
        }

        if (entranceDoor != null)
        {
            entranceDoor.OpenForEntry();
        }

        Customer customer = Instantiate(customerPrefab, hit.position, entrancePoint.rotation);
        customers.Add(customer);
        CustomersSpawnedToday++;
        customer.Initialize(this, machine, coinCounterPoint, coinCounter, exitPoint, entranceDoor);
        CustomerSpawned?.Invoke(customer);
        ShopAudio.Play(ShopSound.Arrival);
    }

    public void NotifyCustomerFinished(Customer customer)
    {
        if (customers.Remove(customer))
        {
            if (customer.CompletedPlays > 0) CustomersServedToday++;
            CustomerFinished?.Invoke(customer);
        }
    }

    public void RequestCustomersLeave()
    {
        Customer[] customersSnapshot = customers.ToArray();
        for (int i = 0; i < customersSnapshot.Length; i++)
        {
            if (customersSnapshot[i] != null)
            {
                customersSnapshot[i].RequestLeave();
            }
        }
    }

    public void FinishDay()
    {
        Customer[] snapshot = customers.ToArray();
        foreach (Customer customer in snapshot)
            if (customer != null) customer.FinishDay();
    }

    private void OnDayChanged(int dayNumber)
    {
        CustomersSpawnedToday = 0;
        CustomersServedToday = 0;
    }

    public void RestoreDailyStats(int spawned, int served)
    {
        CustomersSpawnedToday = Mathf.Max(0, spawned);
        CustomersServedToday = Mathf.Max(0, served);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnCustomer();
            float reputation = Mathf.Lerp(1.4f, 0.7f, (CustomerReviews.AverageStars - 1f) / 4f);
            yield return new WaitForSeconds(Mathf.Max(0.2f, spawnInterval * reputation
                * Random.Range(1f - arrivalVariation, 1f + arrivalVariation)));
        }
    }

    public ArcadeMachine ReserveMachine(int affordableCoins = int.MaxValue)
    {
        ArcadeMachine[] machines = FindObjectsByType<ArcadeMachine>();
        ArcadeMachine closest = null;
        int candidates = 0;

        for (int i = 0; i < machines.Length; i++)
        {
            if (machines[i].State != ArcadeMachineState.Available || machines[i].IsReserved
                || machines[i].CoinsPerPlay > affordableCoins)
            {
                continue;
            }

            candidates++;
            if (Random.Range(0, candidates) == 0)
            {
                closest = machines[i];
            }
        }

        if (closest != null && closest.TryReserve())
        {
            return closest;
        }

        return null;
    }
}
