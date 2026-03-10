using System;
using System.Collections.Generic;
using Engine;

public class StateMachine
{
    private IState currentState;

    public void ChangeState(IState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        //Debug.Log("StateMachine.cs: State is now " + newState.GetType().Name);

        if (currentState != null)
        {
            //Debug.Log("StateMachine.cs: Entering State: " + currentState.GetType().Name);
            currentState.Enter();
        }
    }

    public void OnUpdate(float dt) 
    {
        if (currentState != null)
            currentState.OnUpdate(dt);
    }

    public IState GetCurrentState()
    {
        return currentState;
    }
}