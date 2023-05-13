using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradePreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Button button;

    // Start is called before the first frame update
    void Start()
    {  
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIUnitStats.SingleTon.PreviewStats();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIUnitStats.SingleTon.SetStats();
    }
}
