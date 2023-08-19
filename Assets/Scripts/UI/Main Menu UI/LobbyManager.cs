using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

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
        base.OnStartServer();
        this.gameObject.SetActive(true);
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        /*if (CSNetworkManager.instance.numPlayers < 2)
            readyButton.interactable = false;
        else
            readyButton.interactable = true;*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        instance = this;
        CmdUpdatePlayerCount();
    }

    #region Server

    [ClientRpc]
    public void SetPlayerListUI(string name, int index)
    {
        lobbyParent.GetChild(index).GetComponent<PlayerSlotUI>().AssignPlayer(name);
    }

    #endregion

    public void ReadyButton()
    {
        player.ReadyUpButton();
    }

    [Server]
    public void PlayersAreReady(bool ready)
    {
        if (ready == true) playerReadyCount++;
        else playerReadyCount--;

        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.players.Count);

        if (playerReadyCount == CSNetworkManager.instance.players.Count)
        {
            Debug.Log($"Loading next scene... " + LevelSelectorUI.instance.currentLevel);
            ToggleReadyButton(false);
            CSNetworkManager.instance.LoadScene(LevelSelectorUI.instance.currentLevel);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdUpdatePlayerCount()
    {
        RpcUpdatePlayerCount(playerReadyCount, CSNetworkManager.instance.players.Count);
    }

    [ClientRpc]
    void RpcUpdatePlayerCount(int playerReady, int maxPlayerCount)
    {
        readyButtonText.text = $"READY {playerReady}/{maxPlayerCount}";

        //if (maxPlayerCount > 1) readyButton.interactable = true;
        //else if (maxPlayerCount < 2) readyButton.interactable = false;
    }

    [ClientRpc]
    void ToggleReadyButton(bool active)
    {
        readyButtonText.transform.parent.GetComponent<Button>().interactable = active;
    }
}
