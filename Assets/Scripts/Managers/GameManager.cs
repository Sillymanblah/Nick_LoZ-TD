using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    List<GridCell> totalGridCells = new List<GridCell>();
    public static GameManager instance;

    int playerReadyCount;

    [SyncVar]
    public bool gameStarted;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        
        instance = this;
    }

    public override void OnStartServer()
    {
        if (isClient) return;

        Transform gridCellParent = GameObject.Find("GridCells").transform;

        for (int i = 0; i < gridCellParent.childCount; i++)
        {
            totalGridCells.Add(gridCellParent.GetChild(i).GetComponent<GridCell>());
            totalGridCells[i].listIndex = i;
        }
    }

    public override void OnStartClient()
    {
        if (CSNetworkManager.instance.sceneTesting) return;

        Transform gridCellParent = GameObject.Find("GridCells").transform;

        for (int i = 0; i < gridCellParent.childCount; i++)
        {
            totalGridCells.Add(gridCellParent.GetChild(i).GetComponent<GridCell>());
            totalGridCells[i].listIndex = i;
        }
    }

    [Server]
    public GridCell GetGridCell(int index)
    {
        return totalGridCells[index];
    }

    [Server]
    public void SyncGridCellOccupence(bool busy, int index)
    {
        totalGridCells[index].SetOccupence(busy);

        foreach (NetworkIdentity player in CSNetworkManager.instance.players)
        {
            RPCSyncGridCellOccupence(player.connectionToClient, busy, index);
        }
    }

    [TargetRpc]
    void RPCSyncGridCellOccupence(NetworkConnectionToClient sender, bool busy, int index)
    {
        totalGridCells[index].SetOccupence(busy);
    }

    [Server]
    public void UpdatePlayerCount()
    {
        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.numPlayers);
    }

    [ClientRpc]
    void RpcUpdatePlayerCount(int playerReady, int maxPlayerCount)
    {
        UIManager.instance.UpdateReadyButton(playerReady, maxPlayerCount);
    }

    [Server]
    public void PlayersAreReady(bool ready)
    {
        if (ready == true) playerReadyCount++;
        else playerReadyCount--;

        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.numPlayers);

        UIManager.instance.UpdateReadyButton(playerReadyCount, CSNetworkManager.instance.numPlayers);

        if (playerReadyCount == CSNetworkManager.instance.numPlayers)
        {
            gameStarted = true;
            WaveManager.instance.spawnEnemies = true;
            WaveManager.instance.SetHealthWithPlayerCount(CSNetworkManager.instance.numPlayers);
            BaseManager.instance.SetBaseHP(CSNetworkManager.instance.numPlayers);
            playerReadyCount = 0;

            foreach (NetworkIdentity player in CSNetworkManager.instance.players)
            {
                AllReady(player.connectionToClient);
            }
        }
    }

    [TargetRpc]
    void AllReady(NetworkConnectionToClient conn)
    {
        UIManager.instance.DisableReadyButtonLocally();
    }
}
