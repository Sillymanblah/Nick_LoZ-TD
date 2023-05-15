using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using kcp2k;

public class CSNetworkManager : NetworkManager
{
    public static CSNetworkManager instance;
    public List<NetworkIdentity> players = new List<NetworkIdentity>();
    KcpTransport thisTransport;

    [SerializeField] bool DeployingAsServer;

    // Start is called before the first frame update
    public override void Start()
    {
        instance = this;
        Debug.Log($"Server has not started");

        if (!DeployingAsServer) return;

        StartServer();
    }

    // Update is called once per frame
    public override void Update()
    {
        
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        players.Add(conn.identity);
        GameManager.instance.UpdatePlayerCount(true);

        PlayerManager player = conn.identity.GetComponent<PlayerManager>();
        //player.SetDisplayName("Player " + numPlayers);
        player.SetCameraPOV();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        players.Remove(conn.identity);
        base.OnServerDisconnect(conn);
        GameManager.instance.UpdatePlayerCount(false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        thisTransport = GetComponent<KcpTransport>();
        Debug.Log("Server IP: " + networkAddress);
        Debug.Log($"Server Port: " + thisTransport.Port);
    }
}
