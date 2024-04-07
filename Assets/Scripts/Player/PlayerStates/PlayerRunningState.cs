using UnityEngine;
using TheNicksin.Inputsystem;

public class PlayerRunningState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        Debug.Log($"Entered Running State");
    }

    public override void UpdateState(PlayerStateManager player)
    {
        if (!player.playerMovement.IsGrounded()) player.SwitchState(player.FallingState); 

        IdleDetection(player);
        InputsForState(player);
        PlayerInputs(player);
        player.playerMovement.JustPlayerGravity();

        player.playerMovement.MovePlayer(this);
        player.playerAnimations.RunningAnimation();
    }

    void IdleDetection(PlayerStateManager player)
    {
        var InputMan = player.inputManager;

        float horizontal = InputMan.AxisMovementValue(InputMan.right, InputMan.left);
        float vertical = InputMan.AxisMovementValue(InputMan.forward, InputMan.backward);

        if (new Vector2(horizontal, vertical) == Vector2.zero) player.SwitchState(player.GroundedState);
    }

    void InputsForState(PlayerStateManager player)
    {
        var InputMan = player.inputManager;

        if (Input.GetKeyUp(InputMan.run))
            player.SwitchState(player.WalkingState);
    

        else if (Input.GetKey(InputMan.jump))
        {
            player.playerAnimations.PlayJumpSound();
            player.playerMovement.PlayerJumped();
            player.SwitchState(player.FallingState);
        }
        //else if (Input.GetKey(InputMan.crouch))
        //    player.SwitchState(player.CrouchState);
    }

    void PlayerInputs(PlayerStateManager player)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (player.interactable != null)
            {
                player.interactable.DoInteractableThing(player.playerManager); 
                return;
            }
        }

        player.playerUnit.UnitSelectionOptions();
    }
}
