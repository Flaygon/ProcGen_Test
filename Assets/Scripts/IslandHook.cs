using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandHook : MonoBehaviour
{
    private GroundIsland groundIsland;
    private FloatingIsland floatingIsland;

    public GameObject firstHook;
    public GameObject secondHook;
    public GameObject link;

    public float linkRadius;
    public float linkRepeatLength;
    public int linkCircleVertices;

    public void Connect(GroundIsland groundIsland, FloatingIsland floatingIsland)
    {
        this.groundIsland = groundIsland;
        this.floatingIsland = floatingIsland;

        Build();
    }

    private void Build()
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        Vector3 linkDistance = firstHook.transform.position - secondHook.transform.position;
        float linkLength = linkDistance.magnitude;
        Vector3 linkDirection = linkDistance.normalized;
        Vector3 linkRight = Quaternion.LookRotation(linkDirection) * Vector3.right;

        for (float iAngle = 0.0f; iAngle <= 1.0f; iAngle += 1.0f / linkCircleVertices)
        {
            for (int iEnd = 0; iEnd < 2; iEnd++)
            {
                Vector3 normal = new Vector3(Mathf.Cos(iAngle), Mathf.Sin(iAngle), iEnd == 0 ? 0.0f : linkLength).normalized;
                positions.Add(normal * linkRadius);
                normals.Add(normal);
                uvs.Add(new Vector2(iAngle, iEnd == 0 ? 0.0f : linkLength / linkRepeatLength));
            }

            if(iAngle > 0.0f)
            {
                triangles.Add(positions.Count - 4);
                triangles.Add(positions.Count - 3);
                triangles.Add(positions.Count - 2);

                triangles.Add(positions.Count - 1);
                triangles.Add(positions.Count - 2);
                triangles.Add(positions.Count - 3);
            }
        }

        Mesh linkMesh = new Mesh();
        linkMesh.SetVertices(positions);
        linkMesh.SetNormals(normals);
        linkMesh.SetUVs(0, uvs);
        linkMesh.SetTriangles(triangles, 0);
        link.GetComponent<MeshFilter>().mesh = linkMesh;

        CapsuleCollider collider = link.GetComponent<CapsuleCollider>();
        collider.radius = linkRadius;
        collider.height = linkLength * 0.5f;
        collider.center = linkDistance * 0.5f;

        link.transform.rotation = Quaternion.LookRotation(linkDirection);
    }
}