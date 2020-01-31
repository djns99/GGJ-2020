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
        currCarPos = followingObject.transform.localPosition;
        Vector3 newCamPos = new Vector3(prevCarPos.x - currCarPos.x, 0, 0);
        Debug.Log(newCamPos);
        transform.position -= newCamPos;
        prevCarPos = followingObject.transform.localPosition;
    }
}
