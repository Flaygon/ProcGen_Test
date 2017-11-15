using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Island : MonoBehaviour
{
    public int minIslandSize;
    public int maxIslandSize;
    protected int islandSize;

    public float slopeThreshold;

    protected int gridX;
    protected int gridZ;
    protected int seed;

    protected enum VertexNeighbor
    {
        NORTHEAST = 0,
        NORTH,
        NORTHWEST,
        EAST,
        SOUTHEAST,
        SOUTH,
        SOUTHWEST,
        WEST,

        COUNT,
    }

    protected enum Biome
    {
        GRASS = 0,
        GRASS_FALL,
        DARK,
        SNOW,

        COUNT,
    };

    protected Biome biomeType;

    [System.Serializable]
    public class BiomeContainer
    {
        public Material material;

        public List<GameObject> trees;
        public List<GameObject> bushes;
    };

    public List<BiomeContainer> biomes;

    public int minTreeDensity;
    public int maxTreeDensity;
    protected int treeDensity;

    public int minBushDensity;
    public int maxBushDensity;
    protected int bushDensity;

    public IslandSettings settings;

    protected NavMeshData navMesh;

    public virtual void Initialize(int gridX, int gridZ, int seed)
    {
        Random.InitState(seed);

        this.gridX = gridX;
        this.gridZ = gridZ;
        this.seed = seed;

        islandSize = Random.Range(minIslandSize, maxIslandSize);

        biomeType = (Biome)Random.Range(0, (int)Biome.COUNT);
    }

    /*protected void GenerateIslandMap(out Texture2D heightmap, Texture2D mask)
    {
        heightmap = new Texture2D(islandSize, islandSize, TextureFormat.RFloat, false, true);

        // Constructing the heightmap
        for (int yIndex = 0; yIndex <= islandSize; ++yIndex)
        {
            for (int xIndex = 0; xIndex <= islandSize; ++xIndex)
            {
                float xFrac = ((float)xIndex / islandSize);
                float yFrac = ((float)yIndex / islandSize);
                float perlin = Mathf.PerlinNoise(gridX + xFrac * islandRoughness, gridZ + yFrac * islandRoughness);

                // Mask
                Color islandPixel = mask.GetPixelBilinear(xFrac, yFrac);
                heightmap.SetPixel(xIndex, yIndex, new Color(perlin * islandPixel.a, 0.0f, 0.0f, 0.0f));
            }
        }

        heightmap.Apply(false, false);
    }*/

    //protected void GenerateTerrain(Texture2D map, Mesh mesh)
    protected void GenerateTerrain(CoherentNoise.Generator noise, Texture2D mask, Transform splitParent)
    {
        List<Vector3> positions = new List<Vector3>(islandSize * islandSize);
        List<Vector3> normals = new List<Vector3>(islandSize * islandSize);
        List<Color> colors = new List<Color>(islandSize * islandSize);
        List<Vector2> uvs = new List<Vector2>(islandSize * islandSize);
        List<int> triangles = new List<int>(islandSize * islandSize * 6 - (islandSize * 6 * 2 - 6));

        float roughness = settings.GetRoughness();
        float height = settings.GetHeight();

        for (int yIndex = 0; yIndex < islandSize; ++yIndex)
        {
            for (int xIndex = 0; xIndex < islandSize; ++xIndex)
            {
                float xFrac = ((float)xIndex / islandSize);
                float yFrac = ((float)yIndex / islandSize);
                float terrainNoise = noise.GetValue(gridX + xFrac * roughness, gridZ + yFrac * roughness, 0.0f) * mask.GetPixelBilinear(xFrac, yFrac).a;
                positions.Add(new Vector3(xIndex - islandSize * 0.5f, terrainNoise * height, yIndex - islandSize * 0.5f));
                normals.Add(new Vector3(0.0f, 0.0f, 0.0f)); // Placeholder normals since I'll be going through them later
                colors.Add(new Color(0.0f, 0.0f, 0.0f, 1.0f)); // Black: Beach, Red: Grass, Green: Rock
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

        // Parse normals and check if they're steep enough to make them be rocks
        for (int iVertex = 0; iVertex < positions.Count; ++iVertex)
        {
            Color vertexColor = colors[iVertex];
            vertexColor.g = Vector3.Dot(normals[iVertex], Vector3.up) < slopeThreshold ? 1.0f : 0.0f;
            colors[iVertex] = vertexColor;
        }

        int splits = Mathf.CeilToInt(positions.Count / 65000.0f);
        //if(splits > 1)
        {
            int triangleEnd = 0;
            for (int iSplit = 0; iSplit < splits; ++iSplit)
            {
                MeshFilter newFilter = new GameObject().AddComponent<MeshFilter>();
                newFilter.transform.SetParent(transform, false);
                newFilter.name = "Split " + iSplit;
                newFilter.mesh = new Mesh();

                int startIndex = 65000 * iSplit;
                int remainingIndices = iSplit == splits - 1 ? positions.Count - startIndex : 65000;

                Debug.Log("Splits: " + iSplit + " / " + (splits - 1) + ", StartIndex: " + startIndex + ", IndicesToGet: " + remainingIndices);
                newFilter.mesh.SetVertices(positions.GetRange(startIndex, remainingIndices));
                newFilter.mesh.SetNormals(normals.GetRange(startIndex, remainingIndices));
                newFilter.mesh.SetColors(colors.GetRange(startIndex, remainingIndices));
                newFilter.mesh.SetUVs(0, uvs.GetRange(startIndex, remainingIndices));
                int triangleStart = triangleEnd;
                int beginSkippedEnds = triangleStart / islandSize;
                int totalSkippedEndsAfterSplit = (triangleStart + remainingIndices) / islandSize;
                int splitSkippedEnds = totalSkippedEndsAfterSplit - beginSkippedEnds;
                remainingIndices -= splitSkippedEnds + (iSplit != splits - 1 ? 0 : islandSize);
                remainingIndices *= 6;
                triangleEnd += remainingIndices;
                Debug.Log("Triangle Start: " + triangleStart + ", Triangle End: " + triangleEnd + ", Difference: " + (triangleEnd - triangleStart) + ", Begin skip x-ends: " + beginSkippedEnds + ", Total split skip x-ends: " + totalSkippedEndsAfterSplit + ", Difference skip x-ends: " + splitSkippedEnds);
                List<int> normalizedTriangles = triangles.GetRange(triangleStart, triangleEnd/*remainingIndices * 6 - (islandSize * 6 * 2 - 6)*/);
                int firstTriangleIndex = normalizedTriangles[0];
                for (int iNormalized = 0; iNormalized < normalizedTriangles.Count; ++iNormalized)
                    normalizedTriangles[iNormalized] -= firstTriangleIndex;
                newFilter.mesh.SetTriangles(normalizedTriangles, 0);

                newFilter.gameObject.AddComponent<MeshRenderer>();
                newFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = newFilter.mesh;

                newFilter.transform.SetParent(splitParent, false);
            }
        }
        /*else
        {
            mesh.Clear(true);
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
        }*/
    }

    protected void GenerateBiome(float threshold, MeshRenderer[] renderers = null)
    {
        if (renderers == null)
            return;

        foreach (MeshRenderer iRenderer in renderers)
            iRenderer.sharedMaterial = biomes[(int)biomeType].material;

        treeDensity = Random.Range(minTreeDensity, maxTreeDensity);
        bushDensity = Random.Range(minBushDensity, maxBushDensity);

        Bounds totalBounds = new Bounds();
        totalBounds.center = transform.position;
        foreach (MeshRenderer iRenderer in renderers)
            totalBounds.Encapsulate(iRenderer.bounds);

        if (biomes[(int)biomeType].trees.Count > 0)
        {
            for (int iTree = 0; iTree < treeDensity; ++iTree)
            {
                RaycastHit hit = new RaycastHit();
                Ray ray = new Ray();
                ray.origin = new Vector3(Random.Range(totalBounds.center.x - totalBounds.extents.x, totalBounds.center.x + totalBounds.extents.x), totalBounds.center.y + totalBounds.extents.y * 1.1f, Random.Range(totalBounds.center.z - totalBounds.extents.z, totalBounds.center.z + totalBounds.extents.z));
                ray.direction = Vector3.down;
                if (Physics.Raycast(ray, out hit, totalBounds.size.y * 1.1f) && hit.point.y > threshold && Vector3.Dot(hit.normal, Vector3.up) > slopeThreshold)
                {
                    Instantiate(biomes[(int)biomeType].trees[Random.Range(0, biomes[(int)biomeType].trees.Count)], hit.point, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f), transform);
                }
            }
        }

        if (biomes[(int)biomeType].bushes.Count > 0)
        {
            for (int iBush = 0; iBush < bushDensity; ++iBush)
            {
                RaycastHit hit = new RaycastHit();
                Ray ray = new Ray();
                ray.origin = new Vector3(Random.Range(totalBounds.center.x - totalBounds.extents.x, totalBounds.center.x + totalBounds.extents.x), totalBounds.center.y + totalBounds.extents.y * 1.1f, Random.Range(totalBounds.center.z - totalBounds.extents.z, totalBounds.center.z + totalBounds.extents.z));
                ray.direction = Vector3.down;
                if (Physics.Raycast(ray, out hit, totalBounds.size.y * 1.1f) && hit.point.y > threshold && Vector3.Dot(hit.normal, Vector3.up) > slopeThreshold)
                {
                    Instantiate(biomes[(int)biomeType].bushes[Random.Range(0, biomes[(int)biomeType].bushes.Count)], hit.point, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f), transform);
                }
            }
        }
    }

    protected void GenerateNavMesh(Mesh mesh)
    {
        List<NavMeshBuildSource> navMeshBuildSources = new List<NavMeshBuildSource>();

        NavMeshBuildSource newSource = new NavMeshBuildSource();
        newSource.transform = transform.localToWorldMatrix;
        newSource.shape = NavMeshBuildSourceShape.Mesh;
        newSource.sourceObject = mesh;
        newSource.size = new Vector3(islandSize, 100.0f, islandSize);
        newSource.component = this;

        navMeshBuildSources.Add(newSource);

        navMesh = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0), navMeshBuildSources, new Bounds(Vector3.zero, new Vector3(islandSize, 100.0f, islandSize)), transform.position, Quaternion.identity);

        NavMesh.AddNavMeshData(navMesh);
    }

    protected int Neighbor(int currentIndex, VertexNeighbor direction)
    {
        int neighborIndex = currentIndex;

        switch (direction)
        {
            case VertexNeighbor.NORTHEAST:
                {
                    neighborIndex += islandSize + 1;
                    break;
                }
            case VertexNeighbor.NORTH:
                {
                    neighborIndex += islandSize;
                    break;
                }
            case VertexNeighbor.NORTHWEST:
                {
                    neighborIndex += islandSize - 1;
                    break;
                }
            case VertexNeighbor.SOUTHEAST:
                {
                    neighborIndex -= islandSize - 1;
                    break;
                }
            case VertexNeighbor.SOUTH:
                {
                    neighborIndex -= islandSize;
                    break;
                }
            case VertexNeighbor.SOUTHWEST:
                {
                    neighborIndex -= islandSize + 1;
                    break;
                }
            case VertexNeighbor.EAST:
                {
                    neighborIndex += 1;
                    break;
                }
            case VertexNeighbor.WEST:
                {
                    neighborIndex -= 1;
                    break;
                }
        }

        return neighborIndex;
    }

    public bool CanSeeFrom(int gridX, int gridZ, int gridViewDistance)
    {
        return Mathf.Abs(this.gridX - gridX) > gridViewDistance || Mathf.Abs(this.gridZ - gridZ) > gridViewDistance;
    }

    public bool Is(int gridX, int gridZ)
    {
        return this.gridX == gridX && this.gridZ == gridZ;
    }

    public Vector3 FindNavMeshPoint()
    {
        Vector3 point = transform.position;

        for(int iTries = 0; iTries < 10; ++iTries)
        {
            RaycastHit hit = new RaycastHit();
            Ray ray = new Ray();
            ray.origin = new Vector3(Random.Range(transform.position.x + islandSize * 0.5f, transform.position.x - islandSize * 0.5f), 50.0f, Random.Range(transform.position.z + islandSize * 0.5f, transform.position.z - islandSize * 0.5f));
            ray.direction = Vector3.down;
            if (Physics.Raycast(ray, out hit, 100.0f) && Vector3.Dot(hit.normal, Vector3.up) > slopeThreshold)
            {
                point = hit.point;

                break;
            }
        }

        return point;
    }
}