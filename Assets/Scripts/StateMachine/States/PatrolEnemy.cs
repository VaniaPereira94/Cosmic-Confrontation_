using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class PatrolEnemy : BaseState
{
    private EnemyScript _enemyScript;

    public PatrolEnemy(EnemyScript enemyScript) : base(Utils.EnemyStates.PATROL)
    {
        _enemyScript = enemyScript;
    }

    public override void Enter(string previousStateName)
    {
        _enemyScript.SetRandomWalking();
        _enemyScript.Animator.SetBool(Animations.WALKING, true);
        _enemyScript.Agent.isStopped = false;
    }

    public override void Exit(string nextStateName)
    {
        if (Utils.EnemyStates.IDLE.Equals(nextStateName)) {
            //_enemyScript.Agent.isStopped
        }
    }

    public override void Update()
    {
        _enemyScript.SetShootingAnimation(0.0f);
    }
}
