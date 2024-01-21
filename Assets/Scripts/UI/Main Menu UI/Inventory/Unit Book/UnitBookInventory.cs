using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBookInventory : MonoBehaviour
{
    [SerializeField] List<UnitSO> allGameUnits = new List<UnitSO>();
    List<UnitBookSlot> unitBookSlots = new List<UnitBookSlot>();

    [SerializeField] Transform slotParent;

    public void UploadUnits(List<UnitSO> unitsList)
    {
        Debug.Log($"Started uploading units to book");

        for (int i = 0; i < allGameUnits.Count; i++)
        {
            unitBookSlots.Add(slotParent.GetChild(i).GetComponent<UnitBookSlot>());

            bool achievedUnit = unitsList.Contains(allGameUnits[i]); // Cant be other way around because then unitsList may contain an OutOfBounds exception

            unitBookSlots[i].AssignSlot(i, allGameUnits[i], achievedUnit);
        }

        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
