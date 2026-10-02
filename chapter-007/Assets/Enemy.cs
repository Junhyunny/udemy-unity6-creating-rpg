using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected string enemyName;

    private void Update()
    {
        MoveAround();

        if (Input.GetKeyDown(KeyCode.F))
        {
            Attack();
        }
    }

    private void MoveAround()
    {
        Debug.Log(enemyName + " move at speed " + moveSpeed);
    }

    protected virtual void Attack()
    {
        Debug.Log(enemyName + " attacks!");
    }

    public string GetEnemyName()
    {
        return enemyName;
    }
}
