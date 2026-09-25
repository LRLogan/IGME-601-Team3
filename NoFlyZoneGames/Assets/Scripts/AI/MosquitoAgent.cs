
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MosquitoAgent : MonoBehaviour
{
    private NavMeshAgent mAgent;

    private void Awake()
    {
        mAgent = GetComponent<NavMeshAgent>();

        // The mosquito will eventually use custom flying movement.
        // For now, let NavMeshAgent handle movement.
        mAgent.speed = 3f;
    }

    public void MoveTo(Vector3 target)
    {
        if (!mAgent.isOnNavMesh)
        {
            Debug.LogWarning($"{name} is not on the NavMesh.");
            return;
        }

        mAgent.SetDestination(target);
        Debug.Log(
    $"On NavMesh: {mAgent.isOnNavMesh} | " +
    $"Destination: {mAgent.destination}");
    }
}

