using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritAltar : MonoBehaviour
{
    public SpiritOrb orbPrefab;

    public Transform orbPosition;

    private void Start()
    {
        SpiritOrb newOrb = Instantiate(orbPrefab, orbPosition, false);
    }
}