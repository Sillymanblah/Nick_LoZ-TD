using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Level", menuName ="Levels/Level")]
public class LevelSO : ScriptableObject
{
    [Scene] public string sceneName;
    public string levelName;
    public Sprite levelPicture;
    public int levelID;

}
