using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class STDBPlayerManager
    {
        public Dictionary<uint, PlayerController> Players = new Dictionary<uint, PlayerController>();

        private DbConnection conn;
        private GameObject localPlayerPrefab;
        private GameObject remotePlayerPrefab;

        public static OnlinePlayer localPlayer = null;

        public void Init(DbConnection connection, GameObject localPrefab, GameObject remotePrefab)
        {
            conn = connection;
            localPlayerPrefab = localPrefab;
            remotePlayerPrefab = remotePrefab;

            STDBRoomManager.OnRoomLeave += ClearAllPlayers;

            // Register event handlers
            conn.Db.OnlinePlayer.OnInsert += OnPlayerInsert;
            conn.Db.OnlinePlayer.OnUpdate += OnPlayerUpdate;
            conn.Db.OnlinePlayer.OnDelete += OnPlayerDelete;
        }

        private void OnPlayerInsert(EventContext ctx, OnlinePlayer player)
        {
            bool isLocal = player.UserId == STDBBackendManager.LocalUserId;
            if (isLocal) localPlayer = player;

            if (player.RoomId != STDBRoomManager.CurrentRoomId) return;

            // Disable main camera if it exists
            if (STDBBackendManager.Instance.mainCamera != null)
                STDBBackendManager.Instance.mainCamera.gameObject.SetActive(false);

            PlayerController controller;

            if (isLocal)
            {
                controller = Object.Instantiate(localPlayerPrefab).GetComponentInChildren<PlayerController>();
            }
            else
            {
                controller = Object.Instantiate(remotePlayerPrefab).GetComponent<PlayerController>();
            }

            Debug.Log($"Player {player.UserId} ({player.Username}) created in room {player.RoomId}.");
            controller.transform.position =
                new Vector3(player.LastPosition.X, player.LastPosition.Y, player.LastPosition.Z);

            controller.Init(player, isLocal);
            Players[player.UserId] = controller;
        }

        private void OnPlayerUpdate(EventContext ctx, OnlinePlayer oldData, OnlinePlayer newData)
        {
            bool isLocal = newData.UserId == STDBBackendManager.LocalUserId;
            if (isLocal) localPlayer = newData;

            if (newData.UserId == STDBBackendManager.LocalUserId) return;

            if (Players.TryGetValue(newData.UserId, out PlayerController controller))
            {
                controller.UpdatePlayer(newData);
            }
            else if (newData.RoomId == STDBRoomManager.CurrentRoomId)
            {
                OnPlayerInsert(ctx, newData);
            }
        }

        private void OnPlayerDelete(EventContext ctx, OnlinePlayer player)
        {
            bool isLocal = player.UserId == STDBBackendManager.LocalUserId;
            if (isLocal) localPlayer = player;

            if (Players.TryGetValue(player.UserId, out PlayerController controller))
            {
                if (controller.isLocalPlayer)
                {
                    Object.Destroy(controller.transform.parent.gameObject);
                }
                else
                {
                    // If not local, just destroy the controller's gameObject
                    Object.Destroy(controller.gameObject);
                }

                Players.Remove(player.UserId);
            }

            // Reactivate camera if local player
            if (player.UserId == STDBBackendManager.LocalUserId &&
                STDBBackendManager.Instance.mainCamera != null)
                STDBBackendManager.Instance.mainCamera.gameObject.SetActive(true);
        }

        public void SetPlayerProfile(string color)
        {
            // conn.Reducers.SetPlayerProfile(color);
        }

        public void UpdatePlayerPosition(Vector3 position, float rotation)
        {
            conn.Reducers.UpdateLastPosition(new DbVector3(position.x, position.y, position.z), rotation);
        }

        public void ClearAllPlayers()
        {
            foreach (var player in Players.Values)
                Object.Destroy(player.gameObject);

            Players.Clear();
        }

        public PlayerController GetLocalPlayer()
        {
            Players.TryGetValue(STDBBackendManager.LocalUserId, out PlayerController localPlayer);
            return localPlayer;
        }

        public PlayerController GetPlayerByUserId(uint userId)
        {
            Players.TryGetValue(userId, out PlayerController player);
            return player;
        }
    }
}