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

    public float flatGapWidthPixels = 20.0f;
    public float gapWidthPixels = 20.0f;
    public float maxCliffHeightPixels = 20.0f;
    public float jumpCliffWidthPixels = 20.0f;
    public float jumpCliffMaxHeightPixels = 20.0f;
    public float bumpyGroundLength = 100.0f;
    public float bumpyGroundNoise = 0.5f;
    public float bumpyGroundWeight = 0.5f;
    public float spikePitDepthPixels = 20.0f;
    public float spikePitWidthPixels = 200.0f;
    public float jumpPitWidthPixels = 100.0f;
    public int jumpPitMidWidthSegments = 10;
    public float pylonMaxHeight = 30.0f;
    public int pylonWidthSegments = 10;
    public float pylonPitMaxHeight = 30.0f;
    public float pylonPitWidthPixels = 60.0f;

    public int numObstacles = 9;

    public Material bridgeMaterial;
    public Material groundMaterial;

    public Material bridgeLineMaterial;
    public Material groundLineMaterial;
    public Material spikeLineMaterial;

    private int progress = 0;
    private int nextObstacleProgressMin;
    private int nextObstacleProgressMax;

    private float trackAllowedRangeMin;
    private float trackAllowedRangeMax;

    public bool pointIsInTrack(Vector3 vec) {
        foreach (var line in lines) {
            if (line.GetComponent<PolygonCollider2D>().bounds.Contains(vec))
                return true;
        }
        return false;
    }


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
        vec.z = -1;
        linePositions3.Add(vec);
        vec.y += trackElement.GetComponent<LineRenderer>().startWidth / 2;
        linePositions2.Add(vec);
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

    void createNewLine(bool linkPrevious = true, Material material = null, Material lineMaterial = null) {
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

            if (material != null)
            {
                line.GetComponent<MeshRenderer>().material = material;
                line.GetComponent<LineRenderer>().material = lineMaterial;
            }
            else
            {
                line.GetComponent<MeshRenderer>().material = groundMaterial;
                line.GetComponent<LineRenderer>().material = groundLineMaterial;
            }
            var mesh_filter = line.GetComponent<MeshFilter>();
            mesh_filter.mesh = collider.CreateMesh(false, false);
            mesh_filter.mesh.SetUVs(0, mesh_filter.mesh.vertices);
           
            List<Vector3> normals = new List<Vector3>();
            foreach (var _ in mesh_filter.mesh.vertices) {
                normals.Add(Vector3.back);
            }
            mesh_filter.mesh.SetNormals(normals);

            linePositions2.Clear();
            linePositions3.Clear();

            if(linkPrevious)
                addPointToLinePos(lastCopy);
        }
    }

    Vector3 getLinePointAtIndex(GameObject line, int index) {
        var renderer = line.GetComponent<LineRenderer>();
        return renderer.GetPosition(index >= 0 ? index : (renderer.positionCount + index));
    }

    void addPerlinPoint() {
        // Continue with noise
        var next = new Vector3(progress * segmentWidth, getNoise(progress), 0.0f);
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

        int segmentsInCam = Mathf.CeilToInt(camWidth / segmentWidth);
        Vector3 pos = new Vector3(-segmentsInCam * segmentWidth, trackAllowedRangeMin, 0);
        addPointToLinePos(pos);
        pos.x = segmentsInCam * segmentWidth;
        addPointToLinePos(pos);
        progress = segmentsInCam;

        createNewLine();

        bezeirCurveBackToPerlin(pos, 100);

        createNewLine();

        updateLine();
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.WorldToViewportPoint(getLinePointAtIndex(lines.Last.Value, -1)).x < 3f)
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
        int controlLength = tailWidth / 5;
        progress += tailNumSegments;
        var target = new Vector3(pos.x + tailNumSegments * segmentWidth, getNoise(progress));
        var targetControl = new Vector3(pos.x + (tailNumSegments - 1) * segmentWidth, getNoise(progress - 1));
        targetControl = target + (targetControl - target).normalized * controlLength;
        var start = pos;
        var startControl = new Vector3(pos.x + segmentWidth, pos.y);
        startControl = start + (startControl - start).normalized * controlLength;

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
        createNewLine(false, bridgeMaterial, bridgeLineMaterial);

        // Gap
        float targetWidth = gapWidthPixels;
        int numSegments = Mathf.CeilToInt(targetWidth / segmentWidth);
        float realWidth = numSegments * segmentWidth;
        progress += numSegments;

        pos.x += realWidth;
        int numTrailOff = 25;
        pos.y = getNoise(progress + numTrailOff);
        addPointToLinePos(pos);
        pos.x += segmentWidth * numTrailOff;
        progress += numTrailOff;
        addPointToLinePos(pos);

        // End obstalce
        createNewLine(true, bridgeMaterial, bridgeLineMaterial);

        // Ease back into normal oepration
        bezeirCurveBackToPerlin(pos);

        createNewLine();
    }

    void flatGap()
    {
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
        createNewLine(false, bridgeMaterial, bridgeLineMaterial);

        // Gap
        float targetWidth = flatGapWidthPixels;
        int numSegments = Mathf.CeilToInt(targetWidth / segmentWidth);
        float realWidth = numSegments * segmentWidth;
        progress += numSegments;


        pos.x += realWidth;
        int numTrailOff = 25;
        addPointToLinePos(pos);
        pos.x += segmentWidth * numTrailOff;
        progress += numTrailOff;
        addPointToLinePos(pos);

        // End obstalce
        createNewLine(true, bridgeMaterial, bridgeLineMaterial);

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
        createNewLine(false, bridgeMaterial, bridgeLineMaterial);

        // Gap
        float targetWidth = gapWidthPixels;
        int numSegments = Mathf.CeilToInt(targetWidth / segmentWidth);
        float realWidth = numSegments * segmentWidth;
        progress += numSegments;

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
        createNewLine(true, bridgeMaterial, bridgeLineMaterial);

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

        createNewLine(false);

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

        createNewLine(false);

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

    //void bumpyGround()
    //{
    //    var numSegments = Mathf.CeilToInt(bumpyGroundLength / segmentWidth);
    //    int segmentsDampening = 20;
    //    for (int i = 0; i < numSegments + segmentsDampening * 2; i++)
    //    {
    //        // Continue with noise
    //        var noise1 = getNoise(progress);
    //        var noise2 = getNoise(progress, bumpyGroundNoise) * bumpyGroundWeight;
    //        if (i < segmentsDampening)
    //        {
    //            noise2 *= (float)i / segmentsDampening;
    //        }
    //        else if(i >= numSegments + segmentsDampening)
    //        {
    //            noise2 *= 1.0f - (float)(i - numSegments - segmentsDampening) / segmentsDampening;
    //        }
    //        var next = new Vector3(progress * segmentWidth - camWidth, noise1 + noise2, 0.0f);
    //        addPointToLinePos(next);
    //        progress++;
    //    }
    //}

    void pylon() {
        createNewLine();

        var lastPos = linePositions3[0];
        var pos = smoothIntoFlat(lastPos, getLinePointAtIndex(lines.Last.Value, -2));

        createNewLine();

        // Flat leading up to pylon
        int numLeadIn = 10;
        pos.x += numLeadIn * segmentWidth;
        progress += numLeadIn;
        addPointToLinePos(pos);

        createNewLine(false);

        pos.y = Mathf.Min(trackAllowedRangeMax, pos.y + pylonMaxHeight);
        addPointToLinePos(pos);

        pos.x += pylonWidthSegments * segmentWidth;
        progress += pylonWidthSegments;
        addPointToLinePos(pos);

        createNewLine(false);

        // Flat leading away from pylon
        int numTrailOff = 10;
        pos.y = getNoise(progress + numTrailOff);
        addPointToLinePos(pos);
        pos.x += numTrailOff * segmentWidth;
        progress += numTrailOff;
        addPointToLinePos(pos);

        createNewLine();

        bezeirCurveBackToPerlin(pos);

        createNewLine();
    }

    void pylonPit()
    {
        createNewLine();

        var lastPos = linePositions3[0];
        var pos = smoothIntoFlat(lastPos, getLinePointAtIndex(lines.Last.Value, -2));

        createNewLine();

        // Flat leading up to pylon
        int numLeadIn = 10;
        pos.x += numLeadIn * segmentWidth;
        progress += numLeadIn;
        addPointToLinePos(pos);

        createNewLine(false);

        pos.y = Mathf.Min(trackAllowedRangeMax, pos.y + pylonMaxHeight);
        addPointToLinePos(pos);

        pos.x += pylonWidthSegments * segmentWidth;
        progress += pylonWidthSegments;
        addPointToLinePos(pos);

        createNewLine(false);

        // Skip pit
        int pitWidthSgements = Mathf.CeilToInt(pylonPitWidthPixels / segmentWidth);
        pos.x += pitWidthSgements * segmentWidth;
        addPointToLinePos(pos);
        progress += pitWidthSgements;

        // Make second pylon
        pos.x += pylonWidthSegments * segmentWidth;
        progress += pylonWidthSegments;
        addPointToLinePos(pos);

        createNewLine(false);
        // Flat leading away from pylon
        int numTrailOff = 10;
        pos.y = getNoise(progress + numTrailOff);
        addPointToLinePos(pos);
        pos.x += numTrailOff * segmentWidth;
        progress += numTrailOff;
        addPointToLinePos(pos);

        createNewLine();

        bezeirCurveBackToPerlin(pos);

        createNewLine();
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

        createNewLine(false);

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

        createNewLine(false, groundMaterial, spikeLineMaterial);

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

    void jumpPit() {

        createNewLine();

        var lastPos = linePositions3[0];
        // Transition into a flat line
        var pos = smoothIntoFlat(lastPos, getLinePointAtIndex(lines.Last.Value, -2));

        // Start new line so we can retexture
        createNewLine();

        //TODO Do texturing etc
        int numLeadIn = 10;
        pos.x += segmentWidth * numLeadIn;
        progress += numLeadIn;
        addPointToLinePos(pos);

        // End left half
        createNewLine(false, bridgeMaterial, bridgeLineMaterial);

        // Gap
        int numSegments = Mathf.CeilToInt(jumpPitWidthPixels / segmentWidth);
        int numPlatformUnits = (numSegments + jumpPitMidWidthSegments - 1) / jumpPitMidWidthSegments;
        if (numSegments % 2 == 0)
            numSegments++;

        int halfSegments = (numPlatformUnits - 1) * jumpPitMidWidthSegments;
        float halfWidth = halfSegments * segmentWidth;
        progress += halfSegments;

        pos.x += halfWidth;
        pos.y = getNoise(progress);
        addPointToLinePos(pos);

        for (int i = 0; i < jumpPitMidWidthSegments; i++)
        {
            pos.x += segmentWidth;
            addPointToLinePos(pos);
        }

        progress += jumpPitMidWidthSegments + halfSegments;
        pos.x += halfWidth;

        createNewLine(false, bridgeMaterial, bridgeLineMaterial);

        int numTrailOff = 10;
        pos.y = getNoise(progress + numTrailOff);
        addPointToLinePos(pos);
        pos.x += segmentWidth * numTrailOff;
        progress += numTrailOff;
        addPointToLinePos(pos);

        // End obstalce
        createNewLine(true, bridgeMaterial, bridgeLineMaterial);

        // Ease back into normal oepration
        bezeirCurveBackToPerlin(pos);

        createNewLine();
    }

    void addObstacle()
    {
        int objectId = Random.Range(0, numObstacles);
        switch (objectId)
        {
            case 0:
                flatGap();
                return;
            case 1:
                gap();
                return;
            case 2:
                cliffDown();
                return;
            case 3:
                cliffUp();
                return;
            case 4:
                jumpCliff();
                return;
            case 5:
                spikePit();
                return;
            case 6:
                pylon();
                return;
            case 7:
                pylonPit();
                return;
            case 8:
                jumpPit();
                return;
        }
    }
    #endregion
}
