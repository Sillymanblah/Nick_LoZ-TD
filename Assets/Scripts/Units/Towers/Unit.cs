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
    [SerializeField] UnitSO unitSO;

    [Space]

    bool isAttacking = true;

    [Space]

    [SyncVar]
    [SerializeField] TargettingMode targetMode;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform projectileOutput;

    [Space]

    public List<EnemyUnit> enemiesInRange = new List<EnemyUnit>();
    PlayerUnitManager attachedPlayer;
    [SerializeField] protected SphereCollider rangeCollider;
    [SerializeField] protected Transform rangeVisualSprite;
    [SerializeField] UIUnitHoverStats hoverStats;

    [SyncVar]
    string unitName;
    [SyncVar]
    string ownedPlayerName;

    [SyncVar]
    [SerializeField] int cost;

    [SyncVar]
    [SerializeField] int sellCost;

    [SyncVar]
    [SerializeField] protected float cooldown;

    [SyncVar]
    [SerializeField] protected float attack;

    [SyncVar]
    [SerializeField] protected float range;

    [SyncVar]
    [SerializeField] int level = 1;

    #region Get Unit Stats methods

    public string GetUnitName() { return unitName; }
    public string GetOwnedPlayerName() { return ownedPlayerName; }
    public int GetLevel() { return level; }
    public float GetCooldown() { return cooldown; }
    public float GetAttack() { return attack; }
    public float GetRange() { return range; }
    public int GetCost() { return cost; }
    public int GetSellCost() { return sellCost; }
    public UnitSO GetUnitSO() { return unitSO; }
    public TargettingMode GetTargetMode() { return targetMode; }


    #endregion


    public bool isPlaced = false;
    protected bool isSelected = false;
    int gridCellIndex;
    int loadoutIndex;

    //void UpdateAttackStat(float oldValue, float newValue)

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (!isClient)
        {
            range = unitSO.CurrentRange(level);

            rangeVisualSprite.gameObject.SetActive(true);
            rangeCollider.radius = range / 5;
            rangeVisualSprite.localScale = new Vector3(1,1) * (rangeCollider.radius * 0.2f);
            return;
        }

        if (!isOwned)
        {
            rangeVisualSprite.gameObject.SetActive(false);
        }

        if (!isServer)
        {
            return;
        }
        

        currentCooldown = Time.time + cooldown;
    }

    

    // This is for when u spawn it on the server
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        
        range = unitSO.CurrentRange(level);

        rangeCollider.radius = range / 5;
        rangeVisualSprite.localScale = new Vector3(1,1) * (rangeCollider.radius * 0.2f);
    }

    public override void OnStopServer()
    {
        GameManager.instance.SyncGridCellOccupence(false, gridCellIndex);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isPlaced) return;

        if (!isServer) return;
        
        if (enemiesInRange.Count == 0) return;
        if (enemiesInRange[0] == null) 
        {
            enemiesInRange.RemoveAt(0);
            return;
        }
        AttackEnemy();
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void FixedUpdate()
    {
        Physics.IgnoreLayerCollision(6, 9);
    }

    [Server]
    public void PlacedUnit(int cellIndex, int loadoutIndex)
    {
        isPlaced = true;
        attachedPlayer = netIdentity.connectionToClient.identity.GetComponent<PlayerUnitManager>();

        gridCellIndex = cellIndex;
        this.loadoutIndex = loadoutIndex;

        level = 1;
        sellCost += unitSO.NextCost(level) / 4;
        cost = unitSO.NextCost(level + 1);

        unitName = unitSO.name;
        ownedPlayerName = attachedPlayer.GetComponent<PlayerNetworkInfo>().name;
        attack = unitSO.CurrentAttack(level);
        cooldown = unitSO.CurrentCooldown(level);
        range = unitSO.CurrentRange(level);

        targetMode = TargettingMode.First;

        rangeCollider.radius = range / 5;
        rangeVisualSprite.gameObject.SetActive(false);

        ClientPlacedUnit();
    }

    [TargetRpc]
    void ClientPlacedUnit()
    {
        isPlaced = true;
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

    void CurrentTargettingMode()
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
            if (x.healthPoints == y.healthPoints) { return 0; }
            if (x.healthPoints > y.healthPoints) { return -1; }
            return 1;
        });
    }

    void ListSortByLeastHealth()
    {
        enemiesInRange.Sort(delegate(EnemyUnit x, EnemyUnit y) 
        {
            if (x.healthPoints == y.healthPoints) { return 0; }
            if (x.healthPoints < y.healthPoints) { return -1; }
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

        attack = unitSO.CurrentAttack(level);
        cooldown = unitSO.CurrentCooldown(level);
        range = unitSO.CurrentRange(level);

        rangeCollider.radius = range / 5;
        rangeVisualSprite.localScale = new Vector3(1,1) * (rangeCollider.radius * 0.2f);

        
        UpdateLocalClient(rangeVisualSprite.localScale, true);
    }

    [Command]
    public void SellUnit(NetworkIdentity conn)
    {
        if (conn.netId != attachedPlayer.netId) return;

        GameManager.instance.SyncGridCellOccupence(false, gridCellIndex);
        attachedPlayer.SetMoney(sellCost);
        attachedPlayer.ChangeLoadoutCount(loadoutIndex, -1);
        NetworkServer.Destroy(gameObject);
    }

    [TargetRpc]
    protected virtual void UpdateLocalClient(Vector3 newScale, bool activeUI)
    {
        Debug.Log($"is this being called");
        rangeVisualSprite.localScale = newScale;
        UIUnitStats.instance.UpdateUnitStats(this, activeUI);
    }

    protected float currentCooldown;

    [Server]
    protected virtual void AttackEnemy()
    {
        if (isAttacking == false) return;

        if (enemiesInRange[0].isDead)
        {
            enemiesInRange.RemoveAt(0);
            return;   
        }

        if (Time.time < currentCooldown)
        {
            return;
        }

        CurrentTargettingMode();
        

        if (enemiesInRange[0] == null)
        {
            enemiesInRange.RemoveAt(0);
            return;
        }
        else
        { 
            int rand = Random.Range(0, 100);
            if (rand < unitSO.chanceToMiss)
            {
                Debug.Log($"missed attack");
                goto MissedAttack;
            }
            enemiesInRange[0].DealDamage(attack);
            RpcClientUnitActions(enemiesInRange[0].transform.position);
        }
        

        MissedAttack:
        currentCooldown = Time.time + cooldown;
    }

    [ClientRpc]
    void RpcClientUnitActions(Vector3 firstEnemyPos)
    {
        // CODE IS BROKEN | TEMPORARILY DISABLED
        //StartCoroutine(UnitLookAtEnemyAnimation(firstEnemyPos));
        Vector3 lookEnemyRot = new Vector3(firstEnemyPos.x, 0, firstEnemyPos.z);
        Vector3 modelUnitRot = new Vector3(transform.GetChild(0).position.x, 0, transform.GetChild(0).position.z);
        Debug.Log(transform.GetChild(0));
        Quaternion lookOnLook = Quaternion.LookRotation(lookEnemyRot - modelUnitRot);
        transform.GetChild(0).rotation = lookOnLook;

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
        if (projectile == null) yield break;

        Transform newProjectile = Instantiate(projectile, this.transform.position, Quaternion.identity).transform;

        //                   destination  -  origin
        Vector3 direction = firstEnemyPos - newProjectile.position;

        while (Vector3.Distance(firstEnemyPos, newProjectile.position) > 0.2f)
        {
            newProjectile.Translate(direction * (.1f * .9f));
            yield return null;
        }

        Destroy(newProjectile.gameObject);
    }

    public void ChangeVisualRangeSprite(bool red)
    {
        if (red)
            rangeVisualSprite.GetComponent<SpriteRenderer>().color = new Color (1, 0, 0, 0.3f);
        else
            rangeVisualSprite.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, 0.3f);
    }
    
    public virtual void SelectUnit()
    {
        isSelected = true;
        rangeVisualSprite.gameObject.SetActive(true);
        rangeCollider.radius = range / 5;
        rangeVisualSprite.localScale = new Vector3(1,1) * (rangeCollider.radius * 0.2f);
        UIUnitStats.instance.UpdateUnitStats(this, true);
    }

    public void DeSelectUnit()
    {
        rangeVisualSprite.gameObject.SetActive(false);
        isSelected = false;
        UIUnitStats.instance.UpdateUnitStats(this, false);
    }

    /// <summary>
    /// Called every frame while the mouse is over the GUIElement or Collider.
    /// </summary>
    private void OnMouseOver()
    {
        if (!isClient) return;

        hoverStats.gameObject.SetActive(true);
        hoverStats.UpdateHoverStats();
    }

    /// <summary>
    /// Called when the mouse is not any longer over the GUIElement or Collider.
    /// </summary>
    private void OnMouseExit()
    {
        hoverStats.gameObject.SetActive(false);
    }

    public IEnumerator StunnedEffect(float seconds)
    {
        isAttacking = false;
        
        yield return new WaitForSeconds(seconds);

        isAttacking = true;
    }
}
