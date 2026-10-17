using System;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class PlayerDashState : EntityState

{
    public PlayerDashState(Player player, StateMachine stateMachine) : base(player, stateMachine, "dash")
    {
    }

    public override void Enter()
    {
        base.Enter();
        startTime = player.dashDuration;
    }

    public override void Update()
    {
        base.Update();
        CancelDashIfNeeded();
        player.Dash();
        if (startTime < 0)
        {
            if (player.isGroundDetected)
            {

                stateMachine.ChangeState(player.idleState);
            }
            else
            {

                stateMachine.ChangeState(player.fallState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.Stop();
    }

    private void CancelDashIfNeeded()
    {

        if (player.isWallDetected)
        {
            if (player.isGroundDetected)
            {
                stateMachine.ChangeState(player.idleState);
            }
            else
            {
                stateMachine.ChangeState(player.wallSlideState);
            }
        }
    }
}