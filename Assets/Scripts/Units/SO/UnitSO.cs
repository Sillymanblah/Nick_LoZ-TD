using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName ="Units/Unit")]
public class UnitSO : ScriptableObject
{
    public new string name;
    public int unitID;
    public Sprite icon;
    public GameObject prefab;
    public int chanceToMiss;
    [SerializeField] public GridType gridType; 

    [Header("Level 1")]
    [SerializeField] int cost1;
    [SerializeField] float attack1;
    [SerializeField] float cooldown1;
    [SerializeField] float range1;
    [Space]
    [SerializeField] float DPS1;
    [SerializeField] float rangeDPS1;

    [Header("Level 2")]
    [SerializeField] int cost2;
    [SerializeField] float attack2;
    [SerializeField] float cooldown2;
    [SerializeField] float range2;
    [Space]
    [SerializeField] float DPS2;
    [SerializeField] float rangeDPS2;

    [Header("Level 3")]
    [SerializeField] int cost3;
    [SerializeField] float attack3;
    [SerializeField] float cooldown3;
    [SerializeField] float range3;
    [Space]
    [SerializeField] float DPS3;
    [SerializeField] float rangeDPS3;

    [Header("Level 4")]
    [SerializeField] int cost4;
    [SerializeField] float attack4;
    [SerializeField] float cooldown4;
    [SerializeField] float range4;
    [Space]
    [SerializeField] float DPS4;
    [SerializeField] float rangeDPS4;

    [Header("Level 5")]
    [SerializeField] int cost5;
    [SerializeField] float attack5;
    [SerializeField] float cooldown5;
    [SerializeField] float range5;
    [Space]
    [SerializeField] float DPS5;
    [SerializeField] float rangeDPS5;

    public float CurrentAttack(int level)
    {
        switch(level)
        {
            case 1:
                return attack1;
                
            case 2:
                return attack2;
                
            case 3:
                return attack3;
                
            case 4:
                return attack4;
                
            case 5:
                return attack5;
            
            default:
                return 0;
        }
    }

    public float CurrentCooldown(int level)
    {
        switch(level)
        {
            case 1:
                return cooldown1;
                
            case 2:
                return cooldown2;
                
            case 3:
                return cooldown3;
                
            case 4:
                return cooldown4;
                
            case 5:
                return cooldown5;
            
            default:
                return 0;
        }
    }

    public float CurrentRange(int level)
    {
        switch(level)
        {
            case 0:
                return range1;

            case 1:
                return range1;
                
            case 2:
                return range2;
                
            case 3:
                return range3;
                
            case 4:
                return range4;
                
            case 5:
                return range5;
            
            default:
                return 0;
        }
    }

    public int NextCost(int level)
    {
        switch(level)
        {
            case 0:
                return cost1;

            case 1:
                return cost1;
                
            case 2:
                return cost2;
                
            case 3:
                return cost3;
                
            case 4:
                return cost4;
                
            case 5:
                return cost5;
            
            default:
                return 0;
        }
    }

    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    private void OnValidate()
    {
        DPS1 = attack1 / cooldown1;
        DPS2 = attack2 / cooldown2;
        DPS3 = attack3 / cooldown3;
        DPS4 = attack4 / cooldown4;
        DPS5 = attack5 / cooldown5;

        rangeDPS1 = Mathf.FloorToInt(DPS1 * range1);
        rangeDPS2 = Mathf.FloorToInt(DPS2 * range2);
        rangeDPS3 = Mathf.FloorToInt(DPS3 * range3);
        rangeDPS4 = Mathf.FloorToInt(DPS4 * range4);
        rangeDPS5 = Mathf.FloorToInt(DPS5 * range5);
    }
}
