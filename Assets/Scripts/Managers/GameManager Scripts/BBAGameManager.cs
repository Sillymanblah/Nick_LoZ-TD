using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BBAGameManager : GameManager
{
    [SerializeField] int waveCountReward;

    [Server]
    public override void GameHasEnded(object sender, GameManagerEventArgs e)
    {
        base.GameHasEnded(sender, e);

        if (WaveManager.instance.currentWave >= waveCountReward)
        {
            foreach (NetworkIdentity player in CSNetworkManager.instance.players)
            {
                player.GetComponent<PlayerUnitManager>().SetUnitReward(UnitSO.Get("Bombchu Girl").uniqueName);
            }
        }
    }

    [Server]
    public override void StartGame()
    {
        base.StartGame();
        WaveManager.instance.StartGame(CSNetworkManager.instance.players.Count);
    }
}
