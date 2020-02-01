using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    private float aliveCounter = 100f;

    private void Update()
    {
        aliveCounter -= Time.deltaTime;

        if (aliveCounter <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
