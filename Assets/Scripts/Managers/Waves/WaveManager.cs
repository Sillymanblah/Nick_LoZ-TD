using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class WaveManager : NetworkBehaviour
{
    public static WaveManager instance;
    public bool spawnEnemies = false;
    public bool endless = false;

    [SerializeField] List<GameObject> enemies = new List<GameObject>();
    [SerializeField] List<GameObject> bosses = new List<GameObject>();
    

    [Header("Wave Management")]

    [Tooltip("How many waves should there be in a single game")]
    [SerializeField] public int waveAmount = 1;

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
    public float setHealthMultiplier;
    
    public float setGroupMultiplier;
    public float setMoneyDecrements;
    float moneyMultiplier;

    float healthMultiplier = 1;
    float playerMultiplier = 1;
    [SerializeField] int enemiesKilled;

    [SyncVar]
    [SerializeField] public int totalEnemiesSpawned;

    [SerializeField] int playerReadyCount;
    [SerializeField] List<NetworkIdentity> playersReadyToSkip = new List<NetworkIdentity>();

    public event EventHandler<GameManagerEventArgs> OnGameWon;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        moneyMultiplier = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;

        if (BaseManager.instance.deadBase) return;

        if (spawnEnemies == false) return;

        if (currentGroup == waveSize)
        {
            if (enemiesKilled >= totalEnemiesSpawned)
                WaveComplete();
        }

        if (GameManager.instance.intermission) return;

        

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
        int randomEnemy = UnityEngine.Random.Range(0, enemies.Count);

        while (unitSpawnCount < groupSize)
        {
            EnemyUnit newEnemy = Instantiate(enemies[randomEnemy], WayPointsManager.instance.points[0].position, Quaternion.identity).GetComponent<EnemyUnit>();
            
            NetworkServer.Spawn(newEnemy.gameObject);

            newEnemy.SetEnemyHPMultiplier(healthMultiplier);
            newEnemy.SetEnemyMoneyMultiplier(moneyMultiplier);

            totalEnemiesSpawned++;

            unitSpawnCount++;
            yield return new WaitForSeconds(2.0f);

            
        }

        // if the current spawned group is equal to the size of the waves ( - 1 to account for 0)
        if (currentGroup == waveSize - 1)
        {
            SpawnBoss();

            // if the wave we are on isnt the final wave
            if (currentWave != waveAmount)
                StartCoroutine(ActivateSkipWave());
        }

        yield return new WaitForSeconds(5.0f);

        unitSpawnCount = 0;
        currentGroup++;
        healthMultiplier += (setGroupMultiplier * currentWave);
    }

    [Server]
    void SpawnBoss()
    {
        if (bosses.Count == 0) return;

        EnemyUnit newBoss = null;

        /*if (currentWave == 1)
            newBoss = Instantiate(boss1, WayPointsManager.points[0].position, Quaternion.identity).GetComponent<EnemyUnit>();
        else if (currentWave == waveAmount)
            newBoss = Instantiate(boss2, WayPointsManager.points[0].position, Quaternion.identity).GetComponent<EnemyUnit>();*/

        newBoss = Instantiate(bosses[currentWave - 1], WayPointsManager.instance.points[0].position, Quaternion.identity).GetComponent<EnemyUnit>();
        
        totalEnemiesSpawned++;
        
        NetworkServer.Spawn(newBoss.gameObject);
        newBoss.SetEnemyHPMultiplier(playerMultiplier);
        newBoss.SetEnemyMoneyMultiplier(2);
    }

    [Server]
    IEnumerator Intermission()
    {
        GameManager.instance.intermission = true;

        if (Grotto.instance == null)
        {
            yield return new WaitForSeconds(5.0f);
            goto End;
        }

        Grotto.instance.RpcSetNewItems();
        Debug.Log($"Break time");

        yield return new WaitForSeconds(2.0f);

        if (Grotto.instance != null)
            TeleportAllPlayers(Grotto.instance.GetSpawnPosition());

        yield return new WaitForSeconds(20.0f);
        
        if (Grotto.instance != null)
            TeleportAllPlayersUnderShop(NetworkManager.startPositions[0].position);

        Grotto.instance.ResetShop();
    End:
        Debug.Log($"Break time over!");
        GameManager.instance.intermission = false;
        
    }
    [Server]
    void TeleportAllPlayers(Vector3 position) 
    {
        foreach (NetworkIdentity player in CSNetworkManager.instance.players)
        {
            TeleportPlayer(player.connectionToClient, position);
        }
    }

    [Server]
    void TeleportAllPlayersUnderShop(Vector3 position) 
    {
        foreach (NetworkIdentity player in CSNetworkManager.instance.players)
        {
            if (Grotto.instance.CheckPlayerFromGrotto(player.netId))
            {
                TeleportPlayer(player.connectionToClient, position);
            }
        }
    }

    [TargetRpc]
    void TeleportPlayer(NetworkConnectionToClient conn, Vector3 position)
    {
        var playerCC = NetworkClient.localPlayer.GetComponent<CharacterController>();
        Grotto.instance.localPlayer = NetworkClient.localPlayer.connectionToClient;

        playerCC.enabled = false;
        playerCC.transform.position = position;
        playerCC.enabled = true;

        var playerStateManager = NetworkClient.localPlayer.GetComponent<PlayerStateManager>();

        playerStateManager.SwitchState(playerStateManager.FallingState);
    }

    IEnumerator ActivateSkipWave()
    {
        if (endless) yield break;

        float currentTime = 0;

        while (currentTime < 5)
        {
            currentTime += Time.deltaTime;

            yield return null;
        }

        /*foreach (NetworkIdentity player in CSNetworkManager.instance.players)
        {
            RpcToggleSkipWaveButtonAppearance(player.connectionToClient, true);
        }*/

        RpcToggleSkipWaveButtonAppearance(true);

        RpcUpdateSkipWaveCount(playerReadyCount, CSNetworkManager.instance.numPlayers);
    }

    [ClientRpc]
    void RpcToggleSkipWaveButtonAppearance(bool result)
    {
        UIManager.instance.ToggleSkipButtonLocally(result);
        
    }

    [ClientRpc]
    void RpcUpdateSkipWaveCount(int readyCount, int maxPlayers)
    {
        UIManager.instance.UpdateSkipWaveButton(readyCount, maxPlayers);
    }

    [Server]
    public void PlayersAreReady(NetworkIdentity player)
    {
        if (playersReadyToSkip.Count > 0)
        {
            if (playersReadyToSkip.Contains(player) == true)
            {
                playerReadyCount--;
                playersReadyToSkip.Remove(player);
            }
            else
            {
                playerReadyCount++;
                playersReadyToSkip.Add(player);
            }
        }
        else
        {
            playerReadyCount++;
            playersReadyToSkip.Add(player);
        }

        RpcUpdateSkipWaveCount(playerReadyCount, CSNetworkManager.instance.numPlayers);

        if (playerReadyCount == CSNetworkManager.instance.numPlayers)
        {
            playerReadyCount = 0;
            playersReadyToSkip.Clear();
            enemiesKilled = enemiesKilled - totalEnemiesSpawned;
            
            SkipWaveComplete();
            RemoveSkipWaveButton();
        }
    }

    [ClientRpc]
    void RemoveSkipWaveButton()
    {
        UIManager.instance.ToggleSkipButtonLocally(false);
    }

    [Server]
    void WaveComplete()
    {
        if (currentWave == waveAmount)
        {
            GameComplete();
            spawnEnemies = false;
            return;
        }
        else
        {
            Debug.Log($"Wave complete!");
            currentWave++;
            healthMultiplier = playerMultiplier * (currentWave * setHealthMultiplier);
            enemiesKilled = 0;
            totalEnemiesSpawned = 0;
            moneyMultiplier -= setMoneyDecrements;
            if (moneyMultiplier < 0.1f)
                moneyMultiplier = 0.1f;
            
            playersReadyToSkip.Clear();
            playerReadyCount = 0;

            /*foreach (NetworkIdentity player in CSNetworkManager.instance.players)
            {
                RpcToggleSkipWaveButtonAppearance(player.connectionToClient, false);
            }*/

            RpcToggleSkipWaveButtonAppearance(false);


            StopCoroutine(nameof(ActivateSkipWave));
            StartCoroutine(Intermission());
            currentGroup = 0;
        }
    }

    void SkipWaveComplete()
    {
        Debug.Log($"Wave complete!");
        currentWave++;
        healthMultiplier = playerMultiplier * (currentWave * setHealthMultiplier);
        totalEnemiesSpawned = 0;
        moneyMultiplier -= setMoneyDecrements;
        if (moneyMultiplier < 0.1f)
            moneyMultiplier = 0.1f;

        StopCoroutine(nameof(ActivateSkipWave));
        StartCoroutine(Intermission());
        currentGroup = 0;
    }

    [Server]
    void GameComplete()
    {
        Debug.Log($"Game is complete!!!!! :DDD");
        GameManager.instance.intermission = true;
        StartCoroutine(DelayEndingGame());
    }

    [Server]
    public void EnemyKilled()
    {
        enemiesKilled++;
    }

    [Server]
    public void SetHealthWithPlayerCount(int playerCount)
    {
        playerMultiplier = playerCount;

        if (playerCount == 1)
        {
            playerMultiplier = 1;
        }

        healthMultiplier = playerMultiplier;
    }

    IEnumerator DelayEndingGame()
    {
        
        OnGameWon?.Invoke(this, new GameManagerEventArgs { isDead = false });

        yield return new WaitForSeconds(2.0f);

        if (CSNetworkManager.instance.isSinglePlayer)
        {
            CSNetworkManager.instance.StopServer();
            NetworkClient.Disconnect();
            yield break;
        }

        CSNetworkManager.instance.SwitchScenes("MainMenu");
    }
}
