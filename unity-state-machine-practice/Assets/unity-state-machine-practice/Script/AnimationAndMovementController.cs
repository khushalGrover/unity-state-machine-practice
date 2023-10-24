using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class AnimationAndMovementController : MonoBehaviour
{
    // declare reference variables
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    // variables to store optimized setter/getter paramete IDS
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;

    // variables to store player input values
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    bool isMovementPressed;
    bool isRunPressed;

    // const 
    [SerializeField]
    float rotationFactorPerFrame = 1.0f;
    [SerializeField]
    float speedFactorPerFrame = 1.0f;
    [SerializeField]
    float runMultiplier = 3.0f;
    int zero = 0;

    // gravity variables
    float gravity = -9.8f;
    float groundGravity = -.05f;

    // jumping vairables
    bool isJumpPressed = false;
    [SerializeField]
    float initialJumpVelocity;
    [SerializeField]
    float maxJumpHeight = 4.0f;
    [SerializeField]
    float maxJumpTime = 0.75f;
    bool isJumping = false;
    bool isJumpAnimating = false;

    private void Awake()
    {
        // initially set reference variables
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // set the parameter hash references
        isWalkingHash = Animator.StringToHash("isWalking"); 
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        // Set the player input callbacks ( This callback function are typilcally called when certain events take place, and Provode data for us to use from the event. )
        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;
        playerInput.CharacterControls.Jump.started += onJump;
        playerInput.CharacterControls.Jump.canceled += onJump;

        setupJumpVariables();

    }

    void setupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }


    void handleJump()
    {
        if(!isJumping && characterController.isGrounded && isJumpPressed)
        {
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;
            currentMovement.y = initialJumpVelocity * .5f;
            currentRunMovement.y = initialJumpVelocity * .5f;
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded)
        {
            // animator.SetBool(isJumpingHash, false);
            isJumping = false;
        }
    }
   
    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    // handler function to set the player input values
    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x * speedFactorPerFrame;
        currentMovement.z = currentMovementInput.y * speedFactorPerFrame;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }
    void handleRotation()
    {
        Vector3 positionToLookAt;
        // the change in position our character should point to
        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0;
        positionToLookAt.z = currentMovement.z;
        // the current rotation of our character
        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            // create a new rotation based on where the player is currently pressing
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }

    }
    void handleAnimation()
    {
        // get pararmeter values from animator
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        // start walking if movement presed is true and not aleady waling
        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
            Debug.Log("start walking");
        }
        // stop walking if isMovementpressed is false and not already walking
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }
        // run if movement and run presed are true and not currently running
        if((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }
        // stop running if movement or run pressed are false and currently running
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }

    }

    void handleGravity()
    {
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplayer = 2.0f;
        // apply proper gravity depending on if the character is ground or not
        if(characterController.isGrounded)
        {
            if(isJumpAnimating)
            {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;
            }
            currentMovement.y = groundGravity;
            currentRunMovement.y = groundGravity;
        }else if (isFalling)
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (gravity * fallMultiplayer * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            currentMovement.y = nextYVelocity; 
            currentRunMovement.y = nextYVelocity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        handleRotation();
        handleAnimation();

        if (isRunPressed) {
            characterController.Move(currentRunMovement * Time.deltaTime); 
        } else {
            characterController.Move(currentMovement *Time.deltaTime);
        }

        handleGravity();
        handleJump();
    }

    private void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
}
