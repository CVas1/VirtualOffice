using System;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private LayerMask projectorLayerMask; 
    [SerializeField]
    private CharacterStateController characterStateController;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            characterStateController.EnqueueTransition<SitState>();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            //raycast to the object
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, projectorLayerMask))
            {
                print(hit.collider.gameObject.name);
                GameObject targetObject = hit.collider.gameObject;
                OfficeProjector officeProjector = targetObject.GetComponentInParent<OfficeProjector>();

                if (officeProjector != null)
                {
                    officeProjector.OnClick();
                }
            }
        }
    }
}
