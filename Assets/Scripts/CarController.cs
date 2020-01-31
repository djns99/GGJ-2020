using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private bool wasOnGround;

    private Rigidbody2D carBody;
    private CarComponent[] carComponents;


    private void Start()
    {
        carBody = transform.Find("Body").GetComponent<Rigidbody2D>();
        carComponents = transform.GetComponentsInChildren<CarComponent>();
    }

    private bool IsCarOnGround()
    {
        foreach (CarComponent cc in carComponents)
        {
            if (cc.collideWithGround)
            {
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!IsCarOnGround())
        {
            if (wasOnGround)
            {
                carBody.freezeRotation = true;
                wasOnGround = false;
            }
        } else
        {
            if (!wasOnGround)
            {
                carBody.freezeRotation = false;
                wasOnGround = true;
            }
        }
    }
}
