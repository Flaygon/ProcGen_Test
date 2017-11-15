using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject groundIslandPrefab;
    public GameObject floatingIslandPrefab;
    public GameObject mazePrefab;
    public GameObject twisterPrefab;

    public GameObject playerPrefab;
    private GameObject playerObject;

    //public float groundLooseGridMultiplier;
    public static float groundGridSize = 200.0f;
    public int groundGridViewDistance;

    //public float floatingLooseGridMultiplier;
    public static float floatingGridSize = 500.0f;
    public int floatingGridViewDistance;

    //public float mazeLooseGridMultiplier;
    public static float mazeGridSize = 1000.0f;
    public int mazeGridViewDistance;

    public static float twisterGridSize = 300.0f;
    public int twisterGridViewDistance;

    public int reverseIslandDensity;
    public int reverseFloatingIslandDensity;
    public int reverseMazeDensity;
    public int reverseTwisterDensity;

    List<GroundIsland> groundIslands;
    List<FloatingIsland> floatingIslands;
    List<Maze> mazes;
    List<Twister> twisters;

    public Water water;
    public Water clouds;

    private static int worldSeed = 0;

    private Vector3 lastPlayerPosition;

    public static float cloudsHigh = 100.0f;
    public static float cloudsLow = 90.0f;

    public enum Grids
    {
        ISLAND_GROUND,
        ISLAND_FLOATING,
        MAZE,
    };

    private void Awake()
    {
        playerObject = Instantiate(playerPrefab, Vector3.up * 10.0f, Quaternion.identity);
        lastPlayerPosition = playerObject.transform.position;

        groundIslands = new List<GroundIsland>();
        floatingIslands = new List<FloatingIsland>();
        mazes = new List<Maze>();
        twisters = new List<Twister>();

        GenerateGroundIslands(0, 0);
        GenerateFloatingIslands(0, 0);
        GenerateMazes(0, 0);
        GenerateTwisters(0, 0);
    }

    private void Update()
    {
        Vector3 playerPos = playerObject.transform.position;
        water.transform.position = new Vector3(playerPos.x, 0.0f, playerPos.z);
        clouds.transform.position = new Vector3(playerPos.x, clouds.transform.position.y, playerPos.z);

        int lastGroundGridX = (int)(lastPlayerPosition.x / groundGridSize);
        int lastGroundGridZ = (int)(lastPlayerPosition.z / groundGridSize);
        int newGroundGridX = (int)(playerObject.transform.position.x / groundGridSize);
        int newGroundGridZ = (int)(playerObject.transform.position.z / groundGridSize);
        if (lastGroundGridX != newGroundGridX || lastGroundGridZ != newGroundGridZ)
        {
            GenerateGroundIslands(newGroundGridX, newGroundGridZ);
        }

        int lastFloatingGridX = (int)(lastPlayerPosition.x / floatingGridSize);
        int lastFloatingGridZ = (int)(lastPlayerPosition.z / floatingGridSize);
        int newFloatingGridX = (int)(playerObject.transform.position.x / floatingGridSize);
        int newFloatingGridZ = (int)(playerObject.transform.position.z / floatingGridSize);
        if (lastFloatingGridX != newFloatingGridX || lastFloatingGridZ != newFloatingGridZ)
        {
            GenerateFloatingIslands(newFloatingGridX, newFloatingGridZ);
        }

        int lastMazeGridX = (int)(lastPlayerPosition.x / mazeGridSize);
        int lastMazeGridZ = (int)(lastPlayerPosition.z / mazeGridSize);
        int newMazeGridX = (int)(playerObject.transform.position.x / mazeGridSize);
        int newMazeGridZ = (int)(playerObject.transform.position.z / mazeGridSize);
        if (lastMazeGridX != newMazeGridX || lastMazeGridZ != newMazeGridZ)
        {
            GenerateMazes(newMazeGridX, newMazeGridZ);
        }

        int lastTwisterGridX = (int)(lastPlayerPosition.x / twisterGridSize);
        int lastTwisterGridZ = (int)(lastPlayerPosition.z / twisterGridSize);
        int newTwisterGridX = (int)(playerObject.transform.position.x / twisterGridSize);
        int newTwisterGridZ = (int)(playerObject.transform.position.z / twisterGridSize);
        if (lastTwisterGridX != newTwisterGridX || lastTwisterGridZ != newTwisterGridZ)
        {
            GenerateTwisters(newTwisterGridX, newTwisterGridZ);
        }

        lastPlayerPosition = playerObject.transform.position;
    }

    private void GenerateGroundIslands(int playerGridX, int playerGridZ)
    {
        Random.InitState(worldSeed);

        for (int iIsland = groundIslands.Count - 1; iIsland > -1; --iIsland)
        {
            Island currentIsland = groundIslands[iIsland];
            if (currentIsland.CanSeeFrom(playerGridX, playerGridZ, groundGridViewDistance))
            {
                Destroy(currentIsland.gameObject);
                groundIslands.RemoveAt(iIsland);
            }
        }

        for (int iGridX = playerGridX - groundGridViewDistance; iGridX <= playerGridX + groundGridViewDistance; ++iGridX)
        {
            for (int iGridY = playerGridZ - groundGridViewDistance; iGridY <= playerGridZ + groundGridViewDistance; ++iGridY)
            {
                bool exists = false;
                foreach (Island iIsland in groundIslands)
                {
                    if (iIsland.Is(iGridX, iGridY))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    if (iGridX == 0 && iGridY == 0)
                    {
                        GroundIsland newIsland = Instantiate(groundIslandPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, transform).GetComponent<GroundIsland>();
                        newIsland.Initialize(0, 0, 0);
                        groundIslands.Add(newIsland);
                    }
                    else
                    {
                        XXHash randomHash = new XXHash(worldSeed);
                        int gridSeed = (int)randomHash.GetHash(iGridX, iGridY);
                        if (gridSeed % reverseIslandDensity == 0)
                        {
                            GroundIsland newIsland = Instantiate(groundIslandPrefab, new Vector3(iGridX * groundGridSize + iGridX * 0.5f, 0.0f, iGridY * groundGridSize + iGridY * 0.5f), Quaternion.identity, transform).GetComponent<GroundIsland>();
                            newIsland.Initialize(iGridX, iGridY, gridSeed);
                            groundIslands.Add(newIsland);
                        }
                    }
                }
            }
        }
    }

    private void GenerateFloatingIslands(int playerGridX, int playerGridZ)
    {
        Random.InitState(worldSeed);

        for (int iFloatingIsland = floatingIslands.Count - 1; iFloatingIsland > -1; --iFloatingIsland)
        {
            Island currentIsland = floatingIslands[iFloatingIsland];
            if (currentIsland.CanSeeFrom(playerGridX, playerGridZ, floatingGridViewDistance))
            {
                Destroy(currentIsland.gameObject);
                floatingIslands.RemoveAt(iFloatingIsland);
            }
        }

        for (int iGridX = playerGridX - floatingGridViewDistance; iGridX <= playerGridX + floatingGridViewDistance; ++iGridX)
        {
            for (int iGridY = playerGridZ - floatingGridViewDistance; iGridY <= playerGridZ + floatingGridViewDistance; ++iGridY)
            {
                bool exists = false;
                foreach (FloatingIsland iIsland in floatingIslands)
                {
                    if (iIsland.Is(iGridX, iGridY))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    XXHash randomHash = new XXHash(worldSeed);
                    int gridSeed = (int)randomHash.GetHash(iGridX, iGridY, 1);
                    if (gridSeed % reverseFloatingIslandDensity == 0)
                    {
                        FloatingIsland newIsland = Instantiate(floatingIslandPrefab, new Vector3(iGridX * floatingGridSize + iGridX * 0.5f, 0.0f, iGridY * floatingGridSize + iGridY * 0.5f), Quaternion.identity, transform).GetComponent<FloatingIsland>();
                        newIsland.Initialize(iGridX, iGridY, gridSeed);
                        floatingIslands.Add(newIsland);
                    }
                }
            }
        }
    }

    private void GenerateMazes(int playerGridX, int playerGridZ)
    {
        Random.InitState(worldSeed);

        for (int iMaze = mazes.Count - 1; iMaze > -1; --iMaze)
        {
            Maze currentMaze = mazes[iMaze];
            if (currentMaze.CanSeeFrom(playerGridX, playerGridZ, mazeGridViewDistance))
            {
                Destroy(currentMaze.gameObject);
                mazes.RemoveAt(iMaze);
            }
        }

        for (int iGridX = playerGridX - mazeGridViewDistance; iGridX <= playerGridX + mazeGridViewDistance; ++iGridX)
        {
            for (int iGridY = playerGridZ - mazeGridViewDistance; iGridY <= playerGridZ + mazeGridViewDistance; ++iGridY)
            {
                bool exists = false;
                foreach (Maze iMaze in mazes)
                {
                    if (iMaze.Is(iGridX, iGridY))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    XXHash randomHash = new XXHash(worldSeed);
                    int gridSeed = (int)randomHash.GetHash(iGridX, iGridY);
                    if (gridSeed % reverseMazeDensity == 0)
                    {
                        Maze newMaze = Instantiate(mazePrefab, new Vector3(iGridX * mazeGridSize + iGridX * 0.5f, 0.0f, iGridY * mazeGridSize + iGridY * 0.5f), Quaternion.identity, transform).GetComponent<Maze>();
                        newMaze.Initialize(iGridX, iGridY, gridSeed);
                        mazes.Add(newMaze);
                    }
                }
            }
        }
    }

    private void GenerateTwisters(int playerGridX, int playerGridZ)
    {
        Random.InitState(worldSeed);

        for (int iTwister = twisters.Count - 1; iTwister > -1; --iTwister)
        {
            Twister currentTwister = twisters[iTwister];
            if (currentTwister.CanSeeFrom(playerGridX, playerGridZ, twisterGridViewDistance))
            {
                Destroy(currentTwister.gameObject);
                twisters.RemoveAt(iTwister);
            }
        }

        for (int iGridX = playerGridX - twisterGridViewDistance; iGridX <= playerGridX + twisterGridViewDistance; ++iGridX)
        {
            for (int iGridY = playerGridZ - twisterGridViewDistance; iGridY <= playerGridZ + twisterGridViewDistance; ++iGridY)
            {
                bool exists = false;
                foreach (Twister iTwister in twisters)
                {
                    if (iTwister.Is(iGridX, iGridY))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    if (iGridX != 0 || iGridY != 0)
                    {
                        XXHash randomHash = new XXHash(worldSeed);
                        int gridSeed = (int)randomHash.GetHash(iGridX, iGridY);
                        if (gridSeed % reverseTwisterDensity == 0)
                        {
                            Twister newTwister = Instantiate(twisterPrefab, new Vector3(iGridX * twisterGridSize + iGridX * 0.5f, 0.0f, iGridY * twisterGridSize + iGridY * 0.5f), Quaternion.identity, transform).GetComponent<Twister>();
                            newTwister.Initialize(iGridX, iGridY, gridSeed);
                            twisters.Add(newTwister);
                        }
                    }
                }
            }
        }
    }

    static public Vector2 GetGrid(Vector3 position, Grids grid)
    {
        Vector2 returnGrid = Vector2.zero;

        float gridSize = 0.0f;

        switch(grid)
        {
            case Grids.ISLAND_GROUND:
                {
                    gridSize = groundGridSize;
                    break;
                }
            case Grids.ISLAND_FLOATING:
                {
                    gridSize = floatingGridSize;
                    break;
                }
            case Grids.MAZE:
                {
                    gridSize = mazeGridSize;
                    break;
                }
        }

        returnGrid.x = Mathf.FloorToInt(position.x / gridSize);
        returnGrid.y = Mathf.FloorToInt(position.z / gridSize);
        return returnGrid;
    }
}