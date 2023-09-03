using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SPLevelSelectorUI : MonoBehaviour
{
    [SerializeField] List<LevelSO> levels = new List<LevelSO>();
    int levelNumSelector = 0;
    public static NetLevelSelectorUI instance;

    [SerializeField] Image levelImage;
    [SerializeField] TextMeshProUGUI levelNameText;

    public string currentLevel;

    // Start is called before the first frame update
    void Start()
    {
        levelNumSelector = 0;
        currentLevel = levels[levelNumSelector].sceneName;
        SetLevel();
    }
    public void LeftButton()
    {
        levelNumSelector--;
        if (levelNumSelector < 0) levelNumSelector = levels.Count - 1;

        currentLevel = levels[levelNumSelector].sceneName;
        SetLevel();
    }

    public void RightButton()
    {
        levelNumSelector++;
        if (levelNumSelector >= levels.Count) levelNumSelector = 0;

        currentLevel = levels[levelNumSelector].sceneName;
        
        SetLevel();
    }

    void SetLevel()
    {
        LevelSO selectedLevel = levels[levelNumSelector];
        levelImage.sprite = selectedLevel.levelPicture;
        levelNameText.text = selectedLevel.levelName;
    }

    public void StartGame()
    {
        var networkManager = CSNetworkManager.instance;

        networkManager.StartHost();
        networkManager.isSinglePlayer = true;
        networkManager.SwitchScenes(currentLevel);
    }
}
