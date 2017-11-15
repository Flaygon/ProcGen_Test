using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twister : MonoBehaviour
{
    private int gridX;
    private int gridZ;
    private int seed;

    public float radiusBeginSuck;

    public float minBuildRadius;
    public float maxBuildRadius;

    public int circlesTillTop;
    public int vertexDensityPerCircle;

    public float outerCircleRadius;

    public MeshFilter inner;
    public MeshFilter outer;

    [HideInInspector]
    public Rigidbody hookedObjectBody;

    public float strength;

    public void Initialize(int gridX, int gridZ, int seed)
    {
        this.gridX = gridX;
        this.gridZ = gridZ;
        this.seed = seed;

        BuildInner();
        BuildOuter();

        CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        collider.direction = 2;
        collider.radius = radiusBeginSuck;
        collider.height = test.cloudsLow;
        collider.isTrigger = true;
        collider.center = Vector3.forward * test.cloudsLow * 0.5f;

        transform.rotation = Quaternion.LookRotation(Vector3.up);
    }

    private void BuildInner()
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        float linkLength = test.cloudsLow;
        float perCircleHeight = test.cloudsLow / circlesTillTop;

        for(int iCircle = 0; iCircle < circlesTillTop; ++iCircle)
        {
            float circleRadius = Random.Range(minBuildRadius, maxBuildRadius);
            for (int iAngle = 0; iAngle < vertexDensityPerCircle; ++iAngle)
            {
                float angleProgress = (float)iAngle / vertexDensityPerCircle;
                float radian = (angleProgress * 360.0f) * Mathf.Deg2Rad;
                Vector3 normal = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0.0f).normalized;
                positions.Add(normal * circleRadius + Vector3.forward * (iCircle * perCircleHeight));
                normals.Add(normal);
                uvs.Add(new Vector2(angleProgress, (iCircle * perCircleHeight) / test.cloudsLow));
            }
        }

        for(int iHeight = 0; iHeight < circlesTillTop - 1; ++iHeight)
        {
            int beginVertex = iHeight * vertexDensityPerCircle;
            for (int iVertex = 0; iVertex < vertexDensityPerCircle; ++iVertex)
            {
                if(iVertex == vertexDensityPerCircle - 1)
                {
                    triangles.Add(beginVertex + iVertex);
                    triangles.Add(beginVertex);
                    triangles.Add(beginVertex + iVertex + vertexDensityPerCircle);

                    triangles.Add(beginVertex + vertexDensityPerCircle);
                    triangles.Add(beginVertex + iVertex + vertexDensityPerCircle);
                    triangles.Add(beginVertex);
                }
                else
                {
                    triangles.Add(beginVertex + iVertex);
                    triangles.Add(beginVertex + iVertex + 1);
                    triangles.Add(beginVertex + iVertex + vertexDensityPerCircle);

                    triangles.Add(beginVertex + iVertex + 1 + vertexDensityPerCircle);
                    triangles.Add(beginVertex + iVertex + vertexDensityPerCircle);
                    triangles.Add(beginVertex + iVertex + 1);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(positions);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        inner.mesh = mesh;
    }

    private void BuildOuter()
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        float linkLength = test.cloudsLow;
        float perCircleHeight = test.cloudsLow / circlesTillTop;

        for (int iCircle = 0; iCircle < circlesTillTop; ++iCircle)
        {
            for (int iAngle = 0; iAngle < vertexDensityPerCircle; ++iAngle)
            {
                float angleProgress = (float)iAngle / vertexDensityPerCircle;
                float radian = (angleProgress * 360.0f) * Mathf.Deg2Rad;
                Vector3 normal = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0.0f).normalized;
                positions.Add(normal * outerCircleRadius + Vector3.forward * (iCircle * perCircleHeight));
                normals.Add(normal);
                uvs.Add(new Vector2(angleProgress, (iCircle * perCircleHeight) / test.cloudsLow));
            }
        }

        for (int iHeight = 0; iHeight < circlesTillTop - 1; ++iHeight)
        {
            int beginVertex = iHeight * vertexDensityPerCircle;
            for (int iVertex = 0; iVertex < vertexDensityPerCircle; ++iVertex)
            {
                if (iVertex == vertexDensityPerCircle - 1)
                {
                    triangles.Add(beginVertex + iVertex);
                    triangles.Add(beginVertex);
                    triangles.Add(beginVertex + iVertex + vertexDensityPerCircle);

                    triangles.Add(beginVertex + vertexDensityPerCircle);
                    triangles.Add(beginVertex + iVertex + vertexDensityPerCircle);
                    triangles.Add(beginVertex);
                }
                else
                {
                    triangles.Add(beginVertex + iVertex);
                    triangles.Add(beginVertex + iVertex + 1);
                    triangles.Add(beginVertex + iVertex + vertexDensityPerCircle);

                    triangles.Add(beginVertex + iVertex + 1 + vertexDensityPerCircle);
                    triangles.Add(beginVertex + iVertex + vertexDensityPerCircle);
                    triangles.Add(beginVertex + iVertex + 1);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(positions);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        outer.mesh = mesh;
    }

    private void Update()
    {
        if(hookedObjectBody != null)
        {
            hookedObjectBody.velocity = Vector3.up * strength;
        }
    }

    private void LateUpdate()
    {
        if(hookedObjectBody != null && hookedObjectBody.transform.position.y >= test.cloudsLow)
        {
            hookedObjectBody = null;
        }
    }

    public bool CanSeeFrom(int gridX, int gridZ, int gridViewDistance)
    {
        return Mathf.Abs(this.gridX - gridX) > gridViewDistance || Mathf.Abs(this.gridZ - gridZ) > gridViewDistance;
    }

    public bool Is(int gridX, int gridZ)
    {
        return this.gridX == gridX && this.gridZ == gridZ;
    }

    private void OnTriggerEnter(Collider other)
    {
        hookedObjectBody = other.attachedRigidbody;
        hookedObjectBody.velocity = Vector3.up * strength;
        hookedObjectBody.transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
    }
}