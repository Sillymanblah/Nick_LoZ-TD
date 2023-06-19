using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;
using TMPro;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] Transform body;
    [SerializeField] Transform head;
    [SerializeField] Transform cameraHead; // USE FOR CAMERA!!!!
    [SerializeField] CharacterController controller;
    public Transform HeadPos() { return head; }
    [SerializeField] GameObject playerNameTag;
    public PlayerMovement playerMovement;
    CameraControls cameraControls;
    public bool ingame = false;
    
    public void SetCameraPOV()
    {
        var camBrain = FindObjectOfType<CinemachineFreeLook>();

        camBrain.m_LookAt = cameraHead;
        camBrain.m_Follow = body;
        
        for (int i = 0; i < 3; i++)
        {
            camBrain.GetRig(i).GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 0;
            camBrain.GetRig(i).GetCinemachineComponent<CinemachineTransposer>().m_YDamping = 0;
            camBrain.GetRig(i).GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 0;
        }
    }

    private void Start()
    {
        if (!ingame) return;

        if (!isLocalPlayer) return;

        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        cameraControls = GetComponent<CameraControls>();

        cameraControls.CCStart();

        SetNameCmd();
        //playerNameTag.SetActive(false);
    }

    private void Update()
    {
        if (!ingame) return;

        if (!isLocalPlayer) return;

        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            NetworkClient.Disconnect();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }*/

        
        cameraControls.SwitchCameraMovementControl();
        Physics.IgnoreLayerCollision(8, 3, true);
        Physics.IgnoreLayerCollision(8, 6, true);
        Physics.IgnoreLayerCollision(8, 9, true);

        head.localPosition = new Vector3(0, controller.height - 1.04f, 0);
    }

    #region Username shit

    [SerializeField] TextMeshProUGUI UsernameText = null;

    // Lets us be able to use this method for when the variable has updated
    [SyncVar(hook = nameof(HandleUsernameUpdated))]
    [SerializeField] string Username = "Missing Name";

    [Command] // Makes the username var = to the new username
    public void SetDisplayName(string newName)
    {
        Username = newName;
    }

    // A way for the server to know if a variable has updated
    // Updates the username UI text to the UPDATED username var
    void HandleUsernameUpdated(string oldUsername, string newUsername)
    {
        UsernameText.text = newUsername;
    }

    void SetNameCmd()
    {
        //SetDisplayName(FindObjectOfType<TMP_InputField>().text);
    }

    public void AdjustHeadHeight()
    {
        head.localPosition = new Vector3(0, controller.height - 1, 0);
    }

    #endregion

    #region Menu Lobby shit

    [TargetRpc]
    public void OnStartLobby()
    {
        foreach (Transform child in this.gameObject.transform)
        {
            ingame = false;
            child.gameObject.SetActive(false);
        }
    }

    [TargetRpc]
    public void OnStartGame(NetworkConnectionToClient conn)
    {
        foreach (Transform child in this.gameObject.transform)
        {
            ingame = true;
            child.gameObject.SetActive(true);
        }

        SetCameraPOV();
    }

    #endregion
}
