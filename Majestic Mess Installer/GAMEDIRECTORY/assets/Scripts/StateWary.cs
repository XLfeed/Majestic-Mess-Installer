using System;
using System.Collections.Generic;
using Engine;

public class StateWary : IState
{
    private AIController ai;
    private float distRate;
    Vector3 targetPos;
    private bool prevSheathe;
    private bool prevEmptyGauge;

    public StateWary(AIController ai)
    {
        this.ai = ai;
    }

    public void Enter() 
    {
        /* Start Wary */
        //ai.waryGauge = 0f;
        distRate = 1.0f;

        // play anim startled
        ai.isEmptyGauge = false;
        //ai.anim.SetBool("isEmptyGauge", ai.isEmptyGauge);
        ai.isStartled = true;
        ai.UpdateAnimationFromBools();
        prevSheathe = ai.isSheathe;
        prevEmptyGauge = ai.isEmptyGauge;

        targetPos = ai.targetPosition;
    }

    public void OnUpdate(float dt)
    {    
        //Play animms here
        if (ai.waryGauge < ai.unshealthGuage && !ai.isSheathe)
        {
            ai.isSheathe = true;
            //ai.anim.SetBool("isSheathe", ai.isSheathe);
            if (ai.isSheathe != prevSheathe)
            {
                ai.sheatheTimer = 0.6f;
                ai.UpdateAnimationFromBools();
            }
        }
        else if (ai.waryGauge >= ai.unshealthGuage && ai.isSheathe)
        {
            ai.isSheathe = false; // unsheathe
            //ai.anim.SetBool("isSheathe", ai.isSheathe);
            if (ai.isSheathe != prevSheathe)
                ai.UpdateAnimationFromBools();
        }

        ai.Transform.Rotation = LookAt(ai.Transform.Rotation, ai.Transform.Position, targetPos, ai.rotationSpeed * 0.5f * dt);
        prevSheathe = ai.isSheathe;

        if (!ai.isPlayerDetected)
        {
            ai.waryGauge -= ai.gaugeDecreaseRate * dt;
            if (ai.waryGauge <= 0.0f)
            {
                ai.waryGauge = 0.0f;
          
                //Play - must be the wind anim here ONCE 
                if(!ai.isEmptyGauge)
                {
                    ai.isEmptyGauge = true;
                    //ai.anim.SetBool("isEmptyGauge", ai.isEmptyGauge);
                    if (ai.isEmptyGauge != prevEmptyGauge)
                        ai.UpdateAnimationFromBools();
                    prevEmptyGauge = ai.isEmptyGauge;
                }
                       
               ai.ChangeState(new StatePatrol(ai));
               //ai.ChangeState(new StatePatrol(ai, ai.patrolWaypoints));
                               
               return;
            }
        }
        else
        {
            /* Wary logic */
            ai.isEmptyGauge = false;
            //ai.anim.SetBool("isEmptyGauge", ai.isEmptyGauge);
            prevEmptyGauge = ai.isEmptyGauge;

            // should we check dist to increase the rate?
            Vector3 currentPos = ai.Transform.Position;
            targetPos = ai.playerObj.Transform.Position;
            // Calculate direction towards the player
            Vector3 direction = targetPos - currentPos;
            direction.y = 0; // We only care about movement on the XZ plane (horizontal movement)
            float distanceToPlayer = direction.Mag;

            if (distanceToPlayer <= 10.0f) // immediate front
            {
                distRate = 5.0f;
            }
            else if (distanceToPlayer > 10.0f && distanceToPlayer <= 30.0f) 
            {
                distRate = 2.0f;
            }
            else if (distanceToPlayer > 30.0f)
            {
                distRate = 1.0f;
            }

            // Increase wary gauge over time
            ai.waryGauge += ai.gaugeIncreaseRate * distRate * dt;

            // If gauge reaches threshold, go to Alert
            if (ai.waryGauge >= ai.maxWaryGuage)
            {
                //ai.waryGauge = 0f;
                ai.waryGauge = ai.maxWaryGuage;
                ai.ChangeState(new StateAlert(ai));
                return;
            }
        }

        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    ai.waryGauge = 0f;
        //    ai.ChangeState(new StatePatrol(ai, ai.patrolWaypoints));
        //}
    }

    public void Exit() 
    {
        /* Cleanup */
        //ai.waryGauge = 0f;
       // Debug.Log("StateWary.cs Exit()");

        ai.isStartled = false;
        //ai.anim.SetBool("isStartled", ai.isStartled);
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
