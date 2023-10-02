 using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerUnitManager : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] public int money;

    [Space]

    [SerializeField] public List<UnitSO> unitsLoadout = new List<UnitSO>(3);
    [SerializeField] public List<int> loadoutCount = new List<int>(3);
    [SerializeField] Unit selectedUnit;
    [SerializeField] int unitCap;
    //public Unit GetSelectedUnit() { return selectedUnit; }

    [SerializeField] Transform currentGridTransform;
    [SerializeField] PlayerManager playerManager;
    GameObject preSpawnedUnit;

    bool placedUnit = true;

    public bool playerReady = false;
    // Start is called before the first frame update
    void Start()
    {
        playerManager = GetComponent<PlayerManager>();

        if (!playerManager.ingame) return;

        if (!isLocalPlayer) return;

        var gameManager = GameManager.instance;

        
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerManager.ingame) return;

        if (!isLocalPlayer) return;

        GridPickerRaycast();
        RaycastHpBar();

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
        Debug.Log($"Selling unit");
        
        selectedUnit.SellUnit(this.netIdentity);
        selectedUnit = null;
        UIUnitStats.instance.UpdateUnitStats(null, false);
    }

    public void UpgradeUnit()
    {
        Debug.Log($"Before Upgrading unit");

        if (selectedUnit.GetLevel() < 5)
        {
            if (money < selectedUnit.CostToUpgrade(selectedUnit.GetLevel() + 1))
            {
                ExceptionMsgUI.instance.UIExceptionMessage("You don't have enough money");
                Debug.Log($"You don't have enough money to upgrade");
                return;
            }

            selectedUnit.UpgradeUnit(this.netIdentity);
        }

        Debug.Log($"Upgraded unit");

    }
    [SerializeField] LayerMask layerMask;

    void GridPickerRaycast()
    {
        if (UIManager.instance.MouseOnUI) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, layerMask.value))
        {
            if (hit.collider.gameObject.layer == 3)
            {
                currentGridTransform = hit.transform;
            }

            if (placedUnit == false) return;

            // get unit
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (hit.collider.gameObject.layer == 6)
                {
                    if (selectedUnit != null) selectedUnit.DeSelectUnit();
                    selectedUnit = hit.collider.GetComponent<Unit>();

                    Debug.Log($"Selected unit " + selectedUnit.name);

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

                    Debug.Log($"Deselected unit");
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
        if (unitIndex >= unitsLoadout.Count) return;

        Debug.Log($"Before buying unit " + unitsLoadout[unitIndex].name);

        placedUnit = true;

        if (loadoutCount[unitIndex] >= unitCap)
        {
            ExceptionMsgUI.instance.UIExceptionMessage("You have the maximum units placed for that unit");

            Debug.Log($"You have the max number of units for that tower");
            return;
        }

        if (money < unitsLoadout[unitIndex].NextCost(1))
        {
            ExceptionMsgUI.instance.UIExceptionMessage("You don't have enough money");
            Debug.Log($"You don't have enough money");

            return;
        }

        Debug.Log($"After buying unit " + unitsLoadout[unitIndex].name);


        // for switching units between buying them
        if (preSpawnedUnit != null)
            Destroy(preSpawnedUnit);
                
        StopCoroutine(nameof(PlaceUnitDown));

        placedUnit = false;

        GameObject newUnit = Instantiate(unitsLoadout[unitIndex].prefab, currentGridTransform.position, Quaternion.identity);

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
        Debug.Log($"Before placing unit " + thisUnit.name);


        preSpawnedUnit = thisUnit;
        bool setUnitDown = false;
        Vector3 gridPos = Vector3.zero;
        
        if (selectedUnit != null) 
        {
            selectedUnit.DeSelectUnit();
            selectedUnit = null;
        }

        Unit newUnit = thisUnit.GetComponent<Unit>();
        int newGridCellSize = (int)Mathf.Pow(newUnit.unitGridSize, 2f);

        while (setUnitDown == false)
        {
            if (thisUnit == null) yield break;

            if (!newUnit.CheckGridCellAvailability()) 
                newUnit.ChangeVisualRangeSprite(true);
            else
                newUnit.ChangeVisualRangeSprite(false);

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Debug.Log($"Client placed unit " + thisUnit.name);

                Destroy(thisUnit);
                placedUnit = true;
                yield break;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                setUnitDown = newUnit.CheckGridCellAvailability();

                if (setUnitDown == false)
                {
                    ExceptionMsgUI.instance.UIExceptionMessage("You cannot place that unit there");
                    Debug.Log($"Attempted to placed unit " + thisUnit.name + " in incorrect spot");
                }
                    
                yield return null;
                continue;
            }

            // To prevent units clipping into the ground upon Instantiation
            gridPos = new Vector3 (currentGridTransform.position.x, currentGridTransform.position.y, currentGridTransform.position.z);

            thisUnit.transform.position = gridPos;

            yield return null;
        }

        List<int> currentGridIndex = newUnit.gridCells;
        placedUnit = true;

        Destroy(thisUnit);
        CmdNetworkSpawnUnit(gridPos, currentGridIndex, loadoutIndex, newGridCellSize);
    }

    [Command]
    void CmdNetworkSpawnUnit(Vector3 newGrid, List<int> gridCellIndexes, int loadoutIndex, int unitGridSize)
    {

        // Checks
        if (loadoutCount[loadoutIndex] >= unitCap) return;
        if (gridCellIndexes.Count != unitGridSize) return;
        if (money < unitsLoadout[loadoutIndex].NextCost(1)) return;
        foreach (int gridCell in gridCellIndexes)
        {
            if (GameManager.instance.GetGridCell(gridCell).CheckAvailability(unitsLoadout[loadoutIndex].gridType) == false) return;
        }
        // End Checks

        SetMoney(-unitsLoadout[loadoutIndex].NextCost(1));

        GameObject newUnit = Instantiate(unitsLoadout[loadoutIndex].prefab, newGrid, Quaternion.identity);
        NetworkServer.Spawn(newUnit, this.gameObject);

        Unit thisUnit = newUnit.GetComponent<Unit>();
        thisUnit.PlacedUnit(gridCellIndexes, loadoutIndex);
        loadoutCount[loadoutIndex]++;

        Debug.Log($"Placing client " + this.gameObject.name + " unit " + loadoutCount[loadoutIndex]);

        PlaceUnit(thisUnit, loadoutIndex);
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

    EnemyUnit currentEnemy;
    void RaycastHpBar()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if(hit.collider.CompareTag("Enemy"))
            {
                if (currentEnemy != null)
                {
                    currentEnemy.ToggleHPBar(false);
                }


                currentEnemy = hit.collider.GetComponent<EnemyUnit>();
                currentEnemy.ToggleHPBar(true);
            }

            else
            {
                if (currentEnemy == null) return;

                currentEnemy.ToggleHPBar(false);
                currentEnemy = null;
            }
        }
    }

    // Player Prefs - string "Unit0"
    // Player Prefs - string "Unit1"
    // Player Prefs - string "Unit2"
    [TargetRpc]
    public void UpdateUnitsInventory(NetworkConnectionToClient conn, List<string> unitSOs)
    {
        List<UnitSO> newList = new List<UnitSO>();

        foreach (string unitName in unitSOs)
        {
            UnitSO result = UnitSO.Get(unitName);
            newList.Add(result);
        }

        unitsLoadout = newList;
    }

    public void OnStartGame()
    {
        unitsLoadout.Clear();

        for (int i = 0; i < 3; i++)
        {
            if (!PlayerPrefs.HasKey($"Unit{i}")) continue;

            UnitSO newUnit = UnitSO.Get(PlayerPrefs.GetString($"Unit{i}"));

            unitsLoadout.Add(newUnit);
            Debug.Log(PlayerPrefs.GetString($"Unit{i}"));
        }

        List<string> theseUnits = new List<string>();

        foreach (UnitSO unit in unitsLoadout)
        {
            theseUnits.Add(unit.uniqueName);
        }

        UIUnitStats.instance.localPlayer = this;
        
        if (GameManager.instance.gameStarted == true)
            UIManager.instance.DisableReadyButtonLocally();

        UIManager.instance.player = this;

        UIManager.instance.SlotLoadout();
        UpdateUnitInventory(theseUnits);
    }

    [Command]
    public void UpdateUnitInventory(List<string> units)
    {
        if (CSNetworkManager.instance.isSinglePlayer) return;

        unitsLoadout.Clear();
        Debug.Log(unitsLoadout.Count);

        foreach (string unitName in units)
        {
            unitsLoadout.Add(UnitSO.Get(unitName));
        }
    }

    [TargetRpc]
    public void SetUnitReward(string unitName)
    {
        UIManager.instance.UISetUnitReward(UnitSO.Get(unitName));
        PlayerPrefs.SetString("RewardUnit", unitName);
        PlayerPrefs.Save();
    }
}
