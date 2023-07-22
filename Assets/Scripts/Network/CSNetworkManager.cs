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
    List<string> playerNames = new List<string>();
    KcpTransport thisTransport;
    

    [SerializeField] bool DeployingAsServer;
    [SerializeField] public bool sceneTesting;

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

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (GameManager.instance.gameStarted)
            {
                conn.Disconnect();
                Debug.Log($"ONSERVERCONNECT");

            }

            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        Debug.Log(conn.identity.GetComponent<PlayerNetworkInfo>().name + " has joined the game!");
        players.Add(conn.identity);

        // For lobby
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            var newPlayer = conn.identity.GetComponent<PlayerNetworkInfo>();

            newPlayer.OnClientJoinLobby();
            
            if (players.Count == 0)
                newPlayer.playerIsHost = true;

            return;
        }

        // For ingame and after lobby

        GameManager.instance.UpdatePlayerCount();

        conn.identity.GetComponent<PlayerManager>().OnStartGame(conn);

        //PlayerManager player = conn.identity.GetComponent<PlayerManager>();
        
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log(conn.identity.GetComponent<PlayerNetworkInfo>().name + " has left the game");
        playerNames.Remove(conn.identity.GetComponent<PlayerNetworkInfo>().name);
        players.Remove(conn.identity);
        
        

        // for lobby
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            conn.identity.GetComponent<PlayerNetworkInfo>().OnClientLeaveLobby(conn);
            SetLobbyPlayerNames();
            
            base.OnServerDisconnect(conn);
            return;
        }
        // for ingame

        GameManager.instance.UpdatePlayerCount();
        base.OnServerDisconnect(conn);

        if (numPlayers == 0)
        {
            Debug.LogWarning($"Changing Scenes");
            ServerChangeScene("MainMenu");
            

            players.Clear();
            playerNames.Clear();
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        thisTransport = GetComponent<KcpTransport>();
        Debug.Log("Server IP: " + networkAddress);
        Debug.Log($"Server Port: " + thisTransport.Port);

        if (sceneTesting) return;

        LobbyScene();
    }

    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);
        MainMenuUIManager.instance.FailedToJoinLobby();
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
            if (players.Count == 0)
            {
                LobbyManager.instance.SetPlayerListUI(string.Empty, i);
                continue;
            }

            // if the iteration count is greater than our player count, then we make the null player slots empty
            if (i + 1 > players.Count)
            {
                LobbyManager.instance.SetPlayerListUI(string.Empty, i);
                continue;
            }

            LobbyManager.instance.SetPlayerListUI(playerNames[i], i);
        }
    }

    [Server]
    public void AddPlayerName(string name)
    {
        playerNames.Add(name);
        SetLobbyPlayerNames();
    }

    [Server]
    public void SwitchScenes(string sceneName)
    {
        ServerChangeScene(sceneName);
        players.Clear();
    }
}
