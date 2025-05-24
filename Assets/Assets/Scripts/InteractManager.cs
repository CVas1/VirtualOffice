using System;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

public class InteractManager : MonoBehaviourSingleton<InteractManager>
{
    private CharacterStateController characterStateController;

    [NonSerialized] public GameObject chair;

    private void Start()
    {
        characterStateController = GetComponentInChildren<CharacterStateController>();
        if (characterStateController == null)
        {
            Debug.LogError("InteractManager: CharacterStateController not found.");
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //raycast control characterStateController.EnqueueTransition<SitState>();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                print(hit.collider.gameObject.name);
                GameObject go = hit.collider.gameObject;

                if (go.CompareTag(Tags.Chair))
                {
                    chair = go;
                    characterStateController.EnqueueTransition<SitState>();
                }
                else if (go.CompareTag(Tags.Door))
                {
                    //raycast to the door
                    Door door = go.GetComponentInParent<Door>();
                    if (door != null)
                    {
                        door.OpenCloseDoor(characterStateController.transform);
                    }
                }
                else if (go.CompareTag(Tags.Projector))
                {
                    //raycast to the projector
                    OfficeProjector officeProjector = go.GetComponentInParent<OfficeProjector>();
                    if (officeProjector != null)
                    {
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        {
                            officeProjector.Broadcast();
                        }
                        else
                        {
                            officeProjector.ChangeOnce();
                        }
                    }
                }
            }
        }
    }
}