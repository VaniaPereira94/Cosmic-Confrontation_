using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    public string _name;
    protected StateManager stateManager;

    public string Name { 
        get { return _name; } 
    }

    public BaseState(string name)
    {
        _name = name;
    }


    public abstract void Enter(string previousStateName);
    public abstract void Update();
    public abstract void Exit(string nextStateName);


}
