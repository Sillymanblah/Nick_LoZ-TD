using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingame Item", menuName ="Ingame Items/Item")]
public class IngameShopItemSO : ScriptableObject
{
    public GameObject modelPrefab;
    public new string name; 
    public int cost;
    public Color color;

    [TextArea]
    public string description;
}
