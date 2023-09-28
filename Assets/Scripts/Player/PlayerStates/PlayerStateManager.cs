using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TheNicksin.Inputsystem;

public class PlayerStateManager : NetworkBehaviour
{
    public PlayerManager playerManager;
    public PlayerAnimationManager playerAnimations;
    public CharacterController controller;
    PlayerBaseState currentState;
    public PlayerBaseState CurrentState() { return currentState; }
    public PlayerGroundedState GroundedState = new PlayerGroundedState();
    public PlayerFallingState FallingState = new PlayerFallingState();
    public PlayerCrouchState CrouchState = new PlayerCrouchState();
    public PlayerWalkingState WalkingState = new PlayerWalkingState();
    public PlayerRunningState RunningState = new PlayerRunningState();
    public PlayerShopState ShopState = new PlayerShopState();
    public PlayerMovement playerMovement;
    public InputManager inputManager;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = GetComponent<PlayerManager>();

        

        if (!isLocalPlayer) return;

        playerAnimations = GetComponent<PlayerAnimationManager>();
        playerMovement = GetComponent<PlayerMovement>();
        inputManager = GetComponent<InputManager>();

        currentState = FallingState;

        if (!playerManager.ingame) return;
        
        currentState.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerManager.ingame) return;

        if (!isLocalPlayer) return;

        currentState.UpdateState(this);
    }

    public void SwitchState(PlayerBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }
}
