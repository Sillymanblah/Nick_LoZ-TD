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
    

    [Space]

    public bool isSinglePlayer;

    [Space]

    public bool DeployingAsServer;
    public bool ignorePort;
    public bool noRestrictions;
    [SerializeField] public bool sceneTesting;

    [Space]
    float setRealTime;
    [SerializeField] float lobbyKickTimer;
    [SerializeField] float gameStartTimer;
    public string gameVersion;

    // NetworkManager.cs source code changes
    // line - 1112 | if statement is a change

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

        foreach (AudioSource audio in FindObjectsOfType<AudioSource>())
        {
            Destroy(audio.gameObject);
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (isSinglePlayer)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; 
            return;
        }

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (!DeployingAsServer)
            {
                Debug.Log($"oh brother");
                LobbyManager.instance.SetUpLobbyScene();
                return;
            }
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

        players.Clear();
        playerNames.Clear();
            
        Destroy(GameObject.FindObjectOfType<AudioManager>().gameObject);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (!isSinglePlayer && !sceneTesting)
        {
            if (ShouldRefuseConnection(out string reason, conn))
            {
                conn.Send(new ConnectionRefusedMessage(reason));
                Debug.Log(reason);
                return;
            }
        }
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
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
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
            // 
            
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

        conn.identity.GetComponent<PlayerManager>().OnStartGame(conn);
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
        if (SceneManager.GetActiveScene().buildIndex == 1)
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

    public override void OnStopClient()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (SceneManager.GetActiveScene().buildIndex == 1)
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
        ServerChangeScene(sceneName);
        players.Clear();
    }

    private bool ShouldRefuseConnection(out string reason, NetworkConnectionToClient conn)
    {

        if (SceneManager.GetActiveScene().buildIndex != 1)
        {
            reason = "Game has already started";
            return true;
        }

        else if (numPlayers >= 4)
        {
            reason = "Server is full";
            return true;
        }

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

    IEnumerator GameTimers()
    {
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
