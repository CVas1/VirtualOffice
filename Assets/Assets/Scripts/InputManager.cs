using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public LayerMask projectorLayerMask; 
    private void Update()
    {
        

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
