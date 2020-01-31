using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDraw : MonoBehaviour
{
    public GameObject line;
    public GameObject current_line;
    public LineRenderer line_renderer;
    public EdgeCollider2D edge_collider;
    public List<Vector2> line_points;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetMouseButtonDown( 0 ) )
        {
            current_line = Instantiate(line, Vector3.zero, Quaternion.identity);
            line_renderer = current_line.GetComponent<LineRenderer>();
            edge_collider = current_line.GetComponent<EdgeCollider2D>();

            line_points.Clear();

            line_points.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            line_points.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            line_renderer.SetPosition(0, line_points[0]);
            line_renderer.SetPosition(1, line_points[1]);
            edge_collider.points = line_points.ToArray();
        }
        if( Input.GetMouseButton( 0 ) )
        {
            var finger_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if( Vector2.Distance( finger_pos, line_points[line_points.Count - 1 ]) > 0.1f)
            {
                line_points.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                line_renderer.positionCount++;
                line_renderer.SetPosition(line_points.Count - 1, line_points[line_points.Count - 1]);
                edge_collider.points = line_points.ToArray();
            }
        }
    }
}
