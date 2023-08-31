using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyAuth : MonoBehaviour
{
    public new string name;
    [SerializeField] List<UnitSO> myUnitInventory = new List<UnitSO>();
    PlayerNetworkInfo player;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SyncData(PlayerNetworkInfo player)
    {
        this.player = player;
        player.SetDisplayName(name);
        UpdateUnitInventory(myUnitInventory);
    }

    public void SetName(string newName)
    {
        name = newName;
    }

    public void UpdateUnitInventory(List<UnitSO> newUnits)
    {
        myUnitInventory = newUnits;

        if (myUnitInventory.Count > 3)
        {
            myUnitInventory.RemoveRange(3, 10);
        }

        for (int i = 0; i < 3; i++)
        {
            if (i >= newUnits.Count)
            {
                PlayerPrefs.DeleteKey($"Unit{i}");
                continue;
            }

            PlayerPrefs.SetString($"Unit{i}", myUnitInventory[i].uniqueName);
        }
        PlayerPrefs.Save();

        if (player == null) return;

        List<string> transmittedUnitData = new List<string>();

        foreach (UnitSO unit in myUnitInventory)
        {
            transmittedUnitData.Add(unit.uniqueName);
        }

        
        
        player.UpdateUnitInventory(transmittedUnitData);
    }
}
