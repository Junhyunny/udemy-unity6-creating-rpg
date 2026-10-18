using System;
using UnityEngine;

// TODO: [todos/chapter-025/state-machine-circular-references.md](../../todos/chapter-025/state-machine-circular-references.md)
public abstract class PlayerState : EntityState
{

    protected Player player;

    public PlayerState(Player player, StateMachine stateMachine, string animationStateName) : base(player.animator, stateMachine, animationStateName)
    {
        this.player = player;
    }

    public override void Update()
    {
        base.Update();
        animator.SetFloat("yVelocity", player.GetYVelocity());
        if (player.IsDashPressed() && CanDash())
        {
            stateMachine.ChangeState(player.dashState);
        }
    }


    private bool CanDash()
    {
        if (player.isWallDetected)
        {
            return false;
        }
        if (stateMachine.currentState == player.dashState)
        {
            return false;
        }
        return true;
    }
}
