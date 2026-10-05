using UnityEngine;

public class PlayerJumpState : PlayerAiredState

{
    public PlayerJumpState(Player player, StateMachine stateMachine) : base(player, stateMachine, "jumpFall")
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.Jump();
    }

    public override void Update()
    {
        base.Update();
        if (player.IsFalling())
        {
            stateMachine.ChangeState(player.fallState);
        }
    }
}