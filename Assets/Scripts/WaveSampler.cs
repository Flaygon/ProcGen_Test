using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSampler : MonoBehaviour
{
    public Water water;
    public Water clouds;

    [HideInInspector]
    public Water currentSurface;

    public float density = 500;

    public float damper = 0.1f;

    public bool auto;

    [HideInInspector]
    public Vector3 localWaterArchimedesForce;
    [HideInInspector]
    public Vector3 localCloudsArchimedesForce;

    private void Start()
    {
        float volume = GetComponentInParent<Rigidbody>().mass / density;
        //float archimedesForceMagnitude = water.density * Mathf.Abs(Physics.gravity.y) * volume;
        localWaterArchimedesForce = new Vector3(0, water.density * Mathf.Abs(Physics.gravity.y) * volume, 0);//new Vector3(0, archimedesForceMagnitude, 0) / samplePoints.Count;
        localCloudsArchimedesForce = new Vector3(0, clouds.density * Mathf.Abs(Physics.gravity.y) * volume, 0);//new Vector3(0, archimedesForceMagnitude, 0) / samplePoints.Count;

        currentSurface = transform.position.y < test.cloudsLow ? water : clouds;
    }

    public float Sample()
    {
        return currentSurface.Sample(transform.position) + (currentSurface == water ? 0.0f : test.cloudsHigh) + 1.0f;
    }

    private void LateUpdate()
    {
        currentSurface = transform.position.y < test.cloudsLow ? water : clouds;

        if(auto)
        {
            Vector3 waveOffset = transform.position;
            waveOffset.y = Sample();
            transform.position = waveOffset;
        }
    }

    public Vector3 GetLocalArchimedesForce()
    {
        return currentSurface == water ? localWaterArchimedesForce : localCloudsArchimedesForce;
    }
}