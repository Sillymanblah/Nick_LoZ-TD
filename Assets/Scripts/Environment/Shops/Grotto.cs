using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

public class Grotto : MonoBehaviour
{
    public static Grotto instance;

    [SerializeField] Transform spawnTransform;
    public Vector3 GetSpawnPosition() { return spawnTransform.position; }

    #region Cameras

    [SerializeField] CinemachineVirtualCameraBase[] cameras;

    public CinemachineFreeLook thirdPovCam;
    public CinemachineVirtualCamera shopCamera;

    CinemachineVirtualCameraBase startCamera;
    CinemachineVirtualCameraBase currentCamera;

    #endregion

    bool switchCams;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        startCamera = thirdPovCam;
        currentCamera = startCamera;

        for (int i = 0; i < cameras.Length; i++)
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

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        
    }

    public void SwitchCamera(CinemachineVirtualCameraBase newCam)
    {
        currentCamera = newCam;

        for (int i = 0; i < cameras.Length; i++)
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SwitchCamera(shopCamera);

            var player = other.GetComponent<PlayerStateManager>();
            Debug.Log($"AHHHH I HATE PEOPLE");

            player.SwitchState(player.ShopState);
        }
    }
}
