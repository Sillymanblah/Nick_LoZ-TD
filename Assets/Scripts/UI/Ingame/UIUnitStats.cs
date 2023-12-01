using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitStats : MonoBehaviour
{
    Unit currentUnit;

    [Header("Stats Name Text")]
    [SerializeField] TextMeshProUGUI unitNameText;
    [SerializeField] TextMeshProUGUI rangeNameText;

    #region Value Text

    [Header("Values Text")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI rangeText;
    [SerializeField] TextMeshProUGUI cooldownText;
    [SerializeField] TextMeshProUGUI upgradeText;
    [SerializeField] TextMeshProUGUI sellText;

    #endregion
    [SerializeField] TextMeshProUGUI targetModeText;
    [SerializeField] Transform buttonsParent;
    [SerializeField] Image unitIcon;


    public PlayerUnitManager localPlayer;

    public static UIUnitStats instance;

    private void Start()
    {
        this.gameObject.SetActive(false);
        instance = this;
    }

    public void UpdateUnitStats(Unit unit, bool active)
    {
        if (active)
        {
            gameObject.SetActive(true);
            SetStats(unit);
        }
        else 
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateUnitStatsAoE(Unit unit, bool active)
    {
        if (active)
        {
            gameObject.SetActive(true);
            SetStats(unit);
            AOEStats();
        }
        else 
        {
            gameObject.SetActive(false);
        }
    }

    public void SetStats(Unit unit)
    {
        currentUnit = unit;

        attackText.color = Color.white;
        rangeText.color = Color.white;
        cooldownText.color = Color.white;
        targetModeText.color = Color.white;
        unitNameText.color = Color.white;

        Debug.Log(unit.GetAttack());
        unitNameText.text = currentUnit.GetUnitName().ToString();
        unitIcon.sprite = currentUnit.GetUnitSO().icon;
        levelText.text = "Level: " + currentUnit.GetLevel().ToString();
        attackText.text = currentUnit.GetAttack().ToString();
        rangeText.text = currentUnit.GetRange().ToString();
        cooldownText.text = currentUnit.GetCooldown().ToString();

        if (unit.rangeName == string.Empty)
            rangeNameText.text = "RANGE";
        else
            rangeNameText.text = unit.rangeName;

        if (currentUnit.GetLevel() == 5)
            upgradeText.text = "MAX";
        else
            upgradeText.text = "-" + currentUnit.GetCost().ToString();

        sellText.text = "+" + (currentUnit.GetSellCost()).ToString();

        targetModeText.text = currentUnit.GetTargetMode().ToString();
        buttonsParent.gameObject.SetActive(true);
    }

    // When the unit is already declared (ex. for changing UI text on selected unit)
    public void SetStats()
    {
        int unitLevel = currentUnit.GetLevel();

        if (unitLevel == 5) return;

        attackText.color = Color.white;
        rangeText.color = Color.white;
        cooldownText.color = Color.white;

        levelText.text = "Level: " + unitLevel.ToString();
        attackText.text = currentUnit.GetAttack().ToString();
        rangeText.text = currentUnit.GetRange().ToString();
        cooldownText.text = currentUnit.GetCooldown().ToString();
        upgradeText.text = "-" + currentUnit.GetCost().ToString();
        sellText.text = "+" + (currentUnit.GetSellCost()).ToString();
    }

    public void PreviewStats()
    {
        int unitLevel = currentUnit.GetLevel();

        if (unitLevel == 5) return;

        UnitSO thisCurrentUnitSO = currentUnit.GetUnitSO();

        attackText.text = System.Math.Round(thisCurrentUnitSO.CurrentAttack(unitLevel + 1) * currentUnit.attackMultiplier, 1).ToString();
        rangeText.text = System.Math.Round(thisCurrentUnitSO.CurrentRange(unitLevel + 1) * currentUnit.rangeMultiplier, 1).ToString();
        cooldownText.text = System.Math.Round(thisCurrentUnitSO.CurrentCooldown(unitLevel + 1) / currentUnit.cooldownMultiplier, 1).ToString();

        //If the next stat is greater than or equal to current stat, change text to green, else change to red
        // !TERNARY OPERATOR!
        attackText.color = thisCurrentUnitSO.CurrentAttack(unitLevel + 1) >= currentUnit.GetAttack() ? Color.green : Color.red;
        rangeText.color = thisCurrentUnitSO.CurrentRange(unitLevel + 1) >= currentUnit.GetRange() ? Color.green : Color.red;
        cooldownText.color = thisCurrentUnitSO.CurrentCooldown(unitLevel + 1) <= currentUnit.GetCooldown() ? Color.green : Color.red;
    }

    public void SellUnitButton()
    {
        localPlayer.SellUnit();
        currentUnit = null;
    }

    public void UpgradeUnitButton()
    {
        localPlayer.UpgradeUnit();
    }

    public void SetUnitTargettingModeButton(int targetMode)
    {
        currentUnit.ChangeTargetMode(targetMode);

        TargettingMode thisTargetMode = (TargettingMode)targetMode;

        targetModeText.text = thisTargetMode.ToString();
    }

    public void AOEStats()
    {
        buttonsParent.gameObject.SetActive(false);
        targetModeText.text = "AoE";
    }
}
