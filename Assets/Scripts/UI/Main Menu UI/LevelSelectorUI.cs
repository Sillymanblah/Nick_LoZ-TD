using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectorUI : MonoBehaviour
{
    [SerializeField] List<LevelSO> levels = new List<LevelSO>();
    int levelNumSelector = 0;

    [SerializeField] Image levelImage;
    [SerializeField] TextMeshProUGUI levelNameText;

    // Start is called before the first frame update
    void Start()
    {
        SetLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LeftButton()
    {
        levelNumSelector--;
        if (levelNumSelector < 0) levelNumSelector = levels.Count - 1;

        SetLevel();
    }

    public void RightButton()
    {
        levelNumSelector++;
        if (levelNumSelector >= levels.Count) levelNumSelector = 0;

        SetLevel();
    }

    void SetLevel()
    {
        LevelSO selectedLevel = levels[levelNumSelector];
        levelImage.sprite = selectedLevel.levelPicture;
        levelNameText.text = selectedLevel.levelName;
    }
}
