using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class UnitInventory : MonoBehaviour
{
    [SerializeField] public List<UnitSO> totalAchievedUnits = new List<UnitSO>();
    [SerializeField] List<UnitSO> units = new List<UnitSO>();
    List<UnitInventorySlot> slots= new List<UnitInventorySlot>();
    [SerializeField] Transform slotsParent;

    [SerializeField] UnitHotBarInventory unitHotBarInventory;

    int nextFreeIndex = 0;

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

        

        List<string> totalAchievedUnitsNames = new List<string>();

        foreach (UnitSO unit in totalAchievedUnits)
        {
            units.Add(unit);
            totalAchievedUnitsNames.Add(unit.uniqueName);
        }

        SaveData.SaveInventory(totalAchievedUnitsNames);


        for (int i = 0; i < 3; i++)
        {
            string nextUnitName = PlayerPrefs.GetString($"UnitEquipped{i}");
        
            if (nextUnitName != string.Empty)
            {
                if (units.Contains(UnitSO.Get(nextUnitName)))
                {
                    units.Remove(UnitSO.Get(nextUnitName));
                }
            } else break;
        }

        int uiSlotCounter = 0;

        if (units.Count > 12)
        {
            uiSlotCounter = Mathf.CeilToInt(units.Count / 3) * 3;
            uiSlotCounter -= 12;

            for (int i = 0; i < uiSlotCounter; i++) 
            {
                Instantiate(slotsParent.GetChild(0).gameObject, slotsParent);
            }
        }

        for (int i = 0; i < slotsParent.childCount; i++)
        { 
            // Gets the child (0) which holds all the inv slot children
            slots.Add(slotsParent.GetChild(i).GetComponent<UnitInventorySlot>());
            slots[i].AssignSlot(i, this);
        }

        foreach (UnitSO unit in units)
        {
            SetUnitsOnStart(unit);
        }

        this.transform.parent.gameObject.SetActive(false);
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
