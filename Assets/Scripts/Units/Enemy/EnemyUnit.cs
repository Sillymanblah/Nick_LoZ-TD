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
    [SerializeField] float speed = 10f;
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
    void Start()
    {
        if (!isServer) return;

        dropMoney = Mathf.FloorToInt(moneyMultiplier * healthPoints);
        target = WayPointsManager.points[1];

        previousPosition = transform.position;
        InvokeRepeating(nameof(TrackDistance), 0, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;

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
            NetworkServer.Destroy(gameObject);
            return;
        }

        wavepointIndex++;
        target = WayPointsManager.points[wavepointIndex];
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

    /// <summary>
    /// Called every frame while the mouse is over the GUIElement or Collider.
    /// </summary>
    private void OnMouseOver()
    {
        hpBar.SetActive(true);
    }

    /// <summary>
    /// Called when the mouse is not any longer over the GUIElement or Collider.
    /// </summary>
    private void OnMouseExit()
    {
        hpBar.SetActive(false);
    }
}
