using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarLogic : MonoBehaviour
{
    [SerializeField] GameObject hpBar;

    /// <summary>
    /// Called every frame while the mouse is over the GUIElement or Collider.
    /// </summary>
    private void OnMouseOver()
    {
        hpBar.SetActive(true);
        Debug.Log($"activated");
    }

    /// <summary>
    /// Called when the mouse is not any longer over the GUIElement or Collider.
    /// </summary>
    private void OnMouseExit()
    {
        hpBar.SetActive(false);
        Debug.Log($"deactivated");
    }
}
