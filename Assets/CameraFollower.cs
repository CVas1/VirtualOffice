using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private float maxXDegree = 55;
    [SerializeField] private float minXDegree = -20;

    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 moveDirection = new Vector3(
                Input.GetAxis("Mouse Y") * Mathf.Deg2Rad * rotationSpeed,
                Input.GetAxis("Mouse X") * Mathf.Deg2Rad * rotationSpeed, 
                0);

            transform.Rotate(moveDirection, Space.World);
        }
    }
}