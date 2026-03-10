using System;
using System.Collections.Generic;
using Engine;

public class StateAlert : IState
{
    private AIController ai;
    private EnemyStatesSFX sfx;
    private float alertTime = 1.0f;  // wait for 1 second before transitioning
    public StateAlert(AIController ai)
    {
        this.ai = ai; 
        sfx = ai.GetScript<EnemyStatesSFX>();
        Debug.Log($"StateAlert.cs : Constructor");
    }

    public void Enter()
    {
        if (sfx == null) sfx = ai.GetScript<EnemyStatesSFX>();
        //ai.HandleAlert(); // This calls into the specific AI's behavior
        sfx?.PlayAlertVO();
        ai.isStartled = true;
        ai.UpdateAnimationFromBools();
        Debug.Log($"StateAlert.cs : Enter");
    }

    public void OnUpdate(float dt)
    {
        alertTime -= dt;

        if (alertTime <= 0)
        {
            Debug.Log($"StateAlert.cs : ChangeState");
            ai.HandleAlert();
        }
    }
    public void Exit() 
    { 
        Debug.Log($"StateAlert.cs : Exit");
        ai.isStartled = false;
        ai.UpdateAnimationFromBools();
    }
}
