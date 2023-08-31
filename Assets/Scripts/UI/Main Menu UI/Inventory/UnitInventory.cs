using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class UnitInventory : MonoBehaviour
{
    [SerializeField] public List<UnitSO> totalAchievedUnits = new List<UnitSO>();
    [SerializeField] List<UnitSO> units = new List<UnitSO>();
    List<UnitInventorySlot> slots= new List<UnitInventorySlot>();

    [SerializeField] UnitHotBarInventory unitHotBarInventory;

    int nextFreeIndex = 0;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        
    }
    void Start()
    {
        LoadInventoryData();

        if (PlayerPrefs.HasKey("RewardUnit"))
        {
            UnitSO rewardUnit = UnitSO.Get(PlayerPrefs.GetString("RewardUnit"));

            if (rewardUnit != null) 
            {
                if (!totalAchievedUnits.Contains(rewardUnit)) 
                {
                    totalAchievedUnits.Add(rewardUnit);
                }
            }
            PlayerPrefs.DeleteKey("RewardUnit");
            PlayerPrefs.Save();
        }

        SaveData.SaveInventory(this);

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        { 
            // Gets the child (0) which holds all the inv slot children
            slots.Add(transform.GetChild(0).GetChild(i).GetComponent<UnitInventorySlot>());
            slots[i].AssignSlot(i, this);
        }

        foreach (UnitSO unit in totalAchievedUnits)
        {
            units.Add(unit);
            SetUnitsOnStart(unit);
        }
    }

    void SetUnitsOnStart(UnitSO unit)
    {
        slots[nextFreeIndex].EquipUnit(unit);

        nextFreeIndex++;
    }

    public void AddUnit(UnitSO unit) 
    {
        if (units.Contains(unit))
        {
            Debug.Log($"Unit is already achieved.");
            return;
        }

        units.Add(unit);
        slots[nextFreeIndex].EquipUnit(unit);

        nextFreeIndex++;
    }

    public void RemoveUnit(UnitSO unit, int slotIndex) 
    {
        unitHotBarInventory.AddUnit(unit);

        slots[slotIndex].ThisShitSucks();

        units.Remove(unit);
        OrganizeUnitSlots(slotIndex);

        nextFreeIndex = units.Count;
    }

    void OrganizeUnitSlots(int oldSlotIndex)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ResetSlot();

            if (i >= units.Count) continue;

            slots[i].EquipUnit(units[i]); 
        }
    }

    public bool CheckInventory()
    {
        return unitHotBarInventory.CheckInventory();
    }

    void LoadInventoryData()
    {
        PlayerData data = SaveData.LoadInventory();

        if (data == null) return;

        Debug.LogWarning($"Loading saved inventory data");

        foreach (string unitName in data.unitNamesInventory)
        {
            if (totalAchievedUnits.Contains(UnitSO.Get(unitName))) continue;

            totalAchievedUnits.Add(UnitSO.Get(unitName));
        }
    }
}
