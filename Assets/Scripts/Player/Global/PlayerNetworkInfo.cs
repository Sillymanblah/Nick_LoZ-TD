using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerNetworkInfo : NetworkBehaviour
{
    [SyncVar]
    public new string name;
    bool playerReady = false;
    LobbyAuth playerAuth;
    PlayerManager playerManager;
    PlayerUnitManager playerUnitManager;
    public bool playerIsHost;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        playerUnitManager = GetComponent<PlayerUnitManager>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer) return;

        if (CSNetworkManager.instance.sceneTesting) return;

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            MainMenuUIManager.instance?.LobbyMenu();
            LobbyManager.instance.player = this;
            return;
        }

        name = PlayerPrefs.GetString("NewName");
        SetDisplayName(name);
    }

    [TargetRpc]
    public void OnClientJoinLobby()
    {
        playerAuth = GameObject.FindObjectOfType<LobbyAuth>();
        playerAuth.SyncData(this);
    }

    [Server]
    public void OnClientLeaveLobby(NetworkConnectionToClient conn)
    {
        if (playerReady == true)
        {
            

            playerReady = !playerReady;
            LobbyManager.instance.PlayersAreReady(false);
        }
    }

    [Command]
    public void SetDisplayName(string newName)
    {
        name = newName;

        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            playerManager.SetUserNameNameTag(name);
        }
        else
        {
            SaveClientData(name);
        }
        CSNetworkManager.instance.AddPlayerName(name);
    }

    [TargetRpc]
    void SaveClientData(string value)
    {
        PlayerPrefs.SetString("NewName", value);
        PlayerPrefs.Save();
    }


    /*[TargetRpc]
    public void SetLobbyUI(string name, int index)
    {
        LobbyManager.instance.SetPlayerListUI(name, index);
    }*/

    [Command]
    public void ReadyUpButton()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0) return;

        //if (CSNetworkManager.instance.numPlayers < 2) return; 

        playerReady = !playerReady;
        LobbyManager.instance.PlayersAreReady(playerReady);
    }

    [Command]
    public void UpdateUnitInventory(List<string> units)
    {
        List<UnitSO> newList = new List<UnitSO>();

        foreach (string unitName in units)
        {
            
            UnitSO result = UnitSO.Get(unitName);
            newList.Add(result);
        }

        

        playerUnitManager.unitsLoadout = newList;
        playerUnitManager.UpdateUnitsInventory(this.netIdentity.connectionToClient, units);
    }
}
