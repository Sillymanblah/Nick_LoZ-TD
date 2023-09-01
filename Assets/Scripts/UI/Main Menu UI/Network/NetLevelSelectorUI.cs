using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

public class NetLevelSelectorUI : NetworkBehaviour
{
    [SerializeField] List<LevelSO> levels = new List<LevelSO>();
    [SyncVar] int levelNumSelector = 0;
    public static NetLevelSelectorUI instance;

    [SerializeField] Image levelImage;
    [SerializeField] TextMeshProUGUI levelNameText;

    [SerializeField] GameObject leftButton;
    [SerializeField] GameObject rightButton;

    public string currentLevel;

    // Start is called before the first frame update
    void Start()
    {
        if (!isServer) return;

        levelNumSelector = 0;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentLevel = levels[levelNumSelector].sceneName;
        instance = this;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        UponJoining();
    }

    [TargetRpc]
    public void OnAssignAuthority(NetworkConnectionToClient conn)
    {
        if (!isOwned) return;

        ToggleButtons(true);
    }

    [TargetRpc]
    public void OnDeAssignAuthority(NetworkConnectionToClient conn)
    {
        ToggleButtons(false);
    }

    [Command]
    public void LeftButton()
    {
        levelNumSelector--;
        if (levelNumSelector < 0) levelNumSelector = levels.Count - 1;

        currentLevel = levels[levelNumSelector].sceneName;
        SetLevel();
    }

    [Command]
    public void RightButton()
    {
        levelNumSelector++;
        if (levelNumSelector >= levels.Count) levelNumSelector = 0;

        currentLevel = levels[levelNumSelector].sceneName;
        SetLevel();
    }

    void UponJoining()
    {
        

        LevelSO selectedLevel = levels[levelNumSelector];
        levelImage.sprite = selectedLevel.levelPicture;
        levelNameText.text = selectedLevel.levelName;
    }

    [ClientRpc]
    void SetLevel()
    {
        LevelSO selectedLevel = levels[levelNumSelector];
        levelImage.sprite = selectedLevel.levelPicture;
        levelNameText.text = selectedLevel.levelName;
    }

    void ToggleButtons(bool toggle)
    {
        leftButton.SetActive(toggle);
        rightButton.SetActive(toggle);
    }
}
