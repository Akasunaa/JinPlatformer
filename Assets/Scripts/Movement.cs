using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private float _speed;
    private Vector3 _position;
    private bool _isMovingRight=false;
    private bool _isMovingLeft = false;
    [SerializeField] private int _acceleration;
    [SerializeField] private int _deceleration;
    [SerializeField] private int _maxSpeed;
    [SerializeField] private int _turnSpeed;



    void Update()
    {
        _position += new Vector3(_speed * Time.deltaTime,0,0);
       transform.position = _position;
        if (_isMovingRight && _speed < _maxSpeed)
        {
            //if (_speed < 0) { _speed = _turnSpeed; }
            _speed += _acceleration * Time.deltaTime;
        }
        else if (_isMovingLeft && _speed > -_maxSpeed)
        {
            //if (_speed > 0) { _speed = -_turnSpeed; }
            _speed -= _acceleration * Time.deltaTime;
        }
        else if(!_isMovingRight && !_isMovingLeft)
        {
            if (_speed < -0.1)
            {
                _speed += _deceleration * Time.deltaTime;
            }
            else if(_speed > 0.1)
            {
                _speed -= _deceleration * Time.deltaTime;
            }
            else { _speed = 0; }
        }


    }

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
}
