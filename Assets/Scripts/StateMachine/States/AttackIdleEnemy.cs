using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class AttackIdleEnemy : BaseState
{
    private EnemyScript _enemyScript;
    public AttackIdleEnemy(EnemyScript enemyScript) : base(Utils.EnemyStates.ATTACK_IDLE)
    {
        _enemyScript = enemyScript;
    }

    public override void Enter(string previousStateName)
    {
        _enemyScript.SetShootingAnimation(1.0f);

        _enemyScript.Animator.SetBool(Animations.WALKING, false);
        _enemyScript.Animator.SetBool(Animations.SHOOTING, true);

        _enemyScript.IsShooting = true;
        _enemyScript.Agent.isStopped = true;
        
    }

    public override void Exit(string nextStateName)
    {

        if (!EnemyStates.ATTACK_CHASE.Equals(nextStateName))
        {
            _enemyScript.SetShootingAnimation(0.0f);
            _enemyScript.StopShooting();
        }
    }

    public override void Update()
    {
        _enemyScript.SetShootingAnimation(1.0f);
        _enemyScript.FaceTarget();
    }
}
