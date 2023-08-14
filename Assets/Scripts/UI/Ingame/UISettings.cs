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
    [SerializeField] Slider musicSlider;
    [SerializeField] TextMeshProUGUI musicSliderValueText;

    #region Static Variables

    public static UISettings Singleton;
    static float musicSliderValueNum;

    #endregion

    private void Awake()
    {
        Singleton = this;
        musicSlider.value = musicSliderValueNum;
        
    }
    // Start is called before the first frame update
    void Start()
    {
        mainCanvas = GetComponent<Canvas>();
        mainCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        AdjustMusicSlider();

        //if (MoveData.mainMenu) return;

        if (Input.GetKeyDown(KeyCode.Escape)) PauseGame();

    }

    public void PauseGame()
    {
        settingsOn = !settingsOn;
        mainCanvas.enabled = settingsOn;

        //if (MoveData.mainMenu) return;

        if (settingsOn)
            Time.timeScale = 0;
        else Time.timeScale = 1;
    }
    
    void AdjustMusicSlider()
    {
        musicSliderValueNum = musicSlider.value;

        musicSliderValueText.text = Mathf.RoundToInt(musicSliderValueNum * 100) + "%";
        //AudioManager.SingleTon.audioSource.volume = musicSliderValueNum;
    }
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
