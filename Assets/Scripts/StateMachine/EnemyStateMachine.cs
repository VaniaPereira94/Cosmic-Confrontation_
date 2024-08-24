using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class EnemyStateMachine : StateManager
{
    private EnemyScript _enemyScript;
    private Dictionary<string, BaseState> states;

    public EnemyStateMachine(EnemyScript enemyScript)
    {
        _enemyScript = enemyScript;
    }


    public EnemyScript EnemyScript { 
        get { return _enemyScript; } 
    }


    public override void Start()
    {
        SetStates();
        base.Start();
    }

    protected override BaseState GetInitialState()
    {
        return states[EnemyStates.IDLE];
    }

    private void SetStates()
    {
        states = new Dictionary<string, BaseState>();
        states.Add(EnemyStates.IDLE, new IdleEnemy(_enemyScript));
        states.Add(EnemyStates.PATROL, new PatrolEnemy(_enemyScript));
        states.Add(EnemyStates.DEATH, new DeathEnemy(_enemyScript));
        states.Add(EnemyStates.ATTACK_CHASE, new AttackChaseEnemy(_enemyScript));
        states.Add(EnemyStates.ATTACK_IDLE, new AttackIdleEnemy(_enemyScript));
    }

    public void TakeTransition(string newStateName) {
        ChangeState(states[newStateName]);
    }
}