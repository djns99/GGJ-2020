using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    private Transform followingObject;
    private Vector3 prevCarPos;
    private Vector3 currCarPos;
    // Start is called before the first frame update
    void Start()
    {
        followingObject = GameObject.FindGameObjectWithTag("Player").transform;
        prevCarPos = followingObject.transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (followingObject != null)
        {
            currCarPos = followingObject.transform.position;
            Vector3 newCamPos = new Vector3(currCarPos.x + 50f, transform.position.y, transform.position.z);
            transform.position = newCamPos;
        }
    }
}
