
using UnityEditor;
using UnityEngine;

public class PlayerBasicAttackState : EntityState
{
    private float attackVelocityTimer;
    private int comboIndex = 0;
    private int comboLimit = 3;
    private float lastTimeAttacked;
    private bool comboattackQueued;

    public PlayerBasicAttackState(Player player, StateMachine stateMachine) : base(player, stateMachine, "basicAttack")
    {
    }

    public override void Enter()
    {
        base.Enter();
        comboattackQueued = false;
        ResetComboIndexIfNeeded();
        animator.SetInteger("basicAttackIndex", comboIndex);
        ApplyAttackVelocity();
    }

    public override void Update()
    {
        base.Update();
        HandleAttackVelocity();
        if (player.WasAttackPerformed())
        {
            comboattackQueued = true;
        }
        if (triggerCalled)
        {
            if (comboattackQueued)
                stateMachine.ChangeState(player.basicAttackState);
            else
                stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        comboIndex = (comboIndex + 1) % comboLimit;
        lastTimeAttacked = Time.time;
    }

    private void ResetComboIndexIfNeeded()
    {
        if (Time.time > lastTimeAttacked + player.comboResetTime)
        {
            comboIndex = 0;
        }
    }

    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;
        if (attackVelocityTimer < 0)
        {
            player.StopForAttack();
        }
    }

    private void ApplyAttackVelocity()
    {
        attackVelocityTimer = player.attackVelocityDuration;
        player.GenerateAttackVelocity(comboIndex);
    }
}