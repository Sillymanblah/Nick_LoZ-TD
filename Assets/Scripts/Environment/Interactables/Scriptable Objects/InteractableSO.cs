using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Interactable", menuName ="Interactable/Interactable")]
public class InteractableSO : ScriptableObject
{
    public virtual void DoThing(PlayerManager player)
    {
        Debug.Log("We did a thing");
    }
}
