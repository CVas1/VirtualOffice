using System;
using Assets.Scripts;
using TMPro;
using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
    private Camera mainCamera;

    public TMP_Text name;

    private void Start()
    {
        name.text = GetComponentInParent<PlayerController>().PlayerName;
    }

    void LateUpdate()
    {
        mainCamera = Camera.main;
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }
}
