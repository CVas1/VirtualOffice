using System;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerAnimController : MonoBehaviour
    {
        public enum PlayerState : uint
        {
            Idle,
            Walking,
            Running,
            Sitting,
            Jumping,
            Falling
        }

        // [SerializeField] private CharacterActor localPlayerCharacterActor;

        [SerializeField] private CharacterStateController localPlayerStateController;
        [SerializeField] private Animator remotePlayerAnimator;
        [SerializeField] private PlayerController playerController;

        public uint CurrentAnimState { get; private set; } = (uint)PlayerState.Idle;
        private bool isSitting = false;

        private void Start()
        {
            // if animator is not set, throw an error
            if (!playerController.isLocalPlayer && remotePlayerAnimator == null)
            {
                throw new Exception("Animator component is missing on PlayerAnimController.");
            }

            if (playerController.isLocalPlayer)
            {
                if (localPlayerStateController == null)
                {
                    throw new Exception("Local player state controller is not set on PlayerAnimController.");
                }

                localPlayerStateController.OnStateChange += OnCharacterStateChanged;
            }
        }

        private void Update()
        {
            // check if local player
            if (playerController.isLocalPlayer)
            {
                UpdateLocalPlayerState();
            }
        }

        private void UpdateLocalPlayerState()
        {
            if (isSitting)
            {
                CurrentAnimState = (uint)PlayerState.Sitting;
                return;
            }

            if (playerController.characterActor.IsGrounded)
            {
                if (playerController.characterActor.PlanarVelocity.magnitude > 0.1f)
                    CurrentAnimState = (uint)PlayerState.Running;
                else
                    CurrentAnimState = (uint)PlayerState.Idle;
            }
            else
            {
                if (playerController.characterActor.Velocity.y >= 0f)
                    CurrentAnimState = (uint)PlayerState.Jumping;
                else if (playerController.characterActor.Velocity.y < 0)
                    CurrentAnimState = (uint)PlayerState.Falling;
            }
        }

        private void OnCharacterStateChanged(CharacterState newState, CharacterState oldState)
        {
            isSitting = oldState is SitState;
        }

        public void SetPlayerState(uint state)
        {
            if (playerController.isLocalPlayer) return;

            remotePlayerAnimator.SetInteger("Anim", (int)state);
        }
    }
}