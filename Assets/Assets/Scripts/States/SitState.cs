using System.Collections.Generic;
using UnityEngine;
using Lightbug.CharacterControllerPro.Implementation;

public class SitState : CharacterState
{
    [Header("Chair Detection")] [SerializeField]
    protected string chairTag = "Chair";

    protected bool isSitting = false;
    protected Transform chairTransform = null;

    //increase chair y offset
    [SerializeField] private float addChairUp = 0.15f;
    [SerializeField] private float addChairForward = 0.15f;

    private HashSet<GameObject> chairsInTrigger = new();


    public override void CheckExitTransition()
    {
        // print("Checking exit transition for SitState");
        // if (!GetNearbyChair())
        // {
        //     CharacterStateController.EnqueueTransition<MoveState>();
        // }
    }
    public override bool CheckEnterTransition(CharacterState fromState)
    {
        if(InteractManager.Instance.chair != null)
        {
            chairTransform = InteractManager.Instance.chair.transform;
            return true;
        }
        return false;
    }

    public override void EnterBehaviour(float dt, CharacterState fromState)
    {
        base.EnterBehaviour(dt, fromState);
        isSitting = true;
        chairTransform = InteractManager.Instance.chair.transform;

        // Force the character to be not grounded
        CharacterActor.ForceNotGrounded();
    }

    public override void ExitBehaviour(float dt, CharacterState toState)
    {
        base.ExitBehaviour(dt, toState);
        isSitting = false;
        chairTransform = null;
        InteractManager.Instance.chair = null;

        // Restore normal grounding behavior
        CharacterActor.ForceNotGrounded();
    }

    public override void UpdateBehaviour(float dt)
    {
        // Keep character locked to chair position
        if (isSitting && chairTransform != null)
        {
            CharacterActor.ForceNotGrounded();
            CharacterActor.Position = chairTransform.position + Vector3.up * addChairUp + chairTransform.forward * addChairForward;
            CharacterActor.Forward = chairTransform.forward ;
            // Do NOT set velocity here, since IsKinematic is true

            float horizontal = Input.GetAxisRaw("Movement X");
            float vertical = Input.GetAxisRaw("Movement Y");
            if (Mathf.Abs(horizontal) > 0.0000001f || Mathf.Abs(vertical) > 0.00001f)
            {
                CharacterStateController.EnqueueTransition<MoveState>();
            }
        }
    }

    public override void UpdateIK(int layerIndex)
    {
        if (CharacterActor.Animator == null)
            return;

        // Disable left foot IK
        CharacterActor.Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
        CharacterActor.Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);

        // Disable right foot IK
        CharacterActor.Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
        CharacterActor.Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
    }

    // Helper: Get the transform of a nearby chair
    private Transform GetNearbyChair()
    {
        foreach (var chair in GetChairsInTrigger())
        {
            if (chair.CompareTag(chairTag))
            {
                return chair.transform;
            }
        }

        return null;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(chairTag))
        {
            chairsInTrigger.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(chairTag))
        {
            chairsInTrigger.Remove(other.gameObject);
        }
    }

    // You can call this method from anywhere to get the current "chair" objects inside the trigger
    public List<GameObject> GetChairsInTrigger()
    {
        return new List<GameObject>(chairsInTrigger);
    }
}