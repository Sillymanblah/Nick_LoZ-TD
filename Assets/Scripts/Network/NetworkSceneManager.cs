using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneManager : NetworkBehaviour
{
    [Server]
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        NetworkServer.SendToAll(new SceneChangeMessage { sceneBuildIndex = sceneIndex});
    }
}

public struct SceneChangeMessage : NetworkMessage
{
    public int sceneBuildIndex;
}
