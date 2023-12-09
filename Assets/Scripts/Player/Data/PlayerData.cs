using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public List<string> unitNamesInventory = new List<string>();

    public PlayerData(List<string> unitInventory)
    {
        unitNamesInventory = new List<string>();

        for (int i = 0; i < unitInventory.Count; i++)
        {
            unitNamesInventory.Add(unitInventory[i]);
        }
    }
}
