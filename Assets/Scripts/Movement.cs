using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private float _speed;
    private Vector3 _position;
    private float _gravity;

    private bool _isMovingRight=false;
    private bool _canMovingRight = true;

    private bool _isMovingLeft = false;
    private bool _canMovingLeft = true;


    private BoxCollider2D box;
    [SerializeField] private int _acceleration;
    [SerializeField] private int _deceleration;
    [SerializeField] private int _maxSpeed;
    [SerializeField] private int _turnSpeed;
    [SerializeField] private float _realGravity;
    [SerializeField] private float _jumpValue;


    private void Start()
    {
        box=GetComponent<BoxCollider2D>();
        _gravity = _realGravity;
    }

    void Update()
    {

        //todo : déplacer si tout est bien ?
        //todo : lerp
        //todo : rajouter des raycast


        _position += new Vector3(_speed*Time.deltaTime, -_gravity * Time.deltaTime, 0);
        transform.position = _position;

        if (_isMovingRight && _speed < _maxSpeed)
        {         
            if (_canMovingRight)
            {
                 _speed += _acceleration * Time.deltaTime; 
            }   
        }
        else if (_isMovingLeft && _speed > -_maxSpeed )
        {
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
        CheckCollisionDown();
        CheckCollisionUp();

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
        bool collid=false;        
        RaycastHit2D[] first = Physics2D.RaycastAll(transform.position + new Vector3(box.size.x / 2, -box.size.x / 4), new Vector2(1, 0), _speed * Time.deltaTime * 3);
        RaycastHit2D[] second = Physics2D.RaycastAll(transform.position + new Vector3(box.size.x / 2, 0), new Vector2(1, 0), _speed * Time.deltaTime * 3);
        RaycastHit2D[] third = Physics2D.RaycastAll(transform.position + new Vector3(box.size.x / 2, box.size.x / 4), new Vector2(1, 0), _speed * Time.deltaTime * 3);
        
        RaycastHit2D[] hitsRight = third.Concat(first.Concat(second).ToArray()).ToArray();
        for (int i = 0; i < hitsRight.Length; i++)
        {
            if (hitsRight[i].collider != null && hitsRight[i].collider.tag == "Wall" && _canMovingRight == true && hitsRight[i].distance< _speed * Time.deltaTime)
            {
                _position += new Vector3(hitsRight[i].distance, 0, 0);
                _speed = 0;
                _canMovingRight = false;
                collid = true;
                break;
            }
        }
        if(!collid){ _canMovingRight = true;}
    }

    private void CheckCollisionLeft()
    {
        bool colid = false;
        RaycastHit2D[] first = Physics2D.RaycastAll(transform.position - new Vector3(box.size.x / 2, -box.size.x / 4), new Vector2(-1, 0), -_speed * Time.deltaTime * 3);
        RaycastHit2D[] second = Physics2D.RaycastAll(transform.position - new Vector3(box.size.x / 2, 0), new Vector2(-1, 0), -_speed * Time.deltaTime * 3);
        RaycastHit2D[] third = Physics2D.RaycastAll(transform.position - new Vector3(box.size.x / 2, box.size.x / 4), new Vector2(-1, 0),- _speed * Time.deltaTime * 3);
        RaycastHit2D[] hitsLeft = third.Concat(first.Concat(second).ToArray()).ToArray();

        for (int i = 0; i < hitsLeft.Length; i++)
        {
            if (hitsLeft[i].collider != null && hitsLeft[i].collider.tag == "Wall" && _canMovingLeft == true && hitsLeft[i].distance < -_speed * Time.deltaTime)
            {
                _position -= new Vector3(hitsLeft[i].distance, 0, 0);
                _speed = 0;
                _canMovingLeft = false;
                colid = true;
                break;
            }
        }
        if (!colid) { _canMovingLeft = true; }
    }

    private void CheckCollisionUp()
    {
        //to be coded
    }
    private void CheckCollisionDown()
    {
        bool colid = false;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position - new Vector3(0, box.size.x / 2), new Vector2(0, -1), _gravity * Time.deltaTime * 3);
        Debug.DrawRay(transform.position - new Vector3(0, box.size.x / 2), new Vector2(0, -1)* _gravity * Time.deltaTime);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && hits[i].collider.tag == "Wall" && hits[i].distance < _realGravity*Time.deltaTime)
            {
                _gravity = 0;
                colid = true;
                break;
            }
        }
        if (!colid) { _gravity = _realGravity; }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _position += new Vector3(0, _jumpValue,0);
        }
    }

}
