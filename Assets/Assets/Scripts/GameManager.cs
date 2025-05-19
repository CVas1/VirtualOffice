using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.VoiceRecorder;
using UnityEngine;
using SpacetimeDB;
using SpacetimeDB.Types;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        const string SERVER_URL = "http://127.0.0.1:3000";
        const string MODULE_NAME = "stdboffice";

        public static uint CurrentRoomId { get; private set; } = 0;
        private SubscriptionHandle currentRoomSub;
        private SubscriptionHandle currentChatSub;
        private SubscriptionHandle currentVoiceSub;

        public static GameManager Instance { get; private set; }
        public static Identity LocalIdentity { get; private set; }
        public static DbConnection Conn { get; private set; }

        public Dictionary<Identity, PlayerController> Players = new Dictionary<Identity, PlayerController>();
        public List<GameRoom> Rooms = new List<GameRoom>();

        public (string, string, float)? RoomToJoin = null;

        public static event Action OnConnected;
        public static event Action OnDisconnected;
        // public static event Action OnSubscriptionApplied;

        [SerializeField] private GameObject playerPrefab;

        private void Start()
        {
            // delete player pref
            // PlayerPrefs.DeleteKey("spacetimedb.identity_token");
            // var key = $"spacetimedb.identity_token - {Application.dataPath}";
            // PlayerPrefs.DeleteKey(key);

            Instance = this;

            DbConnectionBuilder<DbConnection> builder = DbConnection.Builder()
                .WithUri(SERVER_URL)
                .WithModuleName(MODULE_NAME)
                .OnConnect(OnConnect)
                .OnDisconnect(OnDisconnect)
                .OnConnectError(OnConnectError);

            // AuthToken.SaveToken("spacetimedb.identity_token" + Random.Range(1000, 9999));
            // Debug.Log(AuthToken.Token);
            // builder = builder.WithToken(Guid.NewGuid().ToString());
            if (!string.IsNullOrEmpty(AuthToken.Token))
            {
                builder = builder.WithToken(AuthToken.Token);
            }

            Conn = builder.Build();
            
            Application.targetFrameRate = 60;
        }

        void OnConnect(DbConnection conn, Identity identity, string token)
        {
            LocalIdentity = identity;
            AuthToken.SaveToken(token);

            Conn.Db.OnlinePlayer.OnDelete += OnOnlinePlayerDelete;
            Conn.Db.OnlinePlayer.OnUpdate += OnOnlinePlayerUpdate;
            Conn.Db.OnlinePlayer.OnInsert += OnOnlinePlayerInsert;
            Conn.Db.RoomEntity.OnInsert += OnBuildingInsert;
            Conn.Db.RoomEntity.OnUpdate += OnBuildingUpdate;
            Conn.Db.RoomEntity.OnDelete += OnBuildingDelete;
            Conn.Db.ChatMessage.OnInsert += OnChatMessageInsert;
            Conn.Db.VoiceClip.OnInsert += OnVoiceClipInsert;
            Conn.Db.VoiceClip.OnUpdate += OnVoiceClipUpdate;

            // Conn.Db.ChatMessage.OnUpdate += OnChatMessageUpdate;
            // Conn.Db.ChatMessage.OnDelete += OnChatMessageDelete;

            conn.Db.GameRoom.OnInsert += OnGameRoomInsert;
            conn.Db.GameRoom.OnDelete += OnGameRoomDelete;

            conn.Reducers.OnJoinRoom += JoinRoomResult;
            conn.Reducers.OnLeaveRoom += LeaveRoomResult;

            OnConnected?.Invoke();

            Conn.SubscriptionBuilder()
                // .OnApplied(OnSubscriptionAppliedHandler)
                .Subscribe(new[]
                {
                    "SELECT * FROM game_room",
                    "SELECT * FROM player_count"
                });
        }

        void OnDisconnect(DbConnection conn, Exception ex)
        {
            Debug.Log("Disconnected from server.");
            if (ex != null)
                Debug.LogException(ex);
            OnDisconnected?.Invoke();
        }

        void OnConnectError(Exception ex)
        {
            Debug.LogError($"Connection failed: {ex}");
        }

        // void OnSubscriptionAppliedHandler(SubscriptionEventContext ctx)
        // {
        //     Debug.Log("Subscription fully applied.");
        //     OnSubscriptionApplied?.Invoke();
        //
        //     // Optional: auto join/create logic or show lobby
        // }

        // --------------------------------
        // Table Event Handlers
        // --------------------------------

        void OnGameRoomInsert(EventContext ctx, GameRoom room)
        {
            // Handle room insert
            //Debug.Log($"Room {room.RoomId} created.");
            Rooms.Add(room);

            if (RoomToJoin == null || room.Name != RoomToJoin.Value.Item1) return;
            if (Time.time > RoomToJoin.Value.Item3)
            {
                RoomToJoin = null;
                return;
            }

            // UIManager.Instance.OnJoinRoom();
            UIManager.Instance.OnClickJoinRoomMenu(room);
            JoinRoom(room.RoomId, RoomToJoin.Value.Item2);
            RoomToJoin = null;
        }

        void OnGameRoomDelete(EventContext ctx, GameRoom room)
        {
            // Handle room delete
            // Debug.Log($"Room {room.RoomId} deleted.");
            Rooms.Remove(room);
        }

        void OnOnlinePlayerInsert(EventContext ctx, OnlinePlayer player)
        {
            // Debug.Log($"Player {player.PlayerId} created.");
            if (player.RoomId != CurrentRoomId) return; // Only spawn players in the same room
            
            //disable camera if exist
            if (Camera.main != null)
            {
                Camera.main.gameObject.SetActive(false);
            }
            
            PlayerController controller = Instantiate(playerPrefab).GetComponent<PlayerController>();
            Debug.Log($"Player {player.PlayerId} created in room {player.RoomId}.");
            controller.transform.position =
                new Vector3(player.LastPosition.X, player.LastPosition.Y, player.LastPosition.Z);
            bool isLocal = player.Identity.Equals(LocalIdentity);

            if (isLocal)
            {
                SubscribeToChat(player.RoomId, player.LastRoomJoinTime);
                SubscribeToVoice(player.RoomId, player.LastRoomJoinTime);
            }

            controller.Init(player, isLocal);
            Players[player.Identity] = controller;
        }

        private void SubscribeToChat(uint roomId, ulong timestamp)
        {
            // unsubscribe old
            if (currentChatSub != null && currentChatSub.IsActive)
                currentChatSub.Unsubscribe();

            // only get new messages
            string sql = $"SELECT * FROM chat_message WHERE room_id = {roomId} AND timestamp > {timestamp}";
            currentChatSub = Conn.SubscriptionBuilder()
                .Subscribe(new[] { sql });
        }

        private void SubscribeToVoice(uint roomId, ulong timestamp)
        {
            // unsubscribe old
            if (currentVoiceSub != null && currentVoiceSub.IsActive)
                currentVoiceSub.Unsubscribe();

            // only get new clips
            string sql = $"SELECT * FROM voice_clip WHERE room_id = {roomId} AND timestamp > {timestamp}";
            currentVoiceSub = Conn.SubscriptionBuilder()
                .Subscribe(new[] { sql });
        }

        void OnOnlinePlayerDelete(EventContext ctx, OnlinePlayer player)
        {
            Debug.Log($"Player {player.PlayerId} deleted.");
            if (Players.TryGetValue(player.Identity, out PlayerController controller))
            {
                Destroy(controller.gameObject);
                Players.Remove(player.Identity);
                
            }

            if (player.Identity.Equals(LocalIdentity))
            {
                // Unsubscribe from the chat messages
                if (currentChatSub != null && currentChatSub.IsActive)
                {
                    currentChatSub.Unsubscribe();
                    currentChatSub = null;
                }
            }
            // setactive camera
            if (Camera.main != null)
            {
                Camera.main.gameObject.SetActive(true);
            }
        }

        void OnOnlinePlayerUpdate(EventContext ctx, OnlinePlayer oldData, OnlinePlayer newData)
        {
            Debug.Log($"Player {newData.PlayerId} updated.");
            if (Players.TryGetValue(newData.Identity, out PlayerController controller))
            {
                controller.UpdatePlayer(newData);
            }
            else
            {
                if (newData.RoomId == CurrentRoomId)
                {
                    OnOnlinePlayerInsert(ctx, newData);
                }
            }
        }

        void OnBuildingInsert(EventContext ctx, RoomEntity roomEntity)
        {
            RoomBuildingManager.Instance.Load(roomEntity.Data);
        }

        void OnBuildingUpdate(EventContext ctx, RoomEntity roomEntityOld, RoomEntity roomEntityNew)
        {
            if (roomEntityNew.Identity == LocalIdentity) return; // Ignore updates from the local player
            RoomBuildingManager.Instance.Load(roomEntityNew.Data);
        }

        void OnBuildingDelete(EventContext ctx, RoomEntity roomEntity)
        {
            RoomBuildingManager.Instance.DeleteAll();
        }

        void OnChatMessageInsert(EventContext ctx, ChatMessage message)
        {
            // Debug.Log($"Chat message: {message.Content} + timestamp: {message.Timestamp}");
            if (Players.TryGetValue(message.Sender, out PlayerController player))
            {
                string username = player.PlayerName;
                Color color = player.PlayerColor;
                string messageText = message.Content;
                ChatManager.Instance.SendChatMessage(username, messageText, color);
            }
        }

        void OnVoiceClipInsert(EventContext ctx, VoiceClip clip)
        {
            if (clip.Sender.Equals(LocalIdentity)) return;
            if (Players.TryGetValue(clip.Sender, out PlayerController player))
            {
                VoiceChatPlayer.Instance.EnqueueAudio(clip.AudioData.ToArray(), player.PlayerId);
            }
        }

        void OnVoiceClipUpdate(EventContext ctx, VoiceClip clipOld, VoiceClip clipNew)
        {
            OnVoiceClipInsert(ctx, clipNew);
        }

        // --------------------------------
        // Public Reducer Invocations
        // --------------------------------

        public void SetPlayerProfile(string playerName, string color)
        {
            Conn.Reducers.SetPlayerProfile(playerName, color);
        }

        public void JoinRoom(uint roomId, string password)
        {
            Conn.Reducers.JoinRoom(roomId, password);
        }

        private void JoinRoomResult(ReducerEventContext ctx, uint roomId, string password)
        {
            if (ctx.Event.Status is Status.Failed failedStatus)
            {
                if (ctx.Event.CallerIdentity == ctx.Identity)
                {
                    // Get the value "Incorrect password" from the string "Failed("System.Exception: Incorrect password\n...."
                    string errorMessage = failedStatus.ToString();
                    int startIndex = errorMessage.IndexOf(":") + 1;
                    int endIndex = errorMessage.IndexOf("\\n");
                    string extractedMessage = errorMessage.Substring(startIndex, endIndex - startIndex).Trim();

                    if (string.IsNullOrWhiteSpace(extractedMessage))
                    {
                        UIManager.Instance.JoinRoomError(errorMessage.Substring(0, 30) + "...");
                    }
                    else
                    {
                        UIManager.Instance.JoinRoomError(extractedMessage);
                    }
                }
            }
            else
            {
                Debug.Log($"Joined room {roomId}");
                CurrentRoomId = roomId;
                UIManager.Instance.OnCloseMenu();
                RoomBuildingManager.Instance.OnRoomJoin();
                ChatManager.Instance.ClearChat();

                // Re-subscribe to only the players/entities in this room
                var newCurrentRoomSub = Conn.SubscriptionBuilder()
                    // .OnApplied((ctx) => { })
                    // .OnError((ctx, ex) => Debug.LogError($"Room subscription error: {ex}"))
                    .Subscribe(new string[]
                    {
                        //write the sql query to get the room id & player id is not mine
                        "SELECT * FROM online_player WHERE room_id = " + roomId,
                        "SELECT * FROM room_entity WHERE room_id = " + roomId,
                        // "SELECT * FROM chat_message WHERE room_id = " + roomId + " AND timestamp > " +
                        // (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    });
                if (currentRoomSub != null && currentRoomSub.IsActive)
                {
                    // Unsubscribe from the previous room subscription
                    currentRoomSub.Unsubscribe();
                }

                currentRoomSub = newCurrentRoomSub;
            }
        }

        public void LeaveRoom()
        {
            Conn.Reducers.LeaveRoom();
        }

        private void LeaveRoomResult(ReducerEventContext ctx)
        {
            if (ctx.Event.CallerIdentity != LocalIdentity) return;
            if (ctx.Event.Status is Status.Failed failedStatus)
            {
                Debug.Log($"Failed to leave room {CurrentRoomId}: {failedStatus}");
            }
            else
            {
                Debug.Log($"Left room {CurrentRoomId}");
                if (currentRoomSub != null && currentRoomSub.IsActive)
                {
                    currentRoomSub.Unsubscribe();
                    currentRoomSub = null;
                }

                CurrentRoomId = 0;

                // Clear all players
                foreach (var player in Players.Values)
                {
                    Destroy(player.gameObject);
                }

                Players.Clear();
                RoomBuildingManager.Instance.OnRoomLeave();
            }
        }

        public void CreateRoom(string roomName, string password)
        {
            Conn.Reducers.CreateRoom(roomName, password);
        }

        public void UpdatePlayerPosition(Vector3 pos, float rot)
        {
            Conn.Reducers.UpdateLastPosition(new DbVector3(pos.x, pos.y, pos.z), rot);
        }

        public void SaveBuildingData(string data)
        {
            Conn.Reducers.SaveEntity(CurrentRoomId, data);
        }

        public void SendChatMessage(string message, bool shout = false)
        {
            Conn.Reducers.SendMessage(message, shout);
        }

        public void SendVoiceClip(byte[] audioData)
        {
            Conn.Reducers.SendVoice(audioData.ToList());
        }

        public uint GetOnlinePlayerCount()
        {
            PlayerCount playerCount = Conn.Db.PlayerCount.Id.Find(0);
            return playerCount?.Count ?? 0;
        }

        public static bool IsConnected() => Conn != null && Conn.IsActive;
    }
}