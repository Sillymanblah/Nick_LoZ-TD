using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public int index;

    bool mouseOver;

    [Header("Stats")]
    public int cost;

    [Space]
    [SerializeField] float speed;
    Quaternion originalRotation;
    Quaternion lastRotation;
    float rotInterpolation;

    #region Frontend

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI costText;

    [SerializeField] GameObject costUi;
    [SerializeField] GameObject selectBorderUI;

    PlayerStateManager localPlayer;

    #endregion

    void Start()
    {
        originalRotation = transform.GetChild(0).localRotation;

        nameText.enabled = false;
        descriptionText.enabled = false;
        costUi.SetActive(false);
        selectBorderUI.SetActive(false);
    }

    void Update()
    {
        if (mouseOver)
        {
            //selectBorderUI.transform.LookAt(selectBorderUI.transform.position + Camera.main.transform.forward);
            selectBorderUI.transform.LookAt(Camera.main.transform.position);

            var borderSprite = selectBorderUI.transform.GetChild(0);
            borderSprite.LookAt(borderSprite.position + Camera.main.transform.forward);

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

    public void InitializeItem(int waveNumber, IngameShopItemSO item)
    {
        nameText.text = item.name;
        nameText.color = item.color;
        cost = item.cost * (int)Mathf.Pow(10, waveNumber);

        // The prefab, its new position, its new rotation, its parent
        GameObject newItem = Instantiate(item.modelPrefab, transform.GetChild(0), false);
        newItem.transform.localPosition = item.newPosition;
        newItem.transform.localScale = item.newScale;
        newItem.transform.localRotation = Quaternion.Euler(item.newRotation);

        costText.text = cost.ToString();
        descriptionText.text = item.description;
    }


    // for clients
    [ClientCallback]
    public void OnStartUp(int index, PlayerStateManager player)
    {
        this.index = index;
        localPlayer = player;
    }

    /// <summary>
    /// Called every frame while the mouse is over the GUIElement or Collider.
    /// </summary>
    private void OnMouseOver()
    {
        if (localPlayer.CurrentState() != localPlayer.ShopState) return;

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
