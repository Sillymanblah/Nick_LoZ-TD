using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyUnit : NetworkBehaviour
{
    [SerializeField] int maxHealthPoints;
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
    Transform target;
    int wavepointIndex = 1;
    public bool isDead = false;

    [SerializeField] HealthBar thisHealthBar;
    [SerializeField] GameObject hpBar;
    // Start is called before the first frame update

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        if (!isServer) return;
    }
    protected virtual void Start()
    {
        if (!isServer) return;

        dropMoney = Mathf.FloorToInt(moneyMultiplier * healthPoints);
        target = WayPointsManager.points[1];

        var lookAtWaypoint = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.LookAt(lookAtWaypoint, Vector3.up);

        previousPosition = transform.position;
        InvokeRepeating(nameof(TrackDistance), 0, 0.1f);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isServer)
        {
            RaycastHpBar();
            return;
        }

        Vector3 direction = target.position - transform.position;
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) <= 0.1f)
        {
            GetNextWayPoint();
        }
    }

    void GetNextWayPoint()
    {
        if (wavepointIndex >= WayPointsManager.points.Length - 1)
        {
            BaseManager.instance.ChangeHealth(-maxHealthPoints);
            WaveManager.instance.EnemyKilled();
            NetworkServer.Destroy(gameObject);
            return;
        }

        wavepointIndex++;
        target = WayPointsManager.points[wavepointIndex];

        var lookAtWaypoint = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.LookAt(lookAtWaypoint, Vector3.up);
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
    public void SetMaxHealthMultiplier(float points)
    {
        float maxHealthMultiplied = (float)maxHealthPoints * points;

        maxHealthPoints = Mathf.FloorToInt(maxHealthMultiplied);
        healthPoints = maxHealthPoints;

        foreach (NetworkIdentity player in CSNetworkManager.instance.players)
        {
            HPBarUIStartUp(player.connectionToClient, maxHealthPoints);
        }
    }

    [TargetRpc]
    void HPBarUIStartUp(NetworkConnectionToClient conn, int maxHealthPoints)
    {
        thisHealthBar.BarValueOnStart(maxHealthPoints);
    }

    void RaycastHpBar()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if(hit.collider.CompareTag("Enemy"))
            {
                hit.collider.GetComponent<EnemyUnit>().ToggleHPBar(true);
            }

            else
            {
                hpBar.SetActive(false);
            }
        }
    }

    public void ToggleHPBar(bool active)
    {
        hpBar.SetActive(active);
    }
}
