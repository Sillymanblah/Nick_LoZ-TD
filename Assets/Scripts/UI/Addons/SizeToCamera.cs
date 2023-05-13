using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SizeToCamera : MonoBehaviour
{
    Camera cam;
    TextMeshProUGUI text;
    [SerializeField] [Range(1, 100)] float sizeMultiplier = 1;
    
    private void Start()
    {
        cam = Camera.main;
        text = GetComponent<TextMeshProUGUI>();
    }
    void Update ()
    {
        if (text != null)
        {
            text.fontSize = Vector3.Distance(transform.position, cam.transform.position) * (0.01f * sizeMultiplier);
        }
        else
        {
            // transforming the scale of Vector3(x, y)
            GetComponent<RectTransform>().localScale = new Vector3(Vector3.Distance(transform.position, cam.transform.position) * (0.01f * sizeMultiplier),
                                       Vector3.Distance(transform.position, cam.transform.position) * (0.01f * sizeMultiplier), 1);
            
        }

        transform.LookAt(transform.position + cam.transform.forward);
    }
}
