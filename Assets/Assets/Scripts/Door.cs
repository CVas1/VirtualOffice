using UnityEngine;
using DG.Tweening;

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

    private void OnTriggerEnter(Collider other)
    {
        if (!isOpen && other.CompareTag("Player"))
        {
            OpenDoorBasedOnPlayer(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isOpen && other.CompareTag("Player"))
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