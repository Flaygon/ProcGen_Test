using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    private Vector3 lastPosition;

    private Collider movingCollider;

    private void OnTriggerEnter(Collider collider)
    {
        movingCollider = collider;

        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        if(movingCollider != null)
            Stick();
    }

    private void OnTriggerExit(Collider collider)
    {
        movingCollider = null;
    }

    private void Stick()
    {
        movingCollider.transform.position += transform.position - lastPosition;
        //if(rotation)
        //    collider.transform.rotation *= Quaternion.FromToRotation(lastRotation.eulerAngles, transform.rotation.eulerAngles);

        lastPosition = transform.position;
    }
}