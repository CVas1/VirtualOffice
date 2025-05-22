using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine.Serialization;

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
        if (!GetNearbyChair())
        {
            CharacterStateController.EnqueueTransition<MoveState>();
        }
    }

    public override bool CheckEnterTransition(CharacterState fromState)
    {
        chairTransform = GetNearbyChair();
        return chairTransform != null;
    }

    public override void EnterBehaviour(float dt, CharacterState fromState)
    {
        base.EnterBehaviour(dt, fromState);
        isSitting = true;
        chairTransform = GetNearbyChair();

        // Force the character to be not grounded
        CharacterActor.ForceNotGrounded();
    }

    public override void ExitBehaviour(float dt, CharacterState toState)
    {
        base.ExitBehaviour(dt, toState);
        isSitting = false;
        chairTransform = null;

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
        print("Trigger Entered");
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
    public List<GameObject> GetChairsInTrigger()
    {
        return new List<GameObject>(chairsInTrigger);
    }
}