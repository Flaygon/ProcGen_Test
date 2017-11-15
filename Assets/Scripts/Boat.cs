using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    private List<WaveSampler> samplePoints;

    private Rigidbody body;

    public float speed;

    [HideInInspector]
    public bool commandeering;

    private void Awake()
    {
        samplePoints = new List<WaveSampler>();
        samplePoints.AddRange(transform.GetComponentsInChildren<WaveSampler>(true));

        body = GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        if(commandeering)
        {
            Vector3 movement = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
                movement.z += 1.0f;
            if (Input.GetKey(KeyCode.S))
                movement.z -= 1.0f;

            if (Input.GetKey(KeyCode.D))
                movement.x += 1.0f;
            if (Input.GetKey(KeyCode.A))
                movement.x -= 1.0f;

            movement = (Camera.main.transform.rotation * movement);
            movement.y = 0.0f;
            transform.position += movement.normalized * speed * Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        foreach(WaveSampler iSampler in samplePoints)
        {
            float waterLevel = iSampler.Sample();
            float voxelHalfHeight = 0.5f;

            if (iSampler.transform.position.y - 0.5f < waterLevel)
            {
                float k = (waterLevel - iSampler.transform.position.y) / (2 * voxelHalfHeight) + 0.5f;
                if (k > 1)
                {
                    k = 1f;
                }
                else if (k < 0)
                {
                    k = 0f;
                }

                var velocity = body.GetPointVelocity(iSampler.transform.position);
                var localDampingForce = -velocity * iSampler.damper * body.mass;
                var force = localDampingForce + Mathf.Sqrt(k) * iSampler.GetLocalArchimedesForce();
                body.AddForceAtPosition(force, iSampler.transform.position);
            }
        }
    }
}