using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WaveManager : NetworkBehaviour
{
    public static WaveManager instance;
    public bool spawnEnemies = false;

    [SerializeField] GameObject enemy1;
    [SerializeField] GameObject boss;

    [SerializeField] bool intermission = false;

    [Header("Wave Management")]

    [Tooltip("How many waves should there be in a single game")]
    [SerializeField] int waveAmount = 1;

    [Tooltip("How many groups we want in a single wave")]  
    [SerializeField] public int waveSize;

    [Tooltip("How many units we want in a single group")]
    [SerializeField] public int groupSize = 1;

    [Space]

    [Tooltip("Which wave are we on?")]
    [SyncVar]              
    [SerializeField] public int currentWave = 1;

    [Tooltip("Counting the amount of groups has spawned")] 
    [SerializeField] int currentGroup = 0;

    [Tooltip("Counting the amount of units that spawn in one grouping")]
    [SerializeField] int unitSpawnCount;

    [Space]

    [Header("Enemy code things")]
    int healthMultiplier = 1;
    [SerializeField] int enemiesKilled;

    [SyncVar]
    [SerializeField] public int totalEnemiesSpawned;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;

        if (currentGroup == waveSize)
        {
            if (enemiesKilled == totalEnemiesSpawned)
                WaveComplete();
        }

        if (intermission) return;

        if (spawnEnemies == false) return;

        // This prevents enemies from spawning before the wave ends and if all enemies on scene havent been killed yet
        if (currentGroup == waveSize) return;

        if (unitSpawnCount == 0)
        {
            StartCoroutine(SpawnUnits());
        }
    }

    
    [Server]
    IEnumerator SpawnUnits()
    {
        while (unitSpawnCount < groupSize)
        {
            EnemyUnit newEnemy = Instantiate(enemy1, WayPointsManager.points[0].position, Quaternion.identity).GetComponent<EnemyUnit>();
            NetworkServer.Spawn(newEnemy.gameObject);
            newEnemy.SetMaxHealthMultiplier(healthMultiplier);
            totalEnemiesSpawned++;

            unitSpawnCount++;
            yield return new WaitForSeconds(2.0f);

            
        }

        if (currentWave == waveAmount - 1 && currentGroup == waveSize - 1)
            SpawnBoss();

        yield return new WaitForSeconds(5.0f);

        unitSpawnCount = 0;
        currentGroup++;
    }

    [Server]
    void SpawnBoss()
    {
        GameObject newBoss = Instantiate(boss, WayPointsManager.points[0].position, Quaternion.identity);
        totalEnemiesSpawned++;

        NetworkServer.Spawn(newBoss);
    }

    IEnumerator Intermission()
    {
        intermission = true;
        Debug.Log($"Break time");
        yield return new WaitForSeconds(5.0f);
        Debug.Log($"Break time over!");

        intermission = false;
    }

    [Server]
    void WaveComplete()
    {
        currentWave++;

        if (currentWave == waveAmount)
        {
            GameComplete();
            spawnEnemies = false;
            return;
        }
        else
        {
            Debug.Log($"Wave complete!");
            healthMultiplier++;
            enemiesKilled = 0;
            totalEnemiesSpawned = 0;

            StartCoroutine(Intermission());
            currentGroup = 0;
        }
    }

    [Server]
    void GameComplete()
    {
        Debug.Log($"Game is complete!!!!! :DDD");
        intermission = true;
    }

    [Server]
    public void EnemyKilled()
    {
        enemiesKilled++;
    }
}
