using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform doorTransform; // Assign the door (child) here
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private string playerTag = "Player";

    private Quaternion closedRotation;
    private HashSet<Collider> playersInTrigger = new HashSet<Collider>();
    private bool isOpen = false;

    private void Start()
    {
        closedRotation = doorTransform.localRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playersInTrigger.Add(other);

        if (!isOpen)
        {
            OpenDoorBasedOnPlayer(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playersInTrigger.Remove(other);

        if (playersInTrigger.Count == 0)
        {
            CloseDoor();
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