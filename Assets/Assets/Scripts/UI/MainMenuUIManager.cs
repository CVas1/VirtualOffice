using UnityEngine;
using TMPro;
using Assets.Scripts.Networking;

namespace Assets.Scripts.UI
{
    public class MainMenuUIManager : MonoBehaviour
    {
        public TMP_InputField usernameInput;
        public ColorPicker colorPicker;

        public TMP_InputField roomNameInput;
        public TMP_InputField roomPasswordInput;

        public TMP_Text errorText;

        private void Start()
        {
            if (STDBAuthManager.LocalPlayer != null)
            {
                usernameInput.text = STDBAuthManager.LocalPlayer.Username;
                Debug.Log(STDBAuthManager.LocalPlayer.Color);
                string colorString = STDBAuthManager.LocalPlayer.Color;
                if (!colorString.StartsWith("#"))
                    colorString = "#" + colorString;

                if (ColorUtility.TryParseHtmlString(colorString, out Color color))
                {
                    colorPicker.color = color;
                    Debug.Log("Parsed player color: " + color);
                }
                else
                {
                    Debug.LogWarning("Failed to parse player color from string: " +
                                     STDBAuthManager.LocalPlayer.Color);
                    colorPicker.color = Color.white; // Default color if parsing fails
                }
            }

            STDBRoomManager.ErrorMessageEvent += OnRoomError;
        }

        public void OnUsernameChanged()
        {
            string newUsername = usernameInput.text.Trim();
            // Validate input
            if (string.IsNullOrEmpty(newUsername) || newUsername.Length < 3 || newUsername.Length > 20)
            {
                Debug.LogWarning("Invalid username. Must be between 3 and 20 characters.");
                return;
            }

            if (string.IsNullOrWhiteSpace(newUsername) || newUsername == STDBAuthManager.LocalPlayer.Username) return;
            // Update backend
            STDBBackendManager.Conn.Reducers.SetPlayerUsername(newUsername);
        }

        public void OnColorChanged()
        {
            Color newColor = colorPicker.color;
            string colorString = ColorUtility.ToHtmlStringRGB(newColor);
            Debug.Log(" OnColorChanged" + colorString);

            if (colorString == STDBAuthManager.LocalPlayer.Color) return; // No change

            // Update backend
            STDBBackendManager.Conn.Reducers.SetPlayerColor(colorString);
        }

        public void JoinRoom()
        {
            string roomName = roomNameInput.text;
            string password = roomPasswordInput.text;
            STDBBackendManager.Instance.roomManager.JoinRoom(roomName, password);
        }

        public void CreateRoom()
        {
            string roomName = roomNameInput.text;
            string password = roomPasswordInput.text;

            if (string.IsNullOrWhiteSpace(roomName) || string.IsNullOrWhiteSpace(password))
            {
                Debug.LogError("Room name and password cannot be empty.");
                return;
            }

            STDBBackendManager.Instance.roomManager.CreateRoom(roomName, password);
        }

        private void OnRoomError(string error)
        {
            errorText.text = error;
        }
    }
}