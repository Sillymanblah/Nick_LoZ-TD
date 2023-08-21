using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{

    [SerializeField] GameObject startMenu;
    [SerializeField] GameObject gamemodesMenu;
    [SerializeField] GameObject connectingMenu;
    [SerializeField] GameObject multiplayerLobby;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject inventoryMenu;

    [SerializeField] TextMeshProUGUI joinExceptionText;

    GameObject currentMenu;
    public static MainMenuUIManager instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        gamemodesMenu.SetActive(false);
        connectingMenu.SetActive(false);
        multiplayerLobby.SetActive(false);
        settingsMenu.SetActive(false);
        inventoryMenu.SetActive(false);

        currentMenu = startMenu;
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        joinExceptionText.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void GoToGamemodes()
    {
        currentMenu = gamemodesMenu;
        gamemodesMenu.SetActive(true);
        startMenu.SetActive(false);
    }

    

    public void GoMultiplayer()
    {
        currentMenu = multiplayerLobby;
        
        gamemodesMenu.SetActive(false);
        connectingMenu.SetActive(true);
        Debug.Log($"starting client");
        NetworkManager.singleton.StartClient();
    }

    public void InvBackButton()
    {
        if (currentMenu == multiplayerLobby)
        {
            inventoryMenu.SetActive(false);
            multiplayerLobby.SetActive(true);
        }
        else
        {
            inventoryMenu.SetActive(false);
            gamemodesMenu.SetActive(true);
        }
    }

    public void FailedToJoinLobby(string reason)
    {
        GoToGamemodes();
        connectingMenu.SetActive(false);
        currentMenu = gamemodesMenu;

        StopCoroutine(nameof(ExceptionText));
        joinExceptionText.text = "Failed To Join: " + reason;
        StartCoroutine(nameof(ExceptionText));
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

    public void LeaveLobby()
    {
        NetworkManager.singleton.StopClient();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoSinglePlayer()
    {
        // load the second scene from here
    }

    public void OnServerStart(NetworkManager server)
    {
        startMenu.SetActive(false);
        gamemodesMenu.SetActive(false);
        connectingMenu.SetActive(false);
        multiplayerLobby.SetActive(true);
    }

    IEnumerator ExceptionText()
    {
        yield return new WaitForSeconds(5.0f);

        joinExceptionText.text = string.Empty;
    }
}
