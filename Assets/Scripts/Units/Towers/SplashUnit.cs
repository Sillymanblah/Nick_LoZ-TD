using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SplashUnit : Unit
{
    [Space]
    [SerializeField] public float splashRange;
    [SerializeField] float splashDamageDivider;
    [SerializeField] SplashWeapon splashWeapon;

    protected override void Start()
    {
        base.Start();   
    }

    protected override void Update()
    {
        base.Update();
    }

    [Server]
    protected override void DealDamageToEnemy()
    {
        enemiesInRange[0].DealDamage(attack);

        float splashDamage = attack / splashDamageDivider;
        Vector3 firstEnemyPos = enemiesInRange[0].transform.position;

        splashWeapon.SplashAttack(splashDamage, firstEnemyPos, enemiesInRange[0]);
    }
}
