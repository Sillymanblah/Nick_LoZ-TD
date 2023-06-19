using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{

    [SerializeField] GameObject startMenu;
    [SerializeField] GameObject gamemodesMenu;
    [SerializeField] GameObject connectingMenu;
    [SerializeField] GameObject multiplayerLobby;
    
    [SerializeField] GameObject backButton;

    GameObject currentMenu;

    public static MainMenuUIManager instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        
        backButton.SetActive(false);
        gamemodesMenu.SetActive(false);
        connectingMenu.SetActive(false);
        multiplayerLobby.SetActive(false);
        currentMenu = startMenu;
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
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
        backButton.SetActive(true);
    }

    public void ReturnButton()
    {
        if (GameObject.ReferenceEquals(currentMenu, gamemodesMenu))
        {
            startMenu.SetActive(true);
            currentMenu = startMenu;

            gamemodesMenu.SetActive(false);
            backButton.SetActive(false);
        }
        else if (GameObject.ReferenceEquals(currentMenu, multiplayerLobby))
        {
            gamemodesMenu.SetActive(true);
            currentMenu = gamemodesMenu;

            multiplayerLobby.SetActive(false);
            backButton.SetActive(true);
            LeaveLobby();
        }
        
    }

    public void GoMultiplayer()
    {
        currentMenu = multiplayerLobby;
        multiplayerLobby.SetActive(true);
        gamemodesMenu.SetActive(false);
        backButton.SetActive(false);
        NetworkManager.singleton.StartClient();
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
        backButton.SetActive(false);
        gamemodesMenu.SetActive(false);
        connectingMenu.SetActive(false);
        multiplayerLobby.SetActive(true);
    }

    
}
