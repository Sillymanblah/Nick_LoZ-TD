using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class BBAGirlUnit : Unit
{
    [Header("BBA Stuff")]
    [SerializeField] EnemyGridCellLocator enemyGridCellLocator;
    //PathwayGridCell currentPathwayGridCell;
    [SerializeField] GameObject thisSummonable;
    Vector3 thisPathwayGridCellPosition;


    protected override void Update()
    {
        if (!isPlaced) return;
        if (!isServer) return; 
        
        if (attacking)
            animations.AttackingAnim(0.2f);
        else
            animations.IdleAnim(0.3f);

        AttackEnemy();
    }

    public override bool CheckGridCellAvailability()
    {
        if (gridCells.Count != 1) return false;

        if (GameManager.instance.GetGridCell(gridCells[0]).CheckAvailability(unitSO.gridType))
        {
            return enemyGridCellLocator.CheckForPathwayGrid();
        }

        else return false;
    }

    

    [Server]
    public override void PlacedUnit(List<int> cellIndexes, int loadoutIndex)
    {
        thisPathwayGridCellPosition = enemyGridCellLocator.GetPathwayGridCell().transform.position;
        transform.LookAt(new Vector3(thisPathwayGridCellPosition.x, this.transform.position.y, thisPathwayGridCellPosition.z));

        isPlaced = true;
        attachedPlayer = netIdentity.connectionToClient.identity.GetComponent<PlayerUnitManager>();

        gridCells = cellIndexes;
        this.loadoutIndex = loadoutIndex;

        level = 1;
        sellCost += unitSO.NextCost(level) / 4;
        cost = unitSO.NextCost(level + 1);

        unitName = unitSO.name;
        
        attack = unitSO.CurrentAttack(level);
        cooldown = unitSO.CurrentCooldown(level);
        range = unitSO.CurrentRange(level);

        ownedPlayerName = attachedPlayer.GetComponent<PlayerNetworkInfo>().name;

        // This sets the grid cells states for EVERYONE \\
        foreach (int cellIndex in cellIndexes)
        {
            GameManager.instance.GetGridCell(cellIndex).SetOccupence(true);
        }       
        GameManager.instance.SyncGridCellOccupence(true, cellIndexes);
        // -/////////////////////\\\\\\\\\\\\\\\\\\\\\- \\

        rangeVisualSprite.gameObject.SetActive(false);

        attachedPlayer.PlaceUnit(this, loadoutIndex);
        ClientPlacedUnit();
    }

    [Server]
    protected override void AttackEnemy()
    {
        if (isAttacking == false) return;

        if (Time.time < currentCooldown)
        {
            return;
        }

        GameObject newSummonable = Instantiate(thisSummonable, thisPathwayGridCellPosition + Vector3.up, Quaternion.identity);
        NetworkServer.Spawn(newSummonable);

        currentCooldown = Time.time + cooldown;
    }
}
