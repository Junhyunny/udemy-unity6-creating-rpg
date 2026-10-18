using UnityEngine;

public abstract class EntityState
{

    protected Animator animator;
    protected StateMachine stateMachine;
    protected float stateTimer = 0;
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
        UpdateAnimationParameters();
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void Exit()
    {
        animator.SetBool(animationStateName, false);
    }

    public void AnimationTrigger()
    {
        triggerCalled = true;
    }

    public virtual void UpdateAnimationParameters()
    {
        
    }
}
