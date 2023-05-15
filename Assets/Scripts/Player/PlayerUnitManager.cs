using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerUnitManager : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] public int money;

    [Space]

    [SerializeField] public List<UnitSO> unitsLoadout = new List<UnitSO>(3);
    [SerializeField] public List<int> loadoutCount = new List<int>(3);
    [SerializeField] Unit selectedUnit;
    //public Unit GetSelectedUnit() { return selectedUnit; }

    [SerializeField] Transform currentGridTransform;
    [SerializeField] GridCell currentGrid;

    bool placedUnit = true;

    public bool playerReady = false;
    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer) return;

        var gameManager = GameManager.instance;

        UIUnitStats.instance.localPlayer = this;
        

        if (GameManager.instance.gameStarted == true)
            UIManager.instance.DisableReadyButtonLocally();

    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isLocalPlayer) return;
        
        UIManager.instance.player = this;
        UIManager.instance.SlotLoadout();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        GridPickerRaycast();

        SpawnUnit();

        if (selectedUnit != null)
        {
            UnitSelectionOptions();
        }
    }

    void UnitSelectionOptions()
    {
        // Upgrading unit 
        if (Input.GetKeyDown(KeyCode.E))
        {
            UpgradeUnit();
        }

        // Selling unit
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            SellUnit();
        }
    }

    public void SellUnit()
    {
        selectedUnit.SellUnit(this.netIdentity);
        selectedUnit = null;
        UIUnitStats.instance.UpdateUnitStats(null, false);
    }

    public void UpgradeUnit()
    {
        Debug.Log($"upgrading this bitch");

        if (selectedUnit.GetLevel() < 5)
        {
            if (money < selectedUnit.CostToUpgrade(selectedUnit.GetLevel() + 1))
            {
                Debug.Log($"You don't have enough money to upgrade");
                return;
            }

            selectedUnit.UpgradeUnit(this.netIdentity);
        }
    }

    void GridPickerRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 60))
        {
            if (hit.collider.gameObject.layer == 3)
            {
                currentGridTransform = hit.transform;
            }

            if (placedUnit == false) return;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (hit.collider.gameObject.layer == 6)
                {
                    if (selectedUnit != null) selectedUnit.DeSelectUnit();
                    selectedUnit = hit.collider.GetComponent<Unit>();

                    if (selectedUnit.isOwned)
                        selectedUnit.SelectUnit();

                    else if (!selectedUnit.isOwned) selectedUnit = null;
                }
                else if (hit.collider.gameObject.layer != 6)
                {
                    if (UIManager.instance.MouseOnUI) return;

                    if (selectedUnit != null)
                        selectedUnit.DeSelectUnit();

                    selectedUnit = null;
                }
            }
        }
    }

    [Server]
    public void SetMoney(int value)
    {
        money += value;
    }

    void SpawnUnit()
    {
        if (currentGridTransform == null) return;

        if (placedUnit == false) return; 

        if (Input.GetKeyDown(GetNumberKeyPress()))
        {
            int keyIndex = (int)GetNumberKeyPress();
            keyIndex -= 49;

            BuyUnit(keyIndex);         
        }
    }

    [Client]
    public void BuyUnit(int unitIndex)
    {
        if (unitsLoadout[unitIndex] == null) return;

        if (loadoutCount[unitIndex] >= 5)
        {
            Debug.Log($"You have the max number of units for that tower");
            return;
        }

        if (money < unitsLoadout[unitIndex].NextCost(1))
        {
            Debug.Log($"You don't have enough money");
            return;
        }

        placedUnit = false;

        GameObject newUnit = Instantiate(unitsLoadout[unitIndex].prefab, currentGridTransform.position, Quaternion.identity);

        var thisRenderer = newUnit.transform.GetChild(0).GetComponent<MeshRenderer>();
        Color newColor = thisRenderer.material.color;
        thisRenderer.material.color = new Color(1, 1, 1, 0.5f);

        StartCoroutine(PlaceUnitDown(newUnit, unitIndex));
    }

    KeyCode GetNumberKeyPress()
    {
        for (int i = 1; i <= 3; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                return (KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + i);
            }
        }

        return KeyCode.None;
    }


    IEnumerator PlaceUnitDown(GameObject thisUnit, int loadoutIndex)
    {
        bool setUnitDown = false;
        Vector3 gridPos = Vector3.zero;
        
        if (selectedUnit != null) 
        {
            selectedUnit.DeSelectUnit();
            selectedUnit = null;
        }

        while (setUnitDown == false)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                currentGrid = currentGridTransform.GetComponent<GridCell>();

                setUnitDown = currentGrid.CheckAvailability();
                yield return null;
                continue;
            }

            // To prevent units clipping into the ground upon Instantiation
            gridPos = new Vector3 (currentGridTransform.position.x, currentGridTransform.position.y + 0.5f, currentGridTransform.position.z);

            thisUnit.transform.position = gridPos;

            yield return null;
        }

        int currentGridIndex = currentGrid.listIndex;
        placedUnit = true;

        Destroy(thisUnit);
        CmdNetworkSpawnUnit(gridPos, currentGridIndex, loadoutIndex);
    }

    [Command]
    void CmdNetworkSpawnUnit(Vector3 newGrid, int gridCellIndex, int loadoutIndex)
    {
        if (loadoutCount[loadoutIndex] >= 5) return;
        if (GameManager.instance.GetGridCell(gridCellIndex).CheckAvailability() == false) return;
        if (money < unitsLoadout[loadoutIndex].NextCost(1)) return;

        SetMoney(-unitsLoadout[loadoutIndex].NextCost(1));

        GameObject newUnit = Instantiate(unitsLoadout[loadoutIndex].prefab, newGrid, Quaternion.identity);
        NetworkServer.Spawn(newUnit, this.gameObject);

        Unit thisUnit = newUnit.GetComponent<Unit>();
        thisUnit.PlacedUnit(gridCellIndex, loadoutIndex);
        PlaceUnit(thisUnit, loadoutIndex);
        loadoutCount[loadoutIndex]++;

        GameManager.instance.GetGridCell(gridCellIndex).SetOccupence(true);
        GameManager.instance.SyncGridCellOccupence(true, gridCellIndex);
    }

    [TargetRpc]
    void PlaceUnit(Unit unit, int loadoutIndex)
    {
        if (selectedUnit != null) selectedUnit.DeSelectUnit();

        if (!isServer)
        {
            loadoutCount[loadoutIndex]++;
        }

        UIManager.instance.SlotUnitCount(loadoutIndex);

        selectedUnit = unit;
        selectedUnit.SelectUnit();
    }

    [Server]
    public void ChangeLoadoutCount(int index, int value)
    {
        loadoutCount[index] += value;
        RpcChangeLoadoutCount(index, value);
    }

    [TargetRpc]
    void RpcChangeLoadoutCount(int index, int value)
    {
        if (!isServer)
        {
            loadoutCount[index] += value;
            UIManager.instance.SlotUnitCount(index);
        }
        else if (isServer && isClient)
        {
            UIManager.instance.SlotUnitCount(index);
        }
    }

    [Command]
    public void ReadyUp()
    {
        playerReady = !playerReady;
        GameManager.instance.PlayersAreReady(playerReady);
    }

    [Command]
    public void SkipWaveReady()
    {
        WaveManager.instance.PlayersAreReady(playerReady, this.netIdentity);
    }
}
