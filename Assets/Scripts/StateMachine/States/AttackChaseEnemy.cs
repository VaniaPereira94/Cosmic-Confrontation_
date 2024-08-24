using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class AttackChaseEnemy : BaseState
{
    private EnemyScript _enemyScript;

    public AttackChaseEnemy(EnemyScript enemyScript) : base(Utils.EnemyStates.ATTACK_CHASE)
    {
        _enemyScript = enemyScript;
    }

    public override void Enter(string previousEnemyState)
    {
        _enemyScript.SetShootingAnimation(1.0f);
        _enemyScript.Agent.isStopped = false;
        _enemyScript.Animator.SetBool(Animations.WALKING, true);
        _enemyScript.Animator.SetBool(Animations.SHOOTING, true);
        _enemyScript.IsShooting = true;
    }

    public override void Exit(string nextEnemyState)
    {
        if(!EnemyStates.ATTACK_IDLE.Equals(nextEnemyState))
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
