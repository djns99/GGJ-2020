using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    private Transform followingObject;
    // Start is called before the first frame update
    void Start()
    {
        followingObject = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPosition = followingObject.position;

       // transform.position = playerPosition;
    }
}
