using Unity.VisualScripting;
using UnityEngine;

public class PlayerGroundedState : EntityState
{
    public PlayerGroundedState(Player player, StateMachine stateMachine, string animationStateName) : base(player, stateMachine, animationStateName)
    {

    }

    public override void Update()
    {
        base.Update();
        if (player.IsFalling() && !player.isGroundDetected)
        {
            stateMachine.ChangeState(player.fallState);
        }
        if (player.WasJumpPressed())
        {
            stateMachine.ChangeState(player.jumpState);
        }
        if (player.WasAttackPerformed())
        {
            stateMachine.ChangeState(player.basicAttackState);
        }
    }
}