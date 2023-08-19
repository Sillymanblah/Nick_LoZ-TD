using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;

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
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            foreach (Transform child in this.gameObject.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (isServer) return;

        

        if (!ingame) return;

        if (!isLocalPlayer) return;

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            ingame = false;
        }

        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        cameraControls = GetComponent<CameraControls>();

        cameraControls.CCStart();

        //playerNameTag.SetActive(false);
    }

    private void Update()
    {
        if (!ingame) return;

        if (!isLocalPlayer) return;
        
        cameraControls.SwitchCameraMovementControl();
        Physics.IgnoreLayerCollision(8, 3, true);
        Physics.IgnoreLayerCollision(8, 6, true);
        Physics.IgnoreLayerCollision(8, 9, true);
    }

    public void DisconnectClient()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        NetworkClient.Disconnect();
    }

    #region Username shit

    [SerializeField] TextMeshProUGUI UsernameText = null;

    [ClientRpc]
    public void SetUserNameNameTag(string newName)
    {
        UsernameText.text = newName;
    }

    /*public void AdjustHeadHeight()
    {
        head.localPosition = new Vector3(0, controller.height - 1, 0);
    }*/

    #endregion

    #region Menu Lobby shit

    public void OnStartLobby()
    {
        ingame = false;
        foreach (Transform child in this.gameObject.transform)
        {
            Debug.Log($"Wtff");
            child.gameObject.SetActive(false);
        }
    }

    [TargetRpc]
    public void OnStartGame(NetworkConnectionToClient conn)
    {
        ingame = true;
        foreach (Transform child in this.gameObject.transform)
        {
            
            child.gameObject.SetActive(true);
        }

        SetCameraPOV();
    }

    #endregion
}
