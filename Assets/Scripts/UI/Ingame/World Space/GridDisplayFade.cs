using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class GridDisplayFade : MonoBehaviour
{
    List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    Transform cellParent;
    static float cycleFactor = 5f;
    public bool isWhite = true;

    IEnumerator StartFade()
    {
        foreach (Transform child in cellParent)
        {
            spriteRenderers.Add(child.GetComponent<SpriteRenderer>());
        }

        while (true)
        {
            foreach (SpriteRenderer child in spriteRenderers)
            {
                if (isWhite)
                    child.color = new Color(1,1,1, Mathf.Abs(Mathf.Sin(Time.time * cycleFactor) / 3) + 0.2f);
                else
                    child.color = new Color(1,0,0, Mathf.Abs(Mathf.Sin(Time.time * cycleFactor) / 3) + 0.2f);
            }
            yield return null;
        }
    }

    public void ToggleCellDisplay(bool active)
    {
        cellParent = this.transform;
        cellParent.gameObject.SetActive(active);

        if (active)
        {
            StopCoroutine(nameof(StartFade));
            StartCoroutine(nameof(StartFade));
        }
        else StopCoroutine(nameof(StartFade));
        
    }
}
