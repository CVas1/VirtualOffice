#if UNITY_5_3_OR_NEWER
using System;
using System.Collections.Generic;
using SpacetimeDB;
using UnityEngine;

namespace SpacetimeDB
{
    public class SpacetimeDBNetworkManager : MonoBehaviour
    {
        internal static SpacetimeDBNetworkManager? _instance;

        [Tooltip("Maximum FrameTick calls per second (default 20)")] [Range(1, 60)]
        public int maxFrameTicksPerSecond = 20;

        private float frameTickInterval => 1f / Mathf.Max(1, maxFrameTicksPerSecond);
        private float frameTickTimer = 0f;

        private readonly List<IDbConnection> activeConnections = new();

        public void Awake()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException(
                    "SpacetimeDBNetworkManager is a singleton and should only be attached once.");
            }
            else
            {
                _instance = this;
            }
        }

        public bool AddConnection(IDbConnection conn)
        {
            if (activeConnections.Contains(conn))
            {
                return false;
            }

            activeConnections.Add(conn);
            return true;
        }

        public bool RemoveConnection(IDbConnection conn)
        {
            return activeConnections.Remove(conn);
        }

        private void ForEachConnection(Action<IDbConnection> action)
        {
            for (var x = activeConnections.Count - 1; x >= 0; x--)
            {
                action(activeConnections[x]);
            }
        }

        private void Update()
        {
            frameTickTimer += Time.deltaTime;
            if (frameTickTimer >= frameTickInterval)
            {
                ForEachConnection(conn => conn.FrameTick());
                frameTickTimer = 0f;
            }
        }

        private void OnDestroy() => ForEachConnection(conn => conn.Disconnect());
    }
}
#endif