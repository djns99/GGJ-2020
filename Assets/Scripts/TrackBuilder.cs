﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBuilder : MonoBehaviour
{
    public GameObject trackElement;
    private GameObject line;
    private List<Vector2> linePositions2 = new List<Vector2>();
    private List<Vector3> linePositions3 = new List<Vector3>();
    public float segmentWidth = 50;
    private int numSegmentsInViewport;

    long lineIndex = 1;

    public int numNoises = 0;
    float[] noiseWeights = { 1.0f, 0.1f, 0.1f, 0.01f, 0.001f };
    float[] noiseMultipliers = { 0.01f, 0.1f, 0.2f, 0.5f, 1.0f };
    float maxNoise = 0;
    float targetNoise = 0;
    long noiseUpdateLength = 10;

    public int noiseFadeInLength = -1;
    int noiseFadeIn = -1;
    int noiseFadeInIndex = 1;

    Camera cam;
    float camHeight;
    float camWidth;

    float trackSeed;

    float getNoise(long val) {

        if(numNoises == 0)
            return 0;

        var maxTrackBounds = camHeight * 0.9f;

        bool fadeIn = noiseFadeIn != -1;
        float fadeInMultiplier = 1.0f;
        if (fadeIn)
        {
            maxNoise = Mathf.MoveTowards(maxNoise, targetNoise, 1.0f / noiseFadeInLength);
            fadeInMultiplier = noiseFadeIn / (float)noiseFadeInLength;
            if (++noiseFadeIn >= noiseFadeInLength)
            {
                noiseFadeIn = -1;
                maxNoise = targetNoise;
            }
        }

        float noise = 0;
        for (int i = 0; i < numNoises; i++)
        {
            bool shouldDampen = fadeIn && i >= noiseFadeInIndex;
            float dampening = shouldDampen ? fadeInMultiplier : 1.0f;
            float roundNoise = Mathf.PerlinNoise(val * segmentWidth * noiseMultipliers[i], trackSeed);
            Debug.Assert(roundNoise < 1.5 && roundNoise > -0.5);
            float finalNoise = roundNoise * noiseWeights[i] * dampening;
            noise += finalNoise;
        }

        // Normalise to between 0-1
        noise /= maxNoise;
        return noise * maxTrackBounds - maxTrackBounds / 2;
    }

    void incrementNoise() {
        if (noiseFadeIn == -1)
        {
            noiseFadeIn = 0;
            noiseFadeInIndex = numNoises;
        }
        else
        {
            noiseFadeIn = 0;
        }

        numNoises++;

        targetNoise = 0.0f;
        for (int i = 0; i < numNoises; i++)
        {
            targetNoise += noiseWeights[i];
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        trackSeed = Random.Range(0, 100f);
        cam = Camera.main;
        camHeight = cam.orthographicSize * 2;
        camWidth = camHeight * cam.aspect;
        numSegmentsInViewport = Mathf.CeilToInt(camWidth / segmentWidth);
        if (noiseFadeInLength == -1)
        {
            noiseFadeInLength = numSegmentsInViewport;
        }

        line = Instantiate(trackElement, new Vector3(), Quaternion.identity);

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
        bool removePrevious = id >= 3;
        id *= numSegmentsInViewport;
        int numToRemove = 0;
        for (numToRemove = 0; numToRemove < linePositions3.Count && cam.WorldToViewportPoint(linePositions3[numToRemove]).x < -0.1f; numToRemove++)
            ;

        linePositions2.RemoveRange(0, numToRemove);
        linePositions3.RemoveRange(0, numToRemove);

        if (linePositions2.Count > 0)
        {
            linePositions2.RemoveRange(linePositions2.Count - 2, 2);
        }

        for (int i = 0; i < numSegmentsInViewport; i++)
        {
            var next = new Vector3(id * segmentWidth - camWidth, getNoise(id), 0.0f);
            addPointToLinePos(next);
            id++;
        }

        Vector3 first = linePositions3[0];
        Vector3 last = linePositions3[linePositions3.Count - 1];
        last.y = -cam.orthographicSize * 4;
        linePositions2.Add(last);
        first.y = -cam.orthographicSize * 4;
        linePositions2.Add(first);

        var collider = line.GetComponent<PolygonCollider2D>();
        collider.SetPath(0, linePositions2.ToArray());
        var line_renderer = line.GetComponent<LineRenderer>();
        line_renderer.positionCount = linePositions3.Count;
        line_renderer.SetPositions(linePositions3.ToArray());
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.WorldToViewportPoint(linePositions3[linePositions3.Count - 1]).x < 1.1f)
        {
            updateLine(++lineIndex);

            if (lineIndex % noiseUpdateLength == 0 && numNoises < noiseWeights.Length)
            {
                incrementNoise();
            }
        }
    }
}
