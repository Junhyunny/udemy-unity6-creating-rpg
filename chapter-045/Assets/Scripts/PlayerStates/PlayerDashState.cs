using UnityEngine;

public class PlayerDashState : PlayerState

{
    public PlayerDashState(Player player, StateMachine stateMachine) : base(player, stateMachine, "dash")
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = player.dashDuration;
    }

    public override void Update()
    {
        base.Update();
        CancelDashIfNeeded();
        player.Dash();
        if (stateTimer < 0)
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