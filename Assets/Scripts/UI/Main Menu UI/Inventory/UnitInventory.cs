using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class UnitInventory : MonoBehaviour
{
    [SerializeField] List<UnitSO> units = new List<UnitSO>();
    List<UnitInventorySlot> slots= new List<UnitInventorySlot>();

    [SerializeField] UnitHotBarInventory unitHotBarInventory;

    int nextFreeIndex = 0;

    void Start()
    {
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        { 
            // Gets the child (0) which holds all the inv slot children
            slots.Add(transform.GetChild(0).GetChild(i).GetComponent<UnitInventorySlot>());
            slots[i].AssignSlot(i, this);
        }

        if (units.Count <= 0) return;

        foreach (UnitSO unit in units)
        {
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
            Debug.Log(i);

            if (i >= units.Count) continue;

            slots[i].EquipUnit(units[i]); 
        }
    }

    public bool CheckInventory()
    {
        return unitHotBarInventory.CheckInventory();
    }
}
