
using System;

public class EnemyGroundedState : EnemyState
{
    public EnemyGroundedState(Enemy enemy, StateMachine stateMachine, String animationStateName) : base(enemy, stateMachine, animationStateName)
    {
    }

    public override void Update()
    {
        base.Update();
        if(enemy.PlayerDetected() == true)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}