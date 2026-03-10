using System;
using System.Collections.Generic;
using Engine;

public class StateDead : IState
{
    private AIController ai;
    private float deathTimer = 0.3f;
    private bool destroyed = false;
    public StateDead(AIController ai)
    {
        this.ai = ai; 
    }

    public void Enter()
    {
        // Called when entering the Dead state
        //Debug.Log("StateDead.cs :  void Enter()");
        //ai.agent.speed = 0f;
        ai.isDying = true;
        //ai.UpdateAnimationFromBools();
    }

    public void OnUpdate(float dt)
    {
        if (destroyed || ai == null || !ai.IsValid())
            return;
        // Called every frame while in Chase state
        deathTimer -= dt;
        if (deathTimer <= 0)    // change this to the Animation.End() or smth;
        {
            deathTimer = 0;
            destroyed = true;
            ai.isDying = false;
            Scene.DestroyEntity(this.ai.ID);
            return;
            //GameObject.Destroy(this.ai.gameObject);
        }

    }

    public void Exit()
    {
        // Called when exiting the Dead state
    }

}
