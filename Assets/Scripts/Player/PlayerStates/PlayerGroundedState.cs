using UnityEngine;
using TheNicksin.Inputsystem;

public class PlayerGroundedState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        Debug.Log($"Entered ground state");
        player.playerMovement.FreezePlayer(false);
    }

    public override void UpdateState(PlayerStateManager player)
    {
        // Puts player in falling state if not grounded
        if (!player.playerMovement.IsGrounded()) player.SwitchState(player.FallingState); 
        
        MovementDetection(player);
        InputsForState(player);
        player.playerMovement.JustPlayerGravity();

        Emoting(player.playerAnimations);
    }

    void MovementDetection(PlayerStateManager player)
    {
        player.playerAnimations.IdleAnimation();
        var InputMan = player.inputManager;

        float horizontal = InputMan.AxisMovementValue(InputMan.right, InputMan.left);
        float vertical = InputMan.AxisMovementValue(InputMan.forward, InputMan.backward);

        // if we arent moving, set velocity to 0
        if (new Vector2(horizontal, vertical) != Vector2.zero) player.SwitchState(player.WalkingState);
        else player.playerMovement.ResetGround();
    }

    void Emoting(PlayerAnimationManager player)
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            player.FlossingAnimation();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            //player.HandWaveAnimation();
        }
    }

    void InputsForState(PlayerStateManager player)
    {
        var InputMan = player.inputManager;

        if (Input.GetKey(InputMan.jump))
        {
            player.playerMovement.PlayerJumped();
            player.SwitchState(player.FallingState);
        }
    }
}
