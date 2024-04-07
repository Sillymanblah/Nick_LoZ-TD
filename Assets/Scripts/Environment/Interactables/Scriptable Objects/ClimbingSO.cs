using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Interactable", menuName ="Interactable/Climbing Interactable")]
public class ClimbingSO : InteractableSO
{
    public override void DoThing(PlayerManager player)
    {
        var playerStateManager = player.playerStateManager;

        playerStateManager.SwitchState(playerStateManager.ClimbingState);
    }
}
