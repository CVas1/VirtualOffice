using System;
using HighlightPlus;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

public class InteractManager : MonoBehaviourSingleton<InteractManager>
{
    private CharacterStateController characterStateController;

    [NonSerialized] public GameObject chair;
    [SerializeField] private LayerMask interactLayerMask;
    private GameObject previousHitObject = null;
    private HighlightEffect previousHighlightEffect = null;

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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject currentHitObject = hit.collider.gameObject;

            if (currentHitObject != null && currentHitObject != previousHitObject)
            {
                if (previousHighlightEffect != null)
                {
                    previousHighlightEffect.highlighted = false;
                }

                previousHitObject = currentHitObject;
                previousHighlightEffect = currentHitObject.GetComponentInParent<HighlightEffect>();
                if (previousHighlightEffect != null)
                {
                    previousHighlightEffect.highlighted = true;
                }
            }
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (currentHitObject.CompareTag(Tags.Chair))
                {
                    chair = currentHitObject;
                    characterStateController.EnqueueTransition<SitState>();
                }
                else if (currentHitObject.CompareTag(Tags.Door))
                {
                    //raycast to the door
                    Door door = currentHitObject.GetComponentInParent<Door>();
                    if (door != null)
                    {
                        door.OpenCloseDoor(characterStateController.transform);
                    }
                }
                else if (currentHitObject.CompareTag(Tags.Projector))
                {
                    //raycast to the projector
                    OfficeProjector officeProjector = currentHitObject.GetComponentInParent<OfficeProjector>();
                    if (officeProjector != null)
                    {
                        officeProjector.OnClick();
                    }
                }
            }
            
        }

        
    }
}