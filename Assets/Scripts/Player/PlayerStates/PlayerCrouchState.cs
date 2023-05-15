using UnityEngine;
using TheNicksin.Inputsystem;

public class PlayerCrouchState : PlayerBaseState
{
    bool keepCrouch;
    public override void EnterState(PlayerStateManager player)
    {
        Debug.Log($"Entered Crouching State");
        player.controller.height = 1.34f;
        player.controller.center = new Vector3(0, -0.31f, 0);

        player.playerManager.AdjustHeadHeight();
    }

    public override void UpdateState(PlayerStateManager player)
    {
        if (!player.playerMovement.IsGrounded()) player.SwitchState(player.FallingState);

        MovementDetection(player);
        InputsForState(player);
        DetectCrouchHeight(player.playerManager);
        player.playerMovement.JustPlayerGravity();

        player.playerMovement.MovePlayer(this);
    }

    void MovementDetection(PlayerStateManager player)
    {
        var InputMan = player.inputManager;

        float horizontal = InputMan.AxisMovementValue(InputMan.right, InputMan.left);
        float vertical = InputMan.AxisMovementValue(InputMan.forward, InputMan.backward);

        if (new Vector2(horizontal, vertical) != Vector2.zero)
        {
            player.playerAnimations.CrouchWalkingAnimation();
        } else player.playerAnimations.CrouchIdleAnimation();
    }

    void InputsForState(PlayerStateManager player)
    {
        var InputMan = player.inputManager;

        Debug.Log(Input.GetKey(InputMan.crouch));
        if (Input.GetKey(InputMan.crouch) == false && keepCrouch == false)
        {
            player.SwitchState(player.GroundedState);
            Debug.Log($"after input");
        }
    }

    void DetectCrouchHeight(PlayerManager player)
    {
        if (!Physics.Raycast(player.HeadPos().position, Vector3.up, 1f, player.playerMovement.groundMask))
        {
            keepCrouch = false;
            return;
        }

        keepCrouch = true;
    }
}
