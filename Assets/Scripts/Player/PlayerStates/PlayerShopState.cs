using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShopState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        Debug.Log($"Entered PlayerShopState");
        player.playerMovement.FreezePlayer(true);
        player.playerAnimations.IdleAnimation();
        player.StartCoroutine(MovementDetection(player));
    }

    public override void UpdateState(PlayerStateManager player)
    {
        InputsForState(player);

        Grotto.instance.ShopItemRayCast(player.connectionToClient, player.playerUnit.money);
    }

    IEnumerator MovementDetection(PlayerStateManager player)
    {
        yield return new WaitForSeconds(2.0f);

        var InputMan = player.inputManager;

        while (true)
        {
            float horizontal = InputMan.AxisMovementValue(InputMan.right, InputMan.left);
            float vertical = InputMan.AxisMovementValue(InputMan.forward, InputMan.backward);

            // this is for if we move
            if (new Vector2(horizontal, vertical) != Vector2.zero)
            {
                if (Grotto.instance != null)
                    player.playerManager.SwitchCamera();

                player.SwitchState(player.GroundedState);
                yield break;
            }

            yield return null;
        }
    }

    void InputsForState(PlayerStateManager player)
    {
        if (Input.GetKey(player.inputManager.jump))
        {
            player.playerAnimations.PlayJumpSound();
            player.playerMovement.PlayerJumped();

            if (Grotto.instance != null)
                player.playerManager.SwitchCamera();

            player.SwitchState(player.FallingState);
        }
    }
}
