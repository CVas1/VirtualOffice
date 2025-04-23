using System;
using UnityEngine;
using TankAndHealerStudioAssets;

namespace Assets.Scripts
{
    public class ChatManager : MonoBehaviour
    {
        [SerializeField] private UltimateChatBox chatBox;

        public static ChatManager Instance;

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
        }

        private void Start()
        {
            chatBox.OnInputFieldSubmitted += OnChatInputSubmitted;
        }

        public void ClearChat()
        {
            chatBox.ClearChat();
        }

        private void OnChatInputSubmitted(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            // if first character is a ! then it is shout
            // if first character is a / then it is command

            char firstChar = message[0];
            if (firstChar == '!')
            {
                // Handle shout command
                message = message.Substring(1);
                GameManager.Instance.SendChatMessage(message, true);
            }
            else if (firstChar == '/')
            {
                // Handle command
                // message = message.Substring(1);
                // GameManager.Instance.SendCommandMessage(message);
            }
            else
            {
                GameManager.Instance.SendChatMessage(message);
            }
        }

        public void SendChatMessage(string username, string message, Color color)
        {
            // usernameBold = false;
            // usernameItalic = false;
            // usernameUnderlined = false;
            // Color usernameColor = Color.clear;
            // disableInteraction = false;
            // noUsernameFollowupText = false;
            // messageBold = false;
            // messageItalic = false;
            // messageUnderlined = false;
            // Color messageColor = Color.clear;

            UltimateChatBox.ChatStyle style = new UltimateChatBox.ChatStyle
            {
                usernameColor = color
            };

            chatBox.RegisterChat(username, message, style);
        }
    }
}