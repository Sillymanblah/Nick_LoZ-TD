using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BaseManager : NetworkBehaviour
{
    
    [SerializeField] int maxBaseHealth;
    [SyncVar(hook = nameof(HandleBaseHealthChange))]
    [SerializeField] int baseHealth;
    public static BaseManager instance;
    public bool deadBase = false;
    [SerializeField] TextMeshProUGUI baseHealthText;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip baseGettingHit;

    public event EventHandler<GameManagerEventArgs> OnBaseDead;

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
            WaveManager.instance.EnemyKilled();

            OnBaseDead?.Invoke(this, new GameManagerEventArgs { isDead = deadBase });
            if (isServerOnly)
                RpcBaseDeadEvent();

            HPTEXTGAMEOVER();
            StartCoroutine(DelayEndingGame());
            return;
        }
        else
        {
            WaveManager.instance.EnemyKilled();
        }
    }

    [ClientRpc]
    void RpcBaseDeadEvent()
    {
        OnBaseDead?.Invoke(this, new GameManagerEventArgs { isDead = true });
    }

    [ClientCallback]
    void UpdateBaseHPUI()
    {
        if (baseGettingHit != null)
            audioSource.PlayOneShot(baseGettingHit, PlayerPrefs.GetFloat("SoundFXVol"));

        if (baseHealthText == null) return;
        if (baseHealth <= 0) return;

        baseHealthText.text = baseHealth + "/" + maxBaseHealth;
    }

    [ClientRpc]
    void HPTEXTGAMEOVER()
    {
        Debug.Log($"YOU DEAD DUMB BOY!!!");

        if (baseHealthText == null) return;

        

        baseHealthText.text = "GAME OVER";
    }

    IEnumerator DelayEndingGame()
    {
        yield return new WaitForSeconds(7.4f);

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
        Debug.Log($"bruhther");
    }
}
