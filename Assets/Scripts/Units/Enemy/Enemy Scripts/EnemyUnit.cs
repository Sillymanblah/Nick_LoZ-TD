using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyUnit : NetworkBehaviour
{
    protected UnitAnimationManager animManager;

    #region Moving

    CharacterController controller;
    float gravity = -9.81f;
    Vector3 velocity;
    [SerializeField] bool isGrounded = false;

    #endregion


    [SerializeField] protected int maxHealthPoints;
    public int GetMaxHealth() { return maxHealthPoints; }

    [Space]
    [SyncVar(hook = nameof(UpdateEnemyHealth))] public int healthPoints = 0;

    public int GetHealthPoints() { return healthPoints; }

    [Space]
    int dropMoney;
    [SerializeField] float moneyMultiplier = 1;

    [SerializeField] protected float speed = 10f;
    public float distanceCovered;
    Vector3 previousPosition;
    protected Transform target;
    protected int waypointIndex = 1;
    public bool isDead = false;

    [SerializeField] HealthBar thisHealthBar;
    [SerializeField] GameObject hpBar;

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

        transform.LookAt(lookAtWaypoint, Vector3.up);

        if (!isServer) return;

        controller = GetComponent<CharacterController>();
        
        previousPosition = transform.position;
        InvokeRepeating(nameof(TrackDistance), 0, 0.1f);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isServer) 
        {
            return;
        }
        
        if (speed > 0)
        {
            if (animManager != null)
            {
                animManager.WalkingAnim(0.1f);
            }
        }

        Physics.IgnoreLayerCollision(9, 3, true);
        Physics.IgnoreLayerCollision(9, 9, true);

        GravityControl();
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
            WaveManager.instance.EnemyKilled();
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
        velocity.y += gravity * Time.deltaTime;

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
    public void DealDamage(float points)
    {
        healthPoints -= Mathf.CeilToInt(points);

        if (healthPoints <= 0)
        {
            isDead = true;

            foreach (NetworkIdentity player in CSNetworkManager.instance.players)
            {
                player.GetComponent<PlayerUnitManager>().SetMoney(dropMoney);
            }
            
            WaveManager.instance.EnemyKilled();
            NetworkServer.Destroy(gameObject); 
        }
    }

    void UpdateEnemyHealth(int oldValue, int newValue)
    {
        healthPoints = newValue;
        thisHealthBar.UpdateBarValue(newValue);
    }

    [Server]
    public void SetEnemyHPMultiplier(float health)
    {
        float maxHealthMultiplied = (float)maxHealthPoints * health;

        maxHealthPoints = Mathf.FloorToInt(maxHealthMultiplied);
        healthPoints = maxHealthPoints;
        
        foreach (NetworkIdentity player in CSNetworkManager.instance.players)
        {
            HPBarUIStartUp(player.connectionToClient, maxHealthPoints);
        }
    }

    [Server]
    public void SetEnemyMoneyMultiplier(float moneyMultiplier)
    {
        this.moneyMultiplier *= moneyMultiplier;
        dropMoney = Mathf.CeilToInt(this.moneyMultiplier * healthPoints);
    }

    [TargetRpc]
    void HPBarUIStartUp(NetworkConnectionToClient conn, int maxHealthPoints)
    {
        thisHealthBar.BarValueOnStart(maxHealthPoints);
    }

    public void ToggleHPBar(bool active)
    {
        hpBar.SetActive(active);
    }
}
