using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class STDBChatManager
    {
        private DbConnection conn;
        private SubscriptionHandle currentChatSub;

        public void Init(DbConnection connection)
        {
            conn = connection;
            conn.Db.ChatMessage.OnInsert += OnChatMessageInsert;
        }

        private void OnChatMessageInsert(EventContext ctx, ChatMessage message)
        {
            if (STDBBackendManager.Instance.playerManager.Players.TryGetValue(message.Sender,
                    out PlayerController player))
            {
                string username = player.PlayerName;
                Color color = player.PlayerColor;
                string messageText = message.Content;
                ChatManager.Instance.SendChatMessage(username, messageText, color);
            }
        }

        public void SendChatMessage(string message, bool shout = false)
        {
            conn.Reducers.SendMessage(message, shout);
        }
    }
}