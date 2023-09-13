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

    [Server]
    protected override void AttackEnemy()
    {
        if (isAttacking == false) return;

        if (enemiesInRange[0].isDead)
        {
            enemiesInRange.RemoveAt(0);
            return;   
        }

        if (Time.time < currentCooldown)
        {
            return;
        }

        CurrentTargettingMode();

        if (enemiesInRange[0] == null)
        {
            enemiesInRange.RemoveAt(0);
            return;
        }
        else
        { 
            StartCoroutine(AttackingAnimationLength());

            int rand = Random.Range(0, 100);
            if (rand < unitSO.chanceToMiss)
            {
                Debug.Log($"missed attack");
                goto MissedAttack;
            }
            
            SplashDamage();
            RpcClientUnitActions(enemiesInRange[0].transform.position);
        }
        
        MissedAttack:
        currentCooldown = Time.time + cooldown;
    }

    [Server]
    void SplashDamage()
    {
        enemiesInRange[0].DealDamage(attack);

        float splashDamage = attack / splashDamageDivider;
        Vector3 firstEnemyPos = enemiesInRange[0].transform.position;

        splashWeapon.SplashAttack(splashRange, splashDamage, firstEnemyPos, enemiesInRange[0]);
    }
}
