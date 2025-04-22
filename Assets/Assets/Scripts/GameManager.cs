using System;
using System.Collections.Generic;
using UnityEngine;
using SpacetimeDB;
using SpacetimeDB.Types;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
public class GameManager : MonoBehaviour
{
    const string SERVER_URL = "http://127.0.0.1:3000";
    const string MODULE_NAME = "stdboffice";

    public static uint CurrentRoomId { get; private set; } = 0;
    private SubscriptionHandle currentRoomSub;
    
    public static GameManager Instance { get; private set; }
    public static Identity LocalIdentity { get; private set; }
    public static DbConnection Conn { get; private set; }

    public Dictionary<uint, PlayerController> Players = new Dictionary<uint, PlayerController>();
    public Dictionary<uint, GameObject> Entities = new Dictionary<uint, GameObject>();
    public List<GameRoom> Rooms = new List<GameRoom>();

    public static event Action OnConnected;
    public static event Action OnDisconnected;
    public static event Action OnSubscriptionApplied;
    
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
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

        Conn = builder.Build();
    }

    void OnConnect(DbConnection conn, Identity identity, string token)
    {
        LocalIdentity = identity;
        // AuthToken.SaveToken(token);

        conn.Db.GameRoom.OnInsert += OnGameRoomInsert;
        conn.Db.GameRoom.OnDelete += OnGameRoomDelete;

        OnConnected?.Invoke();

        Conn.SubscriptionBuilder()
            .OnApplied(OnSubscriptionAppliedHandler)
            .Subscribe(new[]
            {
                "SELECT * FROM game_room",
                "SELECT * FROM player_count"
            });
        // .OnApplied(OnSubscriptionAppliedHandler);
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

    void OnSubscriptionAppliedHandler(SubscriptionEventContext ctx)
    {
        Debug.Log("Subscription fully applied.");
        OnSubscriptionApplied?.Invoke();

        // Optional: auto join/create logic or show lobby
    }

    // --------------------------------
    // Table Event Handlers
    // --------------------------------
    
    void OnGameRoomInsert(EventContext ctx, GameRoom room)
    {
        // Handle room insert
        Debug.Log($"Room {room.RoomId} created.");
        Rooms.Add(room);
    }
    
    void OnGameRoomDelete(EventContext ctx, GameRoom room)
    {
        // Handle room delete
        Debug.Log($"Room {room.RoomId} deleted.");
        Rooms.Remove(room);
    }

    void OnOnlinePlayerInsert(EventContext ctx, OnlinePlayer player)
    {
        Debug.Log($"Player {player.PlayerId} created.");
        // if (player.RoomId != CurrentRoomId) return; // Only spawn players in the same room

        PlayerController controller = Instantiate(playerPrefab).GetComponent<PlayerController>();
        Debug.Log($"Player {player.PlayerId} created in room {player.RoomId}.");
        controller.transform.position = new Vector3(player.LastPosition.X, player.LastPosition.Y, player.LastPosition.Z);
        bool isLocal = player.Identity.Equals(GameManager.LocalIdentity);
        controller.Init(player, isLocal);
        Players[player.PlayerId] = controller;
    }

    void OnOnlinePlayerDelete(EventContext ctx, OnlinePlayer player)
    {
        Debug.Log($"Player {player.PlayerId} deleted.");
        if (Players.TryGetValue(player.PlayerId, out PlayerController controller))
        {
            Destroy(controller.gameObject);
            Players.Remove(player.PlayerId);
        }
    }
    
    void OnOnlinePlayerUpdate(EventContext ctx, OnlinePlayer oldData, OnlinePlayer newData)
    {
        Debug.Log($"Player {newData.PlayerId} updated.");
        if (Players.TryGetValue(newData.PlayerId, out PlayerController controller))
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

        if (newData.RoomId != CurrentRoomId)
        {
            OnOnlinePlayerDelete(ctx, newData);
        }
    }
    
    void OnEntityInsert(EventContext ctx, RoomEntity entity)
    {
        GameObject obj = Instantiate(playerPrefab);
        obj.transform.position = new Vector3(entity.Position.X, entity.Position.Y, entity.Position.Z);
        obj.transform.rotation = Quaternion.Euler(entity.Rotation.X, entity.Rotation.Y, entity.Rotation.Z);
        obj.transform.localScale = new Vector3(entity.Scale.X, entity.Scale.Y, entity.Scale.Z);
        Entities[entity.EntityId] = obj;
    }

    void OnEntityDelete(EventContext ctx, RoomEntity entity)
    {
        if (Entities.TryGetValue(entity.EntityId, out GameObject obj))
        {
            Destroy(obj);
            Entities.Remove(entity.EntityId);
        }
    }

    void OnEntityUpdate(EventContext ctx, RoomEntity oldEntity, RoomEntity newEntity)
    {
        if (Entities.TryGetValue(newEntity.EntityId, out GameObject obj))
        {
            // Update the entity's position, rotation, and scale based on the new entity data
            obj.transform.position = new Vector3(newEntity.Position.X, newEntity.Position.Y, newEntity.Position.Z);
            obj.transform.rotation = Quaternion.Euler(newEntity.Rotation.X, newEntity.Rotation.Y, newEntity.Rotation.Z);
            obj.transform.localScale = new Vector3(newEntity.Scale.X, newEntity.Scale.Y, newEntity.Scale.Z);
        }
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
        CurrentRoomId = roomId;

        // Clear existing players
        foreach (var player in Players.Values)
        {
            Destroy(player.gameObject);
        }
        Players.Clear();
        // Clear existing entities
        foreach (var entity in Entities.Values)
        {
            Destroy(entity);
        }
        Entities.Clear();
        
        Conn.Reducers.JoinRoom(roomId, password);
        
        // Re-subscribe to only the players/entities in this room
        var newCurrentRoomSub = Conn.SubscriptionBuilder()
            .OnApplied((ctx) =>
            {
                Debug.Log("Room subscription applied.");
                // Spawn existing players in the room
                foreach (var player in Conn.Db.OnlinePlayer.Iter())
                {
                    if (player.RoomId == roomId)
                    {
                        OnOnlinePlayerInsert(null, player);
                    }
                }
                
                // Spawn existing entities in the room
                foreach (var entity in Conn.Db.RoomEntity.Iter())
                {
                    if (entity.RoomId == roomId)
                    {
                        OnEntityInsert(null, entity);
                    }
                }
                
                // Subscribe to player and entity events
                Conn.Db.OnlinePlayer.OnDelete += OnOnlinePlayerDelete;
                Conn.Db.OnlinePlayer.OnUpdate += OnOnlinePlayerUpdate;
                Conn.Db.OnlinePlayer.OnInsert += OnOnlinePlayerInsert;
                Conn.Db.RoomEntity.OnInsert += OnEntityInsert;
                Conn.Db.RoomEntity.OnDelete += OnEntityDelete;
                Conn.Db.RoomEntity.OnUpdate += OnEntityUpdate;
            })
            .OnError((ctx, ex) => Debug.LogError($"Room subscription error: {ex}"))
            .Subscribe(new string[]
            {//write the sql query to get the room id
                "SELECT * FROM online_player WHERE room_id = " + roomId,
                "SELECT * FROM room_entity WHERE room_id = " + roomId
            });
        if (currentRoomSub != null && currentRoomSub.IsActive)
        {
            // Unsubscribe from the previous room subscription
            currentRoomSub.Unsubscribe();
        }
        currentRoomSub = newCurrentRoomSub;
    }
    
    public void LeaveRoom()
    {
        if (currentRoomSub != null && currentRoomSub.IsActive)
        {
            Conn.Db.OnlinePlayer.OnDelete -= OnOnlinePlayerDelete;
            Conn.Db.OnlinePlayer.OnUpdate -= OnOnlinePlayerUpdate;
            Conn.Db.OnlinePlayer.OnInsert -= OnOnlinePlayerInsert;
            Conn.Db.RoomEntity.OnInsert -= OnEntityInsert;
            Conn.Db.RoomEntity.OnDelete -= OnEntityDelete;
            Conn.Db.RoomEntity.OnUpdate -= OnEntityUpdate;
            currentRoomSub.Unsubscribe();
        }

        // Clear all players
        foreach (var player in Players.Values)
        {
            Destroy(player.gameObject);
        }
        Players.Clear();
        
        // Clear all entities
        foreach (var entity in Entities.Values)
        {
            Destroy(entity);
        }
        Entities.Clear();
        
        Conn.Reducers.LeaveRoom();

        CurrentRoomId = 0;
    }

    public void CreateRoom(string roomName, string password)
    {
        Conn.Reducers.CreateRoom(roomName, password);
    }

    public void AddEntity(uint roomId, string prefabId, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        Conn.Reducers.AddEntity(
            roomId, prefabId,
            new DbVector3(pos.x, pos.y, pos.z),
            new DbVector3(rot.x, rot.y, rot.z),
            new DbVector3(scale.x, scale.y, scale.z)
        );
    }

    public void RemoveEntity(uint entityId)
    {
        Conn.Reducers.RemoveEntity(entityId);
    }

    public void UpdatePlayerPosition(Vector3 pos)
    {
        Conn.Reducers.UpdateLastPosition(new DbVector3(pos.x, pos.y, pos.z));
    }

    public uint GetOnlinePlayerCount()
    {
        PlayerCount playerCount = Conn.Db.PlayerCount.Id.Find(0);
        return playerCount?.Count ?? 0;
    }

    public static bool IsConnected() => Conn != null && Conn.IsActive;
}
}