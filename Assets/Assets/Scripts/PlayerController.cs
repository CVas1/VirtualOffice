using UnityEngine;
using TMPro;
using SpacetimeDB;
using SpacetimeDB.Types;

namespace Assets.Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float mouseSensitivity = 2f;
        public float gravity = -9.81f;

        [Header("References")]
        public Transform playerCamera;
        [SerializeField] private MeshRenderer meshRenderer;

        private CharacterController controller;
        private Vector3 velocity;
        private float xRotation = 0f;

        private Vector3 targetPosition;
        private bool isLocalPlayer = false;

        public uint PlayerId { get; private set; }
        public Identity Identity { get; private set; }

        private string playerName;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            // Handle local input
            if (isLocalPlayer)
            {
                HandleInput();
                LookAround();
            }
            else
            {
                // Smoothly move toward the target position
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            }

            // Toggle cursor lock state with Tab
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        public void Init(OnlinePlayer playerData, bool isLocal)
        {
            PlayerId = playerData.PlayerId;
            Identity = playerData.Identity;
            isLocalPlayer = isLocal;

            transform.position = ToVector3(playerData.LastPosition);
            targetPosition = transform.position;

            SetColor(playerData.Color);
            SetName(playerData.Name);

            if (isLocalPlayer)
            {
                playerCamera.gameObject.SetActive(true);
                gameObject.name = $"LocalPlayer({playerData.Name})";
            }else
            {
                playerCamera.gameObject.SetActive(false);
                gameObject.name = $"RemotePlayer({playerData.Name})";
            }
        }

        public void UpdatePlayer(OnlinePlayer updatedData)
        {
            targetPosition = ToVector3(updatedData.LastPosition);
            SetColor(updatedData.Color);
            SetName(updatedData.Name);
        }

        private void HandleInput()
        {
            // Movement input
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            controller.Move(move * moveSpeed * Time.deltaTime);

            // Apply gravity
            if (controller.isGrounded && velocity.y < 0)
                velocity.y = -2f; // small value to keep grounded

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            // Update position on the server
            if (move.sqrMagnitude > 0.01f)
            {
                Vector3 newPos = transform.position + move.normalized * (moveSpeed * Time.deltaTime);
                targetPosition = newPos;

                GameManager.Instance.UpdatePlayerPosition(newPos);
            }
        }

        private void LookAround()
        {
            // Mouse input for looking around
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        private void SetColor(string hexColor)
        {
            if (meshRenderer == null) return;
            if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
            {
                meshRenderer.material.color = color;
            }
            else
            {
                Debug.LogError($"Failed to parse color: {hexColor}");
                meshRenderer.material.color = Color.white;
            }
        }

        private void SetName(string playerName)
        {
            this.playerName = playerName;
        }

        private Vector3 ToVector3(DbVector3 dbVec)
        {
            return new Vector3(dbVec.X, dbVec.Y, dbVec.Z);
        }
    }
}