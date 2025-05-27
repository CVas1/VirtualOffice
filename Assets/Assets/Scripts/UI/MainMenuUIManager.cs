using UnityEngine;
using TMPro;
using Assets.Scripts.Networking;

namespace Assets.Scripts.UI
{
    public class MainMenuUIManager : MonoBehaviour
    {
        public TMP_InputField usernameInput;
        public ColorPicker colorPicker;

        private void Start()
        {
            if (STDBPlayerManager.localPlayer != null)
            {
                usernameInput.text = STDBPlayerManager.localPlayer.Username;
                Debug.Log(STDBPlayerManager.localPlayer.Color);
                string colorString = STDBPlayerManager.localPlayer.Color;
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
                                     STDBPlayerManager.localPlayer.Color);
                    colorPicker.color = Color.white; // Default color if parsing fails
                }
            }
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

            if (string.IsNullOrWhiteSpace(newUsername) || newUsername == STDBPlayerManager.localPlayer.Username) return;
            // Update backend
            STDBBackendManager.Conn.Reducers.SetPlayerUsername(newUsername);
        }

        public void OnColorChanged()
        {
            Color newColor = colorPicker.color;
            string colorString = ColorUtility.ToHtmlStringRGB(newColor);
            Debug.Log(" OnColorChanged" + colorString);

            if (colorString == STDBPlayerManager.localPlayer.Color) return; // No change

            // Update backend
            STDBBackendManager.Conn.Reducers.SetPlayerColor(colorString);
        }
    }
}