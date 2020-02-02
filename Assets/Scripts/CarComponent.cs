using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarComponent : MonoBehaviour
{
    public bool CollideWithGround { get; set; }
    public bool IsOutOfScreen { get; set; }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Line"))
        {
            CollideWithGround = true;
        }

        if (collision.gameObject.name.Contains("Death"))
        {
            IsOutOfScreen = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Line"))
        {
            CollideWithGround = false;
        }

        if (collision.gameObject.name.Contains("Death"))
        {
            IsOutOfScreen = true;
        }
    }

    public void OnBecameInvisible()
    {
        IsOutOfScreen = true;
    }
}
