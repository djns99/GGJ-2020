using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private bool wasOnGround;

    private Rigidbody2D carBody;
    private CarComponent[] carComponents;

    private float explodeCounter = 0.6f;
    public bool isExploding;
    private List<Transform> carPieces;
    private List<Vector3> compDirections;

    private float originalSpeed = -2000f;

    public float carStoppingCounter;

    public float prevSpeed;


    private void Start()
    {
        carBody = transform.Find("Body").GetComponent<Rigidbody2D>();
        carComponents = transform.GetComponentsInChildren<CarComponent>();
        carPieces = new List<Transform>();
        compDirections = new List<Vector3>();
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            carPieces.Add(child);
            compDirections.Add(child.position);
        }
    }

    private bool IsCarOnGround()
    {
        foreach (CarComponent cc in carComponents)
        {
            if (cc.CollideWithGround) return true;
        }
        return false;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (carBody.velocity.magnitude <= 10f)
        {
            carBody.AddForce(transform.right * 10000);
            foreach (WheelJoint2D wheelJoint in carBody.GetComponents<WheelJoint2D>())
            {
                JointMotor2D motor = wheelJoint.motor;
                motor.motorSpeed -= 10f;
                wheelJoint.motor = motor;
            }
        }
        else
        {
            foreach (WheelJoint2D wheelJoint in carBody.GetComponents<WheelJoint2D>())
            {
                JointMotor2D motor = wheelJoint.motor;
                motor.motorSpeed = originalSpeed;
                wheelJoint.motor = motor;
            }
        }
        if (Input.GetKeyDown(KeyCode.F)) isExploding = true;
        if (carBody == null) return;
        if (!isExploding) CheckCarFlippedOrWillExplode();
        else
        {
            AnimateCarExplosion();
        }
    }

    private void CheckCarFlippedOrWillExplode()
    {
        float currSpeed = carBody.velocity.magnitude;
        Debug.Log(currSpeed + " vs " + prevSpeed);

        if (prevSpeed - currSpeed >= 60f && IsCarOnGround())
        {
            // Dropped too fast
            isExploding = true;
        }

        if (carBody.velocity.magnitude <= 0.7f)
        {
            carStoppingCounter += Time.deltaTime;
            if (carStoppingCounter >= 2f)
            {
                isExploding = true;
            }
        } else
        {
            // Resets
            carStoppingCounter = 0;
        }

        foreach (CarComponent cc in carComponents)
        {
            if (cc.IsOutOfScreen) isExploding = true;
        }

        if (carBody.transform.up.y < -0.8f && carBody.transform.up.y > -1f && IsCarOnGround())
        {
            Debug.Log("wheel flipped");
            isExploding = true;
        }
        List<Transform> wheels = carPieces.FindAll(element => (element.name.Contains("Front") || element.name.Contains("Back")));
        foreach (Transform wheel in wheels)
        {
            if (Vector3.Distance(carBody.transform.position, wheel.transform.position) >= 20f)
            {
                Debug.Log("wheel extended");
                isExploding = true;
            }
        }

        prevSpeed = currSpeed;
    }

    public void AnimateCarExplosion()
    {
        if (explodeCounter == 0.6f)
        {
            carBody.transform.Find("Particles").gameObject.SetActive(true);
        }
        for (int i = 0; i < carPieces.Count; i++)
        {
            if (carPieces[i] == carBody.transform || carPieces[i] == transform || carPieces[i].name == "Wheel" || carPieces[i].name == "Particles") continue;
            
            carPieces[i].position += (carPieces[i].position - carBody.transform.position).normalized * 15f * Time.deltaTime;
            carPieces[i].Rotate(Vector3.forward * Time.deltaTime * 100f);
        }
        explodeCounter -= Time.deltaTime;

        if (explodeCounter <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Car went boom!");
    }
}
