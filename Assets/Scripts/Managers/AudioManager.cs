using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class AudioManager : MonoBehaviour
{
    #region music
    [SerializeField] AudioClip currentClip;
    [SerializeField] AudioClip gameOver;

    #endregion
    [SerializeField] AudioSource audioSource;
    [SerializeField] Slider volumeSlider;
    [SerializeField] TextMeshProUGUI volumeValueText;

    [SerializeField] bool resetKeys;
     float volume = 1f;

    #region 

    [Space]
    [SerializeField] AudioClip ErrorSound;

    #endregion
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        if (CSNetworkManager.instance.DeployingAsServer) return;

        

        Debug.Log($"after basemanager in audiomanager");

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
        StartCoroutine(SubscribeToBase());
    }

    // for some weird fucking reason, audiomanager START function is being called BEFORE the awak function IN THE BASE MANAGER
    // and script execution order WILL NOT FIX IT
    // this only happens upon builds
    IEnumerator SubscribeToBase()
    {
        while (BaseManager.instance == null)
        {
            yield return null;
        }
        BaseManager.instance.OnBaseDead += BaseDeadMusic;
        Debug.Log($"basemanager subscribed");
    }

    
    

    // Update is called once per frame
    void Update()
    {
        if (CSNetworkManager.instance.DeployingAsServer) return;

        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(currentClip, 1);

    }

    public void ChangeVolume(float value)
    {
        Debug.Log($"brother on audiomanager bruh");

        volume = value;

        audioSource.volume = value;

        volumeValueText.text = string.Format("{0:0}", (value * 100f)) + "%";

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    void SetSliderValue()
    {
        

        volume = PlayerPrefs.GetFloat("MusicVolume");
        Debug.Log(volume);

        volumeSlider.value = volume;
        Debug.Log(volumeSlider.value);
        volumeValueText.text = string.Format("{0:0}", (volume * 100f)) + "%";
    }

    void BaseDeadMusic(object sender, GameManagerEventArgs e)
    {
        Debug.Log($"basedeadmusic");
        audioSource.Stop();
        audioSource.PlayOneShot(gameOver, 1);
    }
}
