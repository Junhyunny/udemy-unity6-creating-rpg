using UnityEngine;

public class PlayerAiredState : EntityState
{
    public PlayerAiredState(Player player, StateMachine stateMachine, string animationStateName) : base(player, stateMachine, animationStateName)
    {

    }

    public override void Update()
    {
        base.Update();
        if (player.moveInput.x != 0)
        {
            player.Aired();
        }
    }
}