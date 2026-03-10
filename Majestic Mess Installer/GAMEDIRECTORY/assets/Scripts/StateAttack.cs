using System;
using System.Collections.Generic;
using Engine;

public class StateAttack : IState
{
    private AIController ai;
    private EnemyMeleeAttack melee;
    private EnemyStatesSFX sfx;
    public StateAttack(AIController ai)
    {
        this.ai = ai;
        melee = ai.GetScript<EnemyMeleeAttack>();
        sfx = ai.GetScript<EnemyStatesSFX>();
        //Debug.Log($"StataAttack.cs : Constructor");
        ai.ResetPath();
        //this.melee = ai.GetComponent<EnemyMeleeAttack>();
        // if (melee && !melee.player) melee.player = ai.playerObj?.transform;
    }

    public void Enter()
    {
        //if (ai.agent) ai.agent.isStopped = true;
        //if (melee && !melee.player) melee.player = ai.playerObj?.transform;
        if (melee == null)
            melee = ai.GetScript<EnemyMeleeAttack>();
        if (sfx == null)
            sfx = ai.GetScript<EnemyStatesSFX>();
        if (melee != null && ai.playerObj != null)
            melee.SetTarget(ai.playerObj);

        sfx?.PlayUnsheath();
        ai.isAttacking = true;
        ai.isChasing = false;
        ai.isStartled = false;
        ai.UpdateAnimationFromBools();
    }

    public void OnUpdate(float dt)
    {
        if (ai.playerObj == null || melee == null)
        {
            Debug.Log("[StateAttack] melee is null, returning to patrol");
            ai.ChangeState(new StatePatrol(ai));
            return;
        }
        // Rotate to face player
        ai.Transform.Rotation = LookAt(ai.Transform.Rotation, ai.Transform.Position, ai.playerObj.Transform.Position, ai.rotationSpeed * dt);

        if (!melee.InRange())
        {
            ai.ChangeState(new StateChase(ai));
            return;
        }
        //Debug.Log($"[StateAttack] Update: Attacking");
        if (melee.TryAttack())
        {
            ai.isAttacking = true;
            ai.UpdateAnimationFromBools();
            sfx?.PlayAttackSfx();
        }

        //     if (ai.playerObj == null || melee == null)
        //     {
        //         ai.ChangeState(new StatePatrol(ai, ai.patrolWaypoints));
        //         return;
        //     }

        //     if (!melee.InRange())
        //     {
        //         ai.isAttacking = false;
        //         ai.anim.SetBool("isAttacking", false);
        //         ai.isChasing = true;
        //         ai.anim.SetBool("isChasing", true);

        //         if (ai.agent) ai.agent.isStopped = false;
        //         ai.anim.speed = 1f;                 
        //         ai.ChangeState(new StateChase(ai));
        //         return;
        //     }

        //    ai.isAttacking = true;
        //    ai.anim.SetBool("isAttacking", true);
        //    ai.isChasing = false;
        //    ai.anim.SetBool("isChasing", false);

        //     // only plays when a new attack actually begins
        //     if (melee.TryAttack())
        //     {
        //         ai.anim.Play("Unsheathe_Attack", 0, 0f);
        //         //  melee.TryAttack();
        //     }
    }

    public void Exit()
    {
        // if (ai.agent) ai.agent.isStopped = false;
        // ai.anim.speed = 1f; // reset
        sfx?.PlaySheath();
        ai.isAttacking = false;
        ai.UpdateAnimationFromBools();
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