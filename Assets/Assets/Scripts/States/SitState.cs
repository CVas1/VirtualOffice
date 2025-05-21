using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;
using Lightbug.CharacterControllerPro.Implementation;

public class SitState : CharacterState
{
    [Header("Chair Detection")]
    [SerializeField]
    protected string chairTag = "Chair";

    protected bool isSitting = false;
    protected Transform chairTransform = null;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void CheckExitTransition()
    {
        print("Checking exit transition for SitState");
        // If there is no chair nearby, exit to NormalMovement
        if (!IsChairNearby())
        {
            CharacterStateController.EnqueueTransition<MoveState>();
        }
    }

    public override bool CheckEnterTransition(CharacterState fromState)
    {
        // Only enter if there is a chair nearby
        chairTransform = GetNearbyChair();
        return chairTransform != null;
    }

    public override void EnterBehaviour(float dt, CharacterState fromState)
    {
        base.EnterBehaviour(dt, fromState);
        isSitting = true;
        chairTransform = GetNearbyChair();

        if (chairTransform != null)
        {
            // Snap character to chair position and rotation
            CharacterActor.Position = chairTransform.position;
            CharacterActor.Forward = chairTransform.forward;

            // Do NOT set IsKinematic here!
        }
    }

    public override void ExitBehaviour(float dt, CharacterState toState)
    {
        base.ExitBehaviour(dt, toState);
        isSitting = false;
        chairTransform = null;
    }

    public override void UpdateBehaviour(float dt)
    {
        // Keep character locked to chair position
        if (isSitting && chairTransform != null)
        {
            CharacterActor.Position = chairTransform.position;
            CharacterActor.Forward = chairTransform.forward;
            // Do NOT set velocity here, since IsKinematic is true

            
            print("sitting");
            if (Input.GetKeyDown(KeyCode.E))
            {
                print("Exiting sit state");
                CharacterStateController.ForceState<MoveState>();
            }
            float horizontal = Input.GetAxisRaw("Movement X");
            float vertical = Input.GetAxisRaw("Movement Y");
            if (Mathf.Abs(horizontal) > 0.0000001f || Mathf.Abs(vertical) > 0.00001f)
            {
                CharacterStateController.EnqueueTransition<MoveState>();
            }
        }
    }

    // Helper: Check if a chair is nearby using triggers
    protected bool IsChairNearby()
    {
        foreach (var trigger in CharacterActor.Triggers)
        {
            if (trigger.transform.CompareTag(chairTag))
                return true;
        }
        return false;
    }

    // Helper: Get the transform of a nearby chair
    protected Transform GetNearbyChair()
    {
        print("sa");
        foreach (var trigger in CharacterActor.Triggers)
        {
            if (trigger.transform.CompareTag(chairTag))
                return trigger.transform;
        }
        return null;
    }
}
