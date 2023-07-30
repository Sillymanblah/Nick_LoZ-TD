using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

public class LevelSelectorUI : NetworkBehaviour
{
    [SerializeField] List<LevelSO> levels = new List<LevelSO>();
    [SyncVar] int levelNumSelector = 0;
    public static LevelSelectorUI instance;

    [SerializeField] Image levelImage;
    [SerializeField] TextMeshProUGUI levelNameText;

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
}
