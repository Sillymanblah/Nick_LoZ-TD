using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitMultiplierType
{
    Attack,
    Range,
    Cooldown
}
[CreateAssetMenu(fileName = "New Ingame Item", menuName ="Ingame Items/Stats Item")]

public class StatsShopItemSO : IngameShopItemSO
{

    [Header("Unit Stats Effects")]
    public UnitMultiplierType unitMultiplierType;
    [Space]
    public float valueMultiplier;

    public override void ItemAbility(Unit unit)
    {
        base.ItemAbility();
        unit.ApplyStatsMultiplier(unitMultiplierType, valueMultiplier);
    }
}
