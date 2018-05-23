/// <summary>
/// Based on:
/// http://answers.unity3d.com/questions/835675/how-to-fill-polygon-collider-with-a-solid-color.html
/// </summary>

using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class ColliderToMesh : MonoBehaviour {
    PolygonCollider2D pc2 ;
    void Start () {
        pc2 = gameObject.GetComponent<PolygonCollider2D>();
    }

    #if UNITY_EDITOR
    void Update(){
        if (Application.isPlaying)
            return;
        if(pc2 != null){
            //Render thing
            int pointCount = 0;
            pointCount = pc2.GetTotalPointCount();
            MeshFilter mf = GetComponent<MeshFilter>();
            LineRenderer lr = GetComponent<LineRenderer>();
            lr.positionCount = pointCount;// + 1;
            Mesh mesh = new Mesh();
            Vector2[] points = pc2.points;
            Vector3[] vertices = new Vector3[pointCount];
            Vector2[] uv = new Vector2[pointCount];
            for(int j=0; j<pointCount; j++){
                Vector2 actual = points[j];
                vertices[j] = new Vector3(actual.x, actual.y, 0);
                uv[j] = actual;

                lr.SetPosition(j, new Vector3(actual.x, actual.y, 0));
            }
            // lr.SetPosition(pointCount, new Vector3(points[0].x, points[0].y, 0));
            Triangulator tr = new Triangulator(points);
            int [] triangles = tr.Triangulate();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mf.mesh = mesh;
            //Render thing
        }
    }
    #endif
}