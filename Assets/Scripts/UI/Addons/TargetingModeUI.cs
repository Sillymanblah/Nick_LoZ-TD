using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TargetingModeUI : MonoBehaviour
{
    [SerializeField] int targetModeByInt = 0;
    public static TargetingModeUI instance;

    private void Awake()
    {
        instance = this;
    }

    public void LeftButton()
    {
        targetModeByInt--;

        if (targetModeByInt == -1)
            targetModeByInt = 3;

        UIUnitStats.instance.SetUnitTargettingModeButton(targetModeByInt);
    }

    public void RightButton()
    {
        targetModeByInt++;

        if (targetModeByInt == 4)
            targetModeByInt = 0;

        UIUnitStats.instance.SetUnitTargettingModeButton(targetModeByInt);
    }
}
