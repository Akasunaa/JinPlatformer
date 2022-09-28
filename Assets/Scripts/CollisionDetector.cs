using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private Movement _movement;

    private void Start()
    {
        _movement = GetComponent<Movement>();
    }
    private void FixedUpdate()
    {
        float laserLength = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position- new Vector3(0, 1,0), new Vector2(0,-1), laserLength);
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position - new Vector3(1, 0, 0), new Vector2(-1, 0), laserLength);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Wall") { 
                print("jdnqsd"); 
                //transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            }
            Debug.Log("Hitting: " + hit.collider.tag);
        }
        if (hit2.collider != null)
        {
            if (hit2.collider.tag == "Wall")
            {
                print("jdnqsd");
                _movement.SetSpeed(0);
                this.gameObject.transform.position -= new Vector3(-1, 0, 0);
            }
            Debug.Log("Hitting: " + hit2.collider.tag);
        }
        Debug.DrawRay(transform.position, new Vector2(0, -1) * laserLength, Color.red);
        Debug.DrawRay(transform.position - new Vector3(1, 0, 0), new Vector2(-1, 0)* laserLength,Color.blue);
    }
}
