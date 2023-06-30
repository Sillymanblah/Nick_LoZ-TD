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
            Debug.Log($"Loading next scene...");
            ToggleReadyButton(false);
            CSNetworkManager.instance.SwitchScenes(playerTest);
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
    }

    [ClientRpc]
    void ToggleReadyButton(bool active)
    {
        readyButtonText.transform.parent.GetComponent<Button>().interactable = active;
    }
}
