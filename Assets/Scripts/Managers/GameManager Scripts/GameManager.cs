using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [System.Serializable]
    public class UnitDrops
    {
        public UnitSO unit;
        public int dropChance;
    }

    public List<UnitDrops> unitDrops = new List<UnitDrops>();

    List<GridCell> totalGridCells = new List<GridCell>();
    public static GameManager instance;


    [SyncVar]
    public bool gameStarted;

    public int intermissionTimer;

    [SyncVar]
    public bool intermission = false;

    
    int playerReadyCount;
    List<NetworkIdentity> playersReadyToPlay = new List<NetworkIdentity>();

    #region Events

    public event EventHandler OnGameStart;


    #endregion

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        if (isServer)
        {
            BaseManager.instance.OnBaseDead += GameHasEnded;
            WaveManager.instance.OnGameWon += GameHasEnded;
        }
    }

    public override void OnStartServer()
    {
        if (isClientOnly) return;

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
        playersReadyToPlay.Clear();
        playerReadyCount = 0;

        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.numPlayers);
    }

    [Server]
    public void PlayersAreReady(NetworkIdentity conn)
    {
        if (playersReadyToPlay.Count > 0)
        {
            if (!playersReadyToPlay.Contains(conn))
            {
                playerReadyCount++;
                playersReadyToPlay.Add(conn);
            }
            else
            {
                playerReadyCount--;
                playersReadyToPlay.Remove(conn);
            }
        }
        else
        {
            playerReadyCount++;
            playersReadyToPlay.Add(conn);
        }

        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.numPlayers);
        UIManager.instance.UpdateReadyButton(playerReadyCount, CSNetworkManager.instance.numPlayers);

        if (playerReadyCount == CSNetworkManager.instance.numPlayers)
        {
            StartGame();
        }
    }

    [ClientRpc]
    void RpcUpdatePlayerCount(int playerReady, int maxPlayerCount)
    {
        UIManager.instance.UpdateReadyButton(playerReady, maxPlayerCount);
    }

    [Server]
    public virtual void StartGame()
    {
        gameStarted = true;
        //WaveManager.instance.StartGame(CSNetworkManager.instance.players.Count, 3);
        BaseManager.instance.SetBaseHP(CSNetworkManager.instance.players.Count);
        playerReadyCount = 0;
        
        RpcAllReady();

        if (isServerOnly)
            OnGameStart?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    void RpcAllReady()
    {
        OnGameStart?.Invoke(this, EventArgs.Empty);

        UIManager.instance.DisableReadyButtonLocally();
    }

    [Server]
    // 2nd parameter - 
    public virtual void GameHasEnded(object sender, GameManagerEventArgs e)
    {
        Debug.Log($"Gamemanager, game has ended");
    }

    protected virtual UnitSO GiveUnitAward()
    {
        return null;
    }
}

public class GameManagerEventArgs : EventArgs
{
    public bool isDead;
}
