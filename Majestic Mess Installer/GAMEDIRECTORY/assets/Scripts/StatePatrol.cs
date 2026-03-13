using System;
using System.Collections.Generic;
using Engine;

public class StatePatrol : IState
{
    private AIController ai;

    //private Transform[] waypoints;
    //private List<Vector3> waypoints;
    //private int currentWaypointIndex = 0;
    private float waypointReachedThreshold = 0.1f;   // intermediate nav nodes
    private float patrolWaypointThreshold = 1.5f;     // final destination (navmesh endpoint can be offset from waypoint)
    //private float waitAnimFinish;


    // public StatePatrol(AIController ai, Transform[] waypoints)
    // {
    //     this.ai = ai;
    //     ai.agent.speed = ai.moveSpeed;
    //     if (waypoints.Length != 0)
    //     {
    //         this.waypoints = waypoints;
    //         currentWaypointIndex = ai.currentPatrolIndex;
    //         ai.currentPatrolStopTime = ai.patrolStopTime[currentWaypointIndex];
    //     }
    // }

    public StatePatrol(AIController ai)
    {
        this.ai = ai;

        if (ai.patrolWaypoints != null && ai.patrolWaypoints.Count != 0)
        {
            //currentWaypointIndex = ai.currentPatrolIndex;
            if (ai.patrolStopTime != null && ai.patrolStopTime.Length > 0)
            {
                // Make sure currentPatrolIndex is within bounds
                if (ai.currentPatrolIndex >= 0 && ai.currentPatrolIndex < ai.patrolStopTime.Length)
                {
                    ai.currentPatrolStopTime = ai.patrolStopTime[ai.currentPatrolIndex];
                }
                else
                {
                    ai.currentPatrolIndex = 0;
                    ai.currentPatrolStopTime = ai.patrolStopTime[0];
                }
            }
        }
    }

    public void Enter()
    {
        /* Start patrolling */
        //currentWaypointIndex = 0;


        //Anims bools
        ai.isStartled = false;
        //ai.anim.SetBool("isStartled", ai.isStartled);

        ai.isPatrolling = true;
        //ai.anim.SetBool("isPatrolling", ai.isPatrolling);
        ai.isIdle = false;
        ai.isChasing = false;
        ai.isAttacking = false;
        ai.UpdateAnimationFromBools();

        //ANim play time
        //waitAnimFinish = 2f;
        //Idle anim type
        //ai.anim.SetBool("isStretching", ai.isStretching); //Initalize the values depending on what was ticked in inspector
        //ai.anim.SetBool("isBrowsing", ai.isBrowsing);
        //ai.anim.SetBool("isTalking", ai.isTalking);

        ai.isPlayingIdle = false;
        //ai.anim.SetBool("isPlayingIdle", ai.isPlayingIdle);
        ai.isWithinIdleThreshold = false;
        //ai.anim.SetBool("isWithinIdleThreshold", ai.isWithinIdleThreshold);

        ai.ResetPath(); // clear any previous path

        // Safety check before accessing waypoints
        if (ai.patrolWaypoints == null || ai.patrolWaypoints.Count == 0)
        {
            Debug.Log($"StatePatrol.cs [{ai.Name}] Enter(): No patrol waypoints configured!");
            return;
        }

        // Calculate Nav Path when enter
        ai.CalculateNavPath(ai.Transform.Position, ai.patrolWaypoints[ai.currentPatrolIndex].Transform.Position);
    }

    public void OnUpdate(float dt)
    {
        // Safety check before accessing waypoints
        if (ai.patrolWaypoints == null || ai.patrolWaypoints.Count == 0)
        {
            Debug.Log($"StatePatrol.cs [{ai.Name}] OnUpdate(): No patrol waypoints configured!");
            return;
        }

        if (ai.currentPatrolIndex >= ai.patrolWaypoints.Count)
            ai.currentPatrolIndex = 0;

        // When Player is detected
        if (ai.isPlayerDetected || ai.CheckPlayerFootstepAudioDist())
        {
            // update wary guage
            ai.ResetPath();
            ai.isAllyDying = false;
            ai.targetPosition = ai.playerObj.Transform.Position;
            ai.waryGauge += 10.0f; // give extra so it wont just reset back to patrol
            ai.ChangeState(new StateWary(ai));
            return;
        }
        else if (ai.isAllyDying)
        {
            ai.ResetPath();
            ai.waryGauge += 100.0f;
            ai.ChangeState(new StateWary(ai));
            return;
        }

        // update wary guage
        if (ai.waryGauge >= 0.0f)
        {
            ai.waryGauge -= ai.gaugeDecreaseRate * dt;
            if (ai.waryGauge < 0.0f)
                ai.waryGauge = 0.0f;
        }

        // If no current nav path, request one
        if (ai.navPath == null || ai.navPath.Count == 0)
        {
            ai.CalculateNavPath(ai.Transform.Position, ai.patrolWaypoints[ai.currentPatrolIndex].Transform.Position);
            return;
        }

        // prevent out of range
        if (ai.currentPathIndex >= ai.navPath.Count)
            ai.currentPathIndex = ai.navPath.Count - 1;


        // HERE IS TO CHECK INBETWEEN WAYPOINTS, POINT A -> A.1 -> A.2 -> .... -> POINT B
        // Current target navmesh node
        Vector3 targetPos = ai.navPath[ai.currentPathIndex];
        Vector3 targetRot = ai.patrolWaypoints[ai.currentPatrolIndex].Transform.Rotation;
        Vector3 currPos = ai.Transform.Position;

        // Move towards current navmesh waypoint
        ai.Transform.Position = MoveTowards(currPos, targetPos, ai.moveSpeed * dt);

        Vector3 patrolTarget = ai.patrolWaypoints[ai.currentPatrolIndex].Transform.Position;
        bool atPatrolWaypoint = Vector3.Distance(
            new Vector3(ai.Transform.Position.x, 0, ai.Transform.Position.z),
            new Vector3(patrolTarget.x, 0, patrolTarget.z)) < patrolWaypointThreshold;

        if (!ai.isPatrolling)
        {
            // Do not re-enable patrol while intentionally idling at waypoint.
            if (!atPatrolWaypoint)
            {
                ai.isPatrolling = true;
                ai.isIdle = false;
                ai.UpdateAnimationFromBools();
            }
        }
        else
        {
            ai.Transform.Rotation = LookAt(ai.Transform.Rotation, ai.Transform.Position, targetPos, ai.rotationSpeed * dt);
        }


        // Check if reached current navmesh waypoint — only advance if not already at the last node
        if (ai.currentPathIndex < ai.navPath.Count - 1 &&
            Vector3.Distance(new Vector3(ai.Transform.Position.x, 0, ai.Transform.Position.z), new Vector3(targetPos.x, 0, targetPos.z)) < waypointReachedThreshold)
        {
            ai.currentPathIndex++;
        }

        // HERE IS TO CHECK WHETHER IT REACHED POINT B
        // Check if patrol waypoint reached (last node in navPath) ignored y axis
        if (atPatrolWaypoint)
        {
            if (ai.isPatrolling)
            {
                if (!ai.idleAtWaypoint)
                {
                    // This knight is configured to not idle at waypoints.
                    ai.currentPatrolIndex = (ai.currentPatrolIndex + 1) % ai.patrolWaypoints.Count;
                    if (ai.patrolStopTime != null)
                        ai.currentPatrolStopTime = ai.patrolStopTime[ai.currentPatrolIndex];

                    Vector3 skipIdleNext = ai.patrolWaypoints[ai.currentPatrolIndex].Transform.Position;
                    ai.CalculateNavPath(ai.Transform.Position, skipIdleNext);
                    return;
                }

                if (ai.currentPatrolStopTime <= 0f)
                    ai.currentPatrolStopTime = 1.5f;

                // Ensure variant-idle knights have enough stop time to complete
                // one full cycle: variant hold + base idle hold.
                if (ai.idleType >= 2 && ai.idleType <= 4)
                {
                    float minVariantCycle = ai.idleVariantHoldSeconds + ai.idleBaseHoldSeconds;
                    if (minVariantCycle > ai.currentPatrolStopTime)
                        ai.currentPatrolStopTime = minVariantCycle;
                }

                

                ai.isPatrolling = false;
                ai.isIdle = true;
                ai.isPlayingIdle = true;
                ai.isWithinIdleThreshold = true;
                ai.UpdateAnimationFromBools();
            }
            ai.Transform.Rotation = RotateTowards(ai.Transform.Rotation, targetRot, ai.rotationSpeed * dt);
            // Decrease stop timer
            ai.currentPatrolStopTime -= dt;
            if (ai.currentPatrolStopTime <= 0f)
            {
                ai.isPlayingIdle = false;
                ai.isWithinIdleThreshold = false;

                // Move to next patrol waypoint
                ai.currentPatrolIndex = (ai.currentPatrolIndex + 1) % ai.patrolWaypoints.Count;

                // Reset stop timer for next waypoint
                if (ai.patrolStopTime != null)
                    ai.currentPatrolStopTime = ai.patrolStopTime[ai.currentPatrolIndex];

                ai.isPatrolling = true;
                ai.isIdle = false;
                ai.UpdateAnimationFromBools();

                // Recalculate navPath for new patrol waypoint
                Vector3 nextTarget = ai.patrolWaypoints[ai.currentPatrolIndex].Transform.Position;
                ai.CalculateNavPath(ai.Transform.Position, nextTarget);
            }
        }
    }

    public void Exit() 
    {
        /* Cleanup */

        ai.isStartled = true;
        //ai.anim.SetBool("isStartled", ai.isStartled);

        ai.isPatrolling = false;
        //ai.anim.SetBool("isPatrolling", ai.isPatrolling);
        ai.isIdle = true;
        ai.UpdateAnimationFromBools();
    }

    public static Vector3 MoveTowards(Vector3 from, Vector3 to, float speed)
    {
        //Vector3 dir = new Vector3(to.x - from.x, to.y - from.y, to.z - from.z);
        Vector3 dir = to - from;
        dir.y = 0.0f;

        float dist = dir.Mag;

        if (dist <= 0.1f || dist == 0f)
            return to;
        Vector3 result = from + dir / dist * speed;
        result.y = from.y;
        //return new Vector3(from.x + dir.x / dist * speed, from.y + dir.y / dist * speed, from.z + dir.z / dist * speed);
        return result;
    }

    private static Vector3 LookAt(Vector3 currentRot, Vector3 fromPos, Vector3 toPos, float maxStep)
    {
        // --- Radian version (active) ---
        float dx = toPos.x - fromPos.x;
        float dz = toPos.z - fromPos.z;

        // Compute target Y rotation (forward = +Z)
        float targetY = (float)Math.Atan2(dx, dz); // radians

        float currentY = currentRot.y;

        // Shortest path
        float delta = targetY - currentY;
        while (delta > Math.PI) delta -= 2f * (float)Math.PI;
        while (delta < -Math.PI) delta += 2f * (float)Math.PI;

        if (Math.Abs(delta) <= maxStep)
        {
            return new Vector3(currentRot.x, targetY, currentRot.z);
        }

        // Clamp rotation step
        float step = Math.Min(Math.Abs(delta), maxStep) * Math.Sign(delta);
        float newY = currentY + step;

        return new Vector3(currentRot.x, newY, currentRot.z);

        /* 
        // --- Degree version (commented out) ---
        float targetYDeg = (float)(Math.Atan2(dx, dz) * 180.0 / Math.PI);
        float currentYDeg = currentRot.y;

        float deltaDeg = targetYDeg - currentYDeg;
        while (deltaDeg > 180f) deltaDeg -= 360f;
        while (deltaDeg < -180f) deltaDeg += 360f;

        float stepDeg = Math.Min(Math.Abs(deltaDeg), maxStep) * Math.Sign(deltaDeg);
        float newYDeg = currentYDeg + stepDeg;

        return new Vector3(currentRot.x, newYDeg, currentRot.z);
        */
    }

    private static Vector3 RotateTowards(Vector3 currentRot, Vector3 targetRot, float maxStep)
    {
        float currentY = currentRot.y;
        float targetY = targetRot.y;

        float delta = targetY - currentY;

        // normalize to shortest path
        while (delta > Math.PI) delta -= 2f * (float)Math.PI;
        while (delta < -Math.PI) delta += 2f * (float)Math.PI;

        float step = Math.Min(Math.Abs(delta), maxStep) * Math.Sign(delta);
        float newY = currentY + step;

        return new Vector3(currentRot.x, newY, currentRot.z);
    }
}
