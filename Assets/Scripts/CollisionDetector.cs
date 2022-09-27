using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private void FixedUpdate()
    {
        float laserLength = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position- new Vector3(0, 1,0), new Vector2(0,-1), laserLength);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Wall") { 
                print("jdnqsd"); 
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            }
            Debug.Log("Hitting: " + hit.collider.tag);
        }
        Debug.DrawRay(transform.position, new Vector2(0, -1) * laserLength, Color.red);
    }
}
