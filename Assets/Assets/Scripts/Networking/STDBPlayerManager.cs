using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class STDBPlayerManager
    {
        public Dictionary<Identity, PlayerController> Players = new Dictionary<Identity, PlayerController>();

        private DbConnection conn;
        private GameObject localPlayerPrefab;
        private GameObject remotePlayerPrefab;

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
            if (player.RoomId != STDBRoomManager.CurrentRoomId) return;

            // Disable main camera if it exists
            if (STDBBackendManager.Instance.mainCamera != null)
                STDBBackendManager.Instance.mainCamera.gameObject.SetActive(false);

            bool isLocal = player.Identity.Equals(STDBBackendManager.LocalIdentity);
            PlayerController controller;

            if (isLocal)
            {
                controller = Object.Instantiate(localPlayerPrefab).GetComponentInChildren<PlayerController>();
            }
            else
            {
                controller = Object.Instantiate(remotePlayerPrefab).GetComponent<PlayerController>();
            }

            Debug.Log($"Player {player.PlayerId} created in room {player.RoomId}.");
            controller.transform.position =
                new Vector3(player.LastPosition.X, player.LastPosition.Y, player.LastPosition.Z);

            controller.Init(player, isLocal);
            Players[player.Identity] = controller;
        }

        private void OnPlayerUpdate(EventContext ctx, OnlinePlayer oldData, OnlinePlayer newData)
        {
            if (newData.Identity.Equals(STDBBackendManager.LocalIdentity)) return;

            if (Players.TryGetValue(newData.Identity, out PlayerController controller))
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
            if (Players.TryGetValue(player.Identity, out PlayerController controller))
            {
                Object.Destroy(controller.gameObject);
                Players.Remove(player.Identity);
            }

            // Reactivate camera if local player
            if (player.Identity.Equals(STDBBackendManager.LocalIdentity) &&
                STDBBackendManager.Instance.mainCamera != null)
                STDBBackendManager.Instance.mainCamera.gameObject.SetActive(true);
        }

        public void SetPlayerProfile(string playerName, string color)
        {
            conn.Reducers.SetPlayerProfile(playerName, color);
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
    }
}