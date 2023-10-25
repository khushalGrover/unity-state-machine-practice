/*
 * Context State
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
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
    [SerializeField] 
    float _rotationFactorPerFrame = 1.0f;
    [SerializeField] 
    float _speedFactorPerFrame = 1.0f;
    [SerializeField]
    float _runMultiplier = 3.0f;
    int _zero = 0;

    // gravity variables
    float _gravity = -9.8f;
    float _groundGravity = -.05f;

    // jumping vairables
    bool _isJumpPressed = false;
    bool _isJumping = false;
    bool _isJumpAnimating = false;
    float _initialJumpVelocity;
    [SerializeField] 
    float _maxJumpHeight = 4.0f;
    [SerializeField] 
    float _maxJumpTime = 0.75f;
    int _jumpCount = 0;
    Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();
    Coroutine _currentJumpResetRoutine = null;

    // State machine variables
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    // Getters and Setters 
    public PlayerBaseState CurrentState {  get { return _currentState; } set { _currentState = value; } } 
    public Animator Animator { get { return _animator; } }
    public Coroutine CurrentJumpResetRoutine { get { return _currentJumpResetRoutine; } set { _currentJumpResetRoutine = value; } }
    public Dictionary<int, float> InitialJumpVelocities { get { return _initialJumpVelocities;  }}
    public int JumpCount { get { return _jumpCount; } set { _jumpCount = value; } }
    public int JumpCountHash { get { return _jumpCountHash; } }
    public int IsJumpingHash { get { return _jumpCountHash; } }
    public bool IsJumpPressed { get { return _isJumpPressed; }}



    private void Awake()
    {
        // initially set reference variables
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        // Setup state
        _states = new PlayerStateFactory(this);
        _currentState = _states.Ground();
        _currentState.EnterState();
        
        // set the parameter hash references
        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");
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

        _jumpGravities.Add(0, _gravity);
        _jumpGravities.Add(1, _gravity);
        _jumpGravities.Add(2, _secondJumpVelocity);
        _jumpGravities.Add(3, _thirdJumpVelocity);

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        handleRotation();
        _characterController.Move(_appliedMovement * Time.deltaTime);

    }
   
    void handleRotation()
    {
        Vector3 _positionToLookAt;
        // the change in position our character should point to
        _positionToLookAt.x = _currentMovement.x;
        _positionToLookAt.y = 0;
        _positionToLookAt.z = _currentMovement.z;
        // the current rotation of our character
        Quaternion _currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            // create a new rotation based on where the player is currently pressing
            Quaternion _targetRotation = Quaternion.LookRotation(_positionToLookAt);
            transform.rotation = Quaternion.Slerp(_currentRotation, _targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }

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
        // _currentMovement.x = _currentMovementInput.x * _speedFactorPerFrame;
        // _currentMovement.z = _currentMovementInput.y * _speedFactorPerFrame;
        // _currentRunMovement.x = _currentMovementInput.x * _runMultiplier;
        // _currentRunMovement.z = _currentMovementInput.y * _runMultiplier;
        _isMovementPressed = _currentMovementInput.x != _zero || _currentMovementInput.y != _zero;
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
