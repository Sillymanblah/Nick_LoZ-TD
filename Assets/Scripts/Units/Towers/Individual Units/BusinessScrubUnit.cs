using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BusinessScrubUnit : Unit
{
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
    protected override void DealDamageToEnemy()
    {
        if ((enemiesInRange[0].GetHealthPoints() - attack) <= 0)
        {
            enemiesInRange[0].SetEnemyMoneyMultiplier(ReturnMoneyMultiplier());
        }

        enemiesInRange[0].DealDamage(attack);
    }

    float ReturnMoneyMultiplier()
    {
        return 1f + ((float)GetLevel() / 10);
    }

    // Inverting the LookRotation because the armature of the model is backwards so itll look backwards
    [ClientRpc]
    protected override void RpcClientUnitActions(Vector3 firstEnemyPos)
    {
        Vector3 lookEnemyRot = new Vector3(firstEnemyPos.x, 0, firstEnemyPos.z);
        Vector3 modelUnitRot = new Vector3(transform.GetChild(0).position.x, 0, transform.GetChild(0).position.z);
        Quaternion lookOnLook = Quaternion.LookRotation(-(lookEnemyRot - modelUnitRot));
        transform.GetChild(0).rotation = lookOnLook;
    }
}
