using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISettings : MonoBehaviour
{
    Canvas mainCanvas;
    bool settingsOn = false;
    public bool GetMainMenu() { return settingsOn; }

    #region Static Variables

    public static UISettings Singleton;
    static float musicSliderValueNum;

    #endregion

    [Header("Setting Sliders")]
    
    // Music settings are changed in the AudioManager script
    //[SerializeField] Slider musicSlider;
    //[SerializeField] TextMeshProUGUI musicSliderValueText;

    [Space]
    [SerializeField] Slider soundFXVolumeSliderUI;
    [SerializeField] TextMeshProUGUI soundFXVolumeValueUI;

    [Space]
    [SerializeField] Slider mouseSensSliderUI;
    [SerializeField] TextMeshProUGUI mouseSensValueUI;
    public event EventHandler<MouseSenSettings> onMouseSensValueChanged;

    private void Awake()
    {
        Singleton = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("SoundFXVol"))
        {
            PlayerPrefs.SetFloat("SoundFXVol", 0.5f);
        }

        if (!PlayerPrefs.HasKey("Mouse Sensitivity"))
        {
            PlayerPrefs.SetFloat("Mouse Sensitivity", 0.5f);
        }

        PlayerPrefs.Save();

        SetSoundFXSliderValue();
        SetMouseSensSliderValue();

        // Call every player's camera controls here
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PauseGame()
    {
        settingsOn = !settingsOn;
        mainCanvas.enabled = settingsOn;

        //if (MoveData.mainMenu) return;

        /*if (settingsOn)
            Time.timeScale = 0;
        else Time.timeScale = 1;*/
    }
    
    /*void AdjustMusicSlider()
    {


        musicSliderValueNum = musicSlider.value;

        musicSliderValueText.text = Mathf.RoundToInt(musicSliderValueNum * 100) + "%";
    }*/

    #region Sound Effects Volume

    public void ChangeSoundFXVolume(float value)
    {
        soundFXVolumeValueUI.text = string.Format("{0:0}", (value * 100f)) + "%";

        PlayerPrefs.SetFloat("SoundFXVol", value);
        PlayerPrefs.Save();
    }

    void SetSoundFXSliderValue()
    {
        if (soundFXVolumeValueUI == null)
        {
            Debug.LogWarning($"Audio Settings missing");
            return;
        }

        float volume = PlayerPrefs.GetFloat("SoundFXVol");

        soundFXVolumeSliderUI.value = volume;
        soundFXVolumeValueUI.text = string.Format("{0:0}", volume * 100f) + "%";
    }

    #endregion

    #region Mouse Sensitivity

    public void ChangeMouseSensitivity(float value)
    {
        mouseSensValueUI.text = string.Format("{0:0}", (value * 100f)) + "%";
        onMouseSensValueChanged?.Invoke(this, new MouseSenSettings { sensValue = value });


        PlayerPrefs.SetFloat("Mouse Sensitivity", value);
        PlayerPrefs.Save();
    }

    void SetMouseSensSliderValue()
    {
        if (mouseSensValueUI == null)
        {
            Debug.LogWarning($"Mouse Settings missing");
            return;
        }

        float value = PlayerPrefs.GetFloat("Mouse Sensitivity");

        mouseSensSliderUI.value = value;
        mouseSensValueUI.text = string.Format("{0:0}", value * 100f) + "%";
    }

    #endregion

    public void ContinueButton()
    {
        Debug.Log($"Continue game");
        PauseGame();
    }

    public void RestartButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void QuitButton()
    {
        Debug.Log($"Quit the game");
        Application.Quit();
    }
}

public class MouseSenSettings : EventArgs
{
    public float sensValue;
}
