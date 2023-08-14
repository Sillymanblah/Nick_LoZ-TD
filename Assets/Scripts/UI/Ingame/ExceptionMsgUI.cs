using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ExceptionMsgUI : MonoBehaviour
{
    public static ExceptionMsgUI instance;
    [SerializeField] TextMeshProUGUI exceptionText;

    private void Start()
    {
        instance = this;
    }

    public void UIExceptionMessage(string message)
    {
        StopCoroutine("StartExceptionAnimation");
        exceptionText.text = message;
        
        StartCoroutine("StartExceptionAnimation");
    }

    IEnumerator StartExceptionAnimation()
    {
        exceptionText.color = new Color(1,1,1,1);
        yield return new WaitForSeconds(1.0f);
        bool active = false;
        var alphaColor = 1f;
        
        while(active == false)
        {
            exceptionText.color = new Color(1, 1, 1, alphaColor -= Time.deltaTime);
            if (alphaColor <= 0)
                active = true;

            yield return null;
        }
    }
}
