using UnityEngine;

public class PlayerWallSlideState : EntityState
{
    public PlayerWallSlideState(Player player, StateMachine stateMachine) : base(player, stateMachine, "wallSlide")
    {
    }

    public override void Update()
    {
        base.Update();
        HandleWallSlide();
        if (player.WasJumpPressed())
        {
            stateMachine.ChangeState(player.wallJumpState);
            player.Flip();
        }
        if (!player.isWallDetected)
        {
            stateMachine.ChangeState(player.fallState);
        }
        if (player.isGroundDetected)
        {
            stateMachine.ChangeState(player.idleState);
            player.Flip();
        }
    }

    private void HandleWallSlide()
    {
        if (player.moveInput.y < 0)
        {
            player.WallSlide(1f);
        }
        else
        {
            player.WallSlide(player.wallSlideSlowMultiplier);
        }
    }
}
