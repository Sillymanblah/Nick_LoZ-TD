using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-authenticators
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class CSNetworkAuthenticator : NetworkAuthenticator
{
    #region Messages

    [Header("Server Credentials")]
    public int serverAccessToken;
    public string serverVersion;

    [Header("Client Credentials")]
    public int clientAccessToken;
    public string clientVersion;

    [Space]
    [SerializeField] [Scene] private string mainMenuScene;
    readonly HashSet<NetworkConnection> connectionsPendingDisconnect = new HashSet<NetworkConnection>();

    public struct AuthRequestMessage : NetworkMessage 
    { 
        public int accessToken;
        public string version;
    }

    public struct AuthResponseMessage : NetworkMessage 
    { 
        public string message;
    }

    #endregion

    #region Server

    /// Called on server from StartServer to initialize the Authenticator
    /// Server message handlers should be registered in this method.
    public override void OnStartServer()
    {
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }


    /// Called on server from OnServerConnectInternal when a client needs to authenticate
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnectionToClient conn) { }

    /// Called on server when the client's AuthRequestMessage arrives
    /// <param name="conn">Connection to client.</param>
    /// <param name="msg">The message payload</param>
    public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        Debug.Log($"Authentication Request: {msg.accessToken} {msg.version}");

        if (connectionsPendingDisconnect.Contains(conn)) return;

        if (SceneManager.GetActiveScene().path != mainMenuScene)
        {
            ServerAccessDenied(conn, "Game has already started");
            return;
        }

        else if (CSNetworkManager.instance.numPlayers >= 4)
        {
            ServerAccessDenied(conn, "Game is full");
            return;
        }

         // check the credentials by calling your web server, database table, playfab api, or any method appropriate.
        if (msg.accessToken == serverAccessToken && msg.version == serverVersion)
        {
            // create and send msg to client so it knows to proceed
            AuthResponseMessage authResponseMessage = new AuthResponseMessage
            {
                message = "Success"
            };

            conn.Send(authResponseMessage);

            ServerAccept(conn);
        }
        else // denied access to server
        {
            ServerAccessDenied(conn, "Invalid Game");
        }
    }

    void ServerAccessDenied(NetworkConnectionToClient conn, string message)
    {
        connectionsPendingDisconnect.Add(conn);

        // create and send msg to client so it knows to disconnect
        AuthResponseMessage authResponseMessage = new AuthResponseMessage
        {
            message = message
        };

        conn.Send(authResponseMessage);

        // must set NetworkConnection isAuthenticated = false
        conn.isAuthenticated = false;

        // disconnect the client after 1 second so that response message gets delivered
        StartCoroutine(DelayedDisconnect(conn, 1f));
    }

    IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // Reject the unsuccessful authentication
        ServerReject(conn);

        yield return null;

        // remove conn from pending connections
        connectionsPendingDisconnect.Remove(conn);
    }

    /// Called when server stops, used to unregister message handlers if needed.
    public override void OnStopServer()
    {
        // Unregister the handler for the authentication request
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    #endregion

    #region Client

    /// Called on client from StartClient to initialize the Authenticator
    /// Client message handlers should be registered in this method
    public override void OnStartClient()
    {
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    /// Called on client from OnClientConnectInternal when a client needs to authenticate
    public override void OnClientAuthenticate()
    {
        // we are setting the variables from authrequestmessage to our client ones
        AuthRequestMessage authRequestMessage = new AuthRequestMessage
        {
            accessToken = clientAccessToken,
            version = clientVersion
        };

        NetworkClient.Send(authRequestMessage);
    }

    /// Called on client when the server's AuthResponseMessage arrives
    public void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        if (msg.message == "Success")
        {
            ClientAccept();
        }
        else
        {
            Debug.LogWarning($"Authentication Response: {msg.message}");
            MainMenuUIManager.instance.FailedToJoinLobby(msg.message);

            ClientReject();
        }
    }

    /// Called when client stops, used to unregister message handlers if needed.
    public override void OnStopClient()
    {
        // Unregister the handler for the authentication response
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    #endregion
}
