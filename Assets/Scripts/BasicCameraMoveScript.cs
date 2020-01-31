using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCameraMoveScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = Camera.main.transform.position;
        pos.x += 100 * Time.deltaTime;
        Camera.main.transform.position = pos;
    }
}
