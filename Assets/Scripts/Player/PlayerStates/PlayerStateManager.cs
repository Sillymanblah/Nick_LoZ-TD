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
    public PlayerMovement playerMovement;
    public InputManager inputManager;

    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer) return;

        playerAnimations = GetComponent<PlayerAnimationManager>();
        playerMovement = GetComponent<PlayerMovement>();
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();

        currentState = FallingState;

        currentState.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        currentState.UpdateState(this);
    }

    public void SwitchState(PlayerBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }
}
