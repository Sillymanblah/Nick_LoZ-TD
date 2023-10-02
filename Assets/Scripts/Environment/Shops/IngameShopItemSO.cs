using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingame Item", menuName ="Ingame Items/Item")]
public class IngameShopItemSO : ScriptableObject
{
    public GameObject modelPrefab;
    public int cost;

    [TextArea]
    public string description;
}
