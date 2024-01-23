using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICentralBarSystem : MonoBehaviour
{
    TextMeshProUGUI labelTextUI;
    Slider barSlider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // If wavemax = 0, display no wave max
    public void StartWaveBar(int waveNum, int waveMax)
    {
        if (waveMax == 0) labelTextUI.text = "Wave " + waveNum;

        labelTextUI.text = $"Wave {waveNum}/{waveMax}";
    }

    public void UpdateWaveBar(float wavePercentage)
    {

    }
}
