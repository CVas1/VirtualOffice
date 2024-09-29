using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 10.0f;
    [SerializeField] private float rotSpeed = 10.0f;
    [SerializeField] private float zoomSpeed = 10.0f;

    [SerializeField] private float minXRotaion = 20;
    [SerializeField] private float maxXRotaion = 90;

    private Vector3 dragOrigin;

    [SerializeField] private CinemachineFreeLook virtualCamera;
    private Camera mainCamera;

    void Start()
    {
        CinemachineCore.GetInputAxis = GetAxisCustom;
        mainCamera = Camera.main;
    }

    public float GetAxisCustom(string axisName)
    {
        if (axisName == "Mouse X")
        {
            if (Input.GetMouseButton(1))
            {
                return -UnityEngine.Input.GetAxis("Mouse X");
            }
            else
            {
                return 0;
            }
        }
        else if (axisName == "Mouse Y")
        {
            if (Input.GetMouseButton(1))
            {
                return -UnityEngine.Input.GetAxis("Mouse Y") * (1/Time.deltaTime) * zoomSpeed;
            }
            else
            {
                return 0;
            }
        }

        return UnityEngine.Input.GetAxis(axisName);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = new Vector3(0, 0, 0);

        // Get the forward and right vectors relative to the camera's orientation
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Flatten the forward and right vectors so the movement stays horizontal
        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Handle movement input
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += cameraForward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveDirection -= cameraForward;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveDirection -= cameraRight;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += cameraRight;
        }

        // Normalize the direction and move the object
        moveDirection.Normalize();
        transform.Translate(moveDirection * movementSpeed * Time.deltaTime, Space.World);
    }
}