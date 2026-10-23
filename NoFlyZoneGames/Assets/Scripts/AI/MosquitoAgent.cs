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
    [SerializeField] private float mMaxSpeed = 4f;
    [SerializeField] private float mMaxForce = 10f;

    [Header("Movement Collision")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float collisionSkin = 0.02f;

    [Header("Vertical Flight")]
    [SerializeField] private float verticalSpeed = 3f;
    [SerializeField] private float verticalAcceleration = 10f;
    [SerializeField] private float flightVariation = 0.4f;
    [SerializeField] private float variationFrequency = 1.5f;

    [Header("Jitter")]
    [SerializeField] private float jitterRadius = 0.75f;
    [SerializeField] private float jitterForce = 50f;
    [SerializeField] private float maxJitterSpeed = 5f;

    [Tooltip("Time in seconds")]
    [SerializeField] private float jitterChangeInterval = 0.75f;

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
    private Vector3 mJitterVelocity;
    private float mFlightSeed;
    private readonly Stack<Vector3> mTargetStack = new Stack<Vector3>();

    // External control vars
    // External control is for COMPLETE control over the agent
    // If you still want the agent to influince behavior DO NOT use external control
    private bool mExternallyControlled;
    private Task mPreviousTask;
    private Vector3 mPreviousTarget;
    private bool mPreviousFleeing;

    // Inline way to get the current target
    private Vector3 CurrentTarget => mTargetStack.Peek();

    private void Awake()
    {
        mAgent = GetComponent<NavMeshAgent>();

        // NavMeshAgent is responsible for calculating the path,
        // but NOT for moving or rotating the mosquito.
        mAgent.updatePosition = false;
        mAgent.updateRotation = false;

        mAgent.speed = moveSpeed;

        mFlightSeed = Random.Range(0f, 1000f);
        jitterTarget = Random.insideUnitSphere * jitterRadius;
        CurrentTask = Task.Idle;
    }

    private void Update()
    {
        if (mExternallyControlled) return;

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

        // Movement and external forces are processed in one place,
        // even if the agent has no active navigation task.
        ApplyVelocity();
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
            CurrentTask = Task.Idle;
            return;
        }

        // FleeFrom uses its own target, rather than the target stack.
        if (mFleeing)
        {
            CurrentTask = Task.Move;
            return;
        }

        if (mTargetStack.Count == 0)
        {
            CurrentTask = Task.Idle;
            return;
        }

        mTarget = CurrentTarget;

        // Map the 3D destination onto the surface used for XZ routing.
        // This DOES NOT replace mTarget.y, which is used for flight.
        if (!NavMesh.SamplePosition(mTarget, out NavMeshHit hit,
                20f, mAgent.areaMask) ||
            !mAgent.SetDestination(hit.position))
        {
            Debug.LogWarning("Could not find a NavMesh destination.", this);
            CurrentTask = Task.Idle;
            return;
        }

        CurrentTask = Task.Move;
    }

    private void Move()
    {
        if (!mFleeing)
        {
            if (mTargetStack.Count == 0)
            {
                CurrentTask = Task.Idle;
                return;
            }

            mTarget = CurrentTarget;

            if (Vector3.Distance(transform.position, mTarget) <= arriveDistance)
            {
                CompleteCurrentTarget();
                return;
            }

            // When the path reaches the goal's XZ, vertical flight
            // continues until the mosquito reaches the true 3D target.
            if (!mAgent.pathPending)
            {
                if (mAgent.remainingDistance <= arriveDistance)
                    Arrive();
                else
                    Seek();
            }
        }
        else
        {
            Flee();
        }

        if (CurrentTask == Task.Move)
        {
            ApplyVerticalSteering();
            UpdateJitter();
        }
    }

    private void ApplyVelocity()
    {
        // No residual navigation/jitter drifting after a task ends.
        if (CurrentTask != Task.Move)
        {
            mVelocity = Vector3.zero;
            mJitterVelocity = Vector3.zero;
        }

        Vector3 navigationVelocity = Vector3.ClampMagnitude(mVelocity, mMaxSpeed);
        Vector3 jitterVelocity = CurrentTask == Task.Move
            ? mJitterVelocity : Vector3.zero;

        Vector3 finalVelocity = navigationVelocity + jitterVelocity + mExternalVelocity;
        MoveWithCollision(finalVelocity * Time.deltaTime);

        mExternalVelocity = Vector3.Lerp(
            mExternalVelocity, Vector3.zero, externalDrag * Time.deltaTime);

        SyncNavMeshPosition();

        if (navigationVelocity.sqrMagnitude > 0.001f)
            transform.forward = navigationVelocity.normalized;
    }

    private void MoveWithCollision(Vector3 displacement)
    {
        float distance = displacement.magnitude;

        if (distance <= 0.0001f)
            return;

        Vector3 direction = displacement / distance;
        Vector3 startPosition = transform.position;

        // Check whether the mosquito would hit an obstacle.
        if (Physics.SphereCast(
            startPosition,
            collisionRadius,
            direction,
            out RaycastHit hit,
            distance + collisionSkin,
            obstacleLayers,
            QueryTriggerInteraction.Ignore))
        {
            // Move up to the obstacle without penetrating it.
            float safeDistance = Mathf.Clamp(
                hit.distance - collisionSkin,
                0f,
                distance
            );

            Vector3 safeMovement = direction * safeDistance;

            // Calculate the remaining movement.
            Vector3 remainingMovement = displacement - safeMovement;

            // Project along the obstacle instead of moving into it.
            Vector3 slideMovement = Vector3.ProjectOnPlane(
                remainingMovement,
                hit.normal
            );

            Vector3 slideStart = startPosition + safeMovement;

            // Check the sliding direction for another obstacle.
            float slideDistance = slideMovement.magnitude;

            if (slideDistance > 0.0001f)
            {
                Vector3 slideDirection = slideMovement / slideDistance;

                if (Physics.SphereCast(
                    slideStart,
                    collisionRadius,
                    slideDirection,
                    out RaycastHit slideHit,
                    slideDistance + collisionSkin,
                    obstacleLayers,
                    QueryTriggerInteraction.Ignore))
                {
                    slideDistance = Mathf.Clamp(
                        slideHit.distance - collisionSkin,
                        0f,
                        slideDistance
                    );
                }

                safeMovement += slideDirection * slideDistance;
            }

            transform.position += safeMovement;
        }
        else
        {
            transform.position += displacement;
        }
    }

    private void SyncNavMeshPosition()
    {
        if (!mAgent.isOnNavMesh) return;

        // Keep the navigation simulation on its surface while
        // allowing the mosquito transform to fly freely in Y.
        Vector3 navPosition = mAgent.nextPosition;
        navPosition.x = transform.position.x;
        navPosition.z = transform.position.z;
        mAgent.nextPosition = navPosition;
    }

    private void ApplyVerticalSteering()
    {
        // Flee uses the opposite altitude direction from the threat.
        float targetY = mTarget.y;
        if (mFleeing)
        {
            float sign = Mathf.Sign(transform.position.y - mTarget.y);
            if (Mathf.Approximately(sign, 0f)) sign = 1f;
            targetY = transform.position.y + sign * fleeDistance;
        }

        float distance = Vector3.Distance(transform.position, mTarget);
        float variationWeight = mFleeing ? 1f :
            Mathf.Clamp01(distance / Mathf.Max(0.01f, arriveDistance * 3f));

        float noise = Mathf.PerlinNoise(
            mFlightSeed, Time.time * variationFrequency) * 2f - 1f;

        float desiredHeight = targetY +
            noise * flightVariation * variationWeight;

        float desiredYSpeed = Mathf.Clamp(
            (desiredHeight - transform.position.y) * 2f,
            -verticalSpeed, verticalSpeed);

        mVelocity.y = Mathf.MoveTowards(
            mVelocity.y, desiredYSpeed,
            verticalAcceleration * Time.deltaTime);
    }

    private void UpdateJitter()
    {
        timeSinceLastJitter += Time.deltaTime;
        if (timeSinceLastJitter >= Mathf.Max(0.01f, jitterChangeInterval))
        {
            timeSinceLastJitter = 0f;
            jitterTarget = Random.insideUnitSphere * jitterRadius;
        }

        // Jitter is its own small, capped velocity; it no longer
        // accumulates indefinitely inside Seek() or Arrive().
        Vector3 desired = jitterTarget.sqrMagnitude > 0.0001f
            ? jitterTarget.normalized * maxJitterSpeed
            : Vector3.zero;

        mJitterVelocity = Vector3.MoveTowards(
            mJitterVelocity, desired,
            jitterForce * Time.deltaTime);

        // Fade jitter out near a 3D target so arrival is achievable.
        if (!mFleeing)
        {
            float distance = Vector3.Distance(transform.position, mTarget);
            mJitterVelocity *= Mathf.Clamp01(
                distance / Mathf.Max(0.01f, arriveDistance * 2f));
        }

        Debug.DrawRay(transform.position, mJitterVelocity, Color.red);
    }

    private void CompleteCurrentTarget()
    {
        mTargetStack.Pop();
        mVelocity = Vector3.zero;
        mJitterVelocity = Vector3.zero;

        if (mTargetStack.Count == 0)
        {
            Stop();
            CurrentTask = Task.Idle;
        }
        else
        {
            CurrentTask = Task.Pathfinding;
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
        Vector3 desired = mAgent.desiredVelocity;
        desired.y = 0f;
        if (desired.sqrMagnitude < 0.0001f) return;

        SteerHorizontal(desired.normalized * moveSpeed);
    }

    private void SteerHorizontal(Vector3 desiredXZ)
    {
        Vector3 currentXZ = new Vector3(mVelocity.x, 0f, mVelocity.z);
        Vector3 steering = Vector3.ClampMagnitude(
            desiredXZ - currentXZ, mMaxForce);

        Vector3 nextXZ = currentXZ +
            steering / Mathf.Max(0.01f, mMass) * Time.deltaTime;

        mVelocity.x = nextXZ.x;
        mVelocity.z = nextXZ.z;
        // Preserve mVelocity.y for vertical steering.
    }

    /// <summary>
    /// Arrive behavior.
    /// Slows the mosquito as it approaches its final target.
    /// </summary>
    private void Arrive()
    {
        Vector3 desired = mAgent.desiredVelocity;
        desired.y = 0f;

        // Use horizontal distance for horizontal slowing.
        Vector3 delta = mTarget - transform.position;
        delta.y = 0f;
        float horizontalDistance = delta.magnitude;

        float speedPercent = Mathf.Clamp01(
            horizontalDistance / Mathf.Max(0.01f, arriveDistance));

        if (desired.sqrMagnitude > 0.0001f)
            desired = desired.normalized * moveSpeed * speedPercent;
        else
            desired = Vector3.zero;

        SteerHorizontal(desired);
        // Do NOT stop the agent here: Move() handles 3D arrival
        // and popping the target stack.
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

            CurrentTask = mTargetStack.Count > 0
                ? Task.Pathfinding : Task.Idle;
            return;
        }

        Vector3 fleeDirection = transform.position - mTarget;
        fleeDirection.y = 0f;

        if (fleeDirection.sqrMagnitude < 0.001f)
            fleeDirection = transform.forward;

        fleeDirection.y = 0f;
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
    /// Gives external systems temporary control of the mosquito.
    /// The current task state is saved so it can be resumed later.
    /// </summary>
    public void BeginExternalControl()
    {
        if (mExternallyControlled) return;

        // Save the current task state.
        mPreviousTask = CurrentTask;
        mPreviousTarget = mTarget;
        mPreviousFleeing = mFleeing;

        // Stop the agent from controlling movement.
        mVelocity = Vector3.zero;
        mExternalVelocity = Vector3.zero;

        mExternallyControlled = true;
    }

    /// <summary>
    /// Returns control to the mosquito's normal AI.
    /// The previous task is resumed from the mosquito's new position.
    /// </summary>
    public void EndExternalControl()
    {
        if (!mExternallyControlled) return;

        mExternallyControlled = false;

        // Restore the task state.
        CurrentTask = mPreviousTask;
        mTarget = mPreviousTarget;
        mFleeing = mPreviousFleeing;

        // The external controller may have moved the mosquito,
        // so synchronize the NavMeshAgent with its new position.
        SyncNavMeshPosition();

        // A new path is required because our position may have changed.
        if (CurrentTask == Task.Move ||
            CurrentTask == Task.Pathfinding)
        {
            CurrentTask = Task.Pathfinding;
        }
    }

    /// <summary>
    /// Simple move to behavior 
    /// </summary>
    /// <param name="target"></param>
    public void MoveTo(Vector3 target)
    {
        mTargetStack.Push(target);
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
        if (mAgent.isOnNavMesh)
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

    /// <summary>
    /// Compleatly reset the agent used when despawning / sending to object pool
    /// </summary>
    public void ResetAgent()
    {
        mTargetStack.Clear();

        mVelocity = Vector3.zero;
        mExternalVelocity = Vector3.zero;

        mFleeing = false;
        mExternallyControlled = false;
        mJitterVelocity = Vector3.zero;
        timeSinceLastJitter = 0f;
        jitterTarget = Random.insideUnitSphere * jitterRadius;
        CurrentTask = Task.Idle;

        if (mAgent.isOnNavMesh)
        {
            mAgent.ResetPath();
            SyncNavMeshPosition();
        }
    }

    #endregion
}