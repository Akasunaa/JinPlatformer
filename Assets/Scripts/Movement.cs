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
    [SerializeField] private float _jumpDuration = 5f;                          // duration of the classic jump (when hold)
    [SerializeField] private float _downwardGravityFactor = 4f;                 // gravity amplifier used when falling down
    [SerializeField] private bool _variableJumpHeight = false;                  // true : variable jump height / false : fixed jump height
    [SerializeField] private float _jumpCutoff = 5f;                            // how fast the jump is interrupted whend releasing the button (apply only if the varaible jump height is activated)

    /* Both gravity and initial impulse are computed from the desired 
     * jump height and duration (perfect parabola trajectory).
     * The other previous parameters tweak the trajectory for a better feeling. 
     */
    private float _standardGravity;                                             // gravity for the classical jump
    private float _initialJumpImpulse;                                          // initial impulse for the classical jump
    private bool _jumpPrematurelyEnded = true;                                  // true if the jump button has been released during the jump;
    private float _lastJumpButtonRelease;
    #endregion

    #region Double Jump Variables
    [Header("Double Jump")]
    [SerializeField] private float _maxSecondJumpHeight = 4f;
    [SerializeField] private float _secondJumpDuration = 5f;


    private float _gravityOnSecondJump;                                         // gravity for the classical jump
    private float _initialSecondJumpImpulse;                                    // initial impulse for the classical jump
    #endregion

    #region Aerial Movements Variables
    [Header("Aerial Movements")]
    [SerializeField] private float _airAcceleration = 45f;                      // the lateral acceleration of the player in the air.
    [SerializeField] private float _airControl = 80f;                           // the turn speed of the player in the air
    [SerializeField] private float _airBrake = 40f;                             // the lateral deceleration of the player in the air
    [SerializeField] private float _airApexBonusManiabilty = 1.5f;              // bonus maniabilty at the apex of the jump (enhance both airAcceleration and air Control).
    [SerializeField] private float _maxFallSpeed = 20f;
                                       
    private float _apexProximityFactor = 0;                                         // factor depending of the proximty to the last jump apex. At the closest point of the apex, this factor is equal to one. This is used as interpolation parameter for the _airApexBonusManiability.
    [SerializeField] private float _apexProximityThreshold = 1f;                                      // Beyond this threshold, the apexProximity Factor = 0;

    #endregion// the maximum vertical speed of the player when falling.

    #region Assists Variables 
    [Header("Assist Parameters")]
    [SerializeField] private float _coyoteTime = 0.1f;                          // authorized delay to press "Jump" Button when falling from a platform
    [SerializeField] private float _jumpBufferTime = 0.2f;                      // authorized delay to press "Jump" Button before landing

    private bool _coyoteUsable = false;
    private float _lastStartFallingDate;
    private float _lastJumpPressedDate;
    #endregion

    #endregion

    private void Awake()
    {
        InitializeValues();
    }

    private void Start()
    {
        _box=GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {

        CalculateHorizontalMove();
        CalculateGravity();
        CalculateJump();


        CheckCollisionRight();
        CheckCollisionLeft();
        CheckCollisionDown();
        CheckCollisionUp();

        UpdatePlayerState();

        UpdatePosition();

        print(playerState);
    }

    private void InitializeValues()
    {

        _standardGravity = (2f * _maxJumpHeight) / (_jumpDuration * _jumpDuration);
        _initialJumpImpulse = 2f * _maxJumpHeight / _jumpDuration;

        _gravityOnSecondJump = (2f * _maxSecondJumpHeight) / (_secondJumpDuration * _secondJumpDuration);
        _initialSecondJumpImpulse = 2f * _maxSecondJumpHeight / _secondJumpDuration;

        _jumpPrematurelyEnded = true;
        _coyoteUsable = false;

        _lastJumpPressedDate = float.MinValue;
        _lastStartFallingDate = float.MinValue;
        _lastJumpButtonRelease = float.MinValue;

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
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall") && _canMovingRight == true && hits[i].distance< _horizontalSpeed * Time.deltaTime&&_horizontalSpeed > 0)
            {
                _position += new Vector3(hits[i].distance, 0, 0);
                _horizontalSpeed = 0;
                _canMovingRight = false;
                colid = true;
                break;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall") && hits[i].distance < 0.1f)
            {
                _canMovingRight = false;
                colid = true;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "WallJump") && _canMovingRight == true && hits[i].distance < _horizontalSpeed * Time.deltaTime && _horizontalSpeed > 0)
            {
                _position += new Vector3(hits[i].distance, 0, 0);
                _horizontalSpeed = 0;
                _canMovingRight = false;
                if (playerState != PlayerState.OnGround) { _onRightWall = true; }

                colid = true;
                _verticalSpeed = 0;
                break;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "WallJump") && hits[i].distance < 0.1f)
            {
                if (playerState != PlayerState.OnGround){ _onRightWall = true; _verticalSpeed = -0.5f; }
                _canMovingRight = false;
                colid = true;
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
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall") && _canMovingLeft == true && hits[i].distance < -_horizontalSpeed * Time.deltaTime && _horizontalSpeed<0)
            {
                _position -= new Vector3(hits[i].distance, 0, 0);
                _horizontalSpeed = 0;
                _canMovingLeft = false;
                colid = true;
                break;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "Wall") && _canMovingLeft == true && hits[i].distance < 0.1f)
            {
                _canMovingLeft = false;
                colid = true;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "WallJump") && _canMovingLeft == true && hits[i].distance < -_horizontalSpeed * Time.deltaTime && _horizontalSpeed < 0)
            {
                _position -= new Vector3(hits[i].distance, 0, 0);
                _horizontalSpeed = 0;
                _canMovingLeft = false;
                if (playerState != PlayerState.OnGround) { _onLeftWall = true; }


                colid = true;
                _verticalSpeed = 0;
                break;
            }
            if (hits[i].collider != null && (hits[i].collider.tag == "WallJump") && hits[i].distance < 0.1f)
            {
                if (playerState != PlayerState.OnGround) { _onLeftWall = true; _verticalSpeed = -0.5f;}


                _canMovingLeft = false;
                colid = true;
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
            if (hits[i].collider != null && hits[i].collider.tag == "Wall" && hits[i].distance < _verticalSpeed * Time.deltaTime&&_verticalSpeed>0 && _canMovingUp==true)
            {
                _position += new Vector3(0, hits[i].distance, 0);
                _verticalSpeed = 0;
                colid = true;
                _canMovingUp = false;
                break;
            }
            if (hits[i].collider != null && hits[i].collider.tag == "Wall" && hits[i].distance < 0.1f)
            {
                _canMovingUp = false;
                colid = true;
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
        }
        if (!colid) {
            _currentGravity = _standardGravity;
            _canMovingDown = true;
            _canMovingUp = true;
        }
    }


    private void UpdatePlayerState()
    {

        //Case 1 : the player just quit the ground by jumping or falling
        if (_canMovingDown && !_onLeftWall && !_onRightWall)
        {
            playerState = PlayerState.Falling;
            _lastStartFallingDate = Time.time;

            _currentAcceleration = _airAcceleration;
            _currentDeceleration = _airBrake;
            _currentTurnSpeed = _airControl;


        }

        //Case 2 : the player just land on the ground
        if (!_canMovingDown)
        {
            playerState = PlayerState.OnGround;
            _coyoteUsable = true;
            _currentGravity = 0f;

            _currentAcceleration = _acceleration;
            _currentDeceleration = _deceleration;
            _currentTurnSpeed = _turnSpeed;
        }

        if (_onLeftWall || _onRightWall)
        {
            playerState = PlayerState.OnWall;

        }

    }

    #endregion

    #region HORIZONTAL MOVEMENT

    private void CalculateHorizontalMove()
    {
        //todo : if in the air, apply apex bonus 
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

    #endregion

    #region VERTICAL MOVEMENT

    private void CalculateGravity() {
        //todo retravailler la formule du cutoff
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
        if(_verticalSpeed < 0) { gravityamplifier = _downwardGravityFactor; }
        else if (_verticalSpeed > 0 && _jumpPrematurelyEnded)
        {
            _verticalSpeed /= _jumpCutoff;
        }

        _verticalSpeed -= _currentGravity * gravityamplifier * Time.deltaTime;

        if(_verticalSpeed < -_maxFallSpeed)
        {
            _verticalSpeed = -_maxFallSpeed;
        }

    }
    private void CalculateJump() {

        //todo : Double Jump (not so difficult)

        if(playerState == PlayerState.OnGround && (_lastJumpPressedDate + _jumpBufferTime > Time.time) )
        {
            ApplyJump();
        }
        else if (playerState == PlayerState.Falling && _coyoteUsable && (_lastStartFallingDate + _coyoteTime > Time.time) && _lastJumpPressedDate == Time.time)
        {
            ApplyJump();
        }

        if (_variableJumpHeight)
        {
            if (playerState == PlayerState.Falling && !_jumpPrematurelyEnded && _verticalSpeed > 0 && _lastJumpButtonRelease == Time.time)
            {
                _jumpPrematurelyEnded = true;
            }
        }
        if (playerState == PlayerState.OnWall && (_lastJumpPressedDate + _jumpBufferTime > Time.time))
        {
            ApplyJumpWall();
        }


    }

    private void ApplyJump()
    {
        _verticalSpeed = _initialJumpImpulse;
        _jumpPrematurelyEnded = false;
        _coyoteUsable = false; // le coyote time n'est plus utilisable si on a d?j? saut? ! 
        _lastStartFallingDate = float.MinValue; 

    }

    private void ApplyJumpWall()
    {
        print("hoho");
        _verticalSpeed = 1f;
        if (_onRightWall)
        {
            _horizontalSpeed = 1f;
            _onRightWall = false;
        }
        else
        {
            _horizontalSpeed = -1f;
            _onLeftWall = false;

        }

    }
    #endregion

    #region UPDATE POSITION

    private void UpdatePosition()
    {
        _position += new Vector3(_horizontalSpeed * Time.deltaTime, _verticalSpeed * Time.deltaTime, 0);
        transform.position = _position;
    }

    #endregion


}
