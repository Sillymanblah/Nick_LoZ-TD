using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbingState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        Debug.Log($"Entered Player Climbing State");
        player.playerMovement.TeleportPlayer(player.interactable.transform.position - (player.interactable.transform.forward / 2));
    }

    public override void UpdateState(PlayerStateManager player)
    {
        player.playerUnit.UnitSelectionOptions();
        player.playerMovement.Climbing();
    }
}
