using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEnemy : BaseState
{
    private EnemyScript _enemyScript;

    public DeathEnemy(EnemyScript enemyScript) : base(Utils.EnemyStates.DEATH)
    {
        _enemyScript = enemyScript;
    }

    public override void Enter(string previousStateName)
    {
        _enemyScript.OnDead();
    }

    public override void Exit(string nextStateName)
    {
        throw new System.NotImplementedException();
    }

    public override void Update()
    {
        _enemyScript.SetShootingAnimation(0.0f);
    }
}
