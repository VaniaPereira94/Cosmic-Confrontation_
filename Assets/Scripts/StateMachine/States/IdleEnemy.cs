using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class IdleEnemy : BaseState
{
    private EnemyScript _enemyScript;
    // Start is called before the first frame update
    public IdleEnemy(EnemyScript enemyScript) : base(Utils.EnemyStates.IDLE) { 
        _enemyScript = enemyScript;
    }

    public override void Enter(string previousStateName)
    {
        _enemyScript.Animator.SetBool(Animations.WALKING, false);
        _enemyScript.Agent.isStopped = true;
    }

    public override void Exit(string nextStateName) { 
        
    }

    public override void Update()
    {
        _enemyScript.SetShootingAnimation(0.0f);
    }
}
