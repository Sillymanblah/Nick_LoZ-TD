using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyUnit : NetworkBehaviour
{
    public string enemyName;

    [SyncVar]
    public bool isBoss;
    [Space]
    protected UnitAnimationManager animManager;

    [Space]

    #region Moving

    CharacterController controller;
    float gravity = -9.81f;
    Vector3 velocity;
    [SerializeField] bool isGrounded = false;

    #endregion


    [Space]
    [SerializeField] protected int maxHealthPoints;
    public int GetMaxHealth() { return maxHealthPoints; }

    [Space]
    [SyncVar(hook = nameof(UpdateEnemyHealth))] 
    protected int healthPoints = 0;

    public int GetHealthPoints() { return healthPoints; }

    [Space]
    protected int dropMoney;
    [SerializeField] float moneyMultiplier = 1;

    [SerializeField] protected float speed = 10f;
    public float distanceCovered;
    Vector3 previousPosition;
    protected Transform target;
    protected int waypointIndex = 1;
    public bool isDead = false;

    [SerializeField] HealthBar thisHealthBar;
    [SerializeField] GameObject hpBar;

    [Space]
    [SerializeField] HoveringUIText missedAttackUI;
    [SerializeField] protected GameObject missedAttackUIObject;

    // Start is called before the first frame update

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        animManager = GetComponent<UnitAnimationManager>();
    }
    protected virtual void Start()
    {
        target = WayPointsManager.instance.points[1];
        var lookAtWaypoint = new Vector3(target.position.x, transform.position.y, target.position.z);
        missedAttackUIObject.SetActive(false);

        transform.LookAt(lookAtWaypoint, Vector3.up);

        if (!isServer) return;

        controller = GetComponent<CharacterController>();
        
        previousPosition = transform.position;
        InvokeRepeating(nameof(TrackDistance), 0, 0.1f);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isServer) return;
        
        if (speed > 0)
        {
            if (animManager != null)
            {
                animManager.WalkingAnim(0.1f);
            }
        }
        Physics.IgnoreLayerCollision(9, 3, true);
        Physics.IgnoreLayerCollision(9, 9, true);
        Physics.IgnoreLayerCollision(9, 8, true);

        GravityControl();
        WayPointControl();
    }

    void WayPointControl()
    {
        Vector3 thisTarget = new Vector3(target.position.x, transform.position.y, target.position.z);

        Vector3 direction = thisTarget - transform.position;
        controller.Move(direction.normalized * speed * Time.deltaTime);

        Vector3 targetDistance = new Vector3(target.position.x, 0, target.position.z);
        Vector3 positionDistance = new Vector3(transform.position.x, 0, transform.position.z);

        if (Vector3.Distance(targetDistance, positionDistance) <= 0.1f)
        {
            GetNextWayPoint();
        }
    }

    // return 0 = false
    // return 1 = true with damaging base
    // return 2 = true without damaging base
    protected virtual void GetNextWayPoint()
    {
        bool wayPointsCheck = WayPointsManager.instance.CheckForEnemyPosition(waypointIndex);

        if (wayPointsCheck)
        {
            BaseManager.instance.ChangeHealth(-maxHealthPoints);
            
            NetworkServer.Destroy(gameObject);
            return;
        }

        waypointIndex++;
        target = WayPointsManager.instance.points[waypointIndex];

        var lookAtWaypoint = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.LookAt(lookAtWaypoint, Vector3.up);
    }

    void GravityControl()
    {
        velocity.y += Mathf.Clamp(gravity * Time.deltaTime, -20, 20);

        controller.Move(velocity * Time.deltaTime);

        if (!isGrounded) { return; }

        if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void TrackDistance()
    {
        distanceCovered += Vector3.Distance(transform.position, previousPosition);
        previousPosition = transform.position;
    }

    [Server]
    public virtual void DealDamage(float points)
    {
        healthPoints -= Mathf.CeilToInt(points);

        if (healthPoints <= 0)
        {
            isDead = true;

            foreach (NetworkIdentity player in CSNetworkManager.instance.players)
            {
                player.GetComponent<PlayerUnitManager>().SetMoney(dropMoney);
            }
            
            RpcIsDead();
            WaveManager.instance.EnemyKilled();
            NetworkServer.Destroy(gameObject);  
        }
    }

    [ClientRpc]
    void RpcIsDead()
    {
        if (isBoss) UICentralBarSystem.instance.UpdateBossValue(enemyName, 0, maxHealthPoints);
    }

    void UpdateEnemyHealth(int oldValue, int newValue)
    {
        thisHealthBar.UpdateBarValue(newValue);

        if (isBoss) UICentralBarSystem.instance.UpdateBossValue(enemyName, newValue, maxHealthPoints);
    }

    [Server]
    public void SetEnemyHPMultiplier(float health, bool isBoss)
    {
        this.isBoss = isBoss;
        float maxHealthMultiplied = (float)maxHealthPoints * health;

        maxHealthPoints = Mathf.FloorToInt(maxHealthMultiplied);
        healthPoints = maxHealthPoints;
        
        HPBarUIStartUp(maxHealthPoints);
        
    }

    [Server]
    public void SetEnemyMoneyMultiplier(float moneyMultiplier)
    {
        
        this.moneyMultiplier *= moneyMultiplier;
        dropMoney = Mathf.CeilToInt(this.moneyMultiplier * healthPoints);
    }

    [ClientRpc]
    void HPBarUIStartUp(int maxHealthPoints)
    {
        thisHealthBar.BarValueOnStart(maxHealthPoints);
    }

    public void ToggleHPBar(bool active)
    {
        hpBar.SetActive(active);
    }

    [ClientRpc]
    public void DamageUIText(float number)
    {
        missedAttackUIObject.SetActive(true);
        missedAttackUI.StartAnimation(number.ToString());
    } 

    
}
