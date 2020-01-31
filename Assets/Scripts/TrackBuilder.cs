using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBuilder : MonoBehaviour
{
    public GameObject trackElement;

    Queue<GameObject> track = new Queue<GameObject>();
    Vector3 last = new Vector3();
    long updates = 1;
    public long frameBetweenUpdates = 1;
    long framesSinceUpdate = 0;

    Camera cam;

    float getNoise(long val) {
        var camHeight = cam.orthographicSize * 0.9f;
        return Mathf.PerlinNoise(val * 0.01f, 0.0f) * camHeight - camHeight / 2;
    }


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;

        var cube = Instantiate(trackElement, new Vector3(), Quaternion.identity);
        var camHeight = cam.orthographicSize * 0.9f;
        last = new Vector3(0.0f, getNoise(0), 0.0f);
        addCube(++updates);
    }

    GameObject addCube(long id) {
        var cube = Instantiate(trackElement, new Vector3(), Quaternion.identity);
        var next = new Vector3(updates * trackElement.GetComponent<BoxCollider2D>().size.x, getNoise(id), 0.0f);
        var between = next - last;
        var distance = between.magnitude;
        Vector3 copy = cube.transform.localScale;
        copy.z = distance;
        cube.transform.localScale = copy;
        cube.transform.position = last + (between / 2.0f);
        cube.transform.LookAt(next);
        track.Enqueue(cube);
        while (cam.WorldToViewportPoint(track.Peek().transform.position).x < -0.5f)
        {
            Destroy(track.Dequeue());
            if (track.Count == 0)
                break;
        }
        last = next;
        return cube;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (++framesSinceUpdate >= frameBetweenUpdates)
        {
            var cube = addCube(++updates);
            framesSinceUpdate = 0;
        }

        var position_copy = last;
        position_copy.x -= cam.orthographicSize * 2.1f;
        position_copy.y = 0;
        position_copy.z = -10.0f;
        cam.transform.position = Vector3.MoveTowards(cam.transform.position, position_copy, 1.0f / frameBetweenUpdates);
    }
}
