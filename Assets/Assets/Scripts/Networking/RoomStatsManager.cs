using System;
using System.Collections.Generic;
using System.Linq;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class RoomStatsManager
    {
        public class RoomSessionHistoryData
        {
            public uint SessionId;
            public uint UserId;
            public string UserName;
            public uint RoomId;
            public ulong EntryTime;
            public ulong ExitTime;
            public ulong DurationMicroseconds;
        }

        private DbConnection conn;
        public List<RoomSessionHistoryData> Stats = new List<RoomSessionHistoryData>();

        public void Init(DbConnection connection)
        {
            conn = connection;

            SubscribeToRoomStats();
            // STDBRoomManager.OnRoomJoin += () =>
            // {
            //     // iterate through stats db
            //     foreach (var stat in conn.Db.RoomSessionHistory.Iter())
            //     {
            //         Debug.Log(stat.SessionId);
            //         Stats.Add(Convert(stat));
            //     }
            // };
        }

        private void SubscribeToRoomStats()
        {
            conn.Db.RoomSessionHistory.OnInsert += OnStatsInsert;
            conn.Db.RoomSessionHistory.OnUpdate += OnStatsUpdate;
            conn.Db.RoomSessionHistory.OnDelete += OnStatsDelete;
        }

        private void OnStatsInsert(EventContext ctx, RoomSessionHistory stats)
        {
            Debug.Log("Stat inserted" + stats.SessionId);
            Stats.Add(Convert(stats));
        }

        private void OnStatsUpdate(EventContext ctx, RoomSessionHistory oldStats, RoomSessionHistory newStats)
        {
            Debug.Log("Stat updated" + newStats.SessionId);
            var index = Stats.FindIndex(s => s.SessionId == newStats.SessionId);
            if (index >= 0)
                Stats[index] = Convert(newStats);
        }

        private void OnStatsDelete(EventContext ctx, RoomSessionHistory stats)
        {
            Debug.Log("Stat deleted" + stats.SessionId);
            Stats.RemoveAll(s => s.SessionId == stats.SessionId);
        }

        private RoomSessionHistoryData Convert(RoomSessionHistory stats)
        {
            return new RoomSessionHistoryData
            {
                SessionId = stats.SessionId,
                UserId = stats.UserId,
                UserName = stats.UserName,
                RoomId = stats.RoomId,
                EntryTime = stats.EntryTime,
                ExitTime = stats.ExitTime,
                DurationMicroseconds = stats.DurationMicroseconds
            };
        }

        public List<RoomSessionHistoryData> GetStatsLast24Hours()
        {
            ulong now = (ulong)STDBBackendManager.GetCurrentServerTimestamp().MicrosecondsSinceUnixEpoch;
            ulong dayAgo = now - 24UL * 60 * 60 * 1_000_000;
            return Stats.Where(s => s.EntryTime >= dayAgo).ToList();
        }

        public List<RoomSessionHistoryData> GetStatsLast7Days()
        {
            ulong now = (ulong)STDBBackendManager.GetCurrentServerTimestamp().MicrosecondsSinceUnixEpoch;
            ulong weekAgo = now - 7UL * 24 * 60 * 60 * 1_000_000;
            return Stats.Where(s => s.EntryTime >= weekAgo).ToList();
        }

        public List<RoomSessionHistoryData> GetStatsLastMonth()
        {
            ulong now = (ulong)STDBBackendManager.GetCurrentServerTimestamp().MicrosecondsSinceUnixEpoch;
            ulong monthAgo = now - 30UL * 24 * 60 * 60 * 1_000_000;
            return Stats.Where(s => s.EntryTime >= monthAgo).ToList();
        }
    }
}