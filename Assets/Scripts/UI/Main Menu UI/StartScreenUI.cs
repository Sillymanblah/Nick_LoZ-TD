using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenUI : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip buttonSound;
    [SerializeField] MainMenuMusic mainMenuMusic;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        mainMenuMusic.PlayStartMusic();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKeyDown)
        {
            audioSource.PlayOneShot(buttonSound);
            StartCoroutine(TransitionToMainMenu());
        }
    }

    IEnumerator TransitionToMainMenu()
    {
        yield return new WaitForSeconds(.5f);
        
        SceneManager.LoadScene(1);
    }
}
