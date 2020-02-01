using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private bool wasOnGround;

    private Rigidbody2D carBody;
    private CarComponent[] carComponents;

    private float explodeCounter = 0.4f;
    public bool isExploding;
    private List<Transform> carPieces;
    private List<Vector3> compDirections;


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
        if (Input.GetKeyDown(KeyCode.F)) isExploding = true;
        if (carBody == null) return;
        if (!isExploding) CheckCarFlippedOrWillExplode();
        else
        {
            AnimateCarExplosion();
        }
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

    private void CheckCarFlippedOrWillExplode()
    {
        if (carBody.transform.up.y < -0.8f && carBody.transform.up.y > -1f && IsCarOnGround()) isExploding = true;
        List<Transform> wheels = carPieces.FindAll(element => (element.name.Contains("Front") || element.name.Contains("Back")));
        foreach (Transform wheel in wheels)
        {
            Debug.Log(wheel.name + "'s distance: " + Vector3.Distance(carBody.transform.position, wheel.transform.position));
            if (Vector3.Distance(carBody.transform.position, wheel.transform.position) >= 6f) isExploding = true;
        }
        
    }

    public void AnimateCarExplosion()
    {
        for (int i = 0; i < carPieces.Count; i++)
        {
            if (carPieces[i] == carBody.transform || carPieces[i] == transform || carPieces[i].name == "Wheel") continue;
            
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
