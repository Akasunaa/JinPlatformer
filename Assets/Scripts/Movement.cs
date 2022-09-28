using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private float _speed;
    private Vector3 _position;

    private bool _isMovingRight=false;
    private bool _canMovingRight = true;

    private bool _isMovingLeft = false;
    private bool _canMovingLeft = true;
    private bool _gigatest = false;

    private BoxCollider2D box;
    [SerializeField] private int _acceleration;
    [SerializeField] private int _deceleration;
    [SerializeField] private int _maxSpeed;
    [SerializeField] private int _turnSpeed;

    private void Start()
    {
        box=GetComponent<BoxCollider2D>();
    }

    void Update()
    {

        //todo : déplacer si tout est bien ?


        _position += new Vector3(_speed*Time.deltaTime,0,0);
        transform.position = _position;






        if (_isMovingRight && _speed < _maxSpeed)
        {
            
            if (_canMovingRight)
            {
                 _speed += _acceleration * Time.deltaTime; 
            }

            //if (_speed < 0) { _speed = _turnSpeed; }
        
        }
        else if (_isMovingLeft && _speed > -_maxSpeed )
        {
            
            //if (_speed > 0) { _speed = -_turnSpeed; }
            if (_canMovingLeft)
            {
                _speed -= _acceleration * Time.deltaTime;
            }

        }
        else if(!_isMovingRight && !_isMovingLeft)
        {
            if (_speed < -0.5)
            {
                _speed += _deceleration * Time.deltaTime;
            }
            else if(_speed > 0.5)
            {
                _speed -= _deceleration * Time.deltaTime;
            }
            else { _speed = 0; }
        }


        CheckCollisionRight();
        CheckCollisionLeft();

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

    public void SetSpeed(float value)
    {
        _speed = value;
    }

    private void CheckCollisionRight()
    {
        RaycastHit2D[] hitsRight = Physics2D.RaycastAll(transform.position + new Vector3(box.size.x / 2, 0), new Vector2(1, 0), box.size.x*3);
        Debug.DrawRay(transform.position + new Vector3(box.size.x / 2, 0), new Vector2(1, 0) * _speed * Time.deltaTime);
        for (int i = 0; i < hitsRight.Length; i++)
        {
            if (hitsRight[i].collider != null && hitsRight[i].collider.tag == "Wall" && _canMovingRight == true && hitsRight[i].distance< _speed * Time.deltaTime)
            {
                this.gameObject.transform.position += new Vector3(_speed/2 * Time.deltaTime, 0, 0);
                _speed = 0;

                _canMovingRight = false;
                break;
            }
            if (i == hitsRight.Length - 1) { _canMovingRight = true; }
        }
    }

    private void CheckCollisionLeft()
    {
        print(_canMovingLeft);
        RaycastHit2D[] hitsLeft = Physics2D.RaycastAll(transform.position - new Vector3(box.size.x/2, 0), new Vector2(-1, 0), box.size.x*3);
        Debug.DrawRay(transform.position - new Vector3(box.size.x/2, 0), new Vector2(-1, 0)* -_speed * Time.deltaTime);
        _gigatest = true;
        for (int i = 0; i < hitsLeft.Length; i++)
        {
            if (hitsLeft[i].collider != null && hitsLeft[i].collider.tag == "Wall" && hitsLeft[i].distance < -_speed * Time.deltaTime)
            {
                if (_canMovingLeft)
                {
                    _speed = 0;
                }

                _gigatest = false;
                //_canMovingLeft = false;
                break;
            }
            //if (i == hitsLeft.Length-1) { _canMovingLeft = true; }
        }
        _canMovingLeft = _gigatest;
    }
}
