using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    List<GridCell> totalGridCells = new List<GridCell>();
    public static GameManager instance;
    public event EventHandler OnGameStart;

    int playerReadyCount;

    [SyncVar]
    public bool gameStarted;
    [SyncVar]
    public bool intermission = false;
    public GameLevelSO gameLevelSO;
    

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

    public GridCell GetGridCell(int index)
    {
        return totalGridCells[index];
    }

    [Server]
    public void SyncGridCellOccupence(bool busy, List<int> indexes)
    {
        foreach (int index in indexes)
        {
            totalGridCells[index].SetOccupence(busy);
        }

        RPCSyncGridCellOccupence(busy, indexes);
    }

    [ClientRpc]
    void RPCSyncGridCellOccupence(bool busy, List<int> indexes)
    {
        foreach (int index in indexes)
        {
            totalGridCells[index].SetOccupence(busy);
        }
    }

    [Server]
    public void UpdatePlayerCount()
    {
        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.numPlayers);
    }

    [Server]
    public void PlayerLeft()
    {
        playerReadyCount--;
        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.numPlayers);

        if (playerReadyCount == CSNetworkManager.instance.numPlayers)
            StartGame();
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
            StartGame();
        }
    }

    [Server]
    public void StartGame()
    {
        gameStarted = true;
        WaveManager.instance.spawnEnemies = true;
        WaveManager.instance.SetHealthWithPlayerCount(CSNetworkManager.instance.players.Count);
        BaseManager.instance.SetBaseHP(CSNetworkManager.instance.players.Count);
        playerReadyCount = 0;
        
        RpcAllReady();

        if (!isServerOnly)
            OnGameStart?.Invoke(this, EventArgs.Empty);


        /*foreach (NetworkIdentity player in CSNetworkManager.instance.players)
        {
            AllReady(player.connectionToClient);
        }*/
    }



    [TargetRpc]
    void AllReady(NetworkConnectionToClient conn)
    {
        
    }

    [ClientRpc]
    void RpcAllReady()
    {
        OnGameStart?.Invoke(this, EventArgs.Empty);

        UIManager.instance.DisableReadyButtonLocally();
    }

    
}
