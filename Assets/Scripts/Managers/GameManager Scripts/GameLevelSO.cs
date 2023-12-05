using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName ="Levels/Game Level")]
public class GameLevelSO : ScriptableObject
{
    public string levelName;
    [Scene] public string sceneName;
    public int levelID;

    [System.Serializable]
    public class UnitDrops
    {
        public UnitSO unit;
        public int dropChance;
    }

    public List<UnitDrops> unitDrops = new List<UnitDrops>();

    public virtual UnitSO GetRandomUnitReward(int waveNumber, bool win)
    {
        if (!win) return null;

        float totalChance = 0;

        foreach (UnitDrops unit in unitDrops)
        {
            totalChance += unit.dropChance;
        }

        float randomValue = Random.value * totalChance;

        foreach (UnitDrops unit in unitDrops)
        {
            if (randomValue <= unit.dropChance)
            {
                return unit.unit;
            }
            randomValue -= unit.dropChance;
        }

        return unitDrops[0].unit;
    }
}
