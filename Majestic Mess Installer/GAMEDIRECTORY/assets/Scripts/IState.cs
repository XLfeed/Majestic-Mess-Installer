using System;
using System.Collections.Generic;
using Engine;

public interface IState
{
    void Enter();
    void OnUpdate(float dt);
    void Exit();
}