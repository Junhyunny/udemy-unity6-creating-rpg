

public class EnemyMoveState : EnemyGroundedState
{
    public EnemyMoveState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine, "move")
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (enemy.isGroundDetected == false || enemy.isWallDetected)
        {
            enemy.Flip();
        }
    }

    public override void Update()
    {
        base.Update();
        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDirection, enemy.rb.linearVelocityY);
        if (enemy.isGroundDetected == false || enemy.isWallDetected)
        {
            stateMachine.ChangeState(enemy.idleState);

        }
    }
}