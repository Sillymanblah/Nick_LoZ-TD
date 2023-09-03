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
    [SerializeField] TextMeshProUGUI onlineStatusText;

    [SerializeField] Button connectButton;
    ushort Port;

    // Start is called before the first frame update
    void Start()
    {
        //SetServerOffline();
        connectButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetServerStats(NetworkDataBase.ServerStats serverStats)
    {
        if (serverStats.InGame == 1)
        {
            ingameBooleanStatusText.text = "In game: Yes";
            connectButton.interactable = false;
        }
        else
        {
            ingameBooleanStatusText.text = "In game: No";

            if (serverStats.Uptime > 0)
            {
                onlineStatusText.text = "Online";
                connectButton.interactable = true;
            }
            else
            {
                onlineStatusText.text = "Offline";
                connectButton.interactable = false;

            }
        }

        serverNameText.text = "Server " + serverStats.ServerID;
        playerCountText.text = serverStats.PlayersOnline + "/4";
        

        
        
        Port = (ushort)serverStats.Port;
    }

    void SetServerOffline()
    {
        connectButton.interactable = false;
        onlineStatusText.text = "Offline";
    }

    public void Connect()
    {
        CSNetworkManager.instance.SetPort(Port);
        MainMenuUIManager.instance.JoinServer();
    }
}
