using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;
using UnityEngine.UIElements;

public class Grotto : NetworkBehaviour
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

    [Header("Raycast Shop Items shit")]
    [SerializeField] LayerMask itemLayer;
    [SerializeField] List<IngameShopItemSO> items = new List<IngameShopItemSO>();
    [SerializeField] List<ShopItem> itemsDisplays = new List<ShopItem>();

    [SerializeField] Transform itemsDisplayParent;

    List<NetworkIdentity> playersThatGotAnItem = new List<NetworkIdentity>();
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

        for (int i = 0; i < itemsDisplayParent.childCount; i++)
        {
            itemsDisplays.Add(itemsDisplayParent.GetChild(i).GetComponent<ShopItem>());
            itemsDisplays[i].index = i;
            itemsDisplays[i].InitializeItem(items[i]);
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
            player.SwitchState(player.ShopState);
        }
    }

    public void ShopItemRayCast(NetworkConnectionToClient conn, int money)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, itemLayer.value))
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                var item = hit.collider.GetComponent<ShopItem>();

                if (items[item.index].cost > money) return;

                BuyShopItem(item.index, conn);
            }
        }
    }

    [Command(requiresAuthority = false)]
    void BuyShopItem(int index, NetworkConnectionToClient conn)
    {
        // Server Auth Checks ///////
        if (!GameManager.instance.intermission) return;

        if (playersThatGotAnItem.Contains(conn.identity)) return;

        var player = conn.identity.GetComponent<PlayerUnitManager>();

        if (items[index].cost > player.money) return;
        // Approved ////////////////

        player.SetMoney(-items[index].cost);

        var playerCC = player.GetComponent<CharacterController>();
            playerCC.enabled = false;
            playerCC.transform.position = NetworkManager.startPositions[0].position;
            playerCC.enabled = true;

        Debug.Log(conn);
        playersThatGotAnItem.Add(conn.identity);
        BoughtItem(conn);
    }

    [TargetRpc]
    void BoughtItem(NetworkConnectionToClient thisConnection)
    {
        Debug.Log($"you bought this bitch");

        SwitchCamera(startCamera);
        var player = NetworkClient.localPlayer.GetComponent<PlayerStateManager>();
        player.SwitchState(player.FallingState);
    }

    [ClientRpc]
    public void SetNewItems()
    {
        for (int i = 0; i < itemsDisplays.Count; i++)
        {
            itemsDisplays[i].InitializeItem(items[i]);
        }
    }

    [Server]
    public void ResetShop()
    {
        playersThatGotAnItem.Clear();
    }
}

