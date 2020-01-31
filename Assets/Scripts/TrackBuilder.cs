using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBuilder : MonoBehaviour
{
    public GameObject trackElement;
    private GameObject line;
    private List<Vector2> linePositions2 = new List<Vector2>();
    private List<Vector3> linePositions3 = new List<Vector3>();
    public int segmentWidth = 50;
    private int numSegmentsInViewport;

    long lineIndex = 1;

    public int numNoises = 1;
    float[] noiseWeights = { 1.0f, 0.1f, 0.5f };
    float[] noiseMultipliers = { 0.01f, 0.1f, 0.05f };
    float maxNoise = 0;
    long noiseUpdateLength = 10;


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
        line = Instantiate(trackElement, new Vector3(), Quaternion.identity);
        int width = (int)(cam.orthographicSize * cam.aspect * 2);
        numSegmentsInViewport = (int)(width / segmentWidth);

        maxNoise = 0.0f;
        for (int i = 0; i < numNoises; i++) {
            maxNoise += noiseWeights[i];
        }

        updateLine(0);
        updateLine(1);
        updateLine(2);
        lineIndex = 2;
    }

    void addPointToLinePos(Vector3 vec) {
        linePositions2.Add(vec);
        linePositions3.Add(vec);
    }

    void updateLine(long id)
    {
        int width = (int)(cam.orthographicSize * cam.aspect * 2);
        bool removePrevious = id >= 3;
        id *= numSegmentsInViewport;
        int numToRemove = 0;
        for (numToRemove = 0; numToRemove < linePositions3.Count && cam.WorldToViewportPoint(linePositions3[numToRemove]).x < 0.1f; numToRemove++)
            ;

        linePositions2.RemoveRange(0, numToRemove);
        linePositions3.RemoveRange(0, numToRemove);

        if (linePositions2.Count > 0)
        {
            linePositions2.RemoveRange(linePositions2.Count - 2, 2);
            linePositions3.RemoveRange(linePositions3.Count - 2, 2);
        }

        for (int i = 0; i < numSegmentsInViewport; i++)
        {
            var next = new Vector3(id * segmentWidth - width, getNoise(id), 0.0f);
            addPointToLinePos(next);
            id++;
        }

        Vector3 first = linePositions3[0];
        Vector3 last = linePositions3[linePositions3.Count - 1];
        last.y = -cam.orthographicSize * 4;
        addPointToLinePos(last);
        first.y = -cam.orthographicSize * 4;
        addPointToLinePos(first);

        var collider = line.GetComponent<PolygonCollider2D>();
        collider.SetPath(0, linePositions2.ToArray());
        var line_renderer = line.GetComponent<LineRenderer>();
        line_renderer.positionCount = linePositions3.Count;
        line_renderer.SetPositions(linePositions3.ToArray());
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.WorldToViewportPoint(linePositions3[linePositions3.Count - 3]).x < 1.1f)
        {
            updateLine(++lineIndex);
        }

        if (lineIndex % noiseUpdateLength == 0 && numNoises < noiseWeights.Length) {
            incrementNoise();
        }
    }
}
