
using UnityEngine;

public class PlayerFallState : PlayerAiredState

{
    public PlayerFallState(Player player, StateMachine stateMachine) : base(player, stateMachine, "jumpFall")
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("I enter fall state. Frame is " + Time.frameCount);
    }

    public override void Update()
    {
        base.Update();
        if (player.isGroundDetected)
        {
            stateMachine.ChangeState(player.idleState);
        }
        if (player.isWallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
        }
    }
}