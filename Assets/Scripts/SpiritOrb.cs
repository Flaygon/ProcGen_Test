using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritOrb : MonoBehaviour
{
    public Animal.SpiritType type;

    public int worth;

    public float aquireTime;
    private float currentAquireTime;
    private bool aquiring;

    private Vector3 startPosition;
    private Player player;

    private void Update()
    {
        if(aquiring)
        {
            currentAquireTime += Time.deltaTime;
            if(currentAquireTime >= aquireTime)
            {
                player.AddSpirit(this);

                Destroy(gameObject);
            }
            else
            {
                transform.position = Vector3.Lerp(startPosition, player.transform.position, currentAquireTime / aquireTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        aquiring = true;

        startPosition = transform.position;
        player = other.GetComponent<Player>();

        GetComponent<Bobbing>().enabled = false;
    }
}