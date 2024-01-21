using UnityEngine;

public class PlayerFallingState : PlayerBaseState
{

    public override void EnterState(PlayerStateManager player)
    {
        //player.playerMovement.PlayerJumped();
        Debug.Log($"Entered Falling State");
        player.playerManager.SwitchCamera();
        player.playerAnimations.PlayJumpSound();
    }

    public override void UpdateState(PlayerStateManager player)
    {
        player.playerAnimations.MidAirJumpAnimation();
        player.playerMovement.PlayerGravityMovement();

        if (player.playerMovement.IsGrounded()) player.SwitchState(player.GroundedState);
    }
}
