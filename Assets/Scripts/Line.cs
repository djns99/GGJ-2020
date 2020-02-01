using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    void OnBecameInvisible()
    {
        Debug.Log("bye line");
        Destroy(gameObject);
    }
}
