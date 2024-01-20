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
        if (serverStats.Version != CSNetworkManager.instance.gameVersion)
        {
            connectButton.interactable = false;
            ingameBooleanStatusText.text = "VersionFail";
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

            return;
        }

        if (serverStats.Uptime > 0)
        {
            onlineStatusText.text = "Online";
            connectButton.interactable = true;
        }
        else
        {
            onlineStatusText.text = "Offline";
            connectButton.interactable = false;
            return;
        }

        if (serverStats.InGame == 1)
        {
            ingameBooleanStatusText.text = "In game: Yes";
            connectButton.interactable = false;
        }
        else
        {
            ingameBooleanStatusText.text = "In game: No"; 
            connectButton.interactable = true;

        }

        serverNameText.text = "Server " + serverStats.ServerID;
        playerCountText.text = serverStats.PlayersOnline + "/4";
        Port = (ushort)serverStats.Port;
    }

    public void Connect()
    {
        CSNetworkManager.instance.SetPort(Port);
        MainMenuUIManager.instance.JoinServer();
    }
}
