using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseManager : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] int maxBaseHealth;
    [SyncVar]
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
            Announcement("YOU LOSE DUM BOY!!!!");
        }
            
        UpdateBaseHPUI();
    }

    [ClientRpc]
    void UpdateBaseHPUI()
    {
        baseHealthText.text = baseHealth + "/" + maxBaseHealth;
    }

    [ClientRpc]
    void Announcement(string message)
    {
        Debug.Log(message);

    }
}
