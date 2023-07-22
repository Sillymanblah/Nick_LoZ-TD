using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class LevelSelectorUI : NetworkBehaviour
{
    [SerializeField] List<LevelSO> levels = new List<LevelSO>();
    [SyncVar] int levelNumSelector = 0;

    [SerializeField] Image levelImage;
    [SerializeField] TextMeshProUGUI levelNameText;

    // Start is called before the first frame update
    void Start()
    {
        if (!isServer) return;

        levelNumSelector = 0;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        UponJoining();
    }

    [Command(requiresAuthority = false)]
    public void LeftButton()
    {
        levelNumSelector--;
        if (levelNumSelector < 0) levelNumSelector = levels.Count - 1;

        SetLevel();
    }

    [Command(requiresAuthority = false)]
    public void RightButton()
    {
        levelNumSelector++;
        if (levelNumSelector >= levels.Count) levelNumSelector = 0;

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
