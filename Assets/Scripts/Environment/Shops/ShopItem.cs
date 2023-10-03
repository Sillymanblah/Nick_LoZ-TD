using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopItem : MonoBehaviour
{
    public int index;
    

    public bool mouseOver;

    [Tooltip("Higher is faster | A value of 10 represents 1 second")]
    [SerializeField] float speed = 10;
    Quaternion originalRotation;
    Quaternion lastRotation;
    float rotInterpolation;

    #region Frontend

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI costText;

    [SerializeField] GameObject costUi;
    [SerializeField] GameObject selectBorderUI;



    #endregion

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = transform.GetChild(0).localRotation;

        nameText.enabled = false;
        descriptionText.enabled = false;
        costUi.SetActive(false);
        selectBorderUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseOver)
        {
            transform.GetChild(0).RotateAround(transform.position, Vector3.up, speed * Time.deltaTime);
            lastRotation = transform.GetChild(0).localRotation;
            rotInterpolation = 1;
        }
        // stop animating
        else
        {
            if (rotInterpolation > 0)
            {
                rotInterpolation -= Time.deltaTime * 2;
                transform.GetChild(0).localRotation = Quaternion.Lerp(originalRotation, lastRotation, rotInterpolation);
            }
        }
    }

    public void InitializeItem(IngameShopItemSO item)
    {
        
       // descriptionText.enabled = true;


        nameText.text = item.name;
        nameText.color = item.color;

        costText.text = item.cost.ToString();

        descriptionText.text = item.description;

        
        //descriptionText.color = item.color;
    }

    /// <summary>
    /// Called every frame while the mouse is over the GUIElement or Collider.
    /// </summary>
    private void OnMouseOver()
    {
        mouseOver = true;
        descriptionText.enabled = true;
        nameText.enabled = true;
        costUi.SetActive(true);
        selectBorderUI.SetActive(true);
    }

    /// <summary>
    /// Called when the mouse is not any longer over the GUIElement or Collider.
    /// </summary>
    private void OnMouseExit()
    {
        mouseOver = false;
        descriptionText.enabled = false;
        nameText.enabled = false;
        costUi.SetActive(false);
        selectBorderUI.SetActive(false);
    }
}
