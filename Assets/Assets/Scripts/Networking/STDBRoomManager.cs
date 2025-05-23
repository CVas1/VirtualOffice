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
        public (string, string, float)? RoomToJoin = null;
        public List<GameRoom> Rooms = new List<GameRoom>();
        public static uint CurrentRoomId { get; private set; } = 0;
        public static event Action OnRoomJoin;
        public static event Action OnRoomLeave;

        private DbConnection conn;
        private SubscriptionHandle playerSub;
        private SubscriptionHandle roomSub;
        private SubscriptionHandle chatSub;
        private SubscriptionHandle voiceSub;
        private SubscriptionHandle imageSub;

        public void Init(DbConnection connection)
        {
            conn = connection;
            conn.Db.GameRoom.OnInsert += OnRoomInsert;
            conn.Db.GameRoom.OnDelete += OnRoomDelete;
            conn.Reducers.OnJoinRoom += OnJoinRoom;
            conn.Reducers.OnLeaveRoom += OnLeaveRoom;
        }

        public void JoinRoom(uint roomId, string password)
        {
            conn.Reducers.JoinRoom(roomId, password);
        }

        private void OnRoomInsert(EventContext ctx, GameRoom room)
        {
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

        private void OnRoomDelete(EventContext ctx, GameRoom room)
        {
            Rooms.Remove(room);
        }

        private void OnJoinRoom(ReducerEventContext ctx, uint roomId, string password)
        {
            if (ctx.Event.CallerIdentity != STDBBackendManager.LocalIdentity) return;

            if (ctx.Event.Status is Status.Failed fail)
            {
                string message = ExtractErrorMessage(fail);
                UIManager.Instance.JoinRoomError(message);
            }
            else
            {
                CurrentRoomId = roomId;
                UIManager.Instance.OnCloseMenu();
                RoomBuildingManager.Instance.OnRoomJoin();
                ChatManager.Instance.ClearChat();
                OnRoomJoin?.Invoke();

                UnsubscribeFromAll();

                SubscribeToAll(ctx.Event.Timestamp);
            }
        }

        private void OnLeaveRoom(ReducerEventContext ctx)
        {
            if (ctx.Event.CallerIdentity != STDBBackendManager.LocalIdentity) return;

            if (ctx.Event.Status is Status.Failed fail)
            {
                string message = ExtractErrorMessage(fail);
                Debug.LogError($"Failed to leave room({CurrentRoomId}): {message}");
            }
            else
            {
                UnsubscribeFromAll();

                CurrentRoomId = 0;


                // STDBBackendManager.Instance.playerManager.ClearAllPlayers();
                RoomBuildingManager.Instance.OnRoomLeave();
                OnRoomLeave?.Invoke();
            }
        }

        public void LeaveRoom() => conn.Reducers.LeaveRoom();
        public void CreateRoom(string name, string password) => conn.Reducers.CreateRoom(name, password);

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
                $"SELECT * FROM voice_clip WHERE room_id = {roomId} AND timestamp > {timestamp} AND sender != '0x{STDBBackendManager.LocalIdentity}'";
            voiceSub = conn.SubscriptionBuilder()
                .Subscribe(new[] { sql });
        }

        private void SubscribeToImages(uint roomId, ulong timestamp)
        {
            string sql =
                $"SELECT * FROM images WHERE room_id = {roomId} AND (timestamp < {timestamp} OR sender != '0x{STDBBackendManager.LocalIdentity}')";
            imageSub = conn.SubscriptionBuilder()
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
        }
    }
}