using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MosquitoAgent : MonoBehaviour
{
    // Identifier to pass into the state machiene controlling the agent behavior
    public enum Task
    {
        Idle,
        Pathfinding,
        Move
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waypointDistance = 0.2f;

    public Task CurrentTask { get; private set; }

    private List<Vector3> mPath = new();
    private int mPathIndex;

    private void Start()
    {
        CurrentTask = Task.Idle;
    }

    private void Update()
    {
        if (CurrentTask != Task.Move) return;

        Move();
    }

    public void MoveTo(Vector3 target)
    {
        CurrentTask = Task.Pathfinding;

        if (!MosquitoPathfinder.FindPath(transform.position, target, mPath))
        {
            CurrentTask = Task.Idle;
            return;
        }

        mPathIndex = 0;
        CurrentTask = Task.Move;
    }

    private void Move()
    {
        if (mPathIndex >= mPath.Count)
        {
            CurrentTask = Task.Idle;
            return;
        }

        Vector3 target = mPath[mPathIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime);

        Vector3 direction = target - transform.position;

        if (direction.sqrMagnitude > 0.001f)
            transform.forward = direction.normalized;

        if (Vector3.Distance(transform.position, target) <= waypointDistance)
            mPathIndex++;
    }
}
