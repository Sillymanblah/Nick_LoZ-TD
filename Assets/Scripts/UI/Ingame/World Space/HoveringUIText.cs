using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder;

public class HoveringUIText : MonoBehaviour
{
    [SerializeField] AnimationCurve animationCurve;
    float speed;
    [SerializeField] TextMeshProUGUI thisUIText;

    private void Awake()
    {
        thisUIText = GetComponent<TextMeshProUGUI>();
        
    }
    public void StartAnimation(string message)
    {
        thisUIText.text = message;
        StopCoroutine(nameof(AnimatingUI));
        StartCoroutine(nameof(AnimatingUI));
    }

    IEnumerator AnimatingUI()
    {
        speed = 0.1f;
        float setTime = 0;
        transform.localPosition = new Vector3(0, -0.5f, 0);
        float alphaValue = 1f;
        var needThisforColor = GetComponent<TextMeshProUGUI>();
        needThisforColor.color = new Color(needThisforColor.color.r, needThisforColor.color.g, needThisforColor.color.b, 1);

        while (setTime < 1f)
        {
            speed = animationCurve.Evaluate(setTime);
            setTime += Time.deltaTime;
            transform.Translate(Vector2.up * (speed * Time.deltaTime) / 3);
            

            if (setTime > 0.5f)
            {
                alphaValue -= Time.deltaTime * 2;
                needThisforColor.color = new Color(needThisforColor.color.r, needThisforColor.color.g, needThisforColor.color.b, alphaValue);
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
