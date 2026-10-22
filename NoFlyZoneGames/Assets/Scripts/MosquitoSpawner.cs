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

    // temporary test hook
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SpawnMosquito();
        }
    }

    public void SpawnMosquito()
    {
        if (mosquitoManager == null)
        {
            Debug.LogWarning("Mosquito manager is not assigned.", this);
            return;
        }

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;

            Vector3 candidatePosition = transform.position +
                new Vector3(randomOffset.x, 0f, randomOffset.y);

            if (!NavMesh.SamplePosition(
                candidatePosition,
                out NavMeshHit hit,
                10f,
                NavMesh.AllAreas))
            {
                Debug.Log("Spawn failed: No NavMesh found.");
                continue;
            }

            if (Physics.CheckSphere(
                hit.position,
                collisionRadius,
                collisionLayers,
                QueryTriggerInteraction.Ignore))
            {
                Debug.Log("Spawn failed: Collision detected.");
                continue;
            }

            MosquitoAgent newAgent = mosquitoManager.SpawnMosquito(
                hit.position,
                transform.rotation
            );

            if (newAgent == null)
            {
                Debug.LogWarning("Manager failed to spawn mosquito.", this);
                return;
            }

            mosquitoManager.MoveOneTo(newAgent, mosquitoManager.pointB);

            return;
        }

        Debug.LogWarning(
            "Unable to find a valid mosquito spawn location.",
            this
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}