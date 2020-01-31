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
    public int numNoises = 1;
    float[] noiseWeights = { 1.0f, 0.1f, 0.5f };
    float[] noiseMultipliers = { 0.01f, 0.1f, 0.05f };
    float maxNoise = 0;
    long noiseUpdateLength = 1000;

    int noiseFadeInLength = 10;
    int noiseFadeIn = -1;
    int noiseFadeInIndex = 1;

    Camera cam;

    float getNoise(long val) {
        var camHeight = cam.orthographicSize * 0.9f;

        bool fadeIn = noiseFadeIn != -1;
        float fadeInMultiplier = 1.0f;
        if (fadeIn)
        {
            fadeInMultiplier = (float)noiseFadeIn / noiseFadeInLength;
            noiseFadeIn++;
            if (noiseFadeIn >= noiseFadeInLength)
            {
                noiseFadeIn = -1;
            }
        }

        float noise = 0;
        for (int i = 0; i < numNoises; i++) {

            noise += Mathf.PerlinNoise(val * noiseMultipliers[i], 0.0f) * noiseWeights[i] * (i >= noiseFadeInIndex  ? fadeInMultiplier : 1.0f);
        }

        // Normalise to between 0-1
        noise /= maxNoise;
        return noise * camHeight - camHeight / 2;
    }

    void incrementNoise() {
        if (noiseFadeIn == -1)
        {
            noiseFadeIn = 0;
            noiseFadeInIndex = numNoises;
            numNoises++;
        }
        else
        {
            noiseFadeIn = 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;

        maxNoise = 0.0f;
        for (int i = 0; i < numNoises; i++) {
            maxNoise += noiseWeights[i];
        }

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

        if (updates % noiseUpdateLength == 0 && numNoises < noiseWeights.Length) {
            incrementNoise();
        }

        var position_copy = last;
        position_copy.x -= cam.orthographicSize * 2.1f;
        position_copy.y = 0;
        position_copy.z = -10.0f;
        cam.transform.position = Vector3.MoveTowards(cam.transform.position, position_copy, 1.0f / frameBetweenUpdates);
    }
}
