using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTrackBuilder : MonoBehaviour
{
    public GameObject trackElement;
    private GameObject line;
    private List<Vector2> linePositions2 = new List<Vector2>();
    private List<Vector3> linePositions3 = new List<Vector3>();
    public float segmentWidth = 1;
    public int minSegmentsBetweenObstacle = 500;
    public int maxSegmentsBetweenObstacle = 10000;
    private int numSegmentsInViewport;

    private int progress = 0;
    private int nextObstacleProgressMin;
    private int nextObstacleProgressMax;

    private float trackAllowedRangeMin;
    private float trackAllowedRangeMax;

    long lineIndex = 1;

    Camera cam;
    float camHeight;
    float camWidth;

    float trackSeed;

    float getNoise(long val)
    {
        var trackWidth = trackAllowedRangeMax - trackAllowedRangeMin;
        float noise = Mathf.PerlinNoise(val * segmentWidth * 0.01f, trackSeed);
        noise = Mathf.Clamp01(noise);
        return noise * trackWidth + trackAllowedRangeMin;
    }

    // Start is called before the first frame update
    void Start()
    {
        trackSeed = Random.Range(0, 100f);
        cam = Camera.main;
        camHeight = cam.orthographicSize * 2;
        camWidth = camHeight * cam.aspect;
        numSegmentsInViewport = Mathf.CeilToInt(camWidth / segmentWidth);

        trackAllowedRangeMin = -(camHeight / 2.0f) * 0.95f;
        trackAllowedRangeMax = (camHeight / 2.0f) * 0.5f;

        nextObstacleProgressMin = minSegmentsBetweenObstacle;
        nextObstacleProgressMax = maxSegmentsBetweenObstacle;

        line = Instantiate(trackElement, new Vector3(), Quaternion.identity);

        updateLine();
        updateLine();
        updateLine();
        lineIndex = 2;
    }

    void addPointToLinePos(Vector3 vec)
    {
        linePositions2.Add(vec);
        linePositions3.Add(vec);
    }

    float getSlerpHeight(float start, float end, float progress)
    {
        Vector3 startVec = new Vector3(start, 0.0f);
        Vector3 endVec = new Vector3(end, 0.0f);
        return Vector3.Slerp(startVec, endVec, progress).x;
    }

    Vector3 getBezierPos(Vector3 start, Vector3 startControl, Vector3 end, Vector3 endControl, float progress)
    {
        var p0 = start;
        var p1 = startControl;
        var p2 = endControl;
        var p3 = end;
        
        var t = progress;
        return Mathf.Pow(1f - t, 3f) * p0 + 3f * Mathf.Pow(1f - t, 2f) * t * p1 + 3f * (1f - t) * Mathf.Pow(t, 2f) * p2 + Mathf.Pow(t, 3f) * p3;
    }

    void addObstacle()
    {
        int numObstacles = 2;
        int objectId = Random.Range(0, numObstacles);
        switch (objectId) {
            case 0:
                {
                    int targetWidth = 50;
                    int numSegments = Mathf.CeilToInt(targetWidth / segmentWidth);
                    float realWidth = numSegments * segmentWidth;
                    progress += numSegments;

                    var pos = linePositions3[linePositions3.Count - 1];
                    pos.y = -camHeight;
                    addPointToLinePos(pos);

                    pos.x += realWidth;
                    addPointToLinePos(pos);

                    pos.y = getNoise(progress - 1);
                    addPointToLinePos(pos);
                    return;
                }
            case 1:
                {
                    

                    float maxHeight = trackAllowedRangeMax;
                    float maxJump = 20f;

                    int headWidth = 10;
                    int headNumSegments = Mathf.CeilToInt(headWidth / segmentWidth);
                    progress += headNumSegments;

                    var pos = linePositions3[linePositions3.Count - 1];
                    var posPrev = linePositions3[linePositions3.Count - 2];
                    var dir = pos - posPrev;
                    for (int i = 0; i < headNumSegments; i++)
                    {
                        dir.y *= 0.7f;
                        pos += dir;
                        addPointToLinePos(pos);
                    }

                    int tailWidth = 50;
                    int tailNumSegments = Mathf.CeilToInt(tailWidth / segmentWidth);
                    progress += tailNumSegments;

                    pos.y = Mathf.Min(maxHeight, pos.y + maxJump);
                    addPointToLinePos(pos);
                    var target = new Vector3(pos.x + tailNumSegments * segmentWidth, getNoise(progress));
                    var targetControl = new Vector3(pos.x + (tailNumSegments - 1) * segmentWidth, getNoise(progress - 1));
                    var start = pos;
                    var startControl = new Vector3(pos.x + segmentWidth, pos.y + segmentWidth / 10f);

                    for (int i = 0; i < tailNumSegments; i++) {
                        pos.y = getBezierPos(start, startControl, target, targetControl, (float)i / tailNumSegments).y;
                        pos.x += segmentWidth;
                        addPointToLinePos(pos);
                    }
                }
                return;

        }
    }

    void updateLine()
    {
        int numToRemove = 0;
        for (numToRemove = 0; numToRemove < linePositions3.Count && cam.WorldToViewportPoint(linePositions3[numToRemove]).x < -0.2f; numToRemove++)
            ;

        linePositions2.RemoveRange(0, numToRemove);
        linePositions3.RemoveRange(0, numToRemove);

        if (linePositions2.Count > 0)
        {
            linePositions2.RemoveRange(linePositions2.Count - 2, 2);
        }

        var endProgress = progress + numSegmentsInViewport;
        while (progress < endProgress)
        {
            var randomness = Random.Range(0, 1);
            var threshold = 1.0f;// 1.0f / (maxSegmentsBetweenObstacle - minSegmentsBetweenObstacle);
            bool randomChoiceObstacle = progress >= nextObstacleProgressMin && randomness < threshold;
            bool forcedObstacle = progress >= nextObstacleProgressMax;
            if (randomChoiceObstacle || forcedObstacle)
            {
                addObstacle();
                nextObstacleProgressMin = progress + minSegmentsBetweenObstacle;
                nextObstacleProgressMax = progress + maxSegmentsBetweenObstacle;
            }
            else
            {
                // Continue with noise
                var next = new Vector3(progress * segmentWidth - camWidth, getNoise(progress), 0.0f);
                addPointToLinePos(next);
                progress++;
            }
        }

        Vector3 first = linePositions3[0];
        Vector3 last = linePositions3[linePositions3.Count - 1];
        last.y = -camHeight * 4;
        linePositions2.Add(last);
        first.y = -camHeight * 4;
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
            updateLine();
        }
    }
}
