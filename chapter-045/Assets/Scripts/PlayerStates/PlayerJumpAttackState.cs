

using UnityEngine;

public class PlayerJumpAttackState : EntityState
{

    private bool touchedGround;

    public PlayerJumpAttackState(Player player, StateMachine stateMachine) : base(player, stateMachine, "jumpAttack")
    {
    }

    public override void Enter()
    {
        base.Enter();
        touchedGround = false;
        player.SetJumpAttackVelocity();
        Debug.Log("I enter jump attack state. Frame is " + Time.frameCount);
    }

    public override void Update()
    {
        base.Update();
        if(player.isGroundDetected && touchedGround == false)
        {
            touchedGround = true;
            animator.SetTrigger("jumpAttackTrigger");
            player.JumpAttack();
        }
        if (triggerCalled && player.isGroundDetected)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}