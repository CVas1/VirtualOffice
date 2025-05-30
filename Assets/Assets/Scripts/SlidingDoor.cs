using UnityEngine;
using DG.Tweening;

public class SlidingDoor : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private float slideDistance = 1.5f;
    [SerializeField] private float animationDuration = 0.5f;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private bool isOpen = false;

    private void Start()
    {
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isOpen && other.CompareTag("Player"))
        {
            OpenDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isOpen && other.CompareTag("Player"))
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        leftDoor.DOLocalMove(leftClosedPos + Vector3.left * slideDistance, animationDuration);
        rightDoor.DOLocalMove(rightClosedPos + Vector3.right * slideDistance, animationDuration);
        isOpen = true;
    }

    private void CloseDoor()
    {
        leftDoor.DOLocalMove(leftClosedPos, animationDuration);
        rightDoor.DOLocalMove(rightClosedPos, animationDuration);
        isOpen = false;
    }
}
