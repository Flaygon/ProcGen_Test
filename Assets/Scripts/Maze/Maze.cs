using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
    public SpiritAltar altar;

    private int gridX;
    private int gridZ;
    private int seed;

    private void Start()
    {
        Instantiate(altar, transform, false);
    }

    public virtual void Initialize(int gridX, int gridZ, int seed)
    {
        Random.InitState(seed);

        this.gridX = gridX;
        this.gridZ = gridZ;
        this.seed = seed;
    }

    public bool CanSeeFrom(int gridX, int gridZ, int gridViewDistance)
    {
        return Mathf.Abs(this.gridX - gridX) > gridViewDistance || Mathf.Abs(this.gridZ - gridZ) > gridViewDistance;
    }

    public bool Is(int gridX, int gridZ)
    {
        return this.gridX == gridX && this.gridZ == gridZ;
    }
}