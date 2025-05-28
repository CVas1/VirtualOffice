using System;
using System.Collections.Generic;
using Assets.Scripts.Networking;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;
using TMPro;
using SpacetimeDB;
using SpacetimeDB.Types;

namespace Assets.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        public SkinnedMeshRenderer meshRenderer;

        private Vector3 lastSyncedPosition;
        private Quaternion lastSyncedRotation;
        public uint lastSyncedState = 0U;
        public bool isLocalPlayer = false;

        public uint PlayerId { get; private set; }
        public Identity Identity { get; private set; }

        public string PlayerName { get; private set; }
        public Color PlayerColor { get; private set; }
        public ulong RoomJoinTime { get; private set; }

        private float lastUpdateTime;
        private const float updateInterval = 0.2f;

        private class State
        {
            public Vector3 position;
            public Quaternion rotation;
            public float timestamp;
            public uint state;
        }

        private Queue<State> stateBuffer = new Queue<State>();
        private float interpolationBackTime = 0.25f; // 250 ms

        [HideInInspector] public CharacterActor characterActor;
        SpawnPoint sp;
        private PlayerAnimController animController;

        private void Awake()
        {
            // meshRenderer = GetComponent<MeshRenderer>();
            sp = FindAnyObjectByType<SpawnPoint>();
            characterActor = GetComponent<CharacterActor>();
            animController = GetComponent<PlayerAnimController>();
        }

        void Update()
        {
            TeleportIfBelowZero();

            if (isLocalPlayer && Time.time - lastUpdateTime >= updateInterval)
            {
                if (lastSyncedState != animController.CurrentAnimState)
                {
                    Debug.Log(animController.CurrentAnimState);
                }

                if (Vector3.Distance(transform.position, lastSyncedPosition) > 0.3f ||
                    Quaternion.Angle(transform.rotation, lastSyncedRotation) > 6f ||
                    lastSyncedState != animController.CurrentAnimState)
                {
                    float yaw = transform.rotation.eulerAngles.y;
                    STDBBackendManager.Instance.playerManager.UpdatePlayerPositionWithAnimation(
                        transform.position, yaw,
                        animController.CurrentAnimState
                    );

                    lastSyncedPosition = transform.position;
                    lastSyncedRotation = transform.rotation;
                    lastSyncedState = animController.CurrentAnimState;
                }

                lastUpdateTime = Time.time;
                return;
            }

            float interpTime = Time.time - interpolationBackTime;

            // Find two states to interpolate between
            State prev = null, next = null;
            foreach (var state in stateBuffer)
            {
                if (state.timestamp <= interpTime)
                    prev = state;
                else
                {
                    next = state;
                    break;
                }
            }

            if (prev != null && next != null)
            {
                float t = Mathf.InverseLerp(prev.timestamp, next.timestamp, interpTime);
                transform.position = Vector3.Lerp(prev.position, next.position, t);
                transform.rotation = Quaternion.Slerp(prev.rotation, next.rotation, t);
                //lastSyncedState = (PlayerAnimController.PlayerState)next.state;
                animController.SetPlayerState(next.state);
                lastSyncedState = next.state;
            }
        }


        private void TeleportIfBelowZero()
        {
            if (!isLocalPlayer || characterActor.IsGrounded) return;

            if (characterActor.transform.position.y > -5) return;
            if (sp != null)
            {
                characterActor.Teleport(sp.spawnPointTransform);
            }
            else
            {
                characterActor.Teleport(new Vector3(0, 1, 0));
            }
        }

        public void Init(OnlinePlayer playerData, bool isLocal)
        {
            PlayerId = playerData.UserId;
            Identity = playerData.Identity;
            isLocalPlayer = isLocal;

            if (isLocalPlayer)
            {
                characterActor.Teleport(
                    ToVector3(playerData.LastPosition),
                    Quaternion.Euler(0, playerData.LastRotation, 0)
                );
            }
            else
            {
                transform.position = ToVector3(playerData.LastPosition);
                transform.rotation = Quaternion.Euler(0, playerData.LastRotation, 0);
                animController.SetPlayerState(playerData.CurrentAnimationState);
            }

            lastSyncedState = playerData.CurrentAnimationState;

            lastSyncedPosition = transform.position;
            lastSyncedRotation = transform.rotation;

            SetColor(playerData.Color);
            SetName(playerData.Username);
            RoomJoinTime = playerData.LastRoomJoinTime;
        }

        public void UpdatePlayer(OnlinePlayer updatedData)
        {
            // Add new state to buffer
            stateBuffer.Enqueue(new State
            {
                position = ToVector3(updatedData.LastPosition),
                rotation = Quaternion.Euler(0, updatedData.LastRotation, 0),
                timestamp = Time.time,
                state = updatedData.CurrentAnimationState
            });

            // Remove old states
            while (stateBuffer.Count > 2 && stateBuffer.Peek().timestamp < Time.time - 1f)
                stateBuffer.Dequeue();
        }

        private void SetColor(string hexColor)
        {
            // if hex collor do not start with '#', prepend it
            if (!hexColor.StartsWith("#"))
            {
                hexColor = "#" + hexColor;
            }

            if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
            {
                PlayerColor = color;
            }
            else
            {
                Debug.LogError($"Failed to parse color: {hexColor}");
                PlayerColor = Color.gray;
            }

            if (meshRenderer == null) return;
            meshRenderer.material.color = PlayerColor;
        }

        private void SetName(string playerName)
        {
            PlayerName = playerName;
        }

        private Vector3 ToVector3(DbVector3 dbVec)
        {
            return new Vector3(dbVec.X, dbVec.Y, dbVec.Z);
        }
    }
}