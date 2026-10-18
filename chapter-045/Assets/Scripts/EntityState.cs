
using UnityEngine;

public abstract class EntityState
{

    protected Animator animator;
    protected StateMachine stateMachine;
    protected float startTime = 0;
    protected string animationStateName;
    protected bool triggerCalled;

    public EntityState(Animator animator, StateMachine stateMachine, string animationStateName)
    {
        this.animator = animator;
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
    }

    public virtual void Exit()
    {
        animator.SetBool(animationStateName, false);
    }

    public void CallAnimationTrigger()
    {
        triggerCalled = true;
    }
}