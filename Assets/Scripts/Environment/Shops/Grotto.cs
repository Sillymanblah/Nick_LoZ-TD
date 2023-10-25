using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Grotto : NetworkBehaviour
{
    public static Grotto instance;

    [SerializeField] Transform spawnTransform;
    public Vector3 GetSpawnPosition() { return spawnTransform.position; }

    #region Cameras

    [Header("Cameras")]
    public CinemachineVirtualCamera shopCamera;


    #endregion

    [Header("Raycast Shop Items shit")]
    [SerializeField] LayerMask itemLayer;
    [SerializeField] List<IngameShopItemSO> items = new List<IngameShopItemSO>();
    [SerializeField] List<ShopItem> itemsDisplays = new List<ShopItem>();

    [SerializeField] Transform itemsDisplayParent;

    List<NetworkIdentity> playersThatGotAnItem = new List<NetworkIdentity>();
    [SerializeField] List<NetworkIdentity> playersInsideGrotto = new List<NetworkIdentity>();
    bool switchCams;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        if (isServerOnly) return;

        for (int i = 0; i < itemsDisplayParent.childCount; i++)
        {
            itemsDisplays.Add(itemsDisplayParent.GetChild(i).GetComponent<ShopItem>());;
            itemsDisplays[i].OnStartUp(i, NetworkClient.localPlayer.GetComponent<PlayerStateManager>());
        }
    }

    public void ShopItemRayCast(NetworkConnectionToClient conn, int money)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5, itemLayer.value))
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

        Debug.Log(conn);
        playersThatGotAnItem.Add(conn.identity);
        BoughtItem(conn);
    }

    [TargetRpc]
    void BoughtItem(NetworkConnectionToClient thisConnection)
    {
        Debug.Log($"you bought this bitch");

        var player = NetworkClient.localPlayer.GetComponent<PlayerStateManager>();
        player.SwitchState(player.FallingState);

        var playerCC = NetworkClient.localPlayer.GetComponent<CharacterController>();

        playerCC.enabled = false;
        playerCC.transform.position = NetworkManager.startPositions[0].position;
        playerCC.enabled = true;
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

    [Server]
    public bool CheckPlayerFromGrotto(uint networkID)
    {
        for (int i = 0; i < playersInsideGrotto.Count; i++)
        {
            if (networkID == playersInsideGrotto[i].netId)
                return true;
            
            continue;
        }

        return false;
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInsideGrotto.Add(other.GetComponent<NetworkIdentity>());
        }
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInsideGrotto.Remove(other.GetComponent<NetworkIdentity>());
        }
    }
}

