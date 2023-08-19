using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeHandler : NetworkBehaviour
{
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        if (isServer) return;

        NetworkClient.RegisterHandler<SceneChangeMessage>(OnSceneChangeMessage);
    }

    private void OnSceneChangeMessage(SceneChangeMessage message)
    {
        SceneManager.LoadScene(message.sceneName);
    }
}
