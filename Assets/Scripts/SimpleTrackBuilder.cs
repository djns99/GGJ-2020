using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public float gapWidthPixels = 20.0f;
    public float maxCliffHeightPixels = 20.0f;
    public float jumpCliffWidthPixels = 20.0f;
    public float jumpCliffMaxHeightPixels = 20.0f;
    public float bumpyGroundLength = 100.0f;
    public float bumpyGroundNoise = 0.5f;
    public float bumpyGroundWeight = 0.5f;
    public float spikePitDepthPixels = 20.0f;
    public float spikePitWidthPixels = 200.0f;

    private int progress = 0;
    private int nextObstacleProgressMin;
    private int nextObstacleProgressMax;

    private float trackAllowedRangeMin;
    private float trackAllowedRangeMax;

    Camera cam;
    float camHeight;
    float camWidth;

    float trackSeed;

    float getNoise(long val, float weight = 0.01f)
    {
        var trackWidth = trackAllowedRangeMax - trackAllowedRangeMin;
        float noise = Mathf.PerlinNoise(val * segmentWidth * weight, trackSeed);
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

            var mesh_filter = line.GetComponent<MeshFilter>();
            mesh_filter.mesh = collider.CreateMesh(true, false);

            linePositions2.Clear();
            linePositions3.Clear();

            addPointToLinePos(lastCopy);
        }
    }

    Vector3 getLinePointAtIndex(GameObject line, int index) {
        var renderer = line.GetComponent<LineRenderer>();
        return renderer.GetPosition(index >= 0 ? index : (renderer.positionCount + index));
    }

    void addPerlinPoint() {
        // Continue with noise
        var next = new Vector3(progress * segmentWidth - camWidth, getNoise(progress), 0.0f);
        addPointToLinePos(next);
        progress++;
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
                addObstacle();
                nextObstacleProgressMin = progress + minSegmentsBetweenObstacle;
                nextObstacleProgressMax = progress + maxSegmentsBetweenObstacle;
            }
            else
            {
                addPerlinPoint();
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

        // Spawn car
        if (Input.GetKeyDown(KeyCode.G))
        {
            SceneManager.LoadScene("CarScene");
        }
    }

    #region obstacles

    Vector3 smoothIntoFlat(Vector3 pos, Vector3 prevPos) {
        var dir = pos - prevPos;
        while (Vector3.Angle(dir, Vector3.right) > 1) {
            dir.y *= 0.7f;
            pos += dir;
            addPointToLinePos(pos);
            progress++;
        }
        return pos;
    }

    void bezeirCurveBackToPerlin(Vector3 pos, int tailWidth = 50)
    {
        // Return to normal
        int tailNumSegments = Mathf.CeilToInt(tailWidth / segmentWidth);
        progress += tailNumSegments;
        var target = new Vector3(pos.x + tailNumSegments * segmentWidth, getNoise(progress));
        var targetControl = new Vector3(pos.x + (tailNumSegments - 1) * segmentWidth, getNoise(progress - 1));
        targetControl = target + (targetControl - target).normalized * 10;
        var start = pos;
        var startControl = new Vector3(pos.x + segmentWidth, pos.y);
        startControl = start + (startControl - start).normalized * 10;

        for (int i = 0; i < tailNumSegments; i++)
        {
            pos.y = getBezierPos(start, startControl, target, targetControl, (float)i / tailNumSegments).y;
            pos.x += segmentWidth;
            addPointToLinePos(pos);
        }
    }

    void colourMesh(GameObject line, Color color)
    {
        Mesh mesh = line.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        // create new colors array where the colors will be created.
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = color;

        // assign the array of colors to the Mesh.
        mesh.colors = colors;
    }

    void gap() {

        createNewLine();

        var lastPos = linePositions3[0];
        // Transition into a flat line
        var pos = smoothIntoFlat(lastPos, getLinePointAtIndex(lines.Last.Value, -2));

        // Start new line so we can retexture
        createNewLine();

        //TODO Do texturing etc
        int numLeadIn = 25;
        pos.x += segmentWidth * numLeadIn;
        progress += numLeadIn;
        addPointToLinePos(pos);

        // End left half
        createNewLine();

        // Gap
        float targetWidth = gapWidthPixels;
        int numSegments = Mathf.CeilToInt(targetWidth / segmentWidth);
        float realWidth = numSegments * segmentWidth;
        progress += numSegments;

        // Clear automatically added link
        linePositions2.Clear();
        linePositions3.Clear();

        pos.x += realWidth;
        int numTrailOff = 25;
        pos.y = getNoise(progress + numTrailOff);
        addPointToLinePos(pos);
        pos.x += segmentWidth * numTrailOff;
        progress += numTrailOff;
        addPointToLinePos(pos);

        // End obstalce
        createNewLine();

        // Ease back into normal oepration
        bezeirCurveBackToPerlin(pos);

        createNewLine();
    }

    void jumpCliff() {
        createNewLine();

        var lastPos = linePositions3[0];
        // Transition into a flat line
        var pos = smoothIntoFlat(lastPos, getLinePointAtIndex(lines.Last.Value, -2));

        // Start new line so we can retexture
        createNewLine();

        int numLeadIn = 10;
        pos.x += segmentWidth * numLeadIn;
        progress += numLeadIn;
        addPointToLinePos(pos);

        // End left half
        createNewLine();

        // Gap
        float targetWidth = gapWidthPixels;
        int numSegments = Mathf.CeilToInt(targetWidth / segmentWidth);
        float realWidth = numSegments * segmentWidth;
        progress += numSegments;

        // Clear automatically added link
        linePositions2.Clear();
        linePositions3.Clear();

        // Set cliff position
        pos.x += realWidth;
        pos.y = Mathf.Max(trackAllowedRangeMin, lastPos.y - jumpCliffMaxHeightPixels);
        addPointToLinePos(pos);

        //TODO Do texturing etc
        int numTrailOff = 50;
        pos.x += segmentWidth * numTrailOff;
        progress += numTrailOff;
        addPointToLinePos(pos);

        // End obstalce
        createNewLine();

        // Ease back into normal oepration
        bezeirCurveBackToPerlin(pos);
    }

    void cliffUp() {
        createNewLine();

        var lastPos = linePositions3[0];
        var pos = smoothIntoFlat(lastPos, getLinePointAtIndex(lines.Last.Value, -2));

        createNewLine();

        // Flat leading up to cliff
        int numLeadIn = 50;
        pos.x += numLeadIn * segmentWidth;
        progress += numLeadIn;
        addPointToLinePos(pos);

        createNewLine();

        // Clear automatically added link
        linePositions2.Clear();
        linePositions3.Clear();

        // Cliff
        pos.y = Mathf.Min(trackAllowedRangeMax, pos.y + maxCliffHeightPixels);
        addPointToLinePos(pos);

        int numTrailOff = 10;
        pos.x += segmentWidth * numTrailOff;
        progress += numTrailOff;
        addPointToLinePos(pos);

        createNewLine();

        bezeirCurveBackToPerlin(pos);
    }

    void cliffDown() {
        createNewLine();

        var lastPos = linePositions3[0];
        var pos = smoothIntoFlat(lastPos, getLinePointAtIndex(lines.Last.Value, -2));

        createNewLine();

        // Flat leading up to cliff
        int numLeadIn = 10;
        pos.x += numLeadIn * segmentWidth;
        progress += numLeadIn;
        addPointToLinePos(pos);

        createNewLine();

        // Clear automatically added link
        linePositions2.Clear();
        linePositions3.Clear();

        // Cliff
        pos.y = Mathf.Max(trackAllowedRangeMin, pos.y - maxCliffHeightPixels);
        addPointToLinePos(pos);

        int numTrailOff = 50;
        pos.x += segmentWidth * numTrailOff;
        progress += numTrailOff;
        addPointToLinePos(pos);

        createNewLine();

        bezeirCurveBackToPerlin(pos);
    }

    void bumpyGround()
    {
        var numSegments = Mathf.CeilToInt(bumpyGroundLength / segmentWidth);
        int segmentsDampening = 20;
        for (int i = 0; i < numSegments + segmentsDampening * 2; i++)
        {
            // Continue with noise
            var noise1 = getNoise(progress);
            var noise2 = getNoise(progress, bumpyGroundNoise) * bumpyGroundWeight;
            if (i < segmentsDampening)
            {
                noise2 *= (float)i / segmentsDampening;
            }
            else if(i >= numSegments + segmentsDampening)
            {
                noise2 *= 1.0f - (float)(i - numSegments - segmentsDampening) / segmentsDampening;
            }
            var next = new Vector3(progress * segmentWidth - camWidth, noise1 + noise2, 0.0f);
            addPointToLinePos(next);
            progress++;
        }
    }

    void spikePit() {
        createNewLine();

        var lastPos = linePositions3[0];
        var pos = smoothIntoFlat(lastPos, getLinePointAtIndex(lines.Last.Value, -2));

        createNewLine();

        // Flat leading up to cliff
        int numLeadIn = 10;
        pos.x += numLeadIn * segmentWidth;
        progress += numLeadIn;
        addPointToLinePos(pos);

        createNewLine();

        // Clear automatically added link
        linePositions2.Clear();
        linePositions3.Clear();

        // Pit start
        var originalY = pos.y;
        pos.y = Mathf.Max(trackAllowedRangeMin, pos.y - spikePitDepthPixels);
        addPointToLinePos(pos);

        int spikePitSegments = Mathf.CeilToInt(spikePitWidthPixels / segmentWidth);
        // Make sure multiple 4 number of spikes
        spikePitSegments += 4 - spikePitSegments % 4;
        float spikeSum = (originalY - pos.y) * 0.2f;
        for (int i = 0; i < spikePitSegments; i++) {
            pos.x += segmentWidth;
            pos.y += spikeSum;
            if (i % 2 == 1)
            {
                spikeSum *= -1;
            }
            addPointToLinePos(pos);
            progress++;
        }

        createNewLine();

        // Clear automatically added link
        linePositions2.Clear();
        linePositions3.Clear();

        // Cliff
        pos.y = originalY;
        addPointToLinePos(pos);

        int numTrailOff = 10;
        pos.x += segmentWidth * numTrailOff;
        progress += numTrailOff;
        addPointToLinePos(pos);

        createNewLine();

        bezeirCurveBackToPerlin(pos);
    }

    void addObstacle()
    {

        int numObstacles = 5;
        int objectId = Random.Range(0, numObstacles);
        switch (objectId)
        {
            case 0:
                gap();
                return;
            case 1:
                cliffUp();
                return;
            case 2:
                cliffDown();
                return;
            case 3:
                jumpCliff();
                return;
            case 4:
                spikePit();
                //bumpyGround();
                return;
        }
    }
    #endregion
}
