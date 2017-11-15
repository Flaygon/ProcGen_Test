using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundIsland : Island
{
    public float minBeachThreshold;
    public float maxBeachThreshold;
    private float beachThreshold;

    //private Texture2D terrainMap;
    public Texture2D islandMask;

    public override void Initialize(int gridX, int gridZ, int seed)
    {
        base.Initialize(gridX, gridZ, seed);

        beachThreshold = Random.Range(minBeachThreshold, maxBeachThreshold);

        //GenerateIslandMap(out terrainMap, islandMask);
        GenerateTerrain(/*terrainMap*//*settings.GetGenerator(seed)*/new CoherentNoise.Generation.Modification.Gain(new CoherentNoise.Generation.Fractal.PinkNoise(seed), 0.25f), islandMask, transform);

        for (int iChild = 0; iChild < transform.childCount; ++iChild)
        {
            MeshFilter childFilter = transform.GetChild(iChild).GetComponent<MeshFilter>();

            Vector3[] positions = childFilter.mesh.vertices;
            Color[] colors = childFilter.mesh.colors;
            for (int iVertex = 0; iVertex < positions.Length; ++iVertex)
            {
                Color vertexColor = colors[iVertex];
                vertexColor.r = positions[iVertex].y > beachThreshold ? 1.0f : 0.0f;
                colors[iVertex] = vertexColor;
            }
            childFilter.mesh.colors = colors;
        }

        GenerateBiome(beachThreshold, GetComponentsInChildren<MeshRenderer>());

        //GetComponent<AnimalHandler>().Initialize(islandSize);
    }
}