using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MosquitoAgent : MonoBehaviour
{
    public enum Task
    {
        Idle,
        Pathfinding,
        Move
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arriveDistance = 2f;
    [SerializeField] private float fleeDistance = 5f;

    public Task CurrentTask { get; private set; }

    private NavMeshAgent mAgent;

    private Vector3 mTarget;
    private bool mFleeing;

    private void Awake()
    {
        mAgent = GetComponent<NavMeshAgent>();

        mAgent.speed = moveSpeed;

        CurrentTask = Task.Idle;
    }

    private void Update()
    {
        switch (CurrentTask)
        {
            case Task.Idle:
                Idle();
                break;

            case Task.Pathfinding:
                Pathfinding();
                break;

            case Task.Move:
                Move();
                break;
        }
    }

    //==================================================
    // TASK SYSTEM
    //==================================================

    private void Idle()
    {
        // Nothing to do.
    }

    private void Pathfinding()
    {
        if (!mAgent.isOnNavMesh)
        {
            Debug.LogWarning($"{name} is not on the NavMesh.");
            CurrentTask = Task.Idle;
            return;
        }

        mAgent.SetDestination(mTarget);

        CurrentTask = Task.Move;
    }

    private void Move()
    {
        if (mFleeing)
        {
            Flee();
            return;
        }

        // If this is the final destination, Arrive.
        if (Vector3.Distance(transform.position, mTarget) <= arriveDistance)
        {
            Arrive();
        }
        else
        {
            Seek();
        }
    }

    //==================================================
    // MOVEMENT BEHAVIORS
    //==================================================

    private void Seek()
    {
        mAgent.speed = moveSpeed;
        mAgent.SetDestination(mTarget);
    }

    private void Arrive()
    {
        float distance = Vector3.Distance(
            transform.position,
            mTarget);

        if (distance <= mAgent.stoppingDistance + 0.1f)
        {
            Stop();
            CurrentTask = Task.Idle;
            return;
        }

        float speedPercent = Mathf.Clamp01(
            distance / arriveDistance);

        mAgent.speed = moveSpeed * speedPercent;
        mAgent.SetDestination(mTarget);
    }

    private void Flee()
    {
        Vector3 fleeDirection =
            transform.position - mTarget;

        fleeDirection.y = 0f;

        if (fleeDirection.sqrMagnitude < 0.001f)
            return;

        fleeDirection.Normalize();

        Vector3 fleePosition =
            transform.position +
            fleeDirection * fleeDistance;

        if (NavMesh.SamplePosition(
                fleePosition,
                out NavMeshHit hit,
                fleeDistance,
                NavMesh.AllAreas))
        {
            mAgent.speed = moveSpeed;
            mAgent.SetDestination(hit.position);

            if (Vector3.Distance(
                    transform.position,
                    mTarget) >= fleeDistance)
            {
                mFleeing = false;
                Stop();
                CurrentTask = Task.Idle;
            }
        }
    }

    //==================================================
    // PUBLIC TASK INTERFACE
    //==================================================

    public void MoveTo(Vector3 target)
    {
        mTarget = target;
        mFleeing = false;

        CurrentTask = Task.Pathfinding;
    }

    public void FleeFrom(Vector3 target)
    {
        mTarget = target;
        mFleeing = true;

        CurrentTask = Task.Pathfinding;
    }

    public void SetTask(Task task)
    {
        CurrentTask = task;
    }

    public void Stop()
    {
        mAgent.ResetPath();
        mAgent.speed = moveSpeed;
    }
}
