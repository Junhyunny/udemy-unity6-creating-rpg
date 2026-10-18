
using UnityEngine;

public class EnemyState : EntityState
{

    protected Enemy enemy;

    public EnemyState(Enemy enemy, StateMachine stateMachine, string animationStateName) : base(enemy.animator, stateMachine, animationStateName)
    {
        this.enemy = enemy;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();
        
        float battleAnimSpeedMultiplier = enemy.battleMoveSpeed / enemy.moveSpeed;
        animator.SetFloat("battleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        animator.SetFloat("moveAnimSpeedMultiplier", enemy.moveAnimSpeedMultiplier);
        animator.SetFloat("xVelocity", enemy.rb.linearVelocity.x);
    }
}