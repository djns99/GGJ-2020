using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDraw : MonoBehaviour
{
    public GameObject line;
    public GameObject current_line;
    public LineRenderer line_renderer;
    public PolygonCollider2D poly_collider;
    public List<Vector2> line_points;
    public List<Vector2> collision_points;



    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        var mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if ( Input.GetMouseButtonDown( 0 ) )
        {
            current_line = Instantiate(line, Vector3.zero, Quaternion.identity);
            var rig = current_line.GetComponent<Rigidbody2D>();
            rig.isKinematic = true;

            line_renderer = current_line.GetComponent<LineRenderer>();
            poly_collider = current_line.GetComponent<PolygonCollider2D>();

            collision_points.Clear();
            line_points.Clear();

            line_points.Add(mouse_pos);
            line_points.Add(mouse_pos);

            line_renderer.SetPosition(0, line_points[0]);
            line_renderer.SetPosition(1, line_points[1]);
            line_renderer.positionCount = 2;


            line_renderer.Simplify( 0.5f );

        }
        if ( Input.GetMouseButton( 0 ) )
        {
            var finger_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if( Vector2.Distance( finger_pos, line_points[line_points.Count - 1 ]) > 4f)
            {
                line_points.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                line_renderer.positionCount++;
                line_renderer.SetPosition(line_points.Count - 1, line_points[line_points.Count - 1]);
            }
        }
        if( Input.GetMouseButtonUp( 0 ) )
        {
            List<Vector2> edgePoints = new List<Vector2>();
            float halfWidth = line_renderer.startWidth / 2f;
            line_renderer.Simplify(0.1f);

            for (int i = 1; i < line_points.Count; i++)
            {
                Vector2 distanceBetweenPoints = line_points[i - 1] - line_points[i];
                Vector3 crossProduct = Vector3.Cross(distanceBetweenPoints, Vector3.forward);

                int num_smoved = 20;
                float reduce = i < num_smoved ? -halfWidth + (line_renderer.startWidth / num_smoved) * i : halfWidth; 

                Vector2 up = (reduce) * new Vector2(crossProduct.normalized.x, crossProduct.normalized.y) + line_points[i - 1];
                Vector2 down = -(halfWidth) * new Vector2(crossProduct.normalized.x, crossProduct.normalized.y) + line_points[i - 1];

                edgePoints.Insert(0, down);
                edgePoints.Add(up);
            }

            poly_collider.SetPath(0, edgePoints.ToArray());
            
            var rig = current_line.GetComponent<Rigidbody2D>();
            rig.isKinematic = false;
        }
    }
}
