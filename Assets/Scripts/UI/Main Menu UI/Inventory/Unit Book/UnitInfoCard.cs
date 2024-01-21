using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitInfoCard : MonoBehaviour
{
    UnitSO thisUnitData;
    [SerializeField] Image unitIcon;
    [SerializeField] TextMeshProUGUI unitName;
    [SerializeField] TextMeshProUGUI unitObtainDesc;

    [SerializeField] TextMeshProUGUI unitLevelText;
    int unitLevel = 1; // We are starting from 1 and NOT 0

    #region Unit Levels UI Elements

    [Header("Unit Levels UI")]
    [SerializeField] TextMeshProUGUI attackValueText;
    [SerializeField] TextMeshProUGUI rangeValueText;
    [SerializeField] TextMeshProUGUI cooldownValueText;
    [SerializeField] UnityEvent disableButtons;
    [SerializeField] UnityEvent enableButtons;


    #endregion

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void AssignCard(UnitSO unit, bool achieved)
    {
        this.gameObject.SetActive(true);
        thisUnitData = unit;

        unitIcon.sprite = unit.icon;
        unitIcon.color = new Color(1,1,1);

        unitLevelText.text = "Level " + 1;
        unitLevel = 1;

        unitName.text = unit.name;
        if (!achieved)
        {
            unitIcon.color = new Color(0,0,0);
            unitName.text = "???";
            unitLevelText.text = "???";

            disableButtons.Invoke();
            SetLevelStats(0); // 0 means unachieved

        } 
        else 
        {
            enableButtons.Invoke();
            SetLevelStats(unitLevel);
        }

        unitObtainDesc.text = unit.toObtainDesc;
    }

    void SetLevelStats(int level)
    {
        if (level == 0) // if we did not achieve the unit quite yet
        {
            attackValueText.text = "???";
            rangeValueText.text = "???";
            cooldownValueText.text = "???";
            return;
        }

        unitLevelText.text = "Level " + level;
        attackValueText.text = thisUnitData.CurrentAttack(level).ToString();
        rangeValueText.text = thisUnitData.CurrentRange(level).ToString();
        cooldownValueText.text = thisUnitData.CurrentCooldown(level).ToString();
    }

    public void LeftButton()
    {
        unitLevel--;

        if (unitLevel <= 0)
        {
            unitLevel = 5;
        }

        SetLevelStats(unitLevel);
    }

    public void RightButton()
    {
        unitLevel++;

        if (unitLevel >= 6)
        {
            unitLevel = 1;
        }

        SetLevelStats(unitLevel);
    }
}
