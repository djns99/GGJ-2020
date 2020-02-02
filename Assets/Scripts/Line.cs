using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    private float aliveCounter = 10f;
    public Camera cam;
    public Vector3 position;
    public bool building = false;

    private void Update()
    {
        aliveCounter -= Time.deltaTime;

        if (aliveCounter <= 0 || cam.ScreenToViewportPoint(position).x < -3f)
        {
            if(!building)
                Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        //Destroy(gameObject);
    }
}
