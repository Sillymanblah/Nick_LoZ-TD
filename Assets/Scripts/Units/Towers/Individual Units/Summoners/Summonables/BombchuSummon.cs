using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BombchuSummon : Summonable
{
    [SerializeField] ParticleSystem thisParticleSystem;

    protected override void Start() => base.Start();

    protected override void Update() => base.Update();

    [Server]
    protected override void ServerInteraction()
    {
        base.ServerInteraction();
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
        base.ClientEnemyInteract();
        thisParticleSystem.Play();
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
}
