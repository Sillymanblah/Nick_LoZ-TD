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

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {

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
