using System;
using Assets.Scripts.Networking;
using UnityEngine;
using TMPro;
using SpacetimeDB;
using SpacetimeDB.Types;

namespace Assets.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        private MeshRenderer meshRenderer;

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool isLocalPlayer = false;

        public uint PlayerId { get; private set; }
        public Identity Identity { get; private set; }

        public string PlayerName { get; private set; }
        public Color PlayerColor { get; private set; }
        public ulong RoomJoinTime { get; private set; }

        private float lastUpdateTime = 0f;
        private const float updateInterval = 0.125f;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if (isLocalPlayer && Time.time - lastUpdateTime >= updateInterval)
            {
                if (Vector3.Distance(transform.position, targetPosition) > 0.3f ||
                    Quaternion.Angle(transform.rotation, targetRotation) > 6f)
                {
                    float yaw = transform.rotation.eulerAngles.y;
                    STDBBackendManager.Instance.playerManager.UpdatePlayerPosition(transform.position, yaw);
                }

                lastUpdateTime = Time.time;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
            }
        }


        // private void TeleportIfBelowZero()
        // {
        //     if (transform.position.y < -5f)
        //     {
        //         transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        //         rb.linearVelocity = Vector3.zero;
        //     }
        // }

        public void Init(OnlinePlayer playerData, bool isLocal)
        {
            PlayerId = playerData.PlayerId;
            Identity = playerData.Identity;
            isLocalPlayer = isLocal;

            transform.position = ToVector3(playerData.LastPosition);
            targetPosition = transform.position;

            SetColor(playerData.Color);
            SetName(playerData.Name);
            RoomJoinTime = playerData.LastRoomJoinTime;
        }

        public void UpdatePlayer(OnlinePlayer updatedData)
        {
            targetPosition = ToVector3(updatedData.LastPosition);
            targetRotation = Quaternion.Euler(0, updatedData.LastRotation, 0);
            SetColor(updatedData.Color);
            SetName(updatedData.Name);
        }

        private void SetColor(string hexColor)
        {
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