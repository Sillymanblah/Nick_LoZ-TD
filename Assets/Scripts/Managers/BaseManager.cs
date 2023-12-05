using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleBaseHealthChange))]
    [SerializeField] int maxBaseHealth;
    [SyncVar(hook = nameof(HandleBaseHealthChange))]
    [SerializeField] int baseHealth;
    public static BaseManager instance;
    bool deadBase = false;
    [SerializeField] TextMeshProUGUI baseHealthText;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

    }

    [Server]
    public void SetBaseHP(int multiplier)
    {
        maxBaseHealth *= multiplier;
        baseHealth = maxBaseHealth;
        UpdateBaseHPUI();
    }

    [Server]
    public void ChangeHealth(int points)
    {
        baseHealth += points;
        
        if (baseHealth <= 0)
        {
            baseHealth = 0;
            if (deadBase) return;

            deadBase = true;

            var newUnitReward = GameManager.instance.gameLevelSO.GetRandomUnitReward(WaveManager.instance.currentWave, false);

            if (newUnitReward != null)
            {
                foreach (NetworkIdentity player in CSNetworkManager.instance.players)
                {
                    player.GetComponent<PlayerUnitManager>().SetUnitReward(newUnitReward.uniqueName);
                }
            }

            HPTEXTGAMEOVER();
            Announcement("YOU LOSE DUM BOY!!!!");
            StartCoroutine(DelayEndingGame());
            return;
        }
    }

    void UpdateBaseHPUI()
    {
        if (baseHealthText == null) return;

        baseHealthText.text = baseHealth + "/" + maxBaseHealth;
    }

    [ClientRpc]
    void HPTEXTGAMEOVER()
    {
        if (baseHealthText == null) return;

        baseHealthText.text = "GAME OVER U VERY DUMB";
    }

    [ClientRpc]
    void Announcement(string message)
    {
        Debug.Log(message);

    }

    IEnumerator DelayEndingGame()
    {
        yield return new WaitForSeconds(3.0f);

        if (CSNetworkManager.instance.isSinglePlayer)
        {
            CSNetworkManager.instance.StopServer();
            NetworkClient.Disconnect();
            yield break;
        }
        
        CSNetworkManager.instance.SwitchScenes("MainMenu");
    }

    void HandleBaseHealthChange(int oldValue, int newValue)
    {
        UpdateBaseHPUI();
    }
}
