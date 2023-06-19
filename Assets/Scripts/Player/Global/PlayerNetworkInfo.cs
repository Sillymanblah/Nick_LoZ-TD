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

    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer) return;

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer) return;
        
        LobbyManager.instance.player = this;
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

        CSNetworkManager.instance.AddPlayerName(name);
    }

    [TargetRpc]
    public void SetLobbyUI(string name, int index)
    {
        LobbyManager.instance.SetPlayerListUI(name, index);
    }

    [Command]
    public void ReadyUpButton()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0) return;

        playerReady = !playerReady;
        LobbyManager.instance.PlayersAreReady(playerReady);
    }
}
