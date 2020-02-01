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

    void UpdateCollision( Vector2 mouse_pos)
    {
        float halfWidth = line_renderer.startWidth / 2f;
        Vector2 bottomPoint = mouse_pos;
        Vector2 topPoint = mouse_pos;
        bottomPoint.y -= halfWidth;
        topPoint.y += halfWidth;

        collision_points.Insert(0, bottomPoint);
        collision_points.Add(topPoint);
            
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
            // line_renderer.Simplify();

            UpdateCollision(mouse_pos);


        }
        if ( Input.GetMouseButton( 0 ) )
        {
            var finger_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if( Vector2.Distance( finger_pos, line_points[line_points.Count - 1 ]) > 0.25f)
            {
                line_points.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                line_renderer.positionCount++;
                line_renderer.SetPosition(line_points.Count - 1, line_points[line_points.Count - 1]);
                UpdateCollision(mouse_pos);
            }
        }
        if( Input.GetMouseButtonUp( 0 ) )
        {
            poly_collider.SetPath(0, collision_points.ToArray());
            
            var rig = current_line.GetComponent<Rigidbody2D>();
            rig.isKinematic = false;
        }
    }
}
