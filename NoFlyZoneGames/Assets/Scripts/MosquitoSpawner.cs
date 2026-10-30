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

    // temporary test hook
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SpawnMosquito();
        }
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
}