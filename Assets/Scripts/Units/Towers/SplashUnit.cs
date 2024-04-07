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
    [SerializeField] ParticleSystem shockwaveParticle;
    [SerializeField] Transform shockwavePosition;

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
            

        splashWeapon.SplashAttack(splashDamage, splashRange,firstEnemyPos, enemiesInRange[0]);
    }

    [Client]
    public void ClientShockwave()
    {
        shockwaveParticle.transform.position = shockwavePosition.position; 
        shockwaveParticle.transform.localScale = Vector3.one * (0.34f * splashRange);
        shockwaveParticle.Play();   
    } 
}
