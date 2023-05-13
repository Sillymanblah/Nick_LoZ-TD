using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIUnitHoverStats : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI rangeText;
    [SerializeField] TextMeshProUGUI cooldownText;
    [SerializeField] TextMeshProUGUI unitNameText;
    [SerializeField] TextMeshProUGUI unitPlayerText;

    Unit thisUnit;
    // Start is called before the first frame update
    void Start()
    {
        thisUnit = transform.parent.GetComponent<Unit>();    
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHoverStats()
    {
        levelText.text = "Lvl: " + thisUnit.GetLevel();
        attackText.text = thisUnit.GetAttack().ToString();
        rangeText.text = thisUnit.GetRange().ToString();
        cooldownText.text = thisUnit.GetCooldown().ToString();
        unitNameText.text = thisUnit.GetUnitName();
        unitPlayerText.text = thisUnit.GetOwnedPlayerName();
    }
}
