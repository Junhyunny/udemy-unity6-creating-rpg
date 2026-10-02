using UnityEngine;

public class EnemyArcher : Enemy
{
    protected override void Attack()
    {
        Debug.Log(enemyName + " shoots an arrow");
    }
}
