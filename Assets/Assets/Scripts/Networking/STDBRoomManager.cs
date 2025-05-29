using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.VoiceRecorder;
using UnityEngine;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine.Serialization;

namespace Assets.Scripts.Networking
{
    public class STDBRoomManager
    {
        public static uint CurrentRoomId { get; private set; } = 0;

        public static event Action OnRoomJoin;
        public static event Action OnRoomLeave;

        public static event Action<string> ErrorMessageEvent;

        private DbConnection conn;
        private SubscriptionHandle playerSub;
        private SubscriptionHandle roomSub;
        private SubscriptionHandle chatSub;
        private SubscriptionHandle voiceSub;
        private SubscriptionHandle imageSub;
        private SubscriptionHandle statsSub;

        private Timestamp joinTimestamp;

        public void Init(DbConnection connection)
        {
            conn = connection;
            // conn.Db.GameRoom.OnInsert += OnRoomInsert;
            // conn.Db.GameRoom.OnDelete += OnRoomDelete;
            conn.Reducers.OnJoinRoom += OnJoinRoomCallback;
            conn.Reducers.OnCreateRoom += OnCreateRoomCallback;
            conn.Reducers.OnLeaveRoom += OnLeaveRoomCallback;
        }


        private void OnJoinRoomCallback(ReducerEventContext ctx, string roomName, string password)
        {
            STDBBackendManager.CacheServerTime(ctx.Event.Timestamp);
            if (ctx.Event.CallerIdentity != STDBBackendManager.LocalIdentity)
            {
                Debug.LogWarning("Received join room event from another user, ignoring.");
                return;
            }

            if (ctx.Event.Status is Status.Failed fail)
            {
                string message = $"Failed to join room: {ExtractErrorMessage(fail)}";
                Debug.LogError(message);
                ErrorMessageEvent?.Invoke(message);
            }
            else
            {
                joinTimestamp = ctx.Event.Timestamp;
                // CurrentRoomId = roomId;

                // UIManager.Instance.OnCloseMenu();
                // RoomBuildingManager.Instance.OnRoomJoin();
                // ChatManager.Instance.ClearChat();
                // OnRoomJoin?.Invoke();

                // UnsubscribeFromAll();
                // SubscribeToAll(ctx.Event.Timestamp);
            }
        }

        public void OnJoinRoom(uint roomId)
        {
            // if (CurrentRoomId != 0)
            // {
            //     Debug.LogWarning($"Already in room {CurrentRoomId}, leaving before joining {roomId}");
            //     LeaveRoom();
            // }

            CurrentRoomId = roomId;

            // load "world" scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("World");

            // UnsubscribeFromAll();
            // SubscribeToAll(joinTimestamp);
            // OnRoomJoin?.Invoke();
            //
            // STDBBackendManager.Instance.playerManager.SpawnLocalPlayer();
        }

        public void InitilizeAfterWorldLoad()
        {
            UnsubscribeFromAll();
            SubscribeToAll(joinTimestamp);
            OnRoomJoin?.Invoke();

            STDBBackendManager.Instance.playerManager.SpawnLocalPlayer();
        }

        private void OnCreateRoomCallback(ReducerEventContext ctx, string roomName, string password)
        {
            STDBBackendManager.CacheServerTime(ctx.Event.Timestamp);
            if (ctx.Event.CallerIdentity != STDBBackendManager.LocalIdentity)
            {
                Debug.LogWarning("Received create room event from another user, ignoring.");
                return;
            }

            if (ctx.Event.Status is Status.Failed fail)
            {
                string message = $"Failed to create room: {ExtractErrorMessage(fail)}";
                Debug.LogError(message);
                ErrorMessageEvent?.Invoke(message);
            }
            else
            {
                JoinRoom(roomName, password);
            }
        }

        private void OnLeaveRoomCallback(ReducerEventContext ctx)
        {
            STDBBackendManager.CacheServerTime(ctx.Event.Timestamp);
            if (ctx.Event.CallerIdentity != STDBBackendManager.LocalIdentity)
            {
                Debug.LogWarning("Received join room event from another user, ignoring.");
                return;
            }

            if (ctx.Event.Status is Status.Failed fail)
            {
                string message = $"Failed to leave room: {ExtractErrorMessage(fail)}";
                Debug.LogError(message);
                ErrorMessageEvent?.Invoke(message);
            }
            else
            {
                UnsubscribeFromAll();
                CurrentRoomId = 0;
                OnRoomLeave?.Invoke();

                // load "main menu" scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }


        public void JoinRoom(string roomName, string password)
        {
            if (!STDBBackendManager.IsAuthenticated())
            {
                Debug.LogError("Must be logged in to join a room");
                return;
            }

            conn.Reducers.JoinRoom(roomName, password);
        }

        public void CreateRoom(string name, string password)
        {
            if (!STDBBackendManager.IsAuthenticated())
            {
                Debug.LogError("Must be logged in to create a room");
                return;
            }

            conn.Reducers.CreateRoom(name, password);
        }

        public void LeaveRoom()
        {
            if (!STDBBackendManager.IsAuthenticated())
            {
                Debug.LogError("Must be logged in to leave a room");
                return;
            }

            conn.Reducers.LeaveRoom();
        }

        private string ExtractErrorMessage(Status.Failed failed)
        {
            string msg = failed.ToString();
            int start = msg.IndexOf(":") + 1;
            int end = msg.IndexOf("\\n");
            return (start > 0 && end > start) ? msg.Substring(start, end - start).Trim() : msg;
        }

        private void SubscribeToPlayers(uint roomId)
        {
            string sql = $"SELECT * FROM online_player WHERE room_id = {roomId}";
            playerSub = conn.SubscriptionBuilder()
                .Subscribe(new[] { sql });
        }

        private void SubscribeToRoom(uint roomId)
        {
            string sql = $"SELECT * FROM room_entity WHERE room_id = {roomId}";
            roomSub = conn.SubscriptionBuilder()
                .Subscribe(new[] { sql });
        }

        private void SubscribeToChat(uint roomId, ulong timestamp)
        {
            string sql =
                $"SELECT * FROM chat_message WHERE room_id = {roomId} AND timestamp > {timestamp}";
            chatSub = conn.SubscriptionBuilder()
                .Subscribe(new[] { sql });
        }

        private void SubscribeToVoice(uint roomId, ulong timestamp)
        {
            string sql =
                $"SELECT * FROM voice_clip WHERE room_id = {roomId} AND timestamp > {timestamp} AND sender_user_id != {STDBAuthManager.LocalUserId}";

            voiceSub = conn.SubscriptionBuilder()
                .Subscribe(new[] { sql });
        }

        private void SubscribeToImages(uint roomId, ulong timestamp)
        {
            string sqlImage =
                $"SELECT * FROM images WHERE room_id = {roomId} AND ( timestamp < {timestamp} OR sender_user_id != {STDBAuthManager.LocalUserId} )";

            string sqlBroadcastLock =
                $"SELECT * FROM image_broadcast_lock WHERE sender_user_id = {STDBAuthManager.LocalUserId}";

            imageSub = conn.SubscriptionBuilder()
                .Subscribe(new[] { sqlImage, sqlBroadcastLock });
        }

        private void SubscribeToStats(uint roomId)
        {
            string sql = $"SELECT * FROM room_session_history WHERE room_id = {roomId}";
            statsSub = conn.SubscriptionBuilder()
                .Subscribe(new[] { sql });
        }

        private void SubscribeToAll(Timestamp timestamp)
        {
            if (CurrentRoomId == 0) return;

            //convert timestamp to ulong
            ulong ts = (ulong)timestamp.MicrosecondsSinceUnixEpoch;

            SubscribeToPlayers(CurrentRoomId);
            SubscribeToRoom(CurrentRoomId);
            SubscribeToChat(CurrentRoomId, ts);
            SubscribeToVoice(CurrentRoomId, ts);
            SubscribeToImages(CurrentRoomId, ts);
            SubscribeToStats(CurrentRoomId);
        }

        private void UnsubscribeFromAll()
        {
            if (playerSub != null && playerSub.IsActive)
                playerSub?.Unsubscribe();

            if (roomSub != null && roomSub.IsActive)
                roomSub?.Unsubscribe();

            if (chatSub != null && chatSub.IsActive)
                chatSub?.Unsubscribe();

            if (voiceSub != null && voiceSub.IsActive)
                voiceSub?.Unsubscribe();

            if (imageSub != null && imageSub.IsActive)
                imageSub?.Unsubscribe();

            if (statsSub != null && statsSub.IsActive)
                statsSub?.Unsubscribe();
        }
    }
}