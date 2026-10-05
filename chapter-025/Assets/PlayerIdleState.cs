using Unity.VisualScripting;
using UnityEngine;

public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(Player player, StateMachine stateMachine) : base(player, stateMachine, "idle")
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.StopMovement();
    }

    public override void Update()
    {
        base.Update();
        if(player.moveInput.x != 0)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}