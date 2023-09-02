using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager instance;
    [SerializeField] Transform lobbyParent;
    [SerializeField] TextMeshProUGUI readyButtonText;
    [SerializeField] Button readyButton;
    public PlayerNetworkInfo player;
    public int playerReadyCount;
    [SerializeField] [Scene] string playerTest;


    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        
    }

    public override void OnStartServer()
    {
        instance = this;
        base.OnStartServer();
        this.gameObject.SetActive(true);

        if (!isServerOnly) return;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!isServer)
        {
            SetUpLobbyScene();

            if (CSNetworkManager.instance.noRestrictions) return;

            if (CSNetworkManager.instance.numPlayers < 2)
                readyButton.interactable = false;
            else
                readyButton.interactable = true;

            return;
        }
        
        
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        instance = this;
        CmdUpdatePlayerCount();
    }

    #region Server

    [ClientRpc]
    public void SetPlayerListUI(List<string> playerNames)
    {
        for (int i = 0; i < 4; i++)
        {
            PlayerSlotUI playerSlotUI = lobbyParent.GetChild(i).GetComponent<PlayerSlotUI>();

            if (playerNames.Count == 0)
            {
                playerSlotUI.AssignPlayer(string.Empty);
                continue;
            }

            if (i + 1 > playerNames.Count)
            {
                playerSlotUI.AssignPlayer(string.Empty);
                continue;
            }

            playerSlotUI.AssignPlayer(playerNames[i]);
        }
    }

    #endregion

    public void SetUpLobbyScene()
    {
        MainMenuUIManager.instance.OnServerStart();
    }

    public void ReadyButton()
    {
        player.ReadyUpButton();
    }

    [Server]
    public void PlayersAreReady(bool ready)
    {
        if (!CSNetworkManager.instance.noRestrictions)
        {
            if (CSNetworkManager.instance.players.Count < 2)
            {
                Debug.LogWarning($"Cannot start with less than 2 people");
                return;
            }
        }

        if (ready == true) playerReadyCount++;
        else playerReadyCount--;

        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.players.Count);

        if (playerReadyCount == CSNetworkManager.instance.players.Count)
        {
            Debug.Log($"Loading next scene... " + NetLevelSelectorUI.instance.currentLevel);
            ToggleReadyButton(false);
            CSNetworkManager.instance.SwitchScenes(NetLevelSelectorUI.instance.currentLevel);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdUpdatePlayerCount()
    {
        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.players.Count);
    }

    [ClientRpc]
    public void RpcUpdatePlayerCount(int playerReady, int maxPlayerCount)
    {
        readyButtonText.text = $"READY {playerReady}/{maxPlayerCount}";

        if (CSNetworkManager.instance.noRestrictions) return;

        if (maxPlayerCount > 1) readyButton.interactable = true;
        else if (maxPlayerCount < 2) readyButton.interactable = false;
    }

    [ClientRpc]
    void ToggleReadyButton(bool active)
    {
        readyButtonText.transform.parent.GetComponent<Button>().interactable = active;
    }


}
