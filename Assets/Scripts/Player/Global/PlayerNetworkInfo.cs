using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerNetworkInfo : NetworkBehaviour
{
    [SyncVar]
    public new string name;

    LobbyAuth playerAuth;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [TargetRpc]
    public void OnClientJoinLobby()
    {
        playerAuth = GameObject.FindObjectOfType<LobbyAuth>();
        playerAuth.SyncData(this);
    }

    [Command]
    public void SetDisplayName(string newName)
    {
        name = newName;

        CSNetworkManager.instance.SetLobbyPlayerNames();
    }

    [ClientRpc]
    public void SetLobbyUI(string name, int index)
    {
        LobbyManager.instance.SetPlayerListUI(name, 0);
    }
}
