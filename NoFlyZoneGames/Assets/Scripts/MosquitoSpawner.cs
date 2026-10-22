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
            // Generate a random position within the spawn radius.
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;

            Vector3 candidatePosition = transform.position +
                new Vector3(randomOffset.x, 0f, randomOffset.y);

            // Find a valid nearby NavMesh position.
            if (!NavMesh.SamplePosition(
                candidatePosition,
                out NavMeshHit hit,
                1f,
                NavMesh.AllAreas))
            {
                continue;
            }

            // Ensure the position is not occupied.
            if (Physics.CheckSphere(
                hit.position,
                collisionRadius,
                collisionLayers,
                QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            // Valid spawn location found.
            mosquitoManager.SpawnMosquito(
                hit.position,
                transform.rotation
            );

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