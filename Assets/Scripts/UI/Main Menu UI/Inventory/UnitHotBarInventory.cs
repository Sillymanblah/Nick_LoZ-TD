using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHotBarInventory : MonoBehaviour
{

    // PlayerPref names:
    // - UnitEquipped0
    // - UnitEquipped1
    // - UnitEquipped2


    [SerializeField] List<UnitSO> units = new List<UnitSO>();
    List<UnitHotBarSlot> slots= new List<UnitHotBarSlot>();

    int nextFreeIndex = 0;
    [SerializeField] int maxUnits;

    [SerializeField] UnitInventory unitInventory;
    [SerializeField] LobbyAuth lobbyAuth;

    private void Awake()
    {
        // We are going to go ahead and create the variables upon first ever launch of game
        if (!PlayerPrefs.HasKey("UnitEquipped0"))
        {
            PlayerPrefs.SetString("UnitEquipped0", "Deku Baba (ground)");
            PlayerPrefs.SetString("UnitEquipped1", "Deku Baba (flying)");
            PlayerPrefs.SetString("UnitEquipped2", string.Empty);

            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        units.Clear();

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        { 
            // Gets the child (0) which holds all the inv slot children
            slots.Add(transform.GetChild(0).GetChild(i).GetComponent<UnitHotBarSlot>());
            slots[i].AssignSlot(i, this);
        }

        for (int i = 0; i < 3; i++)
        {
            string nextUnitNamee = PlayerPrefs.GetString($"UnitEquipped{i}");

            if (nextUnitNamee != string.Empty)
            {
                AddUnit(UnitSO.Get(PlayerPrefs.GetString($"UnitEquipped{i}")));
            }
            else break;
        }
    }

    public bool CheckInventory()
    {
        return units.Count < 3;
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

        PlayerPrefs.SetString($"UnitEquipped{nextFreeIndex}", unit.uniqueName);
        PlayerPrefs.Save();

        lobbyAuth.UpdateUnitInventory(units);

        nextFreeIndex++;
    }

    public void RemoveUnit(UnitSO unit, int slotIndex) 
    {
        unitInventory.AddUnit(unit);

        slots[slotIndex].ThisShitSucks();
        PlayerPrefs.SetString($"UnitEquipped{slotIndex}", string.Empty);

        units.Remove(unit);
        OrganizeUnitSlots();

        nextFreeIndex = units.Count;
        lobbyAuth.UpdateUnitInventory(units);
    }

    void OrganizeUnitSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ResetSlot();


            // if i, the loop index, was outside of the amount of units in the list, that meant that i is currently on a slot that doesnt contain a unit and UP
            if (i >= units.Count) 
            {
                PlayerPrefs.SetString($"UnitEquipped{i}", string.Empty);

                continue;
            }

            PlayerPrefs.SetString($"UnitEquipped{i}", units[i].uniqueName);

            slots[i].EquipUnit(units[i]); 
        }

        for (int i = 0; i < 3; i++)
        {
            Debug.Log(PlayerPrefs.GetString($"UnitEquipped{i}"));
        }
        PlayerPrefs.Save();
    }
}
