using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform doorTransform; 
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationDuration = 0.5f;

    private Quaternion closedRotation;
    private bool isOpen = false;

    private void Start()
    {
        closedRotation = doorTransform.localRotation;
    }


    public void OpenCloseDoor(Transform player)
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoorBasedOnPlayer(player);
        }
    }

    private void OpenDoorBasedOnPlayer(Transform player)
    {
        Vector3 doorToPlayer = player.position - doorTransform.position;
        Vector3 localDirection = doorTransform.InverseTransformDirection(doorToPlayer.normalized);

        float yAngle = (localDirection.z >= 0) ? openAngle : -openAngle;

        doorTransform.DOLocalRotateQuaternion(
            Quaternion.Euler(0f, yAngle, 0f),
            animationDuration
        );

        isOpen = true;
    }

    private void CloseDoor()
    {
        doorTransform.DOLocalRotateQuaternion(closedRotation, animationDuration);
        isOpen = false;
    }
}