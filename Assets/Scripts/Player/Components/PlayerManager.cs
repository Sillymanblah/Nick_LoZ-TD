using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using JetBrains.Annotations;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] Transform body;
    [SerializeField] Transform head;
    [SerializeField] Transform cameraHead; // USE FOR CAMERA!!!!
    [SerializeField] CharacterController controller;
    public Transform HeadPos() { return head; }
    [SerializeField] GameObject playerNameTag;
    public PlayerMovement playerMovement;
    PlayerUnitManager playerUnitManager;
    public PlayerStateManager playerStateManager;
    CameraControls cameraControls;
    public bool ingame = false;
    [Space]
    [Scene] [SerializeField] string mainMenuScene;

    [Header("Tunic Colors")]
    [SerializeField] List<Material> tunicColors = new List<Material>();

    [SyncVar(hook = nameof(HandleColorTunicChange))]
    [SerializeField] int tunicColorIndex;
    [SerializeField] List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

    #region 

    [Header("Cameras")]
    [SerializeField] List<CinemachineVirtualCameraBase> cameras = new List<CinemachineVirtualCameraBase>();
    public CinemachineFreeLook thirdPovCam;

    CinemachineVirtualCameraBase startCamera;
    CinemachineVirtualCameraBase currentCamera;

    #endregion
    
    public void SetCameraPOV()
    {
        var camBrain = FindObjectOfType<CinemachineFreeLook>();

        camBrain.m_LookAt = cameraHead;
        camBrain.m_Follow = body;

        #region Cinemachine switching cameras

        thirdPovCam = camBrain;

        startCamera = thirdPovCam;
        currentCamera = startCamera;
        cameras.Add(thirdPovCam);

        #endregion
        
        for (int i = 0; i < 3; i++)
        {
            camBrain.GetRig(i).GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 0;
            camBrain.GetRig(i).GetCinemachineComponent<CinemachineTransposer>().m_YDamping = 0;
            camBrain.GetRig(i).GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 0;
        }
    }
    private void Start()
    {
        if (SceneManager.GetActiveScene().path == mainMenuScene)
        {
            foreach (Transform child in this.gameObject.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (isServerOnly) return;

        if (isLocalPlayer)
        {
            playerNameTag.SetActive(false);

            for (int i = 0; i < cameras.Count; i++)
            {
                if (cameras[i] == currentCamera)
                {
                    cameras[i].Priority = 20;
                }
                else
                {
                    cameras[i].Priority = 10;
                }
            }
        }

        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        cameraControls = GetComponent<CameraControls>();
        playerStateManager = GetComponent<PlayerStateManager>();

        if (SceneManager.GetActiveScene().path == mainMenuScene)
        {
            ingame = false;
        }

        else if (SceneManager.GetActiveScene().path != mainMenuScene)
        {
            ingame = true;
            
            cameraControls.CCStart();
        }
    }

    private void Update()
    {
        if (!ingame) return;

        if (!isLocalPlayer) return;
        
        cameraControls?.SwitchCameraMovementControl();
        Physics.IgnoreLayerCollision(8, 3, true);
        Physics.IgnoreLayerCollision(8, 6, true);
        Physics.IgnoreLayerCollision(8, 9, true);
    }

    // Called before Start();
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer) return;

        HandleColorTunicChange(tunicColorIndex, tunicColorIndex);
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

        playerUnitManager = GetComponent<PlayerUnitManager>();
        playerUnitManager.OnStartGame();

        SetCameraPOV();
    }

    #endregion

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.CompareTag("Shop"))
        {
            Debug.Log($"bruh");
            SwitchCamera(Grotto.instance.shopCamera);
            
            playerStateManager.SwitchState(playerStateManager.ShopState);
        }
    }

    public void SwitchCamera(CinemachineVirtualCameraBase newCam)
    {
        currentCamera = newCam;

        if (!cameras.Contains(newCam))
            cameras.Add(newCam);

        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] == currentCamera)
            {
                cameras[i].Priority = 20;
            }
            else
            {
                cameras[i].Priority = 10;
            }
        }

       
    }

    // For switching back to the third pov camera | aka. main camera
    public void SwitchCamera()
    {
        currentCamera = thirdPovCam;

        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] == currentCamera)
            {
                cameras[i].Priority = 20;
            }
            else
            {
                cameras[i].Priority = 10;
            }
        }
    }

    [Server]
    public void SetTunicColor(int num)
    {
        tunicColorIndex = num;
        Debug.Log($"AHHHHHHHH");
    }

    void HandleColorTunicChange(int oldValue, int newValue)
    {
        Debug.Log(newValue);

        if (isServerOnly) return;

        tunicColorIndex = newValue;

        Debug.Log($"changing player tunic");
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            Debug.Log(tunicColors[newValue].name);

            renderer.material = tunicColors[newValue];
            
            Debug.Log(renderer.material.name);
        }
    }
}
