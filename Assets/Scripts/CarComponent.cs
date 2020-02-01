using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarComponent : MonoBehaviour
{
    public bool CollideWithGround { get; set; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Line"))
        {
            Debug.Log(collision.relativeVelocity.magnitude);
            CollideWithGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Line"))
        {
            CollideWithGround = false;
        }
    }
}
