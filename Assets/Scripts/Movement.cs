using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    #region VARIABLES DECLARATIONS

    private PlayerState playerState;                                            //current State of the player. 

    #region General Variables 
    private Vector3 _position;
    private float _horizontalSpeed;
    private float _verticalSpeed;
    private float _currentTurnSpeed;
    private float _currentGravity;
    private float _currentAcceleration;                                         //horizontal acceleration 
    private float _currentDeceleration;                                         //horizontol deceleration
    #endregion

    #region Input callbacks logic Variables
    private float _lastJumpPressedDate;
    private float _lastJumpButtonRelease;
    private float _lastDashButtonPressedDate;

    private bool _jumpButtonJustPressed;
    private bool _jumpButtonJustReleased;
    private bool _dashButtonJustPressed;

    private bool _isFacingRight;
    #endregion

    #region Wall Jump 
    [Header("Wall Jump")]
    [SerializeField] private float  _horizontalSpeedWallJump;
    [SerializeField] private float _wallGravity;
    [SerializeField] private float _impulseJumpWall;
    private string _lastWall;
    private float _lastWallContactDate;
    #endregion

    #region Collisions Variables
    private bool _isMovingRight=false;
    private bool _canMovingRight = true;

    private bool _isMovingLeft = false;
    private bool _canMovingLeft = true;

    private bool _canMovingDown=true;

    private bool _canMovingUp = true;

    private bool _onRightWall = false;
    private bool _onLeftWall = false;

    private BoxCollider2D _box;
    #endregion

    #region Run variables 
    [Header("Run")]
    [SerializeField] private int _acceleration;                                 //current horizontal acceleration
    [SerializeField] private int _deceleration;                                 //current horizontal deceleration
    [SerializeField] private int _maxSpeed; 
    [SerializeField] private int _turnSpeed;
    #endregion

    #region Normal Jump Variables
    [Header("Classic Jump")]
    [SerializeField] private float _maxJumpHeight = 4f;                         // height of the classic jump (when hold)
    [SerializeField] private float _jumpDuration = 5f;                          // duration to get to the apex of the classic jump (when hold)
    [SerializeField] private float _downwardGravityFactor = 4f;                 // gravity amplifier used when falling down
    [SerializeField] private bool _variableJumpHeight = false;                  // true : variable jump height / false : fixed jump height
    [SerializeField, Range(0,10)] private float _jumpCutoff = 5f;               // how fast the jump is interrupted whend releasing the button (apply only if the varaible jump height is activated)
    private bool _cutOffApplied = false;
    private const float _maxJumpCutoff = 10f;

    /* Both gravity and initial impulse are computed from the desired 
     * jump height and duration (perfect parabola trajectory).
     * The other previous parameters tweak the trajectory for a better feeling. 
     */
    private float _standardGravity;                                             // gravity for the classical jump
    private float _initialJumpImpulse;                                          // initial impulse for the classical jump
    private bool _jumpPrematurelyEnded = true;                                  // true if the jump button has been released during the jump;
    #endregion

    #region Double Jump Variables
    [Header("Double Jump / Extra Jump ")]
    [SerializeField] private float _maxExtraJumpHeight = 4f;                    // height for each additional jump in the air
    [SerializeField] private float _extraJumpDuration = 5f;                     // duration of each additional jump in the air
    [SerializeField] private int _nbExtraJumps = 1;                             // number of additionnal jump in the air 

    private int _currentNbExtraJumps;
    private bool _onExtraJumpAscension;                                         // true if the player is ascending as a result of an extra jump
    private float _gravityOnExtraJump;                                          // gravity for each extra jump
    private float _initialExtraJumpImpulse;                                     // initial impulse for each extra jump
    #endregion

    #region Aerial Movements Variables
    [Header("Aerial Movements")]
    [SerializeField] private float _airAcceleration = 45f;                      // the lateral acceleration of the player in the air.
    [SerializeField] private float _airControl = 80f;                           // the turn speed of the player in the air
    [SerializeField] private float _airBrake = 40f;                             // the lateral deceleration of the player in the air             
    [SerializeField] private float _maxFallSpeed = 20f;                         // we clamp the fallspeed in order to avoid too big falling velocity


    #endregion// the maximum vertical speed of the player when falling.

    #region Dash Variables
    [Header("Dash")]
    [SerializeField] private float _dashDistance = 3f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 0.5f;

    private int _dashDirection;                                             // equals 1 when the player look at the right when dash button is pressed. -1 otherwhise.
    private float _dashImpulse;
    private float _dashCounterForce;
    private float _lastDashStartDate;
    private bool _isDashStopped;
    #endregion

    #region Assists Variables 
    [Header("Assist Parameters")]
    [SerializeField] private float _coyoteTime = 0.1f;                          // authorized delay to press "Jump" Button when falling from a platform
    [SerializeField] private float _jumpBufferTime = 0.2f;                      // authorized delay to press "Jump" Button before landing
    [SerializeField] private float _wallJumpBufferTime = 0.1f;

    private bool _coyoteUsable = false;
    private float _lastStartFallingDate;
    #endregion

    #endregion

    #region EVENT DECLARATION

    public static event Action JumpingSignal;
    public static event Action LandingSignal;

    #endregion

    private void Awake()
    {
        InitializeValues();
        
    }

    private void Start()
    {
        _box=GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        UpdateInputsLogic();

        if(playerState == PlayerState.OnDash)
        {
            CalculateDashBrake();
            CalcultateDash();
        }

        CalculateHorizontalMove();
        CalculateGravity();
        CalculateJump();

        //UpdateFacingDirection();

        UpdatePlayerState();

        print(playerState);
        //_trailRenderer.widthMultiplier = _horizontalSpeed;
    }

    void FixedUpdate()
    {
        CheckCollisionRight();
        CheckCollisionLeft();
        CheckCollisionDown();
        CheckCollisionUp();

        UpdatePosition();

    }

    private void InitializeValues()
    {

        _standardGravity = (2f * _maxJumpHeight) / (_jumpDuration * _jumpDuration);
        _initialJumpImpulse = 2f * _maxJumpHeight / _jumpDuration;

        _gravityOnExtraJump = (2f * _maxExtraJumpHeight) / (_extraJumpDuration * _extraJumpDuration);
        _initialExtraJumpImpulse = 2f * _maxExtraJumpHeight / _extraJumpDuration;

        _jumpPrematurelyEnded = true;
        _coyoteUsable = false;

        _lastJumpPressedDate = float.MinValue;
        _lastDashButtonPressedDate = float.MinValue;
        _lastJumpButtonRelease = float.MinValue;

        _lastStartFallingDate = float.MinValue;
        _lastDashStartDate = float.MinValue;

        _dashCounterForce = (2f * _dashDistance) / (_dashDuration * _dashDuration);
        _dashImpulse = 2f * _dashDistance / _dashDuration;
        _isDashStopped = true;
        _dashDirection = 1;

        _currentTurnSpeed = _turnSpeed;
        _currentGravity = _standardGravity;
        _currentAcceleration = _acceleration;
        _currentDeceleration = _deceleration;
    }

    #region INPUT CALLBACKS

    public void MoveLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isMovingLeft = true;
            _isFacingRight = false;
        }
        if (context.canceled)
        {
            _isMovingLeft = false;
        }
    }

    public void MoveRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isMovingRight = true;
            _isFacingRight = true;
        }
        if (context.canceled)
        {
            _isMovingRight=false;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _lastJumpPressedDate = Time.time;
        }
        if (context.canceled)
        {
            _lastJumpButtonRelease = Time.time;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _lastDashButtonPressedDate = Time.time;
        }
    }

    public void UpdateInputsLogic()
    {

        _jumpButtonJustPressed = (_lastJumpPressedDate == Time.time);
        _jumpButtonJustReleased = (_lastJumpButtonRelease == Time.time);
        _dashButtonJustPressed = (_lastDashButtonPressedDate == Time.time);
       
    }

    #endregion

    #region COLLISIONS CHECK

    private void CheckCollisionRight()
    {
        bool colid=false;        
        RaycastHit2D[] first = Physics2D.RaycastAll(transform.position + new Vector3(_box.size.x / 2, -_box.size.x*0.45f), new Vector2(1, 0), _horizontalSpeed * Time.deltaTime * 3);
        RaycastHit2D[] second = Physics2D.RaycastAll(transform.position + new Vector3(_box.size.x / 2, 0), new Vector2(1, 0), _horizontalSpeed * Time.deltaTime * 3);
        RaycastHit2D[] third = Physics2D.RaycastAll(transform.position + new Vector3(_box.size.x / 2, _box.size.x* 0.45f), new Vector2(1, 0), _horizontalSpeed * Time.deltaTime * 3);
        
        RaycastHit2D[] hits = third.Concat(first.Concat(second).ToArray()).ToArray();
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall" || hits[i].collider.tag == "OneWay") && _canMovingRight && hits[i].distance< _horizontalSpeed * Time.deltaTime&&_horizontalSpeed > 0)
            {
                _position += new Vector3(hits[i].distance, 0, 0);
                _horizontalSpeed = 0;
                _canMovingRight = false;
                colid = true;
                break;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall" || hits[i].collider.tag == "OneWay" ) && hits[i].distance < 0.1f)
            {
                _canMovingRight = false;
                colid = true;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "WallJump") && _canMovingRight && hits[i].distance < _horizontalSpeed * Time.deltaTime)
            {
                if(playerState != PlayerState.OnWall)
                {
                    _horizontalSpeed = 0;
                    _verticalSpeed = 0;
                }
                _position += new Vector3(hits[i].distance, 0, 0);
                _canMovingRight = false;
                if (playerState != PlayerState.OnGround) { _onRightWall = true; _lastWall = "right"; }
                colid = true;
                break;
            }
            //if (hits[i].collider != null && (hits[i].collider.tag == "WallJump") && hits[i].distance < 0.1f)
            //{
            //    if (playerState != PlayerState.OnGround) { _onRightWall = true; }
            //    _canMovingRight = false;
            //    colid = true;
            //}
            if (hits[i].collider != null && hits[i].collider.tag == "End")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1) ;
                break;
            }
        }
        if(!colid){ _canMovingRight = true; _onRightWall = false; }
    }

    private void CheckCollisionLeft()
    {
        bool colid = false;
        RaycastHit2D[] first = Physics2D.RaycastAll(transform.position - new Vector3(_box.size.x / 2, -_box.size.x* 0.45f), new Vector2(-1, 0), -_horizontalSpeed * Time.deltaTime * 3);
        RaycastHit2D[] second = Physics2D.RaycastAll(transform.position - new Vector3(_box.size.x / 2, 0), new Vector2(-1, 0), -_horizontalSpeed * Time.deltaTime * 3);
        RaycastHit2D[] third = Physics2D.RaycastAll(transform.position - new Vector3(_box.size.x / 2, _box.size.x * 0.45f), new Vector2(-1, 0),- _horizontalSpeed * Time.deltaTime * 3);
        RaycastHit2D[] hits = third.Concat(first.Concat(second).ToArray()).ToArray();

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall" || hits[i].collider.tag == "OneWay") && _canMovingLeft && hits[i].distance < -_horizontalSpeed * Time.deltaTime && _horizontalSpeed<0)
            {
                _position -= new Vector3(hits[i].distance, 0, 0);
                _horizontalSpeed = 0;
                _canMovingLeft = false;
                colid = true;
                break;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall" || hits[i].collider.tag == "OneWay") && _canMovingLeft && hits[i].distance < 0.1f)
            {
                _canMovingLeft = false;
                colid = true;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "WallJump") && _canMovingLeft && hits[i].distance < -_horizontalSpeed * Time.deltaTime && _horizontalSpeed < 0)
            {
                _position -= new Vector3(hits[i].distance, 0, 0);
                _horizontalSpeed = 0;
                _canMovingLeft = false;
                if (playerState != PlayerState.OnGround) { _onLeftWall = true; _lastWall = "left"; }


                colid = true;
                _verticalSpeed = 0;
                break;
            }
            //if (hits[i].collider != null && (hits[i].collider.tag == "WallJump") && hits[i].distance < 0.1f)
            //{
            //    if (playerState != PlayerState.OnGround) { _onLeftWall = true; }
            //    _canMovingLeft = false;
            //    colid = true;
            //}
            if (hits[i].collider != null && hits[i].collider.tag == "End")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);

                break;
            }
        }
        if (!colid) { _canMovingLeft = true; _onLeftWall = false; }
    }

    private void CheckCollisionUp()
    {
        bool colid = false;
        RaycastHit2D[] first = Physics2D.RaycastAll(transform.position + new Vector3(-_box.size.x * 0.45f, _box.size.x / 2), new Vector2(0, 1), _verticalSpeed * Time.deltaTime * 3 + 0.5f);
        RaycastHit2D[] second = Physics2D.RaycastAll(transform.position + new Vector3(0, _box.size.x / 2), new Vector2(0, 1), math.abs(_verticalSpeed) * Time.deltaTime * 3 + 0.5f);
        RaycastHit2D[] third = Physics2D.RaycastAll(transform.position + new Vector3(_box.size.x * 0.45f, _box.size.x / 2), new Vector2(0, 1), _verticalSpeed * Time.deltaTime * 3 + 0.5f);
        RaycastHit2D[] hits = third.Concat(first.Concat(second).ToArray()).ToArray();

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall" || hits[i].collider.tag == "WallJump" )&& hits[i].distance < _verticalSpeed * Time.deltaTime&&_verticalSpeed>0 && _canMovingUp==true)
            {
                _position += new Vector3(0, hits[i].distance, 0);
                _verticalSpeed = 0;
                colid = true;
                _canMovingUp = false;
                break;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall" || hits[i].collider.tag == "WallJump") && hits[i].distance < 0.1f)
            {
                _canMovingUp = false;
                colid = true;
            }
            if (hits[i].collider != null && hits[i].collider.tag == "End")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);

                break;
            }
        }
        if (!colid)
        {
            _canMovingUp = true;
        }

    }
    private void CheckCollisionDown()
    {
        bool colid = false;
        RaycastHit2D[] first = Physics2D.RaycastAll(transform.position - new Vector3(-_box.size.x * 0.45f, _box.size.x / 2), new Vector2(0, -1), -_verticalSpeed * Time.deltaTime * 3 + 0.5f);
        RaycastHit2D[] second = Physics2D.RaycastAll(transform.position - new Vector3(0, _box.size.x / 2), new Vector2(0, -1), -_verticalSpeed * Time.deltaTime * 3 + 0.5f);
        RaycastHit2D[] third = Physics2D.RaycastAll(transform.position - new Vector3(_box.size.x* 0.45f, _box.size.x / 2), new Vector2(0, -1), -_verticalSpeed * Time.deltaTime * 3+0.5f);
        RaycastHit2D[] hits = third.Concat(first.Concat(second).ToArray()).ToArray();

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall" || hits[i].collider.tag == "OneWay" ) && hits[i].distance < -_verticalSpeed * Time.deltaTime && _verticalSpeed<0 && _canMovingDown==true)
            {
                _verticalSpeed = 0;
                colid = true;
                _currentGravity = 0;
                _canMovingDown = false;
                _position -= new Vector3(0, hits[i].distance, 0);
                break;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall" || hits[i].collider.tag == "OneWay") && hits[i].distance < 0.1f)
            {

               _canMovingDown = false;

                colid = true;
            }
            if (hits[i].collider != null && hits[i].collider.tag == "End")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);

                break;
            }
        }

        if (!colid) {
            
            _canMovingDown = true;
            _canMovingUp = true;
        }
    }
    #endregion

    #region PLAYER STATE MACHINE

    private void UpdatePlayerState()
    {

     
        if(playerState == PlayerState.OnGround)
        {
            if (CheckLaunchDash())
            {
                playerState = PlayerState.OnDash;
                OnDashStateEnter();
            }
            //Case 1-2 : the player just quit the ground by jumping or falling
            else if (_canMovingDown)
            {
                playerState = PlayerState.Falling;
                OnFallingStateEnter();
            }
        }

        if(playerState == PlayerState.Falling)
        {
            if (CheckLaunchDash())
            {
                playerState = PlayerState.OnDash;
                OnDashStateEnter();
            }

            else if (_onLeftWall || _onRightWall)
            {
                playerState = PlayerState.OnWall;
                OnWallJumpStateEnter();

            }

            //Case 2-2 : the player just land on the ground
            else if (!_canMovingDown)
            {
                playerState = PlayerState.OnGround;
                LandingSignal?.Invoke();
                OnGroundStateEnter();
            };

        }

        if(playerState == PlayerState.OnDash && _isDashStopped)
        {
            if (_canMovingDown)
            {
                playerState = PlayerState.Falling;
                OnFallingStateEnter();
            }
            else
            {
                playerState = PlayerState.OnGround;
                OnGroundStateEnter();
            }
        }

        if (playerState == PlayerState.OnWall)
        {

            if (_canMovingDown && !(_onRightWall || _onLeftWall))
            {
                playerState = PlayerState.Falling;
                OnFallingStateEnter();
                _lastWallContactDate = Time.time;
            }
            else if (!_canMovingDown)
            {
                playerState = PlayerState.OnGround;
                OnGroundStateEnter();

            }
        }

    }

    private bool CheckLaunchDash()
    {
        return (_dashButtonJustPressed && (_lastDashStartDate + _dashCooldown < Time.time));
    }

    private void OnFallingStateEnter()
    {
        _lastStartFallingDate = Time.time;
        _currentGravity = _standardGravity;

        _currentAcceleration = _airAcceleration;
        _currentDeceleration = _airBrake;
        _currentTurnSpeed = _airControl;
    }

    private void OnGroundStateEnter()
    {
        _currentNbExtraJumps = _nbExtraJumps;
        _coyoteUsable = true;
        _currentGravity = 0f;

        _currentAcceleration = _acceleration;
        _currentDeceleration = _deceleration;
        _currentTurnSpeed = _turnSpeed;
    }

    private void OnDashStateEnter()
    {
        _isDashStopped = false;
        _lastDashStartDate = Time.time;
        _dashDirection = _isFacingRight ? 1 : -1;
        _currentGravity = 0;
        _verticalSpeed = 0;
        _horizontalSpeed = _dashImpulse * _dashDirection;
    }

    private void OnWallJumpStateEnter()
    {
        _currentGravity = _wallGravity;
    }

    #endregion

    #region HORIZONTAL MOVEMENT

    private void CalculateHorizontalMove()
    {
        if(playerState == PlayerState.OnDash)
        {
            return; //si on est en train de dasher, on ne prend pas en compte les inputs du joueurs. 
        }

        if (_isMovingRight && _horizontalSpeed < _maxSpeed)
        {
            if (_horizontalSpeed < 0 && playerState == PlayerState.OnGround)
            {
                _horizontalSpeed = _currentTurnSpeed;
            }
            if (_canMovingRight)
            {
                _horizontalSpeed += _currentAcceleration * Time.deltaTime;
            }
        }
        else if (_isMovingLeft && _horizontalSpeed > -_maxSpeed)
        {
            if (_horizontalSpeed > 0 && playerState == PlayerState.OnGround)
            {
                _horizontalSpeed = -_currentTurnSpeed;
            }
            if (_canMovingLeft)
            {
                _horizontalSpeed -= _currentAcceleration * Time.deltaTime;
            }
        }
        else if (!_isMovingRight && !_isMovingLeft)
        {
            _horizontalSpeed = Mathf.MoveTowards(_horizontalSpeed, 0f, _currentDeceleration * Time.deltaTime);
        }
    }

    /*private void UpdateFacingDirection()
    {
        if(_horizontalSpeed > 0)
        {
            _isFacingRight = true;
        } else if (_horizontalSpeed < 0)
        {
            _isFacingRight = false;
        }
    }*/

    private void CalculateDashBrake() // fucntion responsible of dash brutal deceleration.
    {
        //If we bump into a wall during the dash, we set horizontal velocity to zero.
        if(_dashDirection * _horizontalSpeed > 0)
        {
            if( (_dashDirection > 0 && !_canMovingRight) || (_dashDirection < 0 && !_canMovingLeft))
            {
                _horizontalSpeed = 0f;
                return;
            }
        }

        _horizontalSpeed -= _dashCounterForce * _dashDirection * Time.deltaTime;
        if (_dashDirection * _horizontalSpeed <= 0)
        {
            _horizontalSpeed = 0;
        }
    }

    private void CalcultateDash()
    {
        //fin du dash = la vitesse horizontale est nulle ou de signe opposé à la direction du dash
        if (_dashDirection * _horizontalSpeed <= 0)
        {
            _isDashStopped = true;
        }
    }

    #endregion

    #region VERTICAL MOVEMENT

    private void CalculateGravity() {
        //Case 1 : The player is hitting something under it. 
        if (!_canMovingDown)
        {
            if(_verticalSpeed < 0) {
                _verticalSpeed = 0;
                return;
            }
   
        }

        //Case 2 : the player is still falling or jumping
        var gravityamplifier = 1f;
        if(_verticalSpeed < 0) {
            gravityamplifier = _downwardGravityFactor;

            if (_onExtraJumpAscension)
            {
                _currentGravity = _standardGravity;
                _onExtraJumpAscension = false;
            }

        }

        else if (_verticalSpeed > 0 && _jumpPrematurelyEnded && !_cutOffApplied)
        {
            
            _verticalSpeed *= (1f - _jumpCutoff/_maxJumpCutoff);
            _cutOffApplied = true;
        }

        _verticalSpeed -= _currentGravity * gravityamplifier * Time.deltaTime;

        if(_verticalSpeed < -_maxFallSpeed)
        {
            _verticalSpeed = -_maxFallSpeed;
        }

    }
    private void CalculateJump() {

        if( playerState == PlayerState.OnGround)
        {
            if(_lastJumpPressedDate + _jumpBufferTime > Time.time)
            {
                ApplyJump();
            }
        }
        else if ( playerState == PlayerState.Falling)
        {
            if ((_lastWallContactDate + _wallJumpBufferTime)>Time.time && _jumpButtonJustPressed)
            {
                ApplyJumpWall();

            }
            else if(_coyoteUsable && (_lastStartFallingDate + _coyoteTime > Time.time) && _jumpButtonJustPressed)
            {
                ApplyJump();
            }
            else if(_currentNbExtraJumps > 0 && _jumpButtonJustPressed){
                _currentNbExtraJumps--;
                ApplyExtraJump();
            }

            if(_variableJumpHeight && !_jumpPrematurelyEnded && _verticalSpeed > 0 && _jumpButtonJustReleased)
            {
                _jumpPrematurelyEnded = true;
            }
        }
        if (playerState == PlayerState.OnWall && (_lastJumpPressedDate + _wallJumpBufferTime > Time.time))
        {
            ApplyJumpWall();
        }

    }

    private void ApplyJump()
    {
        _verticalSpeed = _initialJumpImpulse;
        _jumpPrematurelyEnded = false;
        _cutOffApplied = false;
        _coyoteUsable = false; // le coyote time n'est plus utilisable si on a d?j? saut? ! 
        _lastStartFallingDate = float.MinValue;
        _lastJumpPressedDate = float.MinValue;

        JumpingSignal?.Invoke();

    }

    private void ApplyExtraJump()
    {
        _verticalSpeed = _initialExtraJumpImpulse;
        _jumpPrematurelyEnded = false;
        _cutOffApplied = false;
        _lastStartFallingDate = float.MinValue;
        _lastJumpPressedDate = float.MinValue;
        _onExtraJumpAscension = true;
        _currentGravity = _gravityOnExtraJump;

        JumpingSignal?.Invoke();
    }

    private void ApplyJumpWall()
    {
        _lastWallContactDate = float.MinValue;
        _verticalSpeed = _impulseJumpWall;
        if (_lastWall == "right")
        {
            _horizontalSpeed = -_horizontalSpeedWallJump;
            _onRightWall = false;
        }
        else
        {
            _horizontalSpeed = _horizontalSpeedWallJump;
            _onLeftWall = false;
        }
    }
    #endregion


    private void UpdatePosition()
    {
        _position += new Vector3(_horizontalSpeed * Time.deltaTime, _verticalSpeed * Time.deltaTime, 0);
        transform.position = _position;
    }

   


}
