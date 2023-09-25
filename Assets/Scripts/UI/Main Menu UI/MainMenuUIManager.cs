using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    #region Audio
    [SerializeField] AudioClip buttonClip;
    #endregion

    [SerializeField] GameObject startMenu;
    [SerializeField] GameObject gamemodesMenu;
    [SerializeField] GameObject connectingMenu;
    [SerializeField] GameObject multiplayerLobby;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject inventoryMenu;
    [SerializeField] GameObject singlePlayerMenu;
    [SerializeField] GameObject creditsMenu;
    [SerializeField] GameObject serverListMenu;

    [SerializeField] TextMeshProUGUI joinExceptionText;
    [SerializeField] GameObject joinExceptionPanel;

    GameObject currentMenu;
    public static MainMenuUIManager instance;

    public List<ServerSlotsUI> serverSlotsList = new List<ServerSlotsUI>();
    [SerializeField] Transform serverListParent;

    #region Network Buttons (necessary because the duplicate network manager is removed along with all its buttons OnClick events)

    [SerializeField] Button refreshServersButton;

    #endregion

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>

    public void ButtonSfx()
    {
        Debug.Log("click");
    }
    private void Awake()
    {
        gamemodesMenu.SetActive(false);
        connectingMenu.SetActive(false);
        multiplayerLobby.SetActive(false);
        settingsMenu.SetActive(false);
        inventoryMenu.SetActive(false);
        singlePlayerMenu.SetActive(false);
        creditsMenu.SetActive(false);
        joinExceptionPanel.SetActive(false);
        


        currentMenu = startMenu;
        instance = this;

        
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform transform in serverListParent)
        {
            serverSlotsList.Add(transform.GetComponent<ServerSlotsUI>());
        }

        joinExceptionText.text = string.Empty;

        PlayerPrefs.DeleteKey("Unit0");
        PlayerPrefs.DeleteKey("Unit1");
        PlayerPrefs.DeleteKey("Unit2");

        PlayerPrefs.Save();

        serverListMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // press any menu
        if(currentMenu == startMenu && Input.anyKey == true)
        {
            currentMenu = gamemodesMenu;
            gamemodesMenu.SetActive(true);
            startMenu.SetActive(false);
        }
    }



    public void GoToGamemodes()
    {
        currentMenu = gamemodesMenu;
        gamemodesMenu.SetActive(true);
        startMenu.SetActive(false);
    }

    

    public void GoMultiplayer()
    {
        currentMenu = serverListMenu;
        gamemodesMenu.SetActive(false);
        serverListMenu.SetActive(true);
        NetworkDataBase.instance.RefreshServers();
    }

    public void JoinServer()
    {
        currentMenu = multiplayerLobby;
        
        gamemodesMenu.SetActive(false);
        serverListMenu.SetActive(false);
        connectingMenu.SetActive(true);
        Debug.Log($"starting client");
        NetworkManager.singleton.StartClient();
    }

    public void InvBackButton()
    {
        inventoryMenu.SetActive(false);

        if (currentMenu == multiplayerLobby)
        {
            
            multiplayerLobby.SetActive(true);
        }
        else if (currentMenu == gamemodesMenu)
        {
            gamemodesMenu.SetActive(true);
        }
        else if (currentMenu == singlePlayerMenu)
        {
            singlePlayerMenu.SetActive(true);
        }
    }

    public void FailedToJoinLobby(string reason)
    {
        connectingMenu.SetActive(false);
        serverListMenu.SetActive(true);
        multiplayerLobby.SetActive(false);
        currentMenu = serverListMenu;
        SetExceptionMessage(reason);
    }

    public void LobbyMenu()
    {
        multiplayerLobby.SetActive(true);
        connectingMenu.SetActive(false);
    }

    public void SettingsMenu()
    {
        startMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void SinglePlayerMenu()
    {
        currentMenu = singlePlayerMenu;
        singlePlayerMenu.SetActive(true);
        gamemodesMenu.SetActive(false);
    }

    public void LeaveLobby()
    {
        currentMenu = serverListMenu;
        NetworkManager.singleton.StopClient();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnServerStart()
    {
        currentMenu = multiplayerLobby;
        startMenu.SetActive(false);
        gamemodesMenu.SetActive(false);
        connectingMenu.SetActive(false);
        serverListMenu.SetActive(false);
        multiplayerLobby.SetActive(true);
    }

    void SetExceptionMessage(string reason)
    {
        joinExceptionPanel.SetActive(true);
        joinExceptionText.text = reason;
    }

    public void RefreshServersUI()
    {
        NetworkDataBase.instance.RefreshServers();
    }
}
