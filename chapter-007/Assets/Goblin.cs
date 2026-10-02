using UnityEngine;

public class Goblin : Enemy
{
    [ContextMenu("Steal Gold")]
    private void StealMoney()
    {
        Debug.Log("steal money");
    }

    // [ContextMenu("SpecialAttack")]
    // private void SpecialAttack()
    // {
    //     StealMoney();
    //     Attack();
    // }

    protected override void Attack()
    {
        base.Attack();
        StealMoney();
    }
}
