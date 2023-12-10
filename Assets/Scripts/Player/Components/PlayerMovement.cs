using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheNicksin.Inputsystem;

public class PlayerMovement : MonoBehaviour
{
    Transform cam;
    private CharacterController controller;
    [SerializeField] private float speed;
    [SerializeField] PlayerStateManager playerSM;
    PlayerManager playerManager;

    private Vector3 velocity;
    private Vector3 moveVelocity;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] bool isGrounded = false;
    public bool IsGrounded() { return isGrounded; }
    //[SerializeField] bool isJumping = false;
    [SerializeField] Transform groundCheck = null;
    [SerializeField] public LayerMask groundMask;
    
    float turnSmoothVelocity;
    [SerializeField] float turnSmoothTime = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        playerSM = GetComponent<PlayerStateManager>();
        playerManager = GetComponent<PlayerManager>();
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckBox(groundCheck.position, new Vector3(0.15f, 0f, 0.15f), Quaternion.identity, groundMask);   
    }
    public void MovePlayer(PlayerBaseState player)
    {
        AdjustSpeed(player);

        var InputMan = playerSM.inputManager;

        float horizontal = InputMan.AxisMovementValue(InputMan.right, InputMan.left);
        float vertical = InputMan.AxisMovementValue(InputMan.forward, InputMan.backward);

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        moveVelocity = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            moveVelocity = moveDir.normalized * speed;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
    }

    public void AdjustSpeed(PlayerBaseState playerState)
    {
        switch (playerState)
        {
            case PlayerWalkingState:
                speed = 2;
                break;
            case PlayerRunningState:
                speed = 4;
                break;
            case PlayerCrouchState:
                speed = 1;
                break;
            default:
                speed = 2;
                break;
        }
    }
    public void PlayerJumped()
    {
        if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void PlayerGravityMovement()
    {
        velocity.y += gravity * Time.deltaTime;

        var newVelocity = new Vector3(moveVelocity.x, velocity.y, moveVelocity.z);
        controller?.Move(newVelocity * Time.deltaTime);

        //if (isGrounded) playerSM.SwitchState(playerSM.GroundedState);
    }

    public void ResetGround()
    {
        moveVelocity = Vector3.zero;
    }
    public void JustPlayerGravity()
    {
        velocity.y += gravity * Time.deltaTime;

        if (velocity.y < -5)
            velocity.y = -5;

        controller.Move(velocity * Time.deltaTime);
    }

    public void FreezePlayer(bool freeze)
    {
        speed = freeze ? 0 : 2; 
    }
}
