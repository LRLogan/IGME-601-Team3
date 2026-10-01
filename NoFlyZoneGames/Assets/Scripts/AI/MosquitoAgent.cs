using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MosquitoAgent : MonoBehaviour
{
    /// <summary>
    /// Overarching tasks the mosquito can partake in 
    /// </summary>
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
    [SerializeField] private float mMass = 1f;
    [SerializeField] private float mMaxSpeed = 3f;
    [SerializeField] private float mMaxForce = 10f;
    [SerializeField] private float maxHeight = 1f;
    [SerializeField] private float minHeight = 10f;
    [SerializeField] private float jitterRadius = 0.5f;
    [SerializeField] private float jitterForce = 0.5f;

    [Tooltip("Time in seconds")]
    [SerializeField] private float jitterChangeInterval = 2f;

    [Header("External Forces")]
    [SerializeField] private float externalDrag = 2f;

    public Task CurrentTask { get; private set; }

    // --- Internal references ---
    private NavMeshAgent mAgent;
    private Vector3 mTarget;
    private Vector3 mVelocity;
    private Vector3 mExternalVelocity;
    private bool mFleeing;
    private float timeSinceLastJitter = 0f;
    private Vector3 jitterTarget;

    private void Awake()
    {
        mAgent = GetComponent<NavMeshAgent>();

        // NavMeshAgent is responsible for calculating the path,
        // but NOT for moving or rotating the mosquito.
        mAgent.updatePosition = false;
        mAgent.updateRotation = false;

        mAgent.speed = moveSpeed;

        CurrentTask = Task.Idle;
    }

    private void Update()
    {
        // Delegates a task to its respective behavior 
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

    #region TASK SYSTEM

    /// <summary>
    /// Idle task
    /// </summary>
    private void Idle()
    {
        // Nothing to do yet
    }

    /// <summary>
    /// Pathfinding task
    /// </summary>
    private void Pathfinding()
    {
        if (!mAgent.isOnNavMesh)
        {
            Debug.LogWarning($"{name} is not on the NavMesh.");
            CurrentTask = Task.Idle;
            return;
        }

        // Ask the NavMeshAgent to calculate a path.
        // It will not move the mosquito because updatePosition is false.
        mAgent.SetDestination(mTarget);

        CurrentTask = Task.Move;
    }

    /// <summary>
    /// Move task
    /// </summary>
    private void Move()
    {
        if (mFleeing)
        {
            Flee();
        }
        else if (Vector3.Distance(transform.position, mTarget) <= arriveDistance)
        {
            Arrive();
        }
        else
        {
            Seek();
        }

        ApplyVelocity();
    }

    /// <summary>
    /// Applies the calculated velocity to the mosquito's position.
    /// The NavMeshAgent does not directly move the mosquito.
    /// </summary>
    private void ApplyVelocity()
    {
        mVelocity = Vector3.ClampMagnitude(
            mVelocity,
            mMaxSpeed);

        Vector3 finalVelocity = mVelocity + mExternalVelocity;
        transform.position += finalVelocity * Time.deltaTime;

        mExternalVelocity = Vector3.Lerp(
            mExternalVelocity,
            Vector3.zero,
            externalDrag * Time.deltaTime);

        // Keep the NavMeshAgent synchronized with our manually
        // controlled position so it can continue calculating paths.
        mAgent.nextPosition = transform.position;
            Mathf.Clamp(transform.position.y, minHeight, maxHeight);

        if (mVelocity.sqrMagnitude > 0.001f)
        {
            transform.forward = mVelocity.normalized;
        }
    }

    #endregion

    #region MOVEMENT BEHAVIORS

    /// <summary>
    /// Seek behavior.
    /// Uses the velocity calculated by the NavMeshAgent's path
    /// as the desired movement direction.
    /// </summary>
    private void Seek()
    {
        Vector3 desiredVelocity = mAgent.desiredVelocity;

        if (desiredVelocity.sqrMagnitude <= 0.001f) return;

        desiredVelocity = desiredVelocity.normalized * moveSpeed;

        Vector3 steering = desiredVelocity - mVelocity;

        steering = Vector3.ClampMagnitude(
            steering,
            mMaxForce);

        Vector3 acceleration = steering / mMass;

        mVelocity += (acceleration + Jitter()) * Time.deltaTime;
    }

    /// <summary>
    /// Adds a jitter behavior to the mosquitos movement
    /// </summary>
    /// <returns>jitter force</returns>
    private Vector3 Jitter()
    {
        if (mAgent == null) return Vector3.zero;

        // Update the time and target
        timeSinceLastJitter += Time.deltaTime;
        if(timeSinceLastJitter >= jitterChangeInterval)
        {
            timeSinceLastJitter = 0;
            jitterTarget = Random.insideUnitSphere * jitterRadius;
        }
        jitterTarget = jitterTarget.normalized * jitterRadius;

        // Find the world position
        Vector3 targetWorld =
            mAgent.gameObject.transform.position + jitterTarget;

        // returns the calculated steering force
        Vector3 steeringForce = 
            (targetWorld - mAgent.gameObject.transform.position).normalized * jitterForce;
        //steeringForce.y = 0;
        Debug.DrawRay(mAgent.gameObject.transform.position, steeringForce, Color.red);
        return steeringForce;
    }

    /// <summary>
    /// Arrive behavior.
    /// Slows the mosquito as it approaches its final target.
    /// </summary>
    private void Arrive()
    {
        float distance = Vector3.Distance(
            transform.position,
            mTarget);

        if (distance <= mAgent.stoppingDistance + 0.1f)
        {
            mVelocity = Vector3.zero;

            Stop();

            CurrentTask = Task.Idle;
            return;
        }

        Vector3 desiredVelocity = mAgent.desiredVelocity;

        if (desiredVelocity.sqrMagnitude <= 0.001f)
            return;

        // Reduce the desired speed as we approach the target.
        float speedPercent = Mathf.Clamp01(
            distance / arriveDistance);

        desiredVelocity =
            desiredVelocity.normalized *
            moveSpeed *
            speedPercent;

        Vector3 steering =
            desiredVelocity - mVelocity;

        steering = Vector3.ClampMagnitude(
            steering,
            mMaxForce);

        Vector3 acceleration =
            steering / mMass;

        mVelocity += acceleration * Time.deltaTime;
    }

    /// <summary>
    /// Flee behavior.
    /// Calculates a point away from the target and asks the
    /// NavMeshAgent to path toward that point.
    /// </summary>
    private void Flee()
    {
        float distance = Vector3.Distance(
            transform.position,
            mTarget);

        // Once sufficiently far away, stop fleeing.
        if (distance >= fleeDistance)
        {
            mFleeing = false;

            mVelocity = Vector3.zero;

            Stop();

            CurrentTask = Task.Idle;
            return;
        }

        Vector3 fleeDirection =
            transform.position - mTarget;

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
            mAgent.SetDestination(hit.position);

            Seek();
        }
    }

    #endregion

    #region PUBLIC TASK INTERFACE

    /// <summary>
    /// Simple move to behavior 
    /// </summary>
    /// <param name="target"></param>
    public void MoveTo(Vector3 target)
    {
        mTarget = target;
        mFleeing = false;

        CurrentTask = Task.Pathfinding;
    }

    /// <summary>
    /// Simple fleeing behavior
    /// </summary>
    /// <param name="target"></param>
    public void FleeFrom(Vector3 target)
    {
        mTarget = target;
        mFleeing = true;

        CurrentTask = Task.Pathfinding;
    }

    /// <summary>
    /// External task setting
    /// </summary>
    /// <param name="task"></param>
    public void SetTask(Task task)
    {
        CurrentTask = task;
    }

    /// <summary>
    /// Reset function 
    /// </summary>
    public void Stop()
    {
        mAgent.ResetPath();
        mAgent.speed = moveSpeed;
    }

    /// <summary>
    /// Applies an external force to the mosquito.
    /// Designed to provide an interface similar to Rigidbody.AddForce().
    /// </summary>
    /// <param name="force">Force vector to apply.</param>
    /// <param name="mode">How the force should affect velocity.</param>
    public void AddForce(Vector3 force, ForceMode mode)
    {
        switch (mode)
        {
            case ForceMode.Force:
                mExternalVelocity +=
                    force / mMass *
                    Time.deltaTime;
                break;

            case ForceMode.Acceleration:
                mExternalVelocity +=
                    force *
                    Time.deltaTime;
                break;

            case ForceMode.Impulse:
                mExternalVelocity +=
                    force / mMass;
                break;

            case ForceMode.VelocityChange:
                mExternalVelocity += force;
                break;
        }

        mExternalVelocity = Vector3.ClampMagnitude(
            mExternalVelocity,
            mMaxSpeed);
    }

    #endregion
}