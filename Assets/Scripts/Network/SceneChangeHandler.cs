using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeHandler : NetworkBehaviour
{
    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<SceneChangeMessage>(OnSceneChangeMessage);
    }

    void OnSceneChangeMessage(SceneChangeMessage message)
    {
        SceneManager.LoadScene(message.sceneBuildIndex);
    }
}
