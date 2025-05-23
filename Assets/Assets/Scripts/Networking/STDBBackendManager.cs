using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class STDBBackendManager : MonoBehaviour
    {
        public static STDBBackendManager Instance { get; private set; }
        public static Identity LocalIdentity { get; private set; }
        public static DbConnection Conn { get; private set; }

        public GameObject localPlayerPrefab;
        public GameObject remotePlayerPrefab;

        public Camera mainCamera;

        public static event Action OnConnected;
        public static event Action OnDisconnected;

        private STDBConnectionManager stdbConnectionManager;
        public STDBRoomManager roomManager;
        public STDBPlayerManager playerManager;
        public STDBVoiceManager voiceManager;
        public STDBChatManager chatManager;
        public STDBImageManager imageManager;
        public STDBBuildingManager buildingManager;

        private void Start()
        {
            Instance = this;

            stdbConnectionManager = new STDBConnectionManager();
            roomManager = new STDBRoomManager();
            playerManager = new STDBPlayerManager();
            voiceManager = new STDBVoiceManager();
            chatManager = new STDBChatManager();
            imageManager = new STDBImageManager();
            buildingManager = new STDBBuildingManager();

            stdbConnectionManager.Connect(
                onConnect: OnConnect,
                onDisconnect: OnDisconnect,
                onError: ex => Debug.LogError($"Connection error: {ex}")
            );
        }

        void OnConnect(DbConnection conn, Identity identity, string token)
        {
            Conn = conn;
            LocalIdentity = identity;
            AuthToken.SaveToken(token);

            roomManager.Init(conn);
            playerManager.Init(conn, localPlayerPrefab, remotePlayerPrefab);
            voiceManager.Init(conn);
            chatManager.Init(conn);
            imageManager.Init(conn);
            buildingManager.Init(conn);

            OnConnected?.Invoke();

            Conn.SubscriptionBuilder()
                // .OnApplied(OnSubscriptionAppliedHandler)
                .Subscribe(new[]
                {
                    "SELECT * FROM game_room",
                    "SELECT * FROM player_count"
                });
        }

        void OnDisconnect()
        {
            Conn = null;
            Debug.Log("Disconnected.");

            OnDisconnected?.Invoke();
        }

        public static bool IsConnected() => Conn != null && Conn.IsActive;
    }
}