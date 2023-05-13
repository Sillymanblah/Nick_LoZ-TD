using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIWaveBar : MonoBehaviour
{
    [SerializeField] Slider waveSlider;
    [SerializeField] TextMeshProUGUI waveCountText;

    // Start is called before the first frame update
    void Start()
    {
        //waveSlider = transform.GetChild(0).GetComponent<Slider>();
        //waveCountText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        

        
        waveSlider.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (WaveManager.instance == null) return;

        var wavemanager = WaveManager.instance;

        waveSlider.maxValue = wavemanager.waveSize * wavemanager.groupSize;
        waveSlider.value = WaveManager.instance.totalEnemiesSpawned;
        waveCountText.text = "Wave " + (WaveManager.instance.currentWave + 1) + "/" + 3;
    }
}
