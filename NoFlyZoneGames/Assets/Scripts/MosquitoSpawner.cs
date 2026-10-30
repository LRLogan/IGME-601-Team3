using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MosquitoSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MosquitoManager mosquitoManager;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float collisionRadius = 0.5f;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private int maxSpawnAttempts = 15;
    [SerializeField] private float targetStopDistance = 0.1f;

    [Header("Spawn Area Visualization")]
    [SerializeField] private Material spawnAreaMaterial;
    [SerializeField] private bool showSpawnArea = true;

    private GameObject spawnAreaIndicator;

    private void Start()
    {
        CreateSpawnIndicator(spawnRadius);
    }

    private void OnDestroy()
    {
        DestroySpawnIndicator();
    }

    public MosquitoAgent SpawnMosquito()
    {
        if (mosquitoManager == null)
        {
            Debug.LogWarning("Mosquito manager is not assigned.", this);
            return null;
        }

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // Generate random position within spawn radius
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;

            Vector3 candidatePosition = transform.position +
                new Vector3(randomOffset.x, 0f, randomOffset.y);

            // Check that there is a reachable NavMesh surface nearby
            if (!NavMesh.SamplePosition(
                candidatePosition,
                out NavMeshHit hit,
                10f,
                NavMesh.AllAreas))
            {
                continue;
            }

            // Keep the original spawn height
            Vector3 spawnPosition = new Vector3(
                hit.position.x,
                candidatePosition.y,
                hit.position.z
            );

            // Check collisions at the actual spawn position
            if (Physics.CheckSphere(
                spawnPosition,
                collisionRadius,
                collisionLayers,
                QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            MosquitoAgent newAgent = mosquitoManager.SpawnMosquito(
                spawnPosition,
                transform.rotation
            );

            if (newAgent == null)
            {
                Debug.LogWarning("Manager failed to spawn mosquito.", this);
                return null;
            }

            mosquitoManager.MoveOneTo(newAgent, mosquitoManager.pointB, targetStopDistance);

            return newAgent;
        }

        Debug.LogWarning(
            "Unable to find a valid mosquito spawn location.",
            this
        );
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    private void CreateSpawnIndicator(float radius)
    {
        DestroySpawnIndicator();

        if (spawnAreaMaterial == null)
        {
            Debug.LogWarning(
                $"{name}: Assign a Spawn Area Material.",
                this);
            return;
        }

        spawnAreaIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spawnAreaIndicator.name = "Spawn Area Indicator";

        // Parent to the spawner so the indicator follows it.
        spawnAreaIndicator.transform.SetParent(transform, false);
        spawnAreaIndicator.transform.localPosition = Vector3.zero;
        spawnAreaIndicator.transform.localRotation = Quaternion.identity;

        // Remove the collider so it doesn't interfere with spawning.
        Collider indicatorCollider =
            spawnAreaIndicator.GetComponent<Collider>();

        indicatorCollider.enabled = false;
        Destroy(indicatorCollider);

        // Unity's primitive sphere has a diameter of 1.
        float diameter = radius * 2f;
        spawnAreaIndicator.transform.localScale =
            Vector3.one * diameter;

        Renderer indicatorRenderer =
            spawnAreaIndicator.GetComponent<Renderer>();

        indicatorRenderer.sharedMaterial = spawnAreaMaterial;
        indicatorRenderer.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
        indicatorRenderer.receiveShadows = false;

        spawnAreaIndicator.SetActive(showSpawnArea);
    }

    private void DestroySpawnIndicator()
    {
        if (spawnAreaIndicator == null)
            return;

        spawnAreaIndicator.SetActive(false);
        Destroy(spawnAreaIndicator);
        spawnAreaIndicator = null;
    }

    #if UNITY_EDITOR
    // Temporary test hook (Editor only)
    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SpawnMosquito();
        }
    }
    #endif
}