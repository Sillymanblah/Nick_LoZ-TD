using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Unity.VisualScripting;

public class BombchuSummon : Summonable
{
    [SerializeField] ParticleSystem thisParticleSystem;

    [Header("Bombchu Variables")]
    [SerializeField] LayerMask collidersLayer;
    [SerializeField] float splashDamageDivider;
    [SerializeField] Transform shockwaveObject;
    float splashRange;
    [SerializeField] int bombTimer;

    protected override void Start()
    {
        base.Start();

        if (isClientOnly) return;

        StartCoroutine(BombTimer());
    }

    protected override void Update() => base.Update();


    [Server]
    protected override void ServerInteraction(Collider other)
    {
        base.ServerInteraction(other);
        var firstEnemy = other.GetComponent<EnemyUnit>();
        firstEnemy.DealDamage(thisUnit.GetAttack());
        controller.enabled = false;

        float splashDamage = thisUnit.GetAttack() / splashDamageDivider;

        SplashAttack(splashDamage, firstEnemy);

        StartCoroutine(ExplosionAndDelay());
    }

    [Server]
    // Server locally destroy the gameobject
    protected override void ServerEnemyInteract()
    {
        base.ServerEnemyInteract();
        isDead = true;
    }

    [ClientRpc]
    // We want to keep the gameobject to use its explosion THEN locally destroy it
    protected override void ClientRpcEnemyInteract()
    {
        base.ClientRpcEnemyInteract();
        shockwaveObject.localScale = Vector3.one * (0.34f * thisUnit.GetRange());
        thisParticleSystem.Play();
        // play sound effect
        transform.GetChild(0).gameObject.SetActive(false);
    }

    [Server]
    IEnumerator ExplosionAndDelay()
    {
        ServerEnemyInteract();
        ClientRpcEnemyInteract();
        yield return new WaitForSeconds(2.0f);
        NetworkServer.Destroy(this.gameObject);
    }

    [Server]
    protected override void ServerEndPathDestroy()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    void SplashAttack(float splashDamage, EnemyUnit firstEnemy)
    {
        List<EnemyUnit> enemyUnitsInRange = new List<EnemyUnit>();

        foreach (Collider collider in Physics.OverlapSphere(this.transform.position, thisUnit.GetRange(), collidersLayer))
        {
            enemyUnitsInRange.Add(collider.GetComponent<EnemyUnit>());
        }

        if (enemyUnitsInRange.Count == 0) return;

        for (int i = 0; i < enemyUnitsInRange.Count; i++)
        {
            if (enemyUnitsInRange[i] == null) 
            {
                enemyUnitsInRange.RemoveAt(i);
                i--;
                continue;
            }

            if (firstEnemy != null)
            {
                if (enemyUnitsInRange[i] == firstEnemy) continue;
            }
            

            enemyUnitsInRange[i].DealDamage(splashDamage);

            if (enemyUnitsInRange[i] == null) 
            {
                enemyUnitsInRange.RemoveAt(i);
            }
            
        }
    }

    [Server]
    IEnumerator BombTimer()
    {
        float timer = 0;

        while (!isDead)
        {
            timer += Time.deltaTime;

            if (timer >= bombTimer)
            {
                StartCoroutine(ExplosionAndDelay());
                SplashAttack(thisUnit.GetAttack() / splashDamageDivider, null);
            }

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (thisUnit == null) return;

        Gizmos.DrawWireSphere(this.transform.position, thisUnit.GetRange());
    }
}
