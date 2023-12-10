using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public enum TargettingMode
{
    First,
    Last,
    Stronger,
    Weaker
}
public class Unit : NetworkBehaviour
{
    [SerializeField] protected  UnitSO unitSO;

    [Space]

    protected bool isAttacking = true;

    [Space]

    [SyncVar]
    [SerializeField] TargettingMode targetMode;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform projectileOutput;
    [SerializeField] int projectileSpeed;

    [Tooltip("Value ^ 2")]
    public int unitGridSize;

    [Space]

    public List<EnemyUnit> enemiesInRange = new List<EnemyUnit>();
    protected PlayerUnitManager attachedPlayer;
    [SerializeField] protected SphereCollider rangeCollider;
    [SerializeField] protected Transform rangeVisualSprite;
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;

    [Header("WorldSpace UI")]
    [SerializeField] UIUnitHoverStats hoverStats;
    [SerializeField] protected GameObject hoverStatsObject;
    [SerializeField] HoveringUIText missedAttackUI;
    [SerializeField] protected GameObject missedAttackUIObject;

    [Space]
    [SerializeField] protected UnitAnimationManager animations;
    [SerializeField] protected GridDisplayFade gridDisplayFade;

    [Space]

    [SyncVar]
    protected string unitName;
    [SyncVar]
    protected string ownedPlayerName;

    [Header("Runtime Stats")]

    [SyncVar]
    [SerializeField] protected int cost;

    [SyncVar]
    [SerializeField] protected int sellCost;

    [SyncVar]
    [SerializeField] protected float cooldown;

    [SyncVar]
    [SerializeField] protected float attack;

    [SyncVar]
    [SerializeField] protected float range;

    #region Stats Multipliers
    [Space]
    [SyncVar]
    [SerializeField] public float attackMultiplier = 1;
    [SyncVar]
    [SerializeField] public float rangeMultiplier = 1;
    [SyncVar]
    [SerializeField] public float cooldownMultiplier = 1;

    #endregion

    [SyncVar]
    [SerializeField] protected int level = 1;

    [Space]
    public List<int> gridCells = new List<int>();

    #region Get Unit Stats methods

    public string GetUnitName() { return unitSO.name; }
    public string GetOwnedPlayerName() { return ownedPlayerName; }
    public int GetLevel() { return level; }
    public float GetCooldown() { return cooldown; }
    public float GetAttack() { return attack; }
    public float GetRange() { return range; }
    public int GetCost() { return cost; }
    public int GetSellCost() { return sellCost; }
    public UnitSO GetUnitSO() { return unitSO; }
    public TargettingMode GetTargetMode() { return targetMode; }

    [Header("Change Stat Names")]
    public string rangeName; 

    protected bool attacking = false;

    #endregion

    public bool isPlaced = false;
    protected bool isSelected = false;
    protected int loadoutIndex;
    

    #region Sound Effects

    protected float currentCooldown;
    
    protected AudioSource audioSource;
    [Space]
    [SerializeField] AudioClip attackSound;

    #endregion

    protected virtual void Start()
    {
        if (isServer)
        {
            currentCooldown = Time.time + cooldown;
            GameManager.instance.OnGameStart += ServerGameHasStarted;
        }

        if (!isClient)
        {
            gridDisplayFade.ToggleCellDisplay(true);
            range = unitSO.CurrentRange(level);
            rangeVisualSprite.gameObject.SetActive(true);
            missedAttackUIObject.SetActive(false);
            rangeCollider.radius = range / 5;
            rangeVisualSprite.localScale = new Vector3(1,1) * (rangeCollider.radius * 0.2f);
            return;
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
            missedAttackUIObject.SetActive(false);
            hoverStatsObject.SetActive(false);
            gridDisplayFade.isWhite = true;

            GameManager.instance.OnGameStart += ClientGameHasStarted;
        }

        if (!isOwned)
        {
            rangeVisualSprite.gameObject.SetActive(false);
            gridDisplayFade.ToggleCellDisplay(false);

        }

    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        range = unitSO.CurrentRange(level);

        if (rangeVisualSprite != null)
            rangeVisualSprite.localScale = new Vector3(1,1) * (rangeCollider.radius * 0.2f);
    }

    public override void OnStopServer()
    {
        GameManager.instance.SyncGridCellOccupence(false, gridCells);
    }

    protected virtual void Update()
    {
        if (!isPlaced) return;
        if (!isServer) return; 
        
        if (attacking)
            animations.AttackingAnim(0.2f);
    
        else
            animations.IdleAnim(0.3f);
            
        if (enemiesInRange.Count == 0) return;
        if (enemiesInRange[0] == null) 
        {
            enemiesInRange.RemoveAt(0);
            return;
        }

        AttackEnemy();
    }

    private void FixedUpdate()
    {
        Physics.IgnoreLayerCollision(6, 9);
    }

    [Client]
    protected virtual void ClientGameHasStarted(object sender, EventArgs e)
    {
        Debug.Log($"Client Unit is now armed");
    }

    [Server]
    protected virtual void ServerGameHasStarted(object sender, EventArgs e)
    {
        Debug.Log($"Server Unit is now armed");
    }

    [Server]
    public virtual void PlacedUnit(List<int> cellIndexes, int loadoutIndex)
    {
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
        
        targetMode = TargettingMode.First;
        rangeCollider.radius = range / 5;
        rangeVisualSprite.gameObject.SetActive(false);

        attachedPlayer.PlaceUnit(this, loadoutIndex);
        ClientPlacedUnit();
    }

    [TargetRpc]
    protected void ClientPlacedUnit()
    {
        isPlaced = true;
        Debug.Log(attack);
        Debug.Log(unitSO.CurrentAttack(1));
    }

    public int CostToUpgrade(int level)
    {
        return unitSO.NextCost(level);
    }

    public int SellingCost(int level)
    {
        int costHolder = 0;

        for (int i = 1; i < level + 1; i++)
        {
            costHolder += unitSO.NextCost(i) / 4;
        }

        return costHolder;
    }

    #region Targetting Modes

    protected void CurrentTargettingMode()
    {
        if (enemiesInRange.Count <= 1) return;

        switch(targetMode)
        {
            case TargettingMode.First:
                ListSortByMostDistance();
                break;
            
            case TargettingMode.Last:
                ListSortByLeastDistance();
                break;

            case TargettingMode.Stronger:
                ListSortByMostHealth();
                break;

            case TargettingMode.Weaker:
                ListSortByLeastHealth();
                break;
        }
    }

    void ListSortByMostDistance()
    {
        enemiesInRange.Sort(delegate(EnemyUnit x, EnemyUnit y) 
        {
            if (x.distanceCovered == y.distanceCovered) { return 0; }
            if (x.distanceCovered > y.distanceCovered) { return -1; }
            return 1;
        });
    }

    void ListSortByLeastDistance()
    {
        enemiesInRange.Sort(delegate(EnemyUnit x, EnemyUnit y) 
        {
            if (x.distanceCovered == y.distanceCovered) { return 0; }
            if (x.distanceCovered < y.distanceCovered) { return -1; }
            return 1;
        });
    }

    void ListSortByMostHealth()
    {
        enemiesInRange.Sort(delegate(EnemyUnit x, EnemyUnit y) 
        {
            if (x.GetHealthPoints() == y.GetHealthPoints()) { return 0; }
            if (x.GetHealthPoints() > y.GetHealthPoints()) { return -1; }
            return 1;
        });
    }

    void ListSortByLeastHealth()
    {
        enemiesInRange.Sort(delegate(EnemyUnit x, EnemyUnit y) 
        {
            if (x.GetHealthPoints() == y.GetHealthPoints()) { return 0; }
            if (x.GetHealthPoints() < y.GetHealthPoints()) { return -1; }
            return 1;
        });
    }

    [Command]
    public void ChangeTargetMode(int newTargetMode)
    {
        targetMode = (TargettingMode)newTargetMode;
    } 

    #endregion

    [Command]
    public void UpgradeUnit(NetworkIdentity conn)
    {
        if (conn.netId != attachedPlayer.netId) return;

        if (level >= 5) return;

        if (attachedPlayer.money < unitSO.NextCost(GetLevel() + 1)) return;

        level++;

        attachedPlayer.SetMoney(-unitSO.NextCost(GetLevel()));

        sellCost += unitSO.NextCost(level) / 4;
        cost = unitSO.NextCost(level + 1);

        attack = (float)System.Math.Round(unitSO.CurrentAttack(level) * attackMultiplier, 1);
        cooldown = (float)System.Math.Round(unitSO.CurrentCooldown(level) / cooldownMultiplier, 1);
        range = (float)System.Math.Round(unitSO.CurrentRange(level) * rangeMultiplier, 1);

        rangeCollider.radius = range / 5;
        rangeVisualSprite.localScale = new Vector3(1,1) * (rangeCollider.radius * 0.2f);
        
        UpdateLocalClient(rangeVisualSprite.localScale, true);
    }

    [Command]
    public void SellUnit(NetworkIdentity conn)
    {
        if (conn.netId != attachedPlayer.netId) return;

        conn.GetComponent<PlayerUnitManager>().ServerSellUnit(this);
        GameManager.instance.SyncGridCellOccupence(false, gridCells);
        attachedPlayer.SetMoney(sellCost);
        attachedPlayer.ChangeLoadoutCount(loadoutIndex, -1);
        NetworkServer.Destroy(gameObject);
    }

    [TargetRpc]
    protected virtual void UpdateLocalClient(Vector3 newScale, bool activeUI)
    {
        rangeVisualSprite.localScale = newScale;
        StartCoroutine(DelayUpdateUnitStats(activeUI));
    }

    // For client
    IEnumerator DelayUpdateUnitStats(bool activeUI)
    {
        yield return null;
        UIUnitStats.instance.UpdateUnitStats(this, activeUI);
    }

    [Server]
    protected virtual void AttackEnemy()
    {
        if (isAttacking == false) return;

        if (enemiesInRange[0].isDead)
        {
            enemiesInRange.RemoveAt(0);
            return;   
        }

        if (Time.time < currentCooldown) return;
        

        CurrentTargettingMode();

        if (enemiesInRange[0] == null)
        {
            enemiesInRange.RemoveAt(0);
            return;
        }
        else
        {
            StartCoroutine(AttackingAnimationLength());
            RpcClientUnitActions(enemiesInRange[0].transform.position);

            int rand = UnityEngine.Random.Range(0, 100);
            if (rand < unitSO.chanceToMiss)
            {
                MissedAttack();
            }
            else DealDamageToEnemy();
        }

        currentCooldown = Time.time + cooldown;
    }

    [Server]
    protected virtual void DealDamageToEnemy()
    {
        enemiesInRange[0].DealDamage(attack);
    }

    [ClientRpc]
    void MissedAttack()
    {
        Debug.Log($"missed attack");
        missedAttackUIObject.SetActive(true);
        missedAttackUI.StartAnimation("MISSED");
    }


    protected IEnumerator AttackingAnimationLength()
    {
        attacking = true;

        yield return new WaitForSeconds(animations.GetAttackAnimLength());

        attacking = false;
    }

    [ClientRpc]
    protected virtual void RpcClientUnitActions(Vector3 firstEnemyPos)
    {
        // CODE IS BROKEN | TEMPORARILY DISABLED
        //StartCoroutine(UnitLookAtEnemyAnimation(firstEnemyPos));

        Vector3 lookEnemyRot = new Vector3(firstEnemyPos.x, 0, firstEnemyPos.z);
        Vector3 modelUnitRot = new Vector3(transform.GetChild(0).position.x, 0, transform.GetChild(0).position.z);
        Quaternion lookOnLook = Quaternion.LookRotation(lookEnemyRot - modelUnitRot);
        transform.GetChild(0).rotation = lookOnLook;

        if (projectile != null)
            StartCoroutine(UnitShootProjectile(firstEnemyPos));
    }

    IEnumerator UnitLookAtEnemyAnimation(Vector3 firstEnemyPos)
    {
        Vector3 lookEnemyRot = new Vector3(firstEnemyPos.x, 0, firstEnemyPos.z);
        Vector3 modelUnitRot = new Vector3(transform.GetChild(0).position.x, 0, transform.GetChild(0).position.z);

        Quaternion lookOnLook = Quaternion.LookRotation(lookEnemyRot - modelUnitRot);

        float timer = 0;
        while (timer < 1)
        {
            timer += Time.deltaTime;
            transform.GetChild(0).rotation = Quaternion.Slerp(transform.GetChild(0).rotation, lookOnLook, 4);
            yield return null;
        }

    }

    IEnumerator UnitShootProjectile(Vector3 firstEnemyPos)
    {
        Transform newProjectile = Instantiate(projectile).transform;
        newProjectile.localPosition = projectileOutput.position;
        Vector3 newEnemyPos = new Vector3(firstEnemyPos.x, firstEnemyPos.y + 0.5f, firstEnemyPos.z);
        //                  destination  -  origin
        Vector3 direction = newEnemyPos - newProjectile.position;

        while (Vector3.Distance(newEnemyPos, newProjectile.position) > 0.2f)
        {
            newProjectile.Translate(direction * projectileSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(newProjectile.gameObject);
    }

    public void ChangeVisualRangeSprite(bool red)
    {
        if (red)
        {
            rangeVisualSprite.GetComponent<SpriteRenderer>().color = new Color (1, 0, 0, 1f);
            gridDisplayFade.isWhite = false;
            
        }
        else
        {
            rangeVisualSprite.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, 1f);
            gridDisplayFade.isWhite = true;
        }
    }
    
    public virtual void SelectUnit()
    {
        isSelected = true;
        rangeVisualSprite.gameObject.SetActive(true);
        rangeCollider.radius = range / 5;
        rangeVisualSprite.localScale = new Vector3(1,1) * (rangeCollider.radius * 0.2f);
        gridDisplayFade.ToggleCellDisplay(true);

        UIUnitStats.instance.UpdateUnitStats(this, true);
    }

    public void DeSelectUnit()
    {
        rangeVisualSprite.gameObject.SetActive(false);
        isSelected = false;
        gridDisplayFade.ToggleCellDisplay(false);

        UIUnitStats.instance.UpdateUnitStats(this, false);
    }

    [TargetRpc]
    void RpcSelectUnit()
    {
        if (!isSelected) return;

        SelectUnit();
        StartCoroutine(DelayUpdateUnitStats(true));
    }

    private void OnMouseOver()
    {
        if (!isClient) return;

        if (!isSelected)
            gridDisplayFade.ToggleCellDisplay(true);
        
        hoverStatsObject.SetActive(true);
        hoverStats.UpdateHoverStats();
    }

    private void OnMouseExit()
    {

        if (!isSelected)
        {
            if (isPlaced)
            {
                gridDisplayFade.ToggleCellDisplay(false);
            }

            if (!isOwned) gridDisplayFade.ToggleCellDisplay(false);
        }

        hoverStatsObject.SetActive(false);
    }

    [Server]
    public virtual IEnumerator StunnedEffect(float seconds)
    {
        isAttacking = false;
        RpcStunEffects(true);

        yield return new WaitForSeconds(seconds);

        RpcStunEffects(false);
        isAttacking = true;
    }

    [ClientRpc]
    void RpcStunEffects(bool stunned)
    {
        if (stunned)
        {
            //#00FFFF
            foreach (Material material in skinnedMeshRenderer.materials)
            {
                material.color = new Color(0, 1, 1, 1);
            }
            animations.PauseAnimation(true);
        }
        else
        {
            foreach (Material material in skinnedMeshRenderer.materials)
            {
                material.color = new Color(1, 1, 1, 1);
            }
            animations.PauseAnimation(false);
        }
    }

    public virtual bool CheckGridCellAvailability()
    {
        if (gridCells.Count == 0) return false;

        if (gridCells.Count != Mathf.Pow(unitGridSize, 2)) return false;

        foreach (int cellIndex in gridCells)
        {
            if (!GameManager.instance.GetGridCell(cellIndex).CheckAvailability(unitSO.gridType))
            {
                return false;
            }
            else continue;
        }

        return true;
    }

    public void PlayAttackSoundClip()
    {
        audioSource.PlayOneShot(attackSound, PlayerPrefs.GetFloat("SoundFXVol"));
    }

    public void DoNothingMethod() {}

    [Server]
    public void ApplyStatsMultiplier(UnitMultiplierType type, float value)
    {
        switch (type)
        {
            case UnitMultiplierType.Attack:
                attackMultiplier += value;
                attack = (float)System.Math.Round(attack * attackMultiplier, 1);
                break;

            case UnitMultiplierType.Cooldown:
                cooldownMultiplier += value;
                cooldown = (float)System.Math.Round(cooldown / cooldownMultiplier, 1);
                break;

            case UnitMultiplierType.Range:
                rangeMultiplier += value;
                range = (float)System.Math.Round(range * rangeMultiplier, 1);
                break;
        }

        RpcSelectUnit();
    }
}
