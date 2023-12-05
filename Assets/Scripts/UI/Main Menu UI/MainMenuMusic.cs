using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuMusic : MonoBehaviour
{
    #region Music

    AudioClip currentClip;
    [SerializeField] AudioClip StartMusic;
    [SerializeField] AudioClip MenuMusic;
    AudioSource audioSource;
    [SerializeField] Slider volumeSlider;
    [SerializeField] TextMeshProUGUI volumeValueText;

    float volume;


    #endregion

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", 0.5f);
            PlayerPrefs.Save();
            volume = PlayerPrefs.GetFloat("MusicVolume");
            audioSource.volume = volume;
        }
        else
        {
            volume = PlayerPrefs.GetFloat("MusicVolume");
            audioSource.volume = volume;
        }

        SetSliderValue();
    }

    private void Update()
    {
        if (audioSource == null) return;

        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(currentClip, 1);
    }

    public void PlayMenuMusic()
    {
        audioSource.Stop();

        if (MenuMusic == null) return;

        audioSource.PlayOneShot(MenuMusic, 1);
        currentClip = MenuMusic;
    }

    public void PlayStartMusic()
    {
        audioSource.PlayOneShot(StartMusic, 1);
        
        currentClip = MenuMusic;
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
        if (volumeValueText == null)
        {
            Debug.LogWarning($"Audio Settings missing");
            return;
        }

        volume = PlayerPrefs.GetFloat("MusicVolume");

        volumeSlider.value = volume;
        volumeValueText.text = string.Format("{0:0}", (volume * 100f)) + "%";
    }


}
