using System;
using SpacetimeDB.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private GameObject ShadowPanel;
        [SerializeField] private GameObject BackButton;
        [SerializeField] private GameObject QuitButton;

        [Header("Main Menu")] [SerializeField] private GameObject mainMenuPanel;

        [Header("Select Room")] [SerializeField]
        private GameObject selectRoomPanel;

        [SerializeField] private Transform scrollViewContent;
        [SerializeField] private GameObject selectRoomButtonPrefab;

        [Header("Join Room")] [SerializeField] private GameObject joinRoomPanel;
        [SerializeField] private TMP_Text joinRoomNameText;
        [SerializeField] private TMP_InputField joinRoomPasswordText;
        [SerializeField] private TMP_Text joinRoomErrorText;

        [Header("Create Room")] [SerializeField]
        private GameObject createRoomPanel;

        [SerializeField] private TMP_InputField createRoomNameInput;
        [SerializeField] private TMP_InputField createRoomPasswordInput;

        [Header("Connection Status")] [SerializeField]
        private Image connectionStatusImage;

        private GameRoom selectedRoom = null;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            GameManager.OnConnected += () => SetConnectionStatus(true);
            GameManager.OnDisconnected += () => SetConnectionStatus(false);

            OnClickBackToMainMenu();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                QuitRoom();
            }

            // Toggle cursor lock state with Tab
            // check if quit button is active

            if (QuitButton.activeSelf && Input.GetKeyDown(KeyCode.Tab))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    ToggleCursorLockState(true);
                }
                else
                {
                    ToggleCursorLockState(false);
                }
            }
        }

        private void ToggleCursorLockState(bool setCursorVisible)
        {
            if (Cursor.lockState == CursorLockMode.Locked && setCursorVisible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Cursor.lockState == CursorLockMode.None && !setCursorVisible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void OnJoinRoom()
        {
            if (selectedRoom == null) return;
            string password = joinRoomPasswordText.text;
            GameManager.Instance.JoinRoom(selectedRoom.RoomId, password);

            //OnCloseMenu();
        }

        public void JoinRoomError(string error)
        {
            joinRoomErrorText.gameObject.SetActive(true);
            joinRoomErrorText.text = error;
        }

        public void OnCreateRoom()
        {
            if (string.IsNullOrWhiteSpace(createRoomNameInput.text))
            {
                Debug.Log("Room name cannot be empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(createRoomPasswordInput.text))
            {
                Debug.Log("Room password cannot be empty.");
                return;
            }

            // set time current than 5 seconds later
            GameManager.Instance.RoomToJoin = (createRoomNameInput.text, createRoomPasswordInput.text, Time.time + 5f);

            string roomName = createRoomNameInput.text;
            string password = createRoomPasswordInput.text;
            GameManager.Instance.CreateRoom(roomName, password);

            // selectedRoom = null;
            // foreach (var room in GameManager.Instance.Rooms)
            // {
            //     if (room.Name == roomName)
            //     {
            //         selectedRoom = room;
            //         GameManager.Instance.JoinRoom(selectedRoom.RoomId, password);
            //         break;
            //     }
            // }
        }

        public void QuitRoom()
        {
            GameManager.Instance.LeaveRoom();
            ToggleCursorLockState(true);
            selectedRoom = null;
            OnClickBackToMainMenu();
        }

        public void OnClickBackToMainMenu()
        {
            QuitButton.SetActive(false);
            BackButton.SetActive(true);
            ShadowPanel.SetActive(true);
            mainMenuPanel.SetActive(true);
            selectRoomPanel.SetActive(false);
            joinRoomPanel.SetActive(false);
            joinRoomErrorText.gameObject.SetActive(false);
            createRoomPanel.SetActive(false);
        }

        public void OnCloseMenu()
        {
            QuitButton.SetActive(true);
            BackButton.SetActive(false);
            ShadowPanel.SetActive(false);
            mainMenuPanel.SetActive(false);
            selectRoomPanel.SetActive(false);
            joinRoomPanel.SetActive(false);
            joinRoomErrorText.gameObject.SetActive(false);
            createRoomPanel.SetActive(false);

            ToggleCursorLockState(false);
        }

        public void OnClickCreateRoomMenu()
        {
            mainMenuPanel.SetActive(false);
            selectRoomPanel.SetActive(false);
            createRoomPanel.SetActive(true);
            joinRoomPanel.SetActive(false);
        }

        public void OnClickListRoomMenu()
        {
            selectedRoom = null;

            mainMenuPanel.SetActive(false);
            selectRoomPanel.SetActive(true);
            createRoomPanel.SetActive(false);
            joinRoomPanel.SetActive(false);

            foreach (Transform child in scrollViewContent)
            {
                Destroy(child.gameObject);
            }

            int x = 0;
            foreach (var room in GameManager.Instance.Rooms)
            {
                x++;
                GameObject roomButton = Instantiate(selectRoomButtonPrefab, scrollViewContent);

                // set each room padding
                RectTransform rectTransform = roomButton.GetComponent<RectTransform>();
                rectTransform.position = new Vector3(480, 370 - (x * 100), 0);

                roomButton.GetComponent<RoomButton>().Init(room);
            }
        }

        public void OnClickJoinRoomMenu(GameRoom room)
        {
            selectedRoom = room;
            mainMenuPanel.SetActive(false);
            joinRoomPanel.SetActive(true);
            createRoomPanel.SetActive(false);
            selectRoomPanel.SetActive(false);
            joinRoomErrorText.gameObject.SetActive(false);
            joinRoomNameText.text = room.Name;
        }

        public void SetConnectionStatus(bool isConnected)
        {
            if (isConnected)
            {
                connectionStatusImage.color = Color.green;
            }
            else
            {
                connectionStatusImage.color = Color.red;
            }
        }
    }
}