using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalHandler : MonoBehaviour
{
    public List<GameObject> animals;

    public int minAnimalsSpawned;
    public int maxAnimalsSpawned;

    public int minSpecialAnimalsSpawned;
    public int maxSpecialAnimalsSpawned;

    public void Initialize(float islandSize)
    {
        int animalsSpawned = Random.Range(minAnimalsSpawned, maxAnimalsSpawned + 1);

        for(int iAnimal = 0; iAnimal < animalsSpawned; ++iAnimal)
        {
            RaycastHit hit = new RaycastHit();
            Ray ray = new Ray();
            ray.origin = new Vector3(Random.Range(transform.position.x + islandSize * 0.5f, transform.position.x - islandSize * 0.5f), 50.0f, Random.Range(transform.position.z + islandSize * 0.5f, transform.position.z - islandSize * 0.5f));
            ray.direction = Vector3.down;
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                Animal newAnimal = Instantiate(animals[0], hit.point, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f), transform).GetComponent<Animal>();
                newAnimal.Initialize();
            }
        }

        int animalsSpecialSpawned = Random.Range(minSpecialAnimalsSpawned, maxSpecialAnimalsSpawned + 1);

        for (int iAnimal = 0; iAnimal < animalsSpecialSpawned; ++iAnimal)
        {
            RaycastHit hit = new RaycastHit();
            Ray ray = new Ray();
            ray.origin = new Vector3(Random.Range(transform.position.x + islandSize * 0.5f, transform.position.x - islandSize * 0.5f), 50.0f, Random.Range(transform.position.z + islandSize * 0.5f, transform.position.z - islandSize * 0.5f));
            ray.direction = Vector3.down;
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                Animal newAnimal = Instantiate(animals[1], hit.point, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f), transform).GetComponent<Animal>();
                newAnimal.Initialize();
            }
        }
    }
}