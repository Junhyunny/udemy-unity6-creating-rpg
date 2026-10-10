using UnityEngine;

public class PlayerWallJumpState : PlayerAiredState

{
    public PlayerWallJumpState(Player player, StateMachine stateMachine) : base(player, stateMachine, "jumpFall")
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.WallJump();
    }

    public override void Update()
    {
        base.Update();
        if(player.IsFalling() && stateMachine.currentState != player.jumpAttackState)
        {
            stateMachine.ChangeState(player.fallState);
        }
        if(player.isWallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
        }
    }
}