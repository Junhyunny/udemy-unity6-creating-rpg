using System;
using UnityEngine;

// TODO: [todos/chapter-025/state-machine-circular-references.md](../../todos/chapter-025/state-machine-circular-references.md)
public abstract class EntityState
{

    protected float startTime = 0;
    protected Player player;
    protected Animator animator;
    protected StateMachine stateMachine;
    protected string animationStateName;
    protected bool triggerCalled;

    public EntityState(Player player, StateMachine stateMachine, string animationStateName)
    {
        this.player = player;
        this.animator = player.animator;
        this.stateMachine = stateMachine;
        this.animationStateName = animationStateName;
    }

    public virtual void Enter()
    {
        animator.SetBool(animationStateName, true);
        triggerCalled = false;
    }

    public virtual void Update()
    {
        startTime -= Time.deltaTime;
        animator.SetFloat("yVelocity", player.GetYVelocity());
        if (player.IsDashPressed() && CanDash())
        {
            stateMachine.ChangeState(player.dashState);
        }
    }

    public virtual void Exit()
    {
        animator.SetBool(animationStateName, false);
    }

    public void CallAnimationTrigger()
    {
        triggerCalled = true;
    }

    private bool CanDash()
    {
        if(player.isWallDetected)
        {
            return false;
        }
        if(stateMachine.currentState == player.dashState)
        {
            return false;
        }
        return true;
    }
}
