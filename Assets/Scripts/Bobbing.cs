using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bobbing : MonoBehaviour
{
    public AnimationCurve bob;
    public float bobStrength;

    public float bobbingTime;
    private float currentBobbingTime;

    private Vector3 startingPosition;

    private void Start()
    {
        startingPosition = transform.position;
    }

    private void Update()
    {
        currentBobbingTime += Time.deltaTime;

        Vector3 newBob = startingPosition;
        newBob.y += bob.Evaluate(currentBobbingTime / bobbingTime);
        transform.position = newBob;
    }
}