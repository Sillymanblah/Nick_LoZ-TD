using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitStats : MonoBehaviour
{
    Unit currentUnit;

    [SerializeField] Text levelText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI rangeText;
    [SerializeField] TextMeshProUGUI cooldownText;
    [SerializeField] TextMeshProUGUI upgradeText;
    [SerializeField] TextMeshProUGUI sellText;
    [SerializeField] TextMeshProUGUI targetModeText;


    public PlayerUnitManager localPlayer;

    public static UIUnitStats SingleTon;

    private void Start()
    {
        this.gameObject.SetActive(false);
        SingleTon = this;
    }
    public void SetStats(Unit unit)
    {
        currentUnit = unit;

        attackText.color = Color.white;
        rangeText.color = Color.white;
        cooldownText.color = Color.white;
        targetModeText.color = Color.white;

        levelText.text = "Level: " + currentUnit.GetLevel().ToString();
        attackText.text = currentUnit.GetAttack().ToString();
        rangeText.text = currentUnit.GetRange().ToString();
        cooldownText.text = currentUnit.GetCooldown().ToString();

        if (currentUnit.GetLevel() == 5)
            upgradeText.text = "MAX";
        else
            upgradeText.text = "-" + currentUnit.GetCost().ToString();

        sellText.text = "+" + (currentUnit.GetSellCost()).ToString();

        targetModeText.text = currentUnit.GetTargetMode().ToString();
    }

    // When the unit is already declared (ex. for changing UI text on selected unit)
    public void SetStats()
    {
        if (currentUnit.GetLevel() == 5) return;

        attackText.color = Color.white;
        rangeText.color = Color.white;
        cooldownText.color = Color.white;

        levelText.text = "Level: " + currentUnit.GetLevel().ToString();
        attackText.text = currentUnit.GetAttack().ToString();
        rangeText.text = currentUnit.GetRange().ToString();
        cooldownText.text = currentUnit.GetCooldown().ToString();
        upgradeText.text = "-" + currentUnit.GetCost().ToString();
        sellText.text = "+" + (currentUnit.GetSellCost()).ToString();
    }

    public void PreviewStats()
    {
        if (currentUnit.GetLevel() == 5) return;

        attackText.text = currentUnit.GetUnitSO().CurrentAttack(currentUnit.GetLevel() + 1).ToString();
        rangeText.text = currentUnit.GetUnitSO().CurrentRange(currentUnit.GetLevel() + 1).ToString();
        cooldownText.text = currentUnit.GetUnitSO().CurrentCooldown(currentUnit.GetLevel() + 1).ToString();

        //If the next stat is greater than or equal to current stat, change text to green, else change to red
        // !TERNARY OPERATOR!
        attackText.color = currentUnit.GetUnitSO().CurrentAttack(currentUnit.GetLevel() + 1) >= currentUnit.GetAttack() ? Color.green : Color.red;
        rangeText.color = currentUnit.GetUnitSO().CurrentRange(currentUnit.GetLevel() + 1) >= currentUnit.GetRange() ? Color.green : Color.red;
        cooldownText.color = currentUnit.GetUnitSO().CurrentCooldown(currentUnit.GetLevel() + 1) >= currentUnit.GetCooldown() ? Color.green : Color.red;
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

        targetModeText.text = thisTargetMode.ToString().ToUpper();
    }
}
