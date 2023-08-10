using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip currentClip;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Slider volumeSlider;
    [SerializeField] TextMeshProUGUI volumeValueText;

    [SerializeField] bool resetKeys;
     float volume = 1f;
    
    // Start is called before the first frame update
    void Start()
    {
        if (resetKeys)
            PlayerPrefs.DeleteKey("MusicVolume");

        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", 0.5f);
            PlayerPrefs.Save();
            volume = PlayerPrefs.GetFloat("MusicVolume");
            Debug.Log(volume);
        }

        SetSliderValue();
        audioSource.PlayOneShot(currentClip, volume);
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(currentClip, volume);

    }

    public void ChangeVolume(float value)
    {
        volume = value;

        audioSource.volume = volume;

        volumeValueText.text = string.Format("{0:0}", (value * 100f)) + "%";

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    void SetSliderValue()
    {
        volume = PlayerPrefs.GetFloat("MusicVolume");

        volumeSlider.value = volume;
        volumeValueText.text = string.Format("{0:0}", (volume * 100f)) + "%";
    }
}
