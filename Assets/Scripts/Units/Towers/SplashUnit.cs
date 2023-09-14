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

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();   
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void DealDamageToEnemy()
    {
        SplashDamage();
    }

    [Server]
    void SplashDamage()
    {
        enemiesInRange[0].DealDamage(attack);

        float splashDamage = attack / splashDamageDivider;
        Vector3 firstEnemyPos = enemiesInRange[0].transform.position;

        splashWeapon.SplashAttack(splashDamage, firstEnemyPos, enemiesInRange[0]);
    }
}
