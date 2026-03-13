using System;
using System.Collections.Generic;
using Engine;

public class StateChase : IState
{
    private AIController ai;
    private EnemyStatesSFX sfx;
    private float nodeThreshold = 0.05f;      // Distance to nav node before moving to next
    private float distThreshold = 3.0f;       // Distance to target to consider "reached"
    private float recalcThreshold = 0.5f;     // Recalc path if player moved more than this

    private float cannotReachTimer = 5f;      // if cannot the reach the next node/point within this period, switch state back to patrol

    public StateChase(AIController ai)
    {
        this.ai = ai;
        sfx = ai.GetScript<EnemyStatesSFX>();
        //Debug.Log($"StateChase.cs : Constructor");
    }

    public void Enter()
    {
        //Debug.Log("StateChase.cs : Enter()");
        if (sfx == null) sfx = ai.GetScript<EnemyStatesSFX>();
        ai.isChasing = true;
        ai.isPatrolling = false;
        ai.isAttacking = false;
        ai.isStartled = false;
        ai.UpdateAnimationFromBools();
        // Force one VO when chase starts; subsequent calls remain cooldown-gated.
        sfx?.PlayChaseVO(true);
        // Clear previous path
        ai.ResetPath();

        if (ai.playerObj != null)
        {
            ai.targetPosition = ai.playerObj.Transform.Position;
            ai.lastTargetPosition = ai.targetPosition;  // Track last seen position
            ai.CalculateNavPath(ai.Transform.Position, ai.targetPosition);
        }
    }

    public void OnUpdate(float dt)
    {
        // Player not found -> return to patrol
        if (ai.playerObj == null)
        {
            if (sfx == null) sfx = ai.GetScript<EnemyStatesSFX>();
            sfx?.PlayChaseEndVO();
            ai.ChangeState(new StatePatrol(ai));
            return;
        }

        // Update target if player detected
        if (ai.isPlayerDetected)
        {
            ai.targetPosition = ai.playerObj.Transform.Position;

            // Only recalc if player moved AND AI is not close enough
            if ((ai.targetPosition - ai.lastTargetPosition).Mag > recalcThreshold &&
                Vector3.Distance(ai.Transform.Position, ai.targetPosition) > distThreshold)
            {
                ai.ResetPath();
                ai.CalculateNavPath(ai.Transform.Position, ai.targetPosition);
                ai.lastTargetPosition = ai.targetPosition;
            }
        }

        // Ensure currentPathIndex doesn't go out of range
        // if (ai.navPath == null || ai.navPath.Count == 0)
        //     return;

        // if (ai.currentPathIndex >= ai.navPath.Count)
        //     ai.currentPathIndex = ai.navPath.Count - 1;
        if (ai.navPath == null || ai.navPath.Count == 0)
        {
            // Fallback: move directly toward targetPosition
            ai.Transform.Position = StateChase.MoveTowards(ai.Transform.Position, ai.targetPosition, ai.chaseSpeed * dt);
            ai.Transform.Rotation = LookAt(ai.Transform.Rotation, ai.Transform.Position, ai.targetPosition, ai.rotationSpeed * dt);

            // If close enough to target, switch state
            if (Vector3.Distance(new Vector3(ai.Transform.Position.x, 0, ai.Transform.Position.z),
                                new Vector3(ai.targetPosition.x, 0, ai.targetPosition.z)) < distThreshold)
            {
                ai.ResetPath();
                if (ai.isPlayerDetected)
                {
                    ai.ChangeState(new StateAttack(ai));
                }
                else
                {
                    ai.isChasing = false;
                    ai.isAttacking = false;
                    ai.isPatrolling = false;
                    ai.isIdle = false;
                    ai.isSheathe = true;
                    ai.isEmptyGauge = true;
                    ai.sheatheTimer = 0.6f;
                    ai.UpdateAnimationFromBools();
                    ai.ChangeState(new StatePatrol(ai));
                }
            }

            return; // Exit early - no path to follow
        }
        
        if (cannotReachTimer <= 0f)
        {
                ai.isChasing = false;
                ai.isAttacking = false;
                ai.isPatrolling = false;
                ai.isIdle = false;
                ai.isSheathe = true;
                ai.isEmptyGauge = true;
                ai.sheatheTimer = 0.6f;
                ai.UpdateAnimationFromBools();
                ai.ChangeState(new StatePatrol(ai));
        }

        ai.currentPathIndex = Math.Clamp(ai.currentPathIndex, 0, ai.navPath.Count - 1);
        // Move towards current navmesh node
        Vector3 targetNode = ai.navPath[ai.currentPathIndex];
        Vector3 currPos = ai.Transform.Position;
        ai.Transform.Position = MoveTowards(currPos, targetNode, ai.chaseSpeed * dt);
        ai.Transform.Rotation = LookAt(ai.Transform.Rotation, ai.Transform.Position, targetNode, ai.rotationSpeed * dt);
        cannotReachTimer -= dt;
        // Increment path index if node reached
        if (Vector3.Distance(new Vector3(ai.Transform.Position.x, 0, ai.Transform.Position.z),
                             new Vector3(targetNode.x, 0, targetNode.z)) < nodeThreshold)
        {
            if (ai.currentPathIndex < ai.navPath.Count - 1)
                ai.currentPathIndex++;
            cannotReachTimer = 5f;
        }

        // Determine final target: player last seen or player still detected
        Vector3 currentTarget = ai.isPlayerDetected ? ai.lastTargetPosition : ai.navPath[ai.navPath.Count - 1];

        // Switch back to patrol if reached target
        if (Vector3.Distance(new Vector3(ai.Transform.Position.x, 0, ai.Transform.Position.z),
                             new Vector3(currentTarget.x, 0, currentTarget.z)) < distThreshold)
        {
            //ai.isPlayerDetected = false;  // reset detection
            cannotReachTimer = 5f;
            ai.ResetPath();
            if (ai.isPlayerDetected)
            {
                ai.ChangeState(new StateAttack(ai));
            }
            else
            {
                ai.isChasing = false;
                ai.isAttacking = false;
                ai.isPatrolling = false;
                ai.isIdle = false;
                ai.isSheathe = true;
                ai.isEmptyGauge = true;
                ai.sheatheTimer = 0.6f;
                ai.UpdateAnimationFromBools();
                ai.ChangeState(new StatePatrol(ai));
            }
        }
    }

    public void Exit()
    {
        //Debug.Log("StateChase.cs : Exit()");
        ai.isChasing = false;
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


    // private static Vector3 LookAt(Vector3 currentRot, Vector3 fromPos, Vector3 toPos, float step)
    // {
    //     float dx = toPos.x - fromPos.x;
    //     float dz = toPos.z - fromPos.z;

    //     // Correct Atan2 order and adjust forward
    //     float targetY = (float)(Math.Atan2(dz, dx) * 180.0 / Math.PI) - 90f;

    //     float currentY = currentRot.y;

    //     float delta = targetY - currentY;
    //     while (delta > 180f) delta -= 360f;
    //     while (delta < -180f) delta += 360f;

    //     float newY = Math.Abs(delta) <= step ? targetY : currentY + Math.Sign(delta) * step;

    //     return new Vector3(currentRot.x, newY, currentRot.z);
    // }

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
}
