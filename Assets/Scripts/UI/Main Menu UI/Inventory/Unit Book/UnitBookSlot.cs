using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBookSlot : MonoBehaviour
{
    UnitSO currentUnit;
    [SerializeField] UnitInfoCard unitCard; 
    [SerializeField] Image unitIcon;

    bool achieved = false;
    int index; // Used for if we unlock units in the main menu, 
               // we have a way to POINT to which slot we want to affect based off the index of the unit in the list

    // Start is called before the first frame update
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        unitIcon.color = new Color(0, 0, 0, 0);
    }

    public void AssignSlot(int newIndex, UnitSO newUnit, bool achieved)
    {
        this.achieved = achieved;

        if (achieved)
            unitIcon.color = new Color(1, 1, 1, 1);
        else
            unitIcon.color = new Color(0, 0, 0, 1);

        index = newIndex;
        currentUnit = newUnit;
        unitIcon.sprite = currentUnit.icon;
    }

    public void ApplyUnitCardStats()
    {
        if (currentUnit == null) return;

        unitCard.AssignCard(currentUnit, achieved);
    }
}
