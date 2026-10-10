using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(Player player, StateMachine stateMachine) : base(player, stateMachine, "move")
    {
    }

    public override void Update()
    {
        base.Update();
        if (player.moveInput.x == 0 || player.isWallDetected)
        {
            stateMachine.ChangeState(player.idleState);
        }
        // TODO, 강의는 여기서 속도를 설정하는데, SetVelocity 메서드에 필요한 인자는 모두 player 내부 속성을 사용하는데 굳이 여기서 다 꺼내서 설정하는게 맞나? 게임 업계 쪽 코드는 이게 일반적인가?
        // player.SetVelocity(player.moveInput.x * player.moveSpeed, player.rb.linearVelocity.y);
        player.Move();
    }
}
