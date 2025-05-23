using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class STDBBuildingManager
    {
        private DbConnection conn;

        public void Init(DbConnection connection)
        {
            conn = connection;
            
            // Register for building-related events
            conn.Db.RoomEntity.OnInsert += OnBuildingInsert;
            conn.Db.RoomEntity.OnUpdate += OnBuildingUpdate;
            conn.Db.RoomEntity.OnDelete += OnBuildingDelete;
        }

        private void OnBuildingInsert(EventContext ctx, RoomEntity roomEntity)
        {
            RoomBuildingManager.Instance.Load(roomEntity.Data);
        }

        private void OnBuildingUpdate(EventContext ctx, RoomEntity roomEntityOld, RoomEntity roomEntityNew)
        {
            // Ignore updates from the local player to avoid loops
            if (roomEntityNew.Identity == STDBBackendManager.LocalIdentity) return;
            RoomBuildingManager.Instance.Load(roomEntityNew.Data);
        }

        private void OnBuildingDelete(EventContext ctx, RoomEntity roomEntity)
        {
            RoomBuildingManager.Instance.DeleteAll();
        }

        public void SaveBuildingData(string data)
        {
            conn.Reducers.SaveEntity(STDBRoomManager.CurrentRoomId, data);
        }
    }
}