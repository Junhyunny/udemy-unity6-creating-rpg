using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{

    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void AttackStarted()
    // {
    //     // call method from "Player" script
    //     // that method should stop movement of the player game object
    //     Debug.Log("AttackStarted");
    //     player.EnableMovementAndJump(false);
    // }

    private void DisableMovementAndJump()
    {
        player.EnableMovementAndJump(false);
    }

    private void EnableMovementAndJump() => player.EnableMovementAndJump(true);
}
