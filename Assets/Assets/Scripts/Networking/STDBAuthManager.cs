using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class STDBAuthManager
    {
        public const string LastUsedEmailKey = "LastUsedEmail";

        private DbConnection conn;
        public static uint LocalUserId { get; private set; } = 0;
        public static OnlinePlayer LocalPlayer;
        public static bool IsLoggedIn => LocalUserId > 0;

        public static event Action<bool> OnAuthenticationStateChanged;
        public static event Action<string> OnAuthenticationError;

        private SubscriptionHandle getUserIdSubscription;

        public void Init(DbConnection connection)
        {
            conn = connection;

            // Register event handlers for authentication reducers
            conn.Reducers.OnRegister += OnRegister;
            conn.Reducers.OnLogin += OnLogin;
            conn.Reducers.OnLogout += OnLogout;
            conn.Reducers.OnCreateOnlinePlayer += OnCreateOnlinePlayer;

            conn.Db.OnlinePlayer.OnInsert += (ctx, player) =>
            {
                if (!player.Identity.Equals(STDBBackendManager.LocalIdentity)) return;

                LocalUserId = player.UserId;
                LocalPlayer = player;
                OnAuthenticationStateChanged?.Invoke(true);

                // STDBRoomManager.OnRoomJoin += () =>
                // {
                //     if (getUserIdSubscription != null && getUserIdSubscription.IsActive)
                //         getUserIdSubscription.Unsubscribe();
                // };
            };

            conn.Db.OnlinePlayer.OnUpdate += (ctx, oldPlayer, newPlayer) =>
            {
                if (!newPlayer.Identity.Equals(STDBBackendManager.LocalIdentity)) return;
                LocalPlayer = newPlayer;

                if (newPlayer.RoomId != STDBRoomManager.CurrentRoomId)
                {
                    if (newPlayer.RoomId != uint.MaxValue)
                        STDBBackendManager.Instance.roomManager.OnJoinRoom(newPlayer.RoomId);
                }
            };
        }

        public void Register(string email, string username, string password)
        {
            if (conn != null && conn.IsActive)
            {
                conn.Reducers.Register(email, username, password);
            }
            else
            {
                OnAuthenticationError?.Invoke("Not connected to server");
            }
        }

        public void Login(string mail, string password)
        {
            if (conn != null && conn.IsActive)
            {
                conn.Reducers.Login(mail, password);
            }
            else
            {
                OnAuthenticationError?.Invoke("Not connected to server");
            }
        }

        public void Logout()
        {
            if (conn != null && conn.IsActive)
            {
                conn.Reducers.Logout();
            }
        }

        private void OnRegister(ReducerEventContext ctx, string mail, string username, string password)
        {
            if (ctx.Event.Status is Status.Failed fail)
            {
                string message = ExtractErrorMessage(fail);
                OnAuthenticationError?.Invoke($"Registration failed: {message}");
            }
            else
            {
                Debug.Log($"Registration successful for: {mail}");
                // After successful registration, automatically login
                Login(mail, password);
            }
        }

        private void OnLogin(ReducerEventContext ctx, string mail, string password)
        {
            if (ctx.Event.Status is Status.Failed fail)
            {
                string message = ExtractErrorMessage(fail);
                OnAuthenticationError?.Invoke($"Login failed: {message}");
            }
            else
            {
                Debug.Log($"Login successful for: {mail}");

                // Save the latest correct mail to PlayerPrefs
                PlayerPrefs.SetString(LastUsedEmailKey, mail);
                PlayerPrefs.Save();

                // subscribe to online player updates
                //$"SELECT * FROM image_broadcast_lock WHERE identity = '0x{STDBBackendManager.LocalIdentity}'";
                if (getUserIdSubscription != null && getUserIdSubscription.IsActive)
                    getUserIdSubscription.Unsubscribe();

                getUserIdSubscription = conn.SubscriptionBuilder()
                    .Subscribe(new[]
                    {
                        $"SELECT * FROM online_player WHERE identity = '0x{STDBBackendManager.LocalIdentity}'"
                    });

                // After successful login, create online player
                conn.Reducers.CreateOnlinePlayer();
            }
        }

        private void OnLogout(ReducerEventContext ctx)
        {
            if (ctx.Event.Status is Status.Failed fail)
            {
                string message = ExtractErrorMessage(fail);
                OnAuthenticationError?.Invoke($"Logout failed: {message}");
            }
            else
            {
                LocalUserId = 0;
                if (getUserIdSubscription != null && getUserIdSubscription.IsActive)
                    getUserIdSubscription.Unsubscribe();
                OnAuthenticationStateChanged?.Invoke(false);
                Debug.Log("Logged out successfully");
            }
        }

        private void OnCreateOnlinePlayer(ReducerEventContext ctx)
        {
            if (ctx.Event.Status is Status.Failed fail)
            {
                string message = ExtractErrorMessage(fail);
                OnAuthenticationError?.Invoke($"Failed to create online player: {message}");
            }
        }

        private string ExtractErrorMessage(Status.Failed failed)
        {
            string msg = failed.ToString();
            int start = msg.IndexOf(":") + 1;
            int end = msg.IndexOf("\\n");
            return (start > 0 && end > start) ? msg.Substring(start, end - start).Trim() : msg;
        }
    }
}