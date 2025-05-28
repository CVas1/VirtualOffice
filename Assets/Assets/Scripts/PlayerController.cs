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

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        public bool isLocalPlayer = false;

        public uint PlayerId { get; private set; }
        public Identity Identity { get; private set; }

        public string PlayerName { get; private set; }
        public Color PlayerColor { get; private set; }
        public ulong RoomJoinTime { get; private set; }

        private float lastUpdateTime = 0f;
        private const float updateInterval = 0.2f;

        public float interpolationSpeedForPosition = 10f;
        public float interpolationSpeedForRotation = 10f;

        private class State
        {
            public Vector3 position;
            public Quaternion rotation;
            public float timestamp;
        }

        private Queue<State> stateBuffer = new Queue<State>();
        private float interpolationBackTime = 0.4f; // 400 ms

        CharacterActor characterActor;
        SpawnPoint sp;

        private void Start()
        {
            // meshRenderer = GetComponent<MeshRenderer>();
            sp = FindAnyObjectByType<SpawnPoint>();
            characterActor = FindAnyObjectByType<CharacterActor>();
        }

        void Update()
        {
            TeleportIfBelowZero();
            if (isLocalPlayer && Time.time - lastUpdateTime >= updateInterval)
            {
                if (Vector3.Distance(transform.position, targetPosition) > 0.3f ||
                    Quaternion.Angle(transform.rotation, targetRotation) > 6f)
                {
                    float yaw = transform.rotation.eulerAngles.y;
                    STDBBackendManager.Instance.playerManager.UpdatePlayerPosition(transform.position, yaw);
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

            transform.position = ToVector3(playerData.LastPosition);
            targetPosition = transform.position;

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
                timestamp = Time.time
            });

            // Remove old states
            while (stateBuffer.Count > 2 && stateBuffer.Peek().timestamp < Time.time - 1f)
                stateBuffer.Dequeue();

            SetColor(updatedData.Color);
            SetName(updatedData.Username);
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