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

    private void Awake()
    {
        thisUnit = transform.parent.GetComponent<Unit>();    
        transform.GetChild(0).gameObject.SetActive(false);
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
