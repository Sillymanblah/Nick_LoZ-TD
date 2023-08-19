using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using kcp2k;
using System;

public class CSNetworkManager : NetworkManager
{
    public static CSNetworkManager instance;
    public List<NetworkIdentity> players = new List<NetworkIdentity>();
    
    Vector3 networkPosition;

    List<string> playerNames = new List<string>();
    KcpTransport thisTransport;
    

    [SerializeField] bool DeployingAsServer;
    [SerializeField] bool ignorePort;
    [SerializeField] public bool sceneTesting;

    // NetworkManager.cs source code changes
    // line - 1112 | if statement is a change
    // line - 1292 | prevents client from reloading current scene

    // Start is called before the first frame update
    public override void Start()
    {
        instance = this;
        Debug.Log($"Server has not started");
        thisTransport = GetComponent<KcpTransport>();

        NetworkClient.RegisterHandler<ConnectionRefusedMessage>(OnConnectionRefused);

        if (!DeployingAsServer) return;

        SceneManager.sceneLoaded += OnSceneLoaded;

        string cmdLinePort = System.Environment.GetCommandLineArgs()[1];

        if (!ignorePort)
        {
            thisTransport.port = ushort.Parse(cmdLinePort);
        }
        else
        {
            thisTransport.port = 7777;
        }

        StartServer();
    }

    

    // Update is called once per frame
    public override void Update()
    {
        
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        
    }

    

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {

        if (ShouldRefuseConnection(out string reason, conn))
        {

            conn.Send(new ConnectionRefusedMessage(reason));
            
            Debug.Log(reason);

            return;
        }

        base.OnServerConnect(conn);

        Debug.Log($"literllay nothing happened");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        var newPlayer = conn.identity.GetComponent<PlayerNetworkInfo>();

        Debug.Log(newPlayer.netIdentity + " connected.");

        players.Add(conn.identity);

        // For lobby
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            // if hes the only one in the lobby
            if (players.Count == 1)
            {
                newPlayer.playerIsHost = true;

                LevelSelectorUI.instance.netIdentity.AssignClientAuthority(newPlayer.netIdentity.connectionToClient);
                LevelSelectorUI.instance.OnAssignAuthority(conn);
            }
            
            else
                LevelSelectorUI.instance.OnDeAssignAuthority(conn);

            newPlayer.OnClientJoinLobby();

            return;
        }

        // For ingame / after lobby


        
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        GameManager.instance.UpdatePlayerCount();
        
        networkPosition = GameObject.FindObjectOfType<NetworkStartPosition>().transform.position;

        foreach (NetworkIdentity player in players)
        {
            player.transform.position = networkPosition;
            player.GetComponent<PlayerManager>().OnStartGame();
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn == null)
        {
            base.OnServerDisconnect(conn);
            return;   
        }

        var newPlayer = conn.identity.GetComponent<PlayerNetworkInfo>();

        Debug.Log(newPlayer.netIdentity + " disconnected.");
        Debug.Log(newPlayer.name + " has left the game");

        playerNames.Remove(newPlayer.name);
        players.Remove(conn.identity);
        
        if (newPlayer.playerIsHost)
        {
            LevelSelectorUI.instance.netIdentity.RemoveClientAuthority();

            if (players.Count != 0)
            {
                players[0].GetComponent<PlayerNetworkInfo>().playerIsHost = true;
                LevelSelectorUI.instance.netIdentity.AssignClientAuthority(players[0].connectionToClient);
                LevelSelectorUI.instance.OnAssignAuthority(players[0].connectionToClient);
            }
        }

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
        
        Debug.Log("Server IP: " + networkAddress);
        Debug.Log($"Server Port: " + thisTransport.Port);

        if (sceneTesting) return;

        LobbyScene();
    }

    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);
        MainMenuUIManager.instance.FailedToJoinLobby(error + ": " + reason);
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
        Debug.Log(name + " has joined the game!");
        SetLobbyPlayerNames();
    }

    /*[Server]
    public void SwitchScenes(string sceneName)
    {
        ServerChangeScene(sceneName);
        players.Clear();
    }*/

    [Server]
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        NetworkServer.SendToAll(new SceneChangeMessage { sceneName = sceneName });
    }

    bool ShouldRefuseConnection(out string reason, NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            reason = "Game has already started";
            return true;
        }

        /*else if (conn.identity == null)
        {
            reason = "Already connected to this game";
            return true;
        }*/

        reason = "Good to go";
        return false;
    }

    private void OnConnectionRefused(ConnectionRefusedMessage message)
    {
        Debug.Log("Connection refused: " + message.reason);
        MainMenuUIManager.instance.FailedToJoinLobby(message.reason);

        NetworkClient.Disconnect();
        NetworkClient.RegisterHandler<ConnectionRefusedMessage>(OnConnectionRefused);

        // Display the refusal reason to the player
        //refusalReasonText.text = "Connection refused: " + message.reason;
    }
}
