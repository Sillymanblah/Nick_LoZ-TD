using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        
        // For lobby
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            players.Add(conn.identity);
            conn.identity.GetComponent<PlayerManager>().OnStartLobby();

            conn.identity.GetComponent<PlayerNetworkInfo>().OnClientJoinLobby();

            return;
        }

        conn.identity.GetComponent<PlayerManager>().OnStartGame();

        // For ingame and after lobby

        GameManager.instance.UpdatePlayerCount(true);

        PlayerManager player = conn.identity.GetComponent<PlayerManager>();
        player.SetCameraPOV();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        players.Remove(conn.identity);
        base.OnServerDisconnect(conn);

        // for lobby
        if (activeScene.buildIndex == 0)
        {
            SetLobbyPlayerNames();
            return;
        }

        // for ingame

        GameManager.instance.UpdatePlayerCount(false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        thisTransport = GetComponent<KcpTransport>();
        Debug.Log("Server IP: " + networkAddress);
        Debug.Log($"Server Port: " + thisTransport.Port);

        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.buildIndex != 0) return;

        LobbyScene();
    }

    void LobbyScene()
    {
        MainMenuUIManager.instance.OnServerStart(this);
    }

    [Server]
    public void SetLobbyPlayerNames()
    {
        for (int i = 0; i < 4; i++)
        {
            PlayerNetworkInfo playerNI = null;

            if (players[0] != null)
                playerNI = players[0].GetComponent<PlayerNetworkInfo>();

            else if (players.Count == 0)
            {
                LobbyManager.instance.SetPlayerListUI(string.Empty, 0);
                continue;
            }

            // if the iteration count is greater than our player count, then we make the null player slots empty
            if (i + 1 > players.Count)
            {
                playerNI.SetLobbyUI(string.Empty, i);
                continue;
            }

            playerNI.SetLobbyUI(playerNI.name, i);
        }
    }
}
