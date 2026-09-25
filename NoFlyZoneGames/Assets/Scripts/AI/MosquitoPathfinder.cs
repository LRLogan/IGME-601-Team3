using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class MosquitoPathfinder
{
    private static readonly NavMeshPath mNavMeshPath = new();

    public static bool FindPath(Vector3 start, Vector3 target, List<Vector3> path)
    {
        path.Clear();

        if (!NavMesh.SamplePosition(
                start,
                out NavMeshHit startHit,
                5f,
                NavMesh.AllAreas))
            return false;

        if (!NavMesh.SamplePosition(
                target,
                out NavMeshHit targetHit,
                5f,
                NavMesh.AllAreas))
            return false;

        if (!NavMesh.CalculatePath(
                startHit.position,
                targetHit.position,
                NavMesh.AllAreas,
                mNavMeshPath))
            return false;

        if (mNavMeshPath.status == NavMeshPathStatus.PathInvalid)
            return false;

        path.AddRange(mNavMeshPath.corners);

        return path.Count > 0;
    }
}
