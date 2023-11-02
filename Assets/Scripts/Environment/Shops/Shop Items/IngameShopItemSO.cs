using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingame Item", menuName ="Ingame Items/Item")]
public class IngameShopItemSO : UnitSOFinder<IngameShopItemSO>
{
    public GameObject modelPrefab;
    public new string name; 
    public int cost;
    public Color color;

    [TextArea]
    public string description;

    [Header("Transform Properties")]
    public Vector3 newPosition;
    public Vector3 newRotation;
    public Vector3 newScale;
    
    public virtual void ItemAbility()
    {
        Debug.Log($"Ingame Shop Item ");
    }

    public virtual void ItemAbility(Unit unit)
    {
        Debug.Log($"Ingame Shop Item ");
    }
}
