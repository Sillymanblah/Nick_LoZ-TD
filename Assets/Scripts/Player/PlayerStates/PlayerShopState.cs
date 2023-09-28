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
        player.StartCoroutine(IdleDetection(player));
    }

    public override void UpdateState(PlayerStateManager player)
    {
        InputsForState(player);
    }

    IEnumerator IdleDetection(PlayerStateManager player)
    {
        yield return new WaitForSeconds(2.0f);

        var InputMan = player.inputManager;

        float horizontal = InputMan.AxisMovementValue(InputMan.right, InputMan.left);
        float vertical = InputMan.AxisMovementValue(InputMan.forward, InputMan.backward);

        while (true)
        {
            if (new Vector2(horizontal, vertical) != Vector2.zero)
            {
                Debug.Log($"muthahumpa");

                if (Grotto.instance != null)
                    Grotto.instance.SwitchCamera(Grotto.instance.thirdPovCam);

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
            player.playerMovement.PlayerJumped();

            if (Grotto.instance != null)
                Grotto.instance.SwitchCamera(Grotto.instance.thirdPovCam);

            player.SwitchState(player.FallingState);
        }

    }
}
