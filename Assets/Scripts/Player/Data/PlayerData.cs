using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string[] unitNamesInventory;

    public PlayerData(UnitInventory unitInventory)
    {
        unitNamesInventory = new string[unitInventory.totalAchievedUnits.Count];

        for (int i = 0; i < unitInventory.totalAchievedUnits.Count; i++)
        {
            unitNamesInventory[i] = unitInventory.totalAchievedUnits[i].uniqueName;
        }
    }
}
