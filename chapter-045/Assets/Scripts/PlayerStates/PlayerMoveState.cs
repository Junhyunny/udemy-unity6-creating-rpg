using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(Player player, StateMachine stateMachine) : base(player, stateMachine, "move")
    {
    }

    public override void Update()
    {
        base.Update();
        if (player.moveInput.x == 0 || player.isWallDetected)
        {
            stateMachine.ChangeState(player.idleState);
        }
        // TODO: [todos/chapter-045/state-velocity-responsibility.md](../../../../todos/chapter-045/state-velocity-responsibility.md)
        // player.SetVelocity(player.moveInput.x * player.moveSpeed, player.rb.linearVelocity.y);
        player.Move();
    }
}
