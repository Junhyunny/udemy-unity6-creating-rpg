
using UnityEngine;

public class EnemySkeleton : Enemy
{

    protected override void Awake()
    {
        base.Awake();
        idleState = new EnemyIdleState(this, stateMachine);
        moveState = new EnemyMoveState(this, stateMachine);
        attackState = new EnemyAttackState(this, stateMachine);
        battleState = new EnemyBattleState(this, stateMachine);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }
}