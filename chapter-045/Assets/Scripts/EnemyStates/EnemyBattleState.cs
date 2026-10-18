

using Unity.AppUI.Core;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityPipeline.Microsoft.CodeAnalysis.Emit;

public class EnemyBattleState : EnemyState
{

    private Transform player;
    private float lastTimeWasInBattle;

    public EnemyBattleState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine, "battle")
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (player == null)
        {
            player = enemy.PlayerDetected().transform;
        }
        if (ShouldRetreat())
        {
            enemy.rb.linearVelocity = new Vector2(enemy.retreatVelocity.x * -DirectionToPlayer(), enemy.retreatVelocity.y);
            enemy.HandleFlip(DirectionToPlayer());
        }
    }

    private bool WithinAttackRange()
    {
        return DistanceToPlayer() < enemy.attackDistance;
    }

    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }
        return Mathf.Abs(player.position.x - enemy.transform.position.x);
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;

    private bool BattleTimeIsOver() => Time.time > lastTimeWasInBattle + enemy.battleTimeDuration;

    private bool ShouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;

    public override void Update()
    {
        base.Update();
        if (enemy.PlayerDetected())
        {
            UpdateBattleTimer();
        }
        if (BattleTimeIsOver())
        {
            stateMachine.ChangeState(enemy.idleState);
        }
        if (WithinAttackRange() && enemy.PlayerDetected())
        {
            stateMachine.ChangeState(enemy.attackState);
        }
        else
        {
            float direction = DirectionToPlayer();
            if (direction != enemy.facingDirection)
            {
                enemy.Flip();
            }
            enemy.SetVelocity(enemy.battleMoveSpeed * direction, enemy.rb.linearVelocityY);
        }
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            return 0;
        }
        return player.position.x > enemy.transform.position.x ? 1 : -1;
    }
}