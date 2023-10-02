using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopItem : MonoBehaviour
{
    public int index;

    public bool mouseOver;

    [Tooltip("Higher is faster | A value of 10 represents 1 second")]
    [SerializeField] float speed = 10;

    Vector3 originalPosition;
    Quaternion originalRotation;

    Quaternion lastRotation;

    float posInterpolation;
    float rotInterpolation;
    bool goUp = true;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Called every frame while the mouse is over the GUIElement or Collider.
    /// </summary>
    private void OnMouseOver()
    {
        mouseOver = true;
    }

    /// <summary>
    /// Called when the mouse is not any longer over the GUIElement or Collider.
    /// </summary>
    private void OnMouseExit()
    {
        mouseOver = false;
    }
}
