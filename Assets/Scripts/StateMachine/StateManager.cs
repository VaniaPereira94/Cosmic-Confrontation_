using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateManager
{
    private BaseState currentState;
    
    // Start is called before the first frame update
    public virtual void Start()
    {
        currentState = GetInitialState();
        currentState.Enter(null);
    }

    // Update is called once per frame
    public void Update()
    {
        currentState.Update();
    }

    protected void ChangeState(BaseState newState)
    {
        string oldStateName = currentState.Name;

        if (oldStateName == newState.Name)
        {
            return;
        }

        currentState.Exit(newState.Name);
        currentState = newState;
        currentState.Enter(oldStateName);
    }

    protected abstract BaseState GetInitialState();

    public string GetCurrentStateName() { 
        return currentState.Name;
    }
}
