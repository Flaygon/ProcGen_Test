using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingIsland : Island
{
    //protected Texture2D terrainMap;
    public Texture2D islandMaskPrefab;
    private Texture2D islandMaskObject;

    public float bottomDepth;

    public Material bottomMaterial;
    public Material lakeMaterial;

    //public IslandHook hook;

    public GameObject top;
    public GameObject bottom;
    public GameObject lake;

    public int minHookGroundRadiusSearch;
    public int maxHookGroundRadiusSearch;

    public override void Initialize(int gridX, int gridZ, int seed)
    {
        base.Initialize(gridX, gridZ, seed);

        GenerateMask();

        CoherentNoise.Generator generator = settings.GetGenerator(seed);//new CoherentNoise.Generation.Fractal.PinkNoise(seed);
        GenerateTerrain(generator, islandMaskObject, top.transform);
        //List<int> edgeTriangles = new List<int>();
        //FindEdges(filter.mesh, edgeTriangles);

        for(int iChild = 0; iChild < top.transform.childCount; ++iChild)
        {
            MeshFilter childFilter = top.transform.GetChild(iChild).GetComponent<MeshFilter>();
            CutAwayTooLowTriangles(childFilter.mesh, islandMaskObject, false);

            Vector3[] positions = childFilter.mesh.vertices;
            Color[] colors = childFilter.mesh.colors;
            for (int iVertex = 0; iVertex < positions.Length; ++iVertex)
            {
                Color vertexColor = colors[iVertex];
                vertexColor.r = 1.0f;
                colors[iVertex] = vertexColor;
            }
            childFilter.mesh.colors = colors;
        }

        //GenerateLake();
        for(int iChild = 0; iChild < top.transform.childCount; ++iChild)
        {
            GameObject newChild = new GameObject();
            MeshFilter childFilter = newChild.AddComponent<MeshFilter>();
            newChild.AddComponent<MeshRenderer>();
            newChild.AddComponent<MeshCollider>();
            newChild.transform.SetParent(bottom.transform, false);
            newChild.name = "Split " + iChild;
            childFilter.mesh = new Mesh();
            GenerateBottomCopy(childFilter, top.transform.GetChild(iChild).GetComponent<MeshFilter>());
        }

        //SplitMesh(filter.mesh);

        GenerateBiome(0.0f, top.GetComponentsInChildren<MeshRenderer>());

        //GetComponent<AnimalHandler>().Initialize(islandSize);

        Vector3 position = transform.position;
        position.y = test.cloudsHigh;
        transform.position = position;
    }

    private void GenerateMask()
    {
        islandMaskObject = new Texture2D(islandMaskPrefab.width, islandMaskPrefab.height, TextureFormat.Alpha8, false);

        CoherentNoise.Generation.Fractal.PinkNoise random = new CoherentNoise.Generation.Fractal.PinkNoise(seed);

        for (int iX = 0; iX < islandMaskObject.width; ++iX)
        {
            for (int iY = 0; iY < islandMaskObject.height; ++iY)
            {
                float xFrac = (float)iX / islandMaskObject.width;
                float yFrac = (float)iY / islandMaskObject.height;

                float maskPixel = islandMaskPrefab.GetPixel(iX, iY).a;
                if (maskPixel > 0.0f)
                    maskPixel += random.GetValue(gridX + xFrac, gridZ + yFrac, 0.0f);
                islandMaskObject.SetPixel(iX, iY, new Color(0.0f, 0.0f, 0.0f, Mathf.Clamp01(maskPixel)));
            }
        }

        islandMaskObject.Apply(false, false);
    }

    private void CutAwayMask(Mesh mesh, Texture2D mask, bool bottom)
    {
        List<Vector3> positions = new List<Vector3>(mesh.vertices);
        List<int> triangles = new List<int>(mesh.triangles);

        float islandMaskMultiplier = 1.0f / islandSize;
        for (int iTriangle = triangles.Count - 3; iTriangle > -1; iTriangle -= 3)
        {
            Vector3 position1 = positions[triangles[iTriangle]];
            Vector3 position2 = positions[triangles[iTriangle + 1]];
            Vector3 position3 = positions[triangles[iTriangle + 2]];

            float tM1 = mask.GetPixelBilinear((islandSize * 0.5f + position1.x) * islandMaskMultiplier, (islandSize * 0.5f + position1.z) * islandMaskMultiplier).a;
            float tM2 = mask.GetPixelBilinear((islandSize * 0.5f + position2.x) * islandMaskMultiplier, (islandSize * 0.5f + position2.z) * islandMaskMultiplier).a;
            float tM3 = mask.GetPixelBilinear((islandSize * 0.5f + position3.x) * islandMaskMultiplier, (islandSize * 0.5f + position3.z) * islandMaskMultiplier).a;

            //Debug.Log("1: " + tM1 + ", 2: " + tM2 + ", 3: " + tM3);
            bool remove = false;
            bool edge = false;

            bool approx1 = Mathf.Approximately(tM1, 0.0f);
            bool approx2 = Mathf.Approximately(tM1, 0.0f);
            bool approx3 = Mathf.Approximately(tM1, 0.0f);
            if (approx1 || approx2 || approx3)
            {
                remove = true;

                if(approx1 != approx2 || approx1 != approx3 || approx2 != approx3)
                {
                    edge = true;
                }
            }

            if (remove)
            {
                if(edge)
                {
                    if (!bottom)
                    {
                        Vector3 newPosition1 = Mathf.Approximately(tM1, 0.0f) ? new Vector3(position1.x, 0.0f, position1.z) : position1;
                        Vector3 newPosition2 = Mathf.Approximately(tM2, 0.0f) ? new Vector3(position2.x, 0.0f, position2.z) : position2;
                        Vector3 newPosition3 = Mathf.Approximately(tM3, 0.0f) ? new Vector3(position3.x, 0.0f, position3.z) : position3;

                        positions.Add(newPosition1);
                        positions.Add(newPosition2);
                        positions.Add(newPosition3);

                        triangles.Add(positions.Count - 3);
                        triangles.Add(positions.Count - 2);
                        triangles.Add(positions.Count - 1);
                    }
                    else
                    {
                        Vector3 newPosition1 = Mathf.Approximately(tM1, 0.0f) ? new Vector3(position1.x, 0.0f, position1.z) : position1;
                        Vector3 newPosition2 = Mathf.Approximately(tM2, 0.0f) ? new Vector3(position2.x, 0.0f, position2.z) : position2;
                        Vector3 newPosition3 = Mathf.Approximately(tM3, 0.0f) ? new Vector3(position3.x, 0.0f, position3.z) : position3;

                        positions.Add(newPosition1);
                        positions.Add(newPosition2);
                        positions.Add(newPosition3);

                        triangles.Add(positions.Count - 3);
                        triangles.Add(positions.Count - 1);
                        triangles.Add(positions.Count - 2);
                    }
                }

                triangles.RemoveAt(iTriangle);
                triangles.RemoveAt(iTriangle);
                triangles.RemoveAt(iTriangle);
            }
        }

        mesh.SetTriangles(triangles, 0);
    }

    private void CutAwayTooLowTriangles(Mesh mesh, Texture2D mask, bool bottom)
    {
        List<Vector3> positions = new List<Vector3>(mesh.vertices);
        List<Vector3> normals = new List<Vector3>(mesh.normals);
        List<Color> colors = new List<Color>(mesh.colors);
        List<Vector2> uvs = new List<Vector2>(mesh.uv);
        List<int> triangles = new List<int>(mesh.triangles);

        for (int iTriangle = triangles.Count - 3; iTriangle > -1; iTriangle -= 3)
        {
            Vector3 position1 = positions[triangles[iTriangle]];
            Vector3 position2 = positions[triangles[iTriangle + 1]];
            Vector3 position3 = positions[triangles[iTriangle + 2]];

            //Debug.Log("1: " + tM1 + ", 2: " + tM2 + ", 3: " + tM3);
            bool remove = false;
            bool edge = false;

            bool heightTest1 = !bottom ? position1.y > 0.0f : position1.y < 0.0f;
            bool heightTest2 = !bottom ? position2.y > 0.0f : position2.y < 0.0f;
            bool heightTest3 = !bottom ? position3.y > 0.0f : position3.y < 0.0f;

            if (!heightTest1 || !heightTest2 || !heightTest3)
            {
                remove = true;

                if (heightTest1 != heightTest2 || heightTest1 != heightTest3 || heightTest2 != heightTest3)
                {
                    edge = true;
                }
            }

            if (remove)
            {
                if (edge)
                {
                    Vector3 newPosition1 = !heightTest1 ? new Vector3(position1.x, 0.0f, position1.z) : position1;
                    Vector3 newPosition2 = !heightTest2 ? new Vector3(position2.x, 0.0f, position2.z) : position2;
                    Vector3 newPosition3 = !heightTest3 ? new Vector3(position3.x, 0.0f, position3.z) : position3;

                    positions.Add(newPosition1);
                    positions.Add(newPosition2);
                    positions.Add(newPosition3);

                    uvs.Add(uvs[triangles[iTriangle]]);
                    uvs.Add(uvs[triangles[iTriangle + 1]]);
                    uvs.Add(uvs[triangles[iTriangle + 2]]);

                    normals.Add(normals[triangles[iTriangle]]);
                    normals.Add(normals[triangles[iTriangle + 1]]);
                    normals.Add(normals[triangles[iTriangle + 2]]);

                    colors.Add(colors[triangles[iTriangle]]);
                    colors.Add(colors[triangles[iTriangle + 1]]);
                    colors.Add(colors[triangles[iTriangle + 2]]);

                    triangles.Add(positions.Count - 3);
                    triangles.Add(positions.Count - 2);
                    triangles.Add(positions.Count - 1);
                }

                triangles.RemoveAt(iTriangle);
                triangles.RemoveAt(iTriangle);
                triangles.RemoveAt(iTriangle);
            }
        }

        mesh.SetVertices(positions);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
    }

    private void FindEdges(Mesh mesh, List<int> edgeTriangles)
    {
        List<Vector3> positions = new List<Vector3>(mesh.vertices);
        List<int> triangles = new List<int>(mesh.triangles);

        for (int iTriangle = triangles.Count - 3; iTriangle > -1; iTriangle -= 3)
        {
            float tH1 = positions[triangles[iTriangle]].y;
            float tH2 = positions[triangles[iTriangle + 1]].y;
            float tH3 = positions[triangles[iTriangle + 2]].y;

            bool edge = false;
            if(tH1 <= 0.0f)
            {
                if (tH2 > 0.0f || tH3 > 0.0f)
                    edge = true;
            }
            else
            {
                if (tH2 <= 0.0f || tH3 <= 0.0f)
                    edge = true;
            }

            if (edge)
            {
                edgeTriangles.Add(triangles[iTriangle]);
                edgeTriangles.Add(triangles[iTriangle + 1]);
                edgeTriangles.Add(triangles[iTriangle + 2]);
            }
        }
    }

    private void TEST1_EXPENSIVE_SplitMesh(Mesh mesh)
    {
        List<Vector3> positions = new List<Vector3>(mesh.vertices);
        List<Vector3> normals = new List<Vector3>(mesh.normals);
        List<Color> colors = new List<Color>(mesh.colors);
        List<Vector2> uvs = new List<Vector2>(mesh.uv);
        List<int> triangles = new List<int>(mesh.triangles);

        int cap = 0;
        while(triangles.Count > 0)
        {
            List<int> connectedTriangleCorners = new List<int>(triangles.Count);
            List<int> alreadyConnectedTriangles = new List<int>(triangles.Count / 3);

            TEST1_EXPENSIVE_CollectTriangleIsland(connectedTriangleCorners, alreadyConnectedTriangles, triangles);
            List<int> newTriangles = new List<int>(connectedTriangleCorners.Count);
            for(int iConnectedTriangle = 0; iConnectedTriangle < connectedTriangleCorners.Count; ++iConnectedTriangle)
            {
                newTriangles.Add(triangles[connectedTriangleCorners[iConnectedTriangle]]);
            }
            for(int iTriangle = triangles.Count - 1; iTriangle > -1; --iTriangle)
            {
                for (int iConnectedTriangle = 0; iConnectedTriangle < connectedTriangleCorners.Count; ++iConnectedTriangle)
                {
                    if(iTriangle == connectedTriangleCorners[iConnectedTriangle])
                    {
                        triangles.RemoveAt(iTriangle);
                        break;
                    }
                }
            }
            //Debug.Log("C.Triangles: " + connectedTriangleCorners.Count + "C.Triangles Divisable by 3: " + (connectedTriangleCorners.Count / 3.0f) + ", remainingTriangles: " + triangles.Count + "Triangles Divisable by 3: " + (triangles.Count / 3.0f));

            GameObject part = new GameObject();
            part.transform.SetParent(transform, false);
            part.name = "Part";
            MeshFilter filter = part.AddComponent<MeshFilter>();
            filter.mesh = new Mesh();
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            part.AddComponent<MeshCollider>().sharedMesh = filter.mesh;
            /*Bobbing newBob = part.AddComponent<Bobbing>();
            newBob.bob = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(0.25f, 1.0f), new Keyframe(0.75f, -1.0f), new Keyframe(1.0f, 0.0f) });
            newBob.bob.postWrapMode = WrapMode.Loop;
            newBob.bobStrength = 5.0f;
            newBob.bobbingTime = 15.0f;*/

            filter.mesh.SetVertices(positions);
            filter.mesh.SetNormals(normals);
            filter.mesh.SetColors(colors);
            filter.mesh.SetUVs(0, uvs);
            filter.mesh.SetTriangles(newTriangles, 0);

            part.GetComponent<MeshCollider>().sharedMesh = filter.mesh;

            //GenerateBiome(renderer, 0.5f);

            if (++cap >= 30)
            {
                Debug.LogWarning("Cap was hit");
                break;
            }
        }

        //GenerateNavMesh(filter.mesh);
    }

    private void TEST1_EXPENSIVE_CollectTriangleIsland(List<int> connectedTriangleCorners, List<int> alreadyConnectedTriangles, List<int> triangles)
    {
        alreadyConnectedTriangles.Add(0);
        connectedTriangleCorners.Add(0);
        connectedTriangleCorners.Add(1);
        connectedTriangleCorners.Add(2);

        bool addedTriangles = true;
        int cap = 0;
        while(addedTriangles)
        {
            addedTriangles = false;

            for (int iTriangle = 3; iTriangle < triangles.Count; iTriangle += 3)
            {
                int currentTriangle = iTriangle / 3;

                bool alreadyConnected = false;
                for (int iConnectedTriangle = 0; iConnectedTriangle < alreadyConnectedTriangles.Count; ++iConnectedTriangle)
                {
                    if (currentTriangle == alreadyConnectedTriangles[iConnectedTriangle])
                    {
                        alreadyConnected = true;
                        break;
                    }
                }

                if (!alreadyConnected)
                {
                    bool newConnectedTriangle = false;
                    for (int iTriangleCorner = 0; iTriangleCorner < 3; ++iTriangleCorner)
                    {
                        int currentTriangleCorner = iTriangle + iTriangleCorner;

                        for (int iConnectedCorner = 0; iConnectedCorner < connectedTriangleCorners.Count; ++iConnectedCorner)
                        {
                            if (triangles[currentTriangleCorner] == triangles[connectedTriangleCorners[iConnectedCorner]])
                            {
                                newConnectedTriangle = true;
                                break;
                            }
                        }

                        if (newConnectedTriangle)
                            break;
                    }

                    if (newConnectedTriangle)
                    {
                        alreadyConnectedTriangles.Add(currentTriangle);
                        connectedTriangleCorners.Add(iTriangle);
                        connectedTriangleCorners.Add(iTriangle + 1);
                        connectedTriangleCorners.Add(iTriangle + 2);

                        addedTriangles = true;
                    }
                }
            }

            if(++cap > 30)
            {
                Debug.LogWarning("Broke building island loop(found triangles:  " + addedTriangles + "), " + alreadyConnectedTriangles.Count);
                break;
            }

            if(!addedTriangles)
            {
                //Debug.Log("Didn't add any triangles. Cap: " + cap + ", AlreadyConnectedTriangles: " + alreadyConnectedTriangles.Count);
            }
        }
    }

    private void GenerateLake()
    {
        MeshFilter filter = transform.GetChild(2).GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;

        List<Vector3> positions = new List<Vector3>(islandSize * islandSize);
        List<Vector3> normals = new List<Vector3>(islandSize * islandSize);
        //List<Color> colors = new List<Color>(islandSize * islandSize);
        List<Vector2> uvs = new List<Vector2>(islandSize * islandSize);
        List<int> triangles = new List<int>(islandSize * islandSize * 3);

        for (int yIndex = 0; yIndex < islandSize; ++yIndex)
        {
            for (int xIndex = 0; xIndex < islandSize; ++xIndex)
            {
                positions.Add(new Vector3(xIndex - islandSize * 0.5f, 0.0f, yIndex - islandSize * 0.5f));
                normals.Add(new Vector3(0.0f, 1.0f, 0.0f));
                //colors.Add(new Color(0.0f, 0.0f, 0.0f, 1.0f)); // Black: Beach, Red: Grass, Green: Rock
                uvs.Add(new Vector2((float)xIndex / islandSize, (float)yIndex / islandSize));
            }

            if (yIndex < islandSize - 1)
            {
                for (int iTriangles = yIndex * islandSize; iTriangles < positions.Count - 1; ++iTriangles)
                {
                    triangles.Add(iTriangles);
                    triangles.Add(Neighbor(iTriangles, VertexNeighbor.NORTH));
                    triangles.Add(Neighbor(iTriangles, VertexNeighbor.EAST));

                    triangles.Add(Neighbor(Neighbor(iTriangles, VertexNeighbor.NORTH), VertexNeighbor.EAST));
                    triangles.Add(Neighbor(iTriangles, VertexNeighbor.EAST));
                    triangles.Add(Neighbor(iTriangles, VertexNeighbor.NORTH));
                }
            }
        }

        mesh.Clear(true);
        mesh.SetVertices(positions);
        mesh.SetNormals(normals);
        //mesh.SetColors(colors);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        filter.GetComponent<MeshRenderer>().sharedMaterial = lakeMaterial;
    }

    private void GenerateBottom()
    {
        MeshFilter filter = transform.GetChild(1).GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;

        List<Vector3> positions = new List<Vector3>(islandSize);
        List<Vector3> normals = new List<Vector3>(islandSize);
        //List<Color> colors = new List<Color>(islandSize);
        List<Vector2> uvs = new List<Vector2>(islandSize);
        List<int> triangles = new List<int>(islandSize * 3);

        for (int yIndex = 0; yIndex < islandSize; ++yIndex)
        {
            for (int xIndex = 0; xIndex < islandSize; ++xIndex)
            {
                Vector2 frac = new Vector2((float)xIndex / islandSize, (float)yIndex / islandSize);
                float terrainNoise = islandMaskObject.GetPixelBilinear(frac.x, frac.y).a;
                /*xFrac *= 2;
                yFrac *= 2;
                xFrac = xFrac > 1.0f ? 1.0f - (xFrac - 1.0f) : xFrac;
                yFrac = yFrac > 1.0f ? 1.0f - (yFrac - 1.0f) : yFrac;
                terrainNoise *= xFrac * yFrac;*/
                terrainNoise *= 1 - Vector2.Distance(new Vector2(0.5f, 0.5f), frac) * 2.0f;
                positions.Add(new Vector3(xIndex - islandSize * 0.5f, terrainNoise * -bottomDepth, yIndex - islandSize * 0.5f));
                normals.Add(new Vector3(0.0f, 0.0f, 0.0f)); // Placeholder normals since I'll be going through them later
                //colors.Add(new Color(0.0f, 0.0f, 0.0f, 1.0f)); // Black: Beach, Red: Grass, Green: Rock
                uvs.Add(new Vector2((float)xIndex / islandSize, (float)yIndex / islandSize));
            }

            if (yIndex < islandSize - 1)
            {
                for (int iTriangles = yIndex * islandSize; iTriangles < positions.Count - 1; ++iTriangles)
                {
                    triangles.Add(iTriangles);
                    triangles.Add(Neighbor(iTriangles, VertexNeighbor.EAST));
                    triangles.Add(Neighbor(iTriangles, VertexNeighbor.NORTH));

                    triangles.Add(Neighbor(Neighbor(iTriangles, VertexNeighbor.NORTH), VertexNeighbor.EAST));
                    triangles.Add(Neighbor(iTriangles, VertexNeighbor.NORTH));
                    triangles.Add(Neighbor(iTriangles, VertexNeighbor.EAST));
                }
            }
        }

        // Finding and blending normals
        for (int iTriangle = 0; iTriangle < triangles.Count; iTriangle += 3)
        {
            int p1 = triangles[iTriangle];
            int p2 = triangles[iTriangle + 1];
            int p3 = triangles[iTriangle + 2];

            Plane plane = new Plane(positions[p1], positions[p2], positions[p3]);

            for (int iNormal = iTriangle; iNormal < iTriangle + 3; ++iNormal)
            {
                int normal = triangles[iNormal];
                // If placeholder normal, grab the normal from the plane
                if (Mathf.Approximately(normals[normal].sqrMagnitude, 0.0f))
                {
                    normals[normal] = plane.normal;
                }
                else // If normal already exists, merge new with previous
                {
                    normals[normal] = Vector3.Normalize(Vector3.RotateTowards(normals[normal], plane.normal, Vector3.Angle(normals[normal], plane.normal) * Mathf.Deg2Rad, 0.0f));
                }
            }
        }

        mesh.Clear(true);
        mesh.SetVertices(positions);
        mesh.SetNormals(normals);
        //mesh.SetColors(colors);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        filter.GetComponent<MeshRenderer>().sharedMaterial = bottomMaterial;
        filter.GetComponent<MeshCollider>().sharedMesh = mesh;

        //filter.transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.down);
    }

    private void GenerateBottomCopy(MeshFilter bottomFilter, MeshFilter topFilter)
    {
        Mesh mesh = bottomFilter.mesh;

        List<Vector3> positions = new List<Vector3>(topFilter.mesh.vertices);
        List<Vector3> normals = new List<Vector3>(islandSize);
        //List<Color> colors = new List<Color>(islandSize);
        List<Vector2> uvs = new List<Vector2>(topFilter.mesh.uv);
        List<int> triangles = new List<int>(topFilter.mesh.triangles);

        for(int iTriangle = 0; iTriangle < triangles.Count; iTriangle += 3)
        {
            int swappedTriangle = triangles[iTriangle + 1];
            triangles[iTriangle + 1] = triangles[iTriangle + 2];
            triangles[iTriangle + 2] = swappedTriangle;
        }

        for(int iPosition = 0; iPosition < positions.Count; ++iPosition)
        {
            Vector3 newHeight = positions[iPosition];
            newHeight.y = -Mathf.Abs(newHeight.y) * bottomDepth;
            positions[iPosition] = newHeight;
        }

        // Finding and blending normals
        /*for (int iTriangle = 0; iTriangle < triangles.Count; iTriangle += 3)
        {
            int p1 = triangles[iTriangle];
            int p2 = triangles[iTriangle + 1];
            int p3 = triangles[iTriangle + 2];

            Plane plane = new Plane(positions[p1], positions[p2], positions[p3]);

            for (int iNormal = iTriangle; iNormal < iTriangle + 3; ++iNormal)
            {
                int normal = triangles[iNormal];
                // If placeholder normal, grab the normal from the plane
                if (Mathf.Approximately(normals[normal].sqrMagnitude, 0.0f))
                {
                    normals[normal] = plane.normal;
                }
                else // If normal already exists, merge new with previous
                {
                    normals[normal] = Vector3.Normalize(Vector3.RotateTowards(normals[normal], plane.normal, Vector3.Angle(normals[normal], plane.normal) * Mathf.Deg2Rad, 0.0f));
                }
            }
        }*/

        mesh.Clear(true);
        mesh.SetVertices(positions);
        mesh.SetNormals(normals);
        //mesh.SetColors(colors);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        bottomFilter.GetComponent<MeshRenderer>().sharedMaterial = bottomMaterial;
        bottomFilter.GetComponent<MeshCollider>().sharedMesh = mesh;

        //filter.transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.down);
    }
}