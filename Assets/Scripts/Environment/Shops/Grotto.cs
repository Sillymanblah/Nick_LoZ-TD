using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Data.SqlTypes;
using System.Linq;
using System;
using JetBrains.Annotations;

public class Grotto : NetworkBehaviour
{
    public static Grotto instance;
    public NetworkConnectionToClient localPlayer;

    [SerializeField] float itemCostMultiplier;

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
    bool switchCams;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        

        if (isServerOnly) return;

        for (int i = 0; i < itemsDisplayParent.childCount; i++)
        {
            //itemsDisplays.Add(itemsDisplayParent.GetChild(i).GetComponent<ShopItem>());
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

                CmdBuyShopItem(item.index, conn);
            }
        }
    }

    [Command(requiresAuthority = false)]
    void CmdBuyShopItem(int index, NetworkConnectionToClient conn)
    {
        // Server Auth Checks ///////
        if (!GameManager.instance.intermission) return;

        if (playersThatGotAnItem.Contains(conn.identity)) return;

        var player = conn.identity.GetComponent<PlayerUnitManager>();

        Debug.Log(player);
        if (itemsDisplays[index].cost > player.money) return;

        // Approved ////////////////

        player.SetMoney(-itemsDisplays[index].cost);
        
        var thisPlayer = conn.identity.GetComponent<PlayerUnitManager>();

        if (thisPlayer.unitsPlaced.Count > 0)
        {
            foreach (Unit unit in thisPlayer.unitsPlaced)
            {
                items[index].ItemAbility(unit);
            }
        }
        Debug.Log(conn);
        playersThatGotAnItem.Add(conn.identity);
        BoughtItem(conn, itemsDisplays[index].cost);
    }

    [Command(requiresAuthority = false)]
    public void CmdTeleportBackToSpawn(NetworkConnectionToClient conn)
    {
        playersThatGotAnItem.Add(conn.identity);
        BoughtItem(conn, 0);
    }

    [TargetRpc]
    void BoughtItem(NetworkConnectionToClient thisConnection, int cost)
    {
        if (PlayerPrefs.HasKey("Grotto Payments"))
        {
            int grottoBalance = PlayerPrefs.GetInt("Grotto Payments");

            if (grottoBalance < 10000 && grottoBalance + cost >= 10000)
            {
                PlayerPrefs.SetInt("Grotto Payments", grottoBalance + cost);
                PlayerData data = SaveData.LoadInventory();
                List<string> unitsData = data.unitNamesInventory;

                if (!unitsData.Contains("Business Scrub"))
                {
                    Debug.Log($"yayyyy business scrub!!!");
                    UIManager.instance.UISetUnitReward(UnitSO.Get("Business Scrub"));
                    unitsData.Add("Business Scrub");
                    SaveData.SaveInventory(unitsData);
                }

                
                
            } else PlayerPrefs.SetInt("Grotto Payments", grottoBalance + cost);

        }
        else PlayerPrefs.SetInt("Grotto Payments", cost);

        PlayerPrefs.Save();

        

        Debug.Log($"you bought this bitch");

        var player = NetworkClient.localPlayer.GetComponent<PlayerStateManager>();
        player.SwitchState(player.FallingState);

        var playerCC = NetworkClient.localPlayer.GetComponent<CharacterController>();

        playerCC.enabled = false;
        playerCC.transform.position = NetworkManager.startPositions[0].position;
        playerCC.enabled = true;
    }

    [ClientRpc]
    public void RpcSetNewItems()
    {
        for (int i = 0; i < itemsDisplays.Count; i++)
        {
            itemsDisplays[i].InitializeItem(WaveManager.instance.currentWave - 1, items[i]);
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
        for (int i = 0; i < playersThatGotAnItem.Count; i++)
        {
            if (networkID == playersThatGotAnItem[i].netId)
                return false;

            continue;
        }

        return true;
    }
}

