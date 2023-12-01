using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIUnitHoverStats : MonoBehaviour
{
    [Header("Value Text")]
    [SerializeField] TextMeshProUGUI levelValueText;
    [SerializeField] TextMeshProUGUI attackValueText;
    [SerializeField] TextMeshProUGUI rangeValueText;
    [SerializeField] TextMeshProUGUI cooldownValueText;
    [SerializeField] TextMeshProUGUI unitNameText;
    [SerializeField] TextMeshProUGUI unitPlayerText;

    [Header("Title Text")]
    [SerializeField] TextMeshProUGUI rangeTitleText;

    Unit thisUnit;

    private void Awake()
    {
        thisUnit = transform.parent.GetComponent<Unit>();    
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void UpdateHoverStats()
    {
        levelValueText.text = "Lvl: " + thisUnit.GetLevel();
        attackValueText.text = thisUnit.GetAttack().ToString();
        rangeValueText.text = thisUnit.GetRange().ToString();
        cooldownValueText.text = thisUnit.GetCooldown().ToString();
        unitNameText.text = thisUnit.GetUnitName();
        unitPlayerText.text = thisUnit.GetOwnedPlayerName();

        if (thisUnit.rangeName == string.Empty)
            rangeTitleText.text = "RANGE";
        else 
            rangeTitleText.text = thisUnit.rangeName;
    }
}
