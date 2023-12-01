using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyAuth : MonoBehaviour
{
    public new string name;
    [SerializeField] TMP_InputField inputFieldText;
    [SerializeField] List<UnitSO> myUnitInventory = new List<UnitSO>();
    PlayerNetworkInfo player;


    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            inputFieldText.text = PlayerPrefs.GetString("PlayerName");
            SetName(PlayerPrefs.GetString("PlayerName"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // network sync it
    public void SyncData(PlayerNetworkInfo player)
    {
        this.player = player;
        player.SetDisplayName(name);
        TransmitUnitInventory();
    }

    public void SetName(string newName)
    {
        if (newName.Length > 24) 
        {
            inputFieldText.text = string.Empty;
        }

        name = newName;
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
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

        // If player was null then that means we arent even connected to a game server
        if (player != null)
            TransmitUnitInventory();
    }

    void TransmitUnitInventory()
    {
        List<string> transmittedUnitData = new List<string>();

        foreach (UnitSO unit in myUnitInventory)
        {
            transmittedUnitData.Add(unit.uniqueName);
        }

        player.UpdateUnitInventory(transmittedUnitData);
    }
}
