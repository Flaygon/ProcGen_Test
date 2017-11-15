using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IslandSettings", menuName = "Island Setting")]
public class IslandSettings : ScriptableObject
{
    public enum FirstGenerator
    {
        BILLOW,
        PINK,
        RIDGE,
    };
    public FirstGenerator firstGenerator;

    public enum SecondGenerator
    {
        NONE,

        BILLOW,
        PINK,
        RIDGE,
        VORONOI_PITS,
        VORONOI_VALLEYS,
        GRADIENT,
    };
    public SecondGenerator secondGenerator;

    public float minTerrainHeight;
    public float maxTerrainHeight;
    protected float terrainHeight;

    public float minIslandRoughness;
    public float maxIslandRoughness;
    protected float islandRoughness;

    public float GetRoughness()
    {
        return Random.Range(minIslandRoughness, maxIslandRoughness);
    }

    public float GetHeight()
    {
        return Random.Range(minTerrainHeight, maxTerrainHeight);
    }

    public CoherentNoise.Generator GetGenerator(int seed)
    {
        CoherentNoise.Generator returnGenerator = null;
        CoherentNoise.Generator secondaryGenerator = GetSecondaryGenerator(seed);
        switch (firstGenerator)
        {
            case FirstGenerator.BILLOW:
                {
                    if(secondaryGenerator != null)
                        returnGenerator = new CoherentNoise.Generation.Fractal.BillowNoise(secondaryGenerator);
                    else
                        returnGenerator = new CoherentNoise.Generation.Fractal.BillowNoise(seed);

                    break;
                }
            case FirstGenerator.PINK:
                {
                    if (secondaryGenerator != null)
                        returnGenerator = new CoherentNoise.Generation.Fractal.PinkNoise(secondaryGenerator);
                    else
                        returnGenerator = new CoherentNoise.Generation.Fractal.PinkNoise(seed);

                    break;
                }
            case FirstGenerator.RIDGE:
                {
                    if (secondaryGenerator != null)
                        returnGenerator = new CoherentNoise.Generation.Fractal.RidgeNoise(secondaryGenerator);
                    else
                        returnGenerator = new CoherentNoise.Generation.Fractal.RidgeNoise(seed);

                    break;
                }
        }
        return returnGenerator;
    }

    private CoherentNoise.Generator GetSecondaryGenerator(int seed)
    {
        CoherentNoise.Generator returnGenerator = null;

        switch(secondGenerator)
        {
            case SecondGenerator.BILLOW:
                {
                    returnGenerator = new CoherentNoise.Generation.Fractal.BillowNoise(seed);
                    break;
                }
            case SecondGenerator.PINK:
                {
                    returnGenerator = new CoherentNoise.Generation.Fractal.PinkNoise(seed);
                    break;
                }
            case SecondGenerator.RIDGE:
                {
                    returnGenerator = new CoherentNoise.Generation.Fractal.RidgeNoise(seed);
                    break;
                }
            case SecondGenerator.VORONOI_PITS:
                {
                    returnGenerator = new CoherentNoise.Generation.Voronoi.VoronoiPits(seed);
                    break;
                }
            case SecondGenerator.VORONOI_VALLEYS:
                {
                    returnGenerator = new CoherentNoise.Generation.Voronoi.VoronoiValleys(seed);
                    break;
                }
            case SecondGenerator.GRADIENT:
                {
                    returnGenerator = new CoherentNoise.Generation.GradientNoise(seed);
                    break;
                }
        }

        return returnGenerator;
    }
}