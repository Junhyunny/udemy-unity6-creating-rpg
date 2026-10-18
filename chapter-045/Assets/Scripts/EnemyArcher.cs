
using UnityEngine;

public class EnemyArcher : Enemy
{

    protected override void Awake()
    {
        base.Awake();
        idleState = new EnemyIdleState(this, stateMachine);
    }
}