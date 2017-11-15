using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    public List<GameObject> bowSpiritMounts;
    public List<GameObject> activeSpiritBowMounts;
    private int currentSpiritMount;

    private Player player;
    private Animator animator;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                animator.SetBool("Holding", true);
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                animator.SetBool("Holding", false);

                RaycastHit info = new RaycastHit();
                if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)), out info, 150.0f, Animal.GetSpiritTypeMask((Animal.SpiritType)currentSpiritMount)))
                {
                    Animal hitAnimal = info.collider.GetComponent<Animal>();
                    if (hitAnimal != null)
                    {
                        hitAnimal.OnHit();
                    }
                }
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll >= 0.1f)
        {
            activeSpiritBowMounts[currentSpiritMount--].SetActive(false);
            if (currentSpiritMount < 0)
                currentSpiritMount = activeSpiritBowMounts.Count - 1;
            activeSpiritBowMounts[currentSpiritMount].SetActive(true);
        }
        else if(scroll <= -0.1f)
        {
            activeSpiritBowMounts[currentSpiritMount++].SetActive(false);
            if (currentSpiritMount >= activeSpiritBowMounts.Count)
                currentSpiritMount = 0;
            activeSpiritBowMounts[currentSpiritMount].SetActive(true);
        }
    }

    public void AddSpiritBowMount(Animal.SpiritType type)
    {
        activeSpiritBowMounts.Add(bowSpiritMounts[(int)type]);
    }
}