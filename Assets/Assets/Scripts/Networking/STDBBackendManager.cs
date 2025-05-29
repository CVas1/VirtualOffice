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

        // public Camera mainCamera;

        public static event Action OnConnected;
        public static event Action OnDisconnected;

        private STDBConnectionManager stdbConnectionManager;
        public STDBAuthManager authManager;
        public STDBRoomManager roomManager;
        public STDBPlayerManager playerManager;
        public STDBVoiceManager voiceManager;
        public STDBChatManager chatManager;
        public STDBImageManager imageManager;
        public STDBBuildingManager buildingManager;
        public RoomStatsManager roomStatsManager;

        private static Timestamp cachedServerTimestamp;
        private static DateTime cachedLocalTime;

        private void Start()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            stdbConnectionManager = new STDBConnectionManager();
            authManager = new STDBAuthManager();
            roomManager = new STDBRoomManager();
            playerManager = new STDBPlayerManager();
            voiceManager = new STDBVoiceManager();
            chatManager = new STDBChatManager();
            imageManager = new STDBImageManager();
            buildingManager = new STDBBuildingManager();
            roomStatsManager = new RoomStatsManager();

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

            // Initialize all managers
            authManager.Init(conn);
            roomManager.Init(conn);
            playerManager.Init(conn, localPlayerPrefab, remotePlayerPrefab);
            voiceManager.Init(conn);
            chatManager.Init(conn);
            imageManager.Init(conn);
            buildingManager.Init(conn);
            roomStatsManager.Init(conn);

            OnConnected?.Invoke();

            Conn.SubscriptionBuilder()
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

        // Call this when you receive the server time
        public static void CacheServerTime(Timestamp serverTimestamp)
        {
            cachedServerTimestamp = serverTimestamp;
            cachedLocalTime = DateTime.UtcNow;
        }

        // Call this to get the estimated current server time
        public static Timestamp GetCurrentServerTimestamp()
        {
            TimeSpan elapsed = DateTime.UtcNow - cachedLocalTime;
            long elapsedMicroseconds = ((long)(elapsed.TotalMilliseconds)) * 1000L;
            return new Timestamp(cachedServerTimestamp.MicrosecondsSinceUnixEpoch + elapsedMicroseconds);
        }

        public static bool IsConnected() => Conn != null && Conn.IsActive;
        public static bool IsAuthenticated() => STDBAuthManager.IsLoggedIn;
    }
}