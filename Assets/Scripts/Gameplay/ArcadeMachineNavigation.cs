using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent, RequireComponent(typeof(ArcadeMachine))]
public sealed class ArcadeMachineNavigation : MonoBehaviour
{
    [SerializeField] private NavMeshObstacle obstacle;
    [SerializeField] private bool fitToModel = true;
    public Bounds LocalBounds { get; private set; }

    private void Awake() { Configure(); }

    public void Configure()
    {
        if (obstacle == null) obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle == null) obstacle = gameObject.AddComponent<NavMeshObstacle>();
        Bounds bounds = new Bounds(Vector3.up, Vector3.one);
        bool found = false;
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.GetComponentInParent<Canvas>() != null || renderer.name == "ArcadeScreen") continue;
            Bounds local = renderer.localBounds;
            Matrix4x4 matrix = transform.worldToLocalMatrix * renderer.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = local.center + Vector3.Scale(local.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 point = matrix.MultiplyPoint3x4(corner);
                if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; }
                else bounds.Encapsulate(point);
            }
        }
        LocalBounds = bounds;
        obstacle.shape = NavMeshObstacleShape.Box;
        if (fitToModel)
        {
            obstacle.center = bounds.center;
            obstacle.size = new Vector3(Mathf.Max(0.2f, bounds.size.x), Mathf.Max(0.5f, bounds.size.y), Mathf.Max(0.2f, bounds.size.z));
        }
        obstacle.carving = true;
        obstacle.carveOnlyStationary = true;
        obstacle.carvingTimeToStationary = 0.1f;
        if (GetComponentInChildren<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.center = bounds.center;
            collider.size = bounds.size;
        }
    }

    public void SetPreview(bool preview)
    {
        if (obstacle != null) obstacle.enabled = !preview;
    }
}
