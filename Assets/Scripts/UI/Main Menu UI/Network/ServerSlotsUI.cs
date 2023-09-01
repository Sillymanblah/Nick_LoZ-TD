using System;
using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.MultipleMatch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerSlotsUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI serverNameText;
    [SerializeField] TextMeshProUGUI playerCountText;
    [SerializeField] TextMeshProUGUI ingameBooleanStatusText;
    [SerializeField] Button connectButton;
    ushort Port;

    // Start is called before the first frame update
    void Start()
    {
        SetServerStats("0", 0, false, 0000);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetServerStats(string serverID, int playerCount, bool ingame, ushort port)
    {
        if (ingame)
        {
            ingameBooleanStatusText.text = "In game: Yes";
            connectButton.interactable = true;
        }
        else
        {
            ingameBooleanStatusText.text = "In game: No";
            connectButton.interactable = false;

        }

        serverNameText.text = "Server " + serverID;
        playerCountText.text = playerCountText + "/4";
        
        Port = port;
    }

    public void Connect()
    {
        CSNetworkManager.instance.SetPort(Port);
        MainMenuUIManager.instance.JoinServer();
    }
}
