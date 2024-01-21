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

    AudioSource audioSource;
    [Space]
    [SerializeField] GameObject selectMenu;
    [SerializeField] GameObject connectingMenu;
    [SerializeField] GameObject multiplayerLobby;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject inventoryMenu;
    [SerializeField] GameObject unitBookMenu;
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

    [Header("Class References")]
    [SerializeField] MainMenuMusic mainMenuMusic;


    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>

    
    private void Awake()
    {
        connectingMenu.SetActive(false);
        multiplayerLobby.SetActive(false);
        settingsMenu.SetActive(false);
        //inventoryMenu.SetActive(false);
        singlePlayerMenu.SetActive(false);
        creditsMenu.SetActive(false);
        joinExceptionPanel.SetActive(false);
        selectMenu.SetActive(true);

        currentMenu = selectMenu;
        instance = this;

        
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        mainMenuMusic.PlayMenuMusic();

        

        

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

    }

    public void GoMultiplayer()
    {
        currentMenu = serverListMenu;
        selectMenu.SetActive(false);
        serverListMenu.SetActive(true);
        NetworkDataBase.instance.RefreshServers();
    }

    public void JoinServer()
    {
        currentMenu = multiplayerLobby;
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
        else if (currentMenu == selectMenu)
        {
            selectMenu.SetActive(true);
        }
        else if (currentMenu == singlePlayerMenu)
        {
            singlePlayerMenu.SetActive(true);
        }
    }

    public void FailedToJoinLobby(string reason)
    {
        // Eventually, make it to where it just disconnects the player BUT if player is still in inv or unit book, dont let them exit it
        // Instead, display a brief message on the screen and when they do exit out of those menus, itll take them to the server list
        // Use C# events IAW this feature
        connectingMenu.SetActive(false);
        serverListMenu.SetActive(true);
        multiplayerLobby.SetActive(false);
        inventoryMenu.SetActive(false);
        unitBookMenu.SetActive(false);
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
        selectMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void SinglePlayerMenu()
    {
        currentMenu = singlePlayerMenu;
        singlePlayerMenu.SetActive(true);
        selectMenu.SetActive(false);
    }

    public void SelectMenu()
    {
        currentMenu = selectMenu;
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
        selectMenu.SetActive(false);
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

    public void ButtonSfx()
    {
        audioSource.PlayOneShot(buttonClip, PlayerPrefs.GetFloat("SoundFXVol"));
    }

    
}
