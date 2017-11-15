using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour
{
    public LayerMask interractables;

    [HideInInspector]
    public List<int> spiritLevels;

    private bool commandeering;

    //public float speed;

    //private CharacterController controller;

    //private MouseLook mouseLook;

    //public float sensitivityX = 2.0f;
    //public float sensitivityY = 2.0f;

    private void Awake()
    {
        /*controller = GetComponent<CharacterController>();

        mouseLook = new MouseLook();
        mouseLook.Init(transform, Camera.main.transform);
        mouseLook.SetCursorLock(true);
        mouseLook.UpdateCursorLock();
        mouseLook.XSensitivity = sensitivityX;
        mouseLook.YSensitivity = sensitivityY;*/

        spiritLevels = new List<int>((int)Animal.SpiritType.COUNT);
        for(int iVisual = 0; iVisual < (int)Animal.SpiritType.COUNT; ++iVisual)
        {
            spiritLevels.Add(0);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            if (!commandeering)
            {
                RaycastHit info = new RaycastHit();
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out info, 5.0f, interractables))
                    EnterVehicle(info.collider.gameObject);
            }
            else if(commandeering)
                ExitVehicle();
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(GetComponent<Rigidbody>().useGravity)
            {
                GetComponent<Rigidbody>().useGravity = false;
                GetComponent<FirstPersonController>().RemoveGravity();
            }
            else
            {
                GetComponent<Rigidbody>().useGravity = true;
                GetComponent<FirstPersonController>().EnableGravity();
            }
        }

        /*Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            movement.z += 1.0f;
        if (Input.GetKey(KeyCode.S))
            movement.z -= 1.0f;

        if (Input.GetKey(KeyCode.D))
            movement.x += 1.0f;
        if (Input.GetKey(KeyCode.A))
            movement.x -= 1.0f;

        controller.SimpleMove((transform.rotation * movement) * speed);

        mouseLook.LookRotation(transform, Camera.main.transform);*/
    }

    private void EnterVehicle(GameObject vehicle)
    {
        commandeering = true;

        vehicle.GetComponent<Boat>().commandeering = true;

        GetComponent<FirstPersonController>().commandeering = true;

        transform.parent = vehicle.transform;
    }

    private void ExitVehicle()
    {
        commandeering = false;

        transform.parent.GetComponent<Boat>().commandeering = false;

        GetComponent<FirstPersonController>().commandeering = false;

        transform.parent = null;
    }

    public void AddSpirit(SpiritOrb orb)
    {
        spiritLevels[(int)orb.type] += orb.worth;

        int spiritTypeMask = Animal.GetSpiritTypeMask(orb.type);
        if ((Camera.main.cullingMask & spiritTypeMask) == 0)
        {
            int cullingMask = Camera.main.cullingMask;
            cullingMask |= spiritTypeMask;
            Camera.main.cullingMask = cullingMask;

            GetComponent<Bow>().AddSpiritBowMount(orb.type);
        }
    }

    public int GetTotalSpiritMask()
    {
        int returningMask = 0;

        for(int iSpirit = 0; iSpirit < spiritLevels.Count; ++iSpirit)
        {
            returningMask |= Animal.GetSpiritTypeMask((Animal.SpiritType)iSpirit);
        }

        return returningMask;
    }
}