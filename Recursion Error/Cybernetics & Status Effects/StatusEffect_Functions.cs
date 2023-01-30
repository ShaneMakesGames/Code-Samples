using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class StatusEffect
{
    public void TakeBurnDamage()
    {
        Debug.Log("Take Burn Damage : " + stacksApplied);
        myEnemy.TakeDamage(stacksApplied);
    }

    public void TakeScorchDamage()
    {
        Debug.Log("Take Scorch Damage : 20");

        myEnemy.TakeDamage(20);
    }
}