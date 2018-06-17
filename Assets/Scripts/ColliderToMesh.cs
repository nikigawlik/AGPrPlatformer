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
    public Vector2 scale = Vector2.one;

    private PolygonCollider2D pc2 ;

    private Vector2[] pointCache;

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
            Mesh mesh = new Mesh();
            Vector2[] points = pc2.points;
            if (points == pointCache) {
                return;
            }
            pointCache = points;


            LineRenderer lr = GetComponent<LineRenderer>();
            lr.positionCount = 0;// + 1;

            int leftPointID = 0;
            int rightPointID = 0;

            for(int j=0; j<pointCount; j++){
                if(points[j].x < points[leftPointID].x) {
                    leftPointID = j;
                }
                if(points[j].x > points[rightPointID].x) {
                    rightPointID = j;
                }
            }

            int currentPointID = rightPointID;
            int runningID = 0;
                
            while(true) {
                lr.positionCount = runningID + 1;
                lr.SetPosition(runningID, new Vector3(points[currentPointID].x, points[currentPointID].y, 0));

                if(currentPointID == leftPointID)
                    break;

                currentPointID++;
                currentPointID = currentPointID % points.Length;
                runningID++;
            }

            Vector3[] vertices = new Vector3[pointCount];
            Vector2[] uv = new Vector2[pointCount];
            for(int j=0; j<pointCount; j++){
                Vector2 actual = points[j];
                vertices[j] = new Vector3(actual.x, actual.y, 0);
                uv[j] = actual / scale;
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