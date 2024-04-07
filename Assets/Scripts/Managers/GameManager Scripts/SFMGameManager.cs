using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SFMGameManager : GameManager
{

    [Server]
    public override void GameHasEnded(object sender, GameManagerEventArgs e)
    {
        base.GameHasEnded(sender, e);

        if (!e.isDead)
        {
            foreach (NetworkIdentity player in CSNetworkManager.instance.players)
            {
                player.GetComponent<PlayerUnitManager>().SetUnitReward(GiveUnitAward().uniqueName);
            }
        }
    }

    [Server]
    protected override UnitSO GiveUnitAward()
    {
        base.GiveUnitAward();

        float totalChance = 0;


        foreach (UnitDrops unit in unitDrops)
        {
            totalChance += unit.dropChance;
        }

        float randomValue = Random.value * totalChance;

        foreach (UnitDrops unit in unitDrops)
        {
            if (randomValue <= unit.dropChance)
            {
                return unit.unit;
            }
            randomValue -= unit.dropChance;
        }

        return unitDrops[0].unit;
    }

    [Server]
    public override void StartGame()
    {
        base.StartGame();
        WaveManager.instance.StartGame(CSNetworkManager.instance.players.Count);
    }
}
