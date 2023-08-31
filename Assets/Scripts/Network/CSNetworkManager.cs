using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using kcp2k;
using UnityEngine.Rendering;

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
    [SerializeField] public bool sceneTesting;

    // NetworkManager.cs source code changes
    // line - 1112 | if statement is a change

    public override void Start()
    {
        instance = this;
        Debug.Log($"Server has not started");
        thisTransport = GetComponent<KcpTransport>();
        DebugManager.instance.enableRuntimeUI = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        NetworkClient.RegisterHandler<ConnectionRefusedMessage>(OnConnectionRefused);

        if (!DeployingAsServer) return;

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

        Debug.Log($"brhhh");

        Destroy(FindObjectOfType<AudioManager>().gameObject);

    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
               

        /*if (SceneManager.GetActiveScene().buildIndex == 0) 
        {
            NetworkClient.RegisterHandler<ConnectionRefusedMessage>(OnConnectionRefused);   
        }*/

        if (isSinglePlayer)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; 
            StartHost();
        }

        if (!DeployingAsServer) return;

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

                NetLevelSelectorUI.instance.netIdentity.AssignClientAuthority(newPlayer.netIdentity.connectionToClient);
                NetLevelSelectorUI.instance.OnAssignAuthority(conn);
            }
            
            else
                NetLevelSelectorUI.instance.OnDeAssignAuthority(conn);

            newPlayer.OnClientJoinLobby();

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
        
        if (newPlayer.playerIsHost)
        {
            NetLevelSelectorUI.instance.netIdentity.RemoveClientAuthority();

            if (players.Count != 0)
            {
                players[0].GetComponent<PlayerNetworkInfo>().playerIsHost = true;
                NetLevelSelectorUI.instance.netIdentity.AssignClientAuthority(players[0].connectionToClient);
                NetLevelSelectorUI.instance.OnAssignAuthority(players[0].connectionToClient);
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
        if (isSinglePlayer) return;

        LobbyScene();
    }

    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);
        MainMenuUIManager.instance.FailedToJoinLobby(reason);
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

        if (isSinglePlayer) return;

        SetLobbyPlayerNames();
    }

    [Server]
    public void SwitchScenes(string sceneName)
    {
        ServerChangeScene(sceneName);
        players.Clear();
    }

    private bool ShouldRefuseConnection(out string reason, NetworkConnectionToClient conn)
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

    public int IngameStatus()
    {
        if (GameManager.instance == null) return 0;
        else return 1;
        
    }

    public ushort GetPort()
    {
        return thisTransport.Port;
    }
}
