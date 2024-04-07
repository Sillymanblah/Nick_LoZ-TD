using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class UICentralBarSystem : MonoBehaviour
{
    enum BarModes
    {
        Wave,
        Boss,
        Intermission
    }

    [SerializeField] TextMeshProUGUI labelTextUI;
    [SerializeField] Slider barSlider;
    [SerializeField] Image fillUI;

    BarModes barModes;

    public static UICentralBarSystem instance;

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
        barSlider.value = 0;
        this.gameObject.SetActive(false);
    }

    // If wavemax = 0, display no wave max
    public void StartWaveBar(int waveNum, int waveMax, int maxEnemyValue, int wavePercentage)
    {
        this.gameObject.SetActive(true);

        barModes = BarModes.Wave;
        if (waveMax == 0) labelTextUI.text = "Wave " + waveNum;
        else labelTextUI.text = $"Wave {waveNum}/{waveMax}";

        barSlider.value = wavePercentage;
        barSlider.maxValue = maxEnemyValue;


        fillUI.color = Color.green;
    }

    public void UpdateWaveValue(float wavePercentage, float maxValue)
    {
        if (barModes != BarModes.Wave) return;

        barSlider.maxValue = maxValue;
        barSlider.value = wavePercentage;
    }

    public void StartBossBar(string enemyName, int healthValue, int maxHealth)
    {
        this.gameObject.SetActive(true);

        barModes = BarModes.Boss;

        labelTextUI.text = $"{enemyName} {healthValue}/{maxHealth}";
        barSlider.maxValue = maxHealth;
        barSlider.value = maxHealth;

        fillUI.color = Color.red;
    }

    public void UpdateBossValue(string enemyName, int healthValue, int maxValue)
    {
        if (barModes != BarModes.Boss) return;

        int newHealthValue = healthValue;
        newHealthValue = Mathf.Clamp(newHealthValue, 0, maxValue);


        labelTextUI.text = $"{enemyName} {newHealthValue}/{maxValue}";
        barSlider.value = healthValue;
    }

    public void StartIntermissionBar(int maxTime)
    {
        this.gameObject.SetActive(true);

        barModes = BarModes.Intermission;

        labelTextUI.text = $"Intermission {maxTime} seconds";
        barSlider.maxValue = maxTime;
        barSlider.value = maxTime;

        fillUI.color = Color.cyan;
    }

    public void UpdateIntermissionValue(int timer)
    {
        if (barModes != BarModes.Intermission) return;

        labelTextUI.text = $"Intermission {timer} seconds";
        barSlider.value = timer;
    }
}
