using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingame Item", menuName ="Ingame Items/Stats Item")]

public class StatsShopItemSO : IngameShopItemSO
{
    [Header("Unit Stats Effects")]
    public int damageMultiplier;
    public int rangeMultiplier;
    public int cooldownMultiplier;

}
