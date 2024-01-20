using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using kcp2k;
using UnityEngine.Rendering;
using System.Collections;

public class CSNetworkManager : NetworkManager
{
    public static CSNetworkManager instance;
    public List<NetworkIdentity> players = new List<NetworkIdentity>();
    List<string> playerNames = new List<string>();
    KcpTransport thisTransport;
    

    [Header("Client Variables")]
    public bool isSinglePlayer;

    [Space]

    [Header("Server Variables")]
    public bool DeployingAsServer;
    public bool ignorePort;

    [Header("Client/Server Variables")]
    public bool noRestrictions;

    [Space]
    [SerializeField] public bool sceneTesting;

    [Space]
    float setRealTime;
    [SerializeField] float lobbyKickTimer;
    [SerializeField] float gameStartTimer;
    public string gameVersion;

    bool cameFromGame;

    #region 

    [Header("Scene References")]
    [SerializeField] [Scene] public string mainMenuScene;

    #endregion

    // NetworkManager.cs source code changes
    // lines 1343 - 1348 | if statement is an addition to prevent client loading same scene upon connecting to server

    public override void Start()
    {
        instance = this;
        Debug.Log($"Server has not started");
        thisTransport = GetComponent<KcpTransport>();
        //StartCoroutine(GetComponent<NetworkDataBase>().GetServer());

        DebugManager.instance.enableRuntimeUI = false;
        PlayerPrefs.DeleteKey($"HostNetID{GetPort() - 7776}");
        PlayerPrefs.Save();

        

        SceneManager.sceneLoaded += OnSceneLoaded;
        NetworkClient.RegisterHandler<ConnectionRefusedMessage>(OnConnectionRefused);


        if (!DeployingAsServer) return;

        StartCoroutine(nameof(LobbyTimers));

        ServerChangeScene("MainMenu");

        GetComponent<NetworkDataBase>().StartDatabase();

        if (!ignorePort)
        {
            string cmdLinePort = System.Environment.GetCommandLineArgs()[1];
            thisTransport.port = ushort.Parse(cmdLinePort);
        }
        else
        {
            thisTransport.port = 7777;
        }

        StartServer();
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (isSinglePlayer)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; 
            return;
        }

        

        if (SceneManager.GetActiveScene().path == mainMenuScene)
        {
            
            StartCoroutine(nameof(LobbyTimers));
        }
        else
        {
            if (DeployingAsServer)
            {
                StopCoroutine(nameof(LobbyTimers));

                setRealTime = Time.time;
                StartCoroutine(GameTimers());
            }
        }

        if (!DeployingAsServer) return;

        players.Clear();
        playerNames.Clear();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        setRealTime = Time.time;

        var newPlayer = conn.identity.GetComponent<PlayerNetworkInfo>();
        Debug.Log(newPlayer.netIdentity + " connected.");
        players.Add(conn.identity);

        // For lobby
        if (SceneManager.GetActiveScene().path == mainMenuScene)
        {
            Debug.Log($"Player is in lobby");

            // if hes the only one in the lobby
            
            newPlayer.OnClientJoinLobby();

            // If server doesnt contain the hostnetID key, 
            //then we go ahead and make first person the host
            // as well as make the key
            if (!PlayerPrefs.HasKey($"HostNetID{GetPort() - 7776}"))
            {
                newPlayer.playerIsHost = true;
                PlayerPrefs.SetInt($"HostNetID{GetPort() - 7776}", conn.connectionId);
                PlayerPrefs.Save();

                NetLevelSelectorUI.instance.netIdentity.AssignClientAuthority(newPlayer.netIdentity.connectionToClient);
                NetLevelSelectorUI.instance.OnAssignAuthority(conn);
            }

            // If the key already exists,
            // And if it matches this connection,
            // then make him the host and give privileges
        
            else
            {
                // THIS IS FOR GOING FROM GAME -> LOBBY client connections
                if (conn.connectionId == PlayerPrefs.GetInt($"HostNetID{GetPort() - 7776}"))
                {
                    newPlayer.playerIsHost = true;

                    NetLevelSelectorUI.instance.netIdentity.AssignClientAuthority(newPlayer.netIdentity.connectionToClient);
                    NetLevelSelectorUI.instance.OnAssignAuthority(conn);
                }

                // this is the same as if players.count > 1
                else
                    NetLevelSelectorUI.instance.OnDeAssignAuthority(conn);
            }
            return;
        }
        // For ingame and after lobby

        GameManager.instance.UpdatePlayerCount();

        Debug.Log($"Server before setting tunic color");

        PlayerManager clientPlayerManager = conn.identity.GetComponent<PlayerManager>();
        clientPlayerManager.OnStartGame(conn);
        clientPlayerManager.SetTunicColor(players.Count - 1);
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
        LobbyManager.instance.RpcUpdatePlayerCount(LobbyManager.instance.playerReadyCount, numPlayers);

        // for lobby
        if (SceneManager.GetActiveScene().path == mainMenuScene)
        {
            if (newPlayer.connectionToClient.connectionId == PlayerPrefs.GetInt($"HostNetID{GetPort() - 7776}"))
            {
                NetLevelSelectorUI.instance.netIdentity.RemoveClientAuthority();

                if (players.Count > 0)
                {
                    PlayerPrefs.SetInt($"HostNetID{GetPort() - 7776}", players[0].connectionToClient.connectionId);
                    PlayerPrefs.Save();
                    players[0].GetComponent<PlayerNetworkInfo>().playerIsHost = true;
                    NetLevelSelectorUI.instance.netIdentity.AssignClientAuthority(players[0].connectionToClient);
                    NetLevelSelectorUI.instance.OnAssignAuthority(players[0].connectionToClient);
                }

                // for if player count = 0 when a client DISCONNECTS
                else
                {
                    PlayerPrefs.DeleteKey($"HostNetID{GetPort() - 7776}");
                    PlayerPrefs.Save();
                }
            }

            conn.identity.GetComponent<PlayerNetworkInfo>().OnClientLeaveLobby(conn);
            LobbyManager.instance.SetPlayerListUI(playerNames);
            base.OnServerDisconnect(conn);
            return;
        }

        // --------------
        // | for ingame |
        // --------------
        if (newPlayer.connectionToClient.connectionId == PlayerPrefs.GetInt($"HostNetID{GetPort() - 7776}"))
        {
            PlayerPrefs.DeleteKey($"HostNetID{GetPort() - 7776}");
            PlayerPrefs.Save();
        }

        GameManager.instance.UpdatePlayerCount();
        base.OnServerDisconnect(conn);

        if (numPlayers == 0)
        {
            Debug.LogWarning($"Changing Scenes");
            PlayerPrefs.DeleteKey($"HostNetID{GetPort() - 7776}");
            ServerChangeScene("MainMenu");

            players.Clear();
            playerNames.Clear();
        }

        if (GameManager.instance.gameStarted == false)
            GameManager.instance.PlayerLeft();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        Debug.Log("Server IP: " + networkAddress);
        Debug.Log($"Server Port: " + thisTransport.Port);

        if (sceneTesting) return;
        if (isSinglePlayer) return;

        //LobbyManager.instance.LobbyScene();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"OnStart Client");

        NetworkClient.RegisterHandler<ConnectionRefusedMessage>(OnConnectionRefused);
    }

    public override void OnStopClient()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        NetworkClient.UnregisterHandler<ConnectionRefusedMessage>();

        if (SceneManager.GetActiveScene().path == mainMenuScene)
        {
            MainMenuUIManager.instance.LeaveLobby();;;;;;;;;;;;;;;;;;
            
        }
    }

    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);
        MainMenuUIManager.instance.FailedToJoinLobby(reason);
    }

    [Server]
    public void AddPlayerName(string name, NetworkConnectionToClient conn)
    {
        playerNames.Add(name);
        Debug.Log(name + " has joined the game!");

        if (isSinglePlayer) return;

        if (PlayerPrefs.HasKey($"HostNetID{GetPort() - 7776}"))
        {
            if (PlayerPrefs.GetInt($"HostNetID{GetPort() - 7776}") == conn.connectionId)
            {
                int nameIndex = playerNames.IndexOf(name);
                playerNames.RemoveAt(nameIndex);
                playerNames.Insert(0, name);
            }
        }

        LobbyManager.instance.SetPlayerListUI(playerNames);
    }

    [Server]
    public void SwitchScenes(string sceneName)
    {
        Debug.Log($"is this the culprit?");
        ServerChangeScene(sceneName);
        players.Clear();
    }

    public int IngameStatus()
    {
        if (GameManager.instance == null) return 0;
        else return 1;
    }

    public ushort GetPort()
    {
        return thisTransport.Port;
    }

    public void SetPort(ushort newPort)
    {
        thisTransport.Port = newPort;
    }

    IEnumerator LobbyTimers()
    {
        float thisTime = 0;

        while (true)
        {
            thisTime = Time.time - setRealTime;

            if (players.Count != 0)
            {
                if (thisTime >= lobbyKickTimer)
                {
                    NetworkServer.SendToAll(new ConnectionRefusedMessage("You have been kicked for being idle"));
                    setRealTime = 0;
                }
            }
            yield return null;
        }
    }

    void OnConnectionRefused(ConnectionRefusedMessage msg)
    {
        MainMenuUIManager.instance.FailedToJoinLobby(msg.reason);
        NetworkClient.Disconnect();
    }

    IEnumerator GameTimers()
    {
        yield return GameManager.instance == null;

        float thisTime = 0;

        while (true)
        {
            thisTime = Time.time - setRealTime;

            if (GameManager.instance.gameStarted) yield break;

            if (thisTime >= gameStartTimer)
            {
                GameManager.instance.StartGame();
                yield break;
            }
            yield return null;
        }
    }
}
