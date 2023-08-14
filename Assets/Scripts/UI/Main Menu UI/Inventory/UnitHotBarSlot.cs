using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHotBarSlot : MonoBehaviour
{
    [SerializeField] UnitSO currentUnit;

    [SerializeField] Image unitImage;
    int index;
    UnitHotBarInventory unitInventory;

    public void AssignSlot(int newIndex, UnitHotBarInventory unitInventory)
    {
        index = newIndex;
        this.unitInventory = unitInventory;
    }

    public bool Occupied()
    {
        return currentUnit != null;
    }

    // UI BUTTON
    public void DeEquipUnit()
    {
        if (currentUnit is null) return; 

        unitImage.color = new Color(0, 0, 0, 0);
        unitInventory.RemoveUnit(currentUnit, index);
    }

    // Have to cross reference this between UnitHotBarInventory.cs because the code above would execute the code below after its done all its removing shit which bugs it out;
    public void ThisShitSucks()
    {
        currentUnit = null;
    }

    public void ResetSlot()
    {
        currentUnit = null;
        unitImage.color = new Color(0, 0, 0, 0);
        unitImage.sprite = null;
    }

    public void EquipUnit(UnitSO unit)
    {
        currentUnit = unit;
        unitImage.sprite = currentUnit.icon;
        unitImage.color = new Color(1,1,1,1);
    }
}
