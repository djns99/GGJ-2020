using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTrackBuilder : MonoBehaviour
{
    public GameObject trackElement;
    private List<Vector2> linePositions2 = new List<Vector2>();
    private List<Vector3> linePositions3 = new List<Vector3>();
    private LinkedList<GameObject> lines = new LinkedList<GameObject>();
    public float segmentWidth = 1;
    public int minSegmentsBetweenObstacle = 500;
    public int maxSegmentsBetweenObstacle = 10000;
    private int numSegmentsInViewport;
    public float cameraTrackMinPercent = 0.025f;
    public float cameraTrackMaxPercent = 0.75f;

    public float targetGapWidthPixels = 20.0f;
    public float maxCliffHeightPixels = 20.0f;

    private int progress = 0;
    private int nextObstacleProgressMin;
    private int nextObstacleProgressMax;

    private float trackAllowedRangeMin;
    private float trackAllowedRangeMax;

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
        var lastPos = getLinePointAtIndex(lines.Last.Value, -1);

        int numObstacles = 3;
        int objectId = Random.Range(0, numObstacles);
        switch (objectId) {
            case 0:
                {
                    float targetWidth = targetGapWidthPixels;
                    int numSegments = Mathf.CeilToInt(targetWidth / segmentWidth);
                    float realWidth = numSegments * segmentWidth;
                    progress += numSegments;

                    // Drop off bottom of screen
                    var pos = lastPos;
                    pos.y = -camHeight;
                    addPointToLinePos(pos);

                    // Skip to other side
                    pos.x += realWidth;
                    addPointToLinePos(pos);

                    // Come back up the other side
                    pos.y = getNoise(progress - 1);
                    addPointToLinePos(pos);
                    return;
                }
            case 1:
                {
                    // Flat leading up to cliff
                    int headWidth = 10;
                    int headNumSegments = Mathf.CeilToInt(headWidth / segmentWidth);
                    progress += headNumSegments;

                    var pos = lastPos;
                    var posPrev = getLinePointAtIndex(lines.Last.Value, -2);
                    var dir = pos - posPrev;
                    for (int i = 0; i < headNumSegments; i++)
                    {
                        dir.y *= 0.7f;
                        pos += dir;
                        addPointToLinePos(pos);
                    }

                    // Cliff
                    pos.y = Mathf.Min(trackAllowedRangeMax, pos.y + maxCliffHeightPixels);
                    addPointToLinePos(pos);

                    // Return to normal
                    int tailWidth = 50;
                    int tailNumSegments = Mathf.CeilToInt(tailWidth / segmentWidth);
                    progress += tailNumSegments;
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
            case 2:
                {
                    // Cliff
                    var pos = lastPos;
                    pos.y = Mathf.Max(trackAllowedRangeMin, pos.y - maxCliffHeightPixels);
                    addPointToLinePos(pos);

                    // Flat at bottom of cliff
                    int headWidth = 50;
                    int headNumSegments = Mathf.CeilToInt(headWidth / segmentWidth);
                    progress += headNumSegments;

                    for (int i = 0; i < headNumSegments; i++)
                    {
                        pos.x += segmentWidth;
                        addPointToLinePos(pos);
                    }

                    // Return to normal
                    int tailWidth = 50;
                    int tailNumSegments = Mathf.CeilToInt(tailWidth / segmentWidth);
                    progress += tailNumSegments;
                    var target = new Vector3(pos.x + tailNumSegments * segmentWidth, getNoise(progress));
                    var targetControl = new Vector3(pos.x + (tailNumSegments - 1) * segmentWidth, getNoise(progress - 1));
                    var start = pos;
                    var startControl = new Vector3(pos.x + segmentWidth, pos.y);

                    for (int i = 0; i < tailNumSegments; i++)
                    {
                        pos.y = getBezierPos(start, startControl, target, targetControl, (float)i / tailNumSegments).y;
                        pos.x += segmentWidth;
                        addPointToLinePos(pos);
                    }
                }
                return;
        }
    }

    void createNewLine() {
        if (linePositions3.Count > 1)
        {
            var line = Instantiate(trackElement, new Vector3(), Quaternion.identity);

            Vector3 first = linePositions3[0];
            Vector3 last = linePositions3[linePositions3.Count - 1];
            Vector3 lastCopy = last;
            last.y = -camHeight * 4;
            linePositions2.Add(last);
            first.y = -camHeight * 4;
            linePositions2.Add(first);

            var collider = line.GetComponent<PolygonCollider2D>();
            collider.SetPath(0, linePositions2.ToArray());
            var line_renderer = line.GetComponent<LineRenderer>();
            line_renderer.positionCount = linePositions3.Count;
            line_renderer.SetPositions(linePositions3.ToArray());
            lines.AddLast(line);

            linePositions2.Clear();
            linePositions3.Clear();

            addPointToLinePos(lastCopy);
        }
    }

    Vector3 getLinePointAtIndex(GameObject line, int index) {
        var renderer = line.GetComponent<LineRenderer>();
        return renderer.GetPosition(index >= 0 ? index : (renderer.positionCount + index));
    }

    void updateLine()
    {
        while (lines.Count != 0)
        {
            var line = lines.First.Value;
            if (line.GetComponent<LineRenderer>().positionCount != 0 && cam.WorldToViewportPoint(getLinePointAtIndex(lines.First.Value, -1)).x < -0.2f)
            {
                Destroy(line);
                lines.RemoveFirst();
            }
            else
            {
                break;
            }
        }

        var endProgress = progress + numSegmentsInViewport;
        while (progress < endProgress)
        {
            var randomness = Random.Range(0.0f, 1.0f);
            var threshold = 1.0f / (maxSegmentsBetweenObstacle - minSegmentsBetweenObstacle);
            bool randomChoiceObstacle = progress >= nextObstacleProgressMin && randomness < threshold;
            bool forcedObstacle = progress >= nextObstacleProgressMax;
            if (randomChoiceObstacle || forcedObstacle)
            {
                // New line when we create an obstacle
                createNewLine();
                addObstacle();
                nextObstacleProgressMin = progress + minSegmentsBetweenObstacle;
                nextObstacleProgressMax = progress + maxSegmentsBetweenObstacle;
                // New line after we finish with the obstacle
                createNewLine();
            }
            else
            {
                // Continue with noise
                var next = new Vector3(progress * segmentWidth - camWidth, getNoise(progress), 0.0f);
                addPointToLinePos(next);
                progress++;
            }
        }

        createNewLine();
    }

    // Start is called before the first frame update
    void Start()
    {
        trackSeed = Random.Range(0, 100f);
        cam = Camera.main;
        camHeight = cam.orthographicSize * 2;
        camWidth = camHeight * cam.aspect;
        numSegmentsInViewport = Mathf.CeilToInt(camWidth / segmentWidth);

        trackAllowedRangeMin = camHeight * cameraTrackMinPercent - camHeight / 2;
        trackAllowedRangeMax = camHeight * cameraTrackMaxPercent - camHeight / 2;

        nextObstacleProgressMin = Mathf.Max(minSegmentsBetweenObstacle, numSegmentsInViewport * 2);
        nextObstacleProgressMax = nextObstacleProgressMin + (maxSegmentsBetweenObstacle - minSegmentsBetweenObstacle);

        createNewLine();

        updateLine();
        updateLine();
        updateLine();
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.WorldToViewportPoint(getLinePointAtIndex(lines.Last.Value, -1)).x < 1.2f)
        {
            updateLine();
        }
    }
}
