using System;
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

    PathwayGridCell thisPathwayGridCell;
    Vector3 thisPathwayGridCellPosition;
    int thisPathwayWayPoint;

    [Space]
    [SerializeField] GameObject bombchuPrefab;
    [SerializeField] Transform hands;

    protected override void Start()
    {
        if (isServer)
        {
            if (!GameManager.instance.gameStarted)
                GameManager.instance.OnGameStart += ServerGameHasStarted;
            else 
                currentCooldown = Time.time + cooldown;

            animations.IdleAnim(0);
        }

        if (!isClient)
        {
            gridDisplayFade.ToggleCellDisplay(true);
            missedAttackUIObject.SetActive(false);
            range = unitSO.CurrentRange(level);
            return;
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
            missedAttackUIObject.SetActive(false);
            hoverStatsObject.SetActive(false);
            gridDisplayFade.isWhite = true;

            if (GameManager.instance.gameStarted)
                StartCoroutine(nameof(DelayBombchuInHand));
            else
                GameManager.instance.OnGameStart += ClientGameHasStarted;

        }
    }

    protected override void Update()
    {
        if (!isPlaced) return;
        if (!isServer) return; 
        if (!GameManager.instance.gameStarted) return;


        Physics.IgnoreLayerCollision(13, 8);
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
        thisPathwayGridCell = enemyGridCellLocator.GetPathwayGridCell();
        thisPathwayGridCellPosition = thisPathwayGridCell.transform.position;
        thisPathwayWayPoint = thisPathwayGridCell.waypointIndex;

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

        GameObject newSummonable = Instantiate(thisSummonable, thisPathwayGridCellPosition + (Vector3.up / 2), Quaternion.identity);
        NetworkServer.Spawn(newSummonable);

        newSummonable.GetComponent<Summonable>().Initialize(thisPathwayWayPoint, this);

        StartCoroutine(AttackAnimationLogic());
        currentCooldown = Time.time + cooldown;
    }

    [Server]
    IEnumerator AttackAnimationLogic()
    {
        // 0.65f is a hard coded value of half the animation, whole anim time is 1.3f seconds from slowing it down by 0.5 (original is 0.65)
        yield return new WaitForSeconds(cooldown - 0.65f);
        animations.AttackingAnim(0.1f);
        
        StartCoroutine(DelayIdleAnimation());
        yield break;
    }

    [Server]
    IEnumerator DelayIdleAnimation()
    {
        yield return new WaitForSeconds(1.3f);
        animations.IdleAnim(0.1f);
    }

    [Client]
    IEnumerator DelayBombchuInHand()
    {
        int delay = 2;
        yield return new WaitForSeconds(cooldown - delay);

        GameObject newBombchu = Instantiate(bombchuPrefab, hands.position, Quaternion.identity, hands);
        newBombchu.transform.localRotation = Quaternion.Euler(new Vector3(-21f, -249, 133.9f));
        newBombchu.transform.localScale = new Vector3(.6f, .6f, .6f);

        yield return new WaitForSeconds(delay);
        Destroy(newBombchu);
        StartCoroutine(nameof(DelayBombchuInHand));
        yield break;
    }

    protected override void ClientGameHasStarted(object sender, EventArgs e)
    {
        base.ClientGameHasStarted(sender, e);
        StartCoroutine(nameof(DelayBombchuInHand));
    }

    protected override void ServerGameHasStarted(object sender, EventArgs e)
    {
        base.ServerGameHasStarted(sender, e);
        StartCoroutine(AttackAnimationLogic());
        currentCooldown = Time.time + cooldown;
    }
}
