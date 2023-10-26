using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class AnimationAndMovementController : MonoBehaviour
{
    // declare reference variables
    PlayerInput _playerInput;
    CharacterController _characterController;
    Animator _animator;

    // variables to store optimized setter/getter paramete IDS
    int _isWalkingHash;
    int _isRunningHash;
    int _isJumpingHash;
    int _jumpCountHash;

    // variables to store player input values
    Vector2 _currentMovementInput;
    Vector3 _currentMovement;
    Vector3 _currentRunMovement;
    Vector3 _appliedMovement;
    bool _isMovementPressed;
    bool _isRunPressed;

    // const 
    [SerializeField] float _rotationFactorPerFrame = 1.0f;
    [SerializeField] float _speedFactorPerFrame = 1.0f;
    [SerializeField] float _runMultiplier = 3.0f;
    int _zero = 0;

    // gravity variables
    float _gravity = -9.8f;
    float _groundGravity = -.05f;

    // jumping vairables
    bool _isJumpPressed = false;
    bool _isJumping = false;
    bool _isJumpAnimating = false;
    float _initialJumpVelocity;
    [SerializeField]float _maxJumpHeight = 4.0f;
    [SerializeField]float _maxJumpTime = 0.75f;
    int jumpCount = 0;
    Dictionary<int , float> _initialJumpVelocities = new Dictionary<int , float>();
    Dictionary<int , float> _jumpGravities = new Dictionary<int , float>();
    Coroutine _currentJumpResetRoutine = null;

    private void Awake()
    {
        // initially set reference variables
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        // set the parameter hash references
        _isWalkingHash = Animator.StringToHash("isWalking"); 
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("_isJumping");
        _jumpCountHash = Animator.StringToHash("jumpCount");

        // Set the player input callbacks ( This callback function are typilcally called when certain events take place, and Provode data for us to use from the event. )
        _playerInput.CharacterControls.Move.started += onMovementInput;
        _playerInput.CharacterControls.Move.canceled += onMovementInput;
        _playerInput.CharacterControls.Move.performed += onMovementInput;
        _playerInput.CharacterControls.Run.started += onRun;
        _playerInput.CharacterControls.Run.canceled += onRun;
        _playerInput.CharacterControls.Jump.started += onJump;
        _playerInput.CharacterControls.Jump.canceled += onJump;

        setupJumpVariables();

    }

    void setupJumpVariables()
    {
        float _timeToApex = _maxJumpTime / 2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(_timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / _timeToApex;
        float _secondJumpVelocity = (-2 * (_maxJumpHeight + 2)) / Mathf.Pow((_timeToApex * 1.25f), 2);
        float _secondJumpInitialVelocity = (2 * (_maxJumpHeight + 2)) / (_timeToApex * 1.25f);
        float _thirdJumpVelocity = (-2 * (_maxJumpHeight + 4)) / Mathf.Pow((_timeToApex * 1.5f), 2);
        float _thirdJumpInitialVelocity = (2 * (_maxJumpHeight + 4)) / (_timeToApex * 1.5f);

        _initialJumpVelocities.Add(1, _initialJumpVelocity);
        _initialJumpVelocities.Add(2, _secondJumpInitialVelocity);
        _initialJumpVelocities.Add(3, _thirdJumpInitialVelocity);

        _jumpGravities.Add(0, _gravity );
        _jumpGravities.Add(1, _gravity);
        _jumpGravities.Add(2, _secondJumpVelocity);
        _jumpGravities.Add(3, _thirdJumpVelocity);

    }


    void handleJump()
    {
        if(!_isJumping && _characterController.isGrounded && _isJumpPressed)
        {
            if(jumpCount < 3 && _currentJumpResetRoutine != null)
            {
                StopCoroutine(_currentJumpResetRoutine);
            }
            _animator.SetBool(_isJumpingHash, true);
            _isJumpAnimating = true;
            _isJumping = true;
            jumpCount++;
            _animator.SetInteger(_jumpCountHash, jumpCount);
            _currentMovement.y = _initialJumpVelocities[jumpCount] * .8f;
            _appliedMovement.y = _initialJumpVelocities[jumpCount] * .8f;
        }
        else if (!_isJumpPressed && _isJumping && _characterController.isGrounded)
        {
            // _animator.SetBool(_isJumpingHash, false);
            _isJumping = false;
        }
    }

    IEnumerator jumpResetRoutine()
    {
        yield return new WaitForSeconds(.5f);
        jumpCount = 0;
    }
   
    void onRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    void onJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
    }

    // handler function to set the player input values
    void onMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x * _speedFactorPerFrame;
        _currentMovement.z = _currentMovementInput.y * _speedFactorPerFrame;
        _currentRunMovement.x = _currentMovementInput.x * _runMultiplier;
        _currentRunMovement.z = _currentMovementInput.y * _runMultiplier;
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }
    void handleRotation()
    {
        Vector3 positionToLookAt;
        // the change in position our character should point to
        positionToLookAt.x = _currentMovement.x;
        positionToLookAt.y = 0;
        positionToLookAt.z = _currentMovement.z;
        // the current rotation of our character
        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            // create a new rotation based on where the player is currently pressing
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }

    }
    void handleAnimation()
    {
        // get pararmeter values from _animator
        bool isWalking = _animator.GetBool(_isWalkingHash);
        bool isRunning = _animator.GetBool(_isRunningHash);

        // start walking if movement presed is true and not aleady waling
        if (_isMovementPressed && !isWalking)
        {
            _animator.SetBool(_isWalkingHash, true);
        }
        // stop walking if _isMovementPressed is false and not already walking
        else if (!_isMovementPressed && isWalking)
        {
            _animator.SetBool(_isWalkingHash, false);
        }
        // run if movement and run presed are true and not currently running
        if((_isMovementPressed && _isRunPressed) && !isRunning)
        {
            _animator.SetBool(_isRunningHash, true);
        }
        // stop running if movement or run pressed are false and currently running
        else if ((!_isMovementPressed || !_isRunPressed) && isRunning)
        {
            _animator.SetBool(_isRunningHash, false);
        }

    }

    void handleGravity()
    {
        bool isFalling = _currentMovement.y <= 0.0f || !_isJumpPressed;
        float fallMultiplayer = 2.0f;
        // apply proper gravity depending on if the character is ground or not
        if(_characterController.isGrounded)
        {
            if(_isJumpAnimating)
            {
                _animator.SetBool(_isJumpingHash, false);
                _isJumpAnimating = false;
                _currentJumpResetRoutine = StartCoroutine(jumpResetRoutine());
                if (jumpCount == 3)
                {
                    jumpCount = 0;
                    _animator.SetInteger(_jumpCountHash, jumpCount);

                }

            }
            _currentMovement.y = _groundGravity;
            _appliedMovement.y = _groundGravity;

        }
        else if (isFalling)
        {
            float previousYVelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (_jumpGravities[jumpCount] * fallMultiplayer * Time.deltaTime);
            _appliedMovement.y = Mathf.Max((previousYVelocity + _currentMovement.y) * .5f, -20.0f);
        }
        else
        {
            float previousYVelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (_jumpGravities[jumpCount] * Time.deltaTime);
            _appliedMovement.y = (previousYVelocity + _currentMovement.y) * .5f;

        }
    }

    // Update is called once per frame
    void Update()
    {
        handleRotation();
        handleAnimation();

        if (_isRunPressed) {
            _appliedMovement.x = _currentRunMovement.x;
            _appliedMovement.z = _currentRunMovement.z;
        } else {
            _appliedMovement.x = _currentMovement.x;
            _appliedMovement.z = _currentMovement.z;
        }

        _characterController.Move(_appliedMovement * Time.deltaTime);

        handleGravity();
        handleJump();
    }

    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }
}
