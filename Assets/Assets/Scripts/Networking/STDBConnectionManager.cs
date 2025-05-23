using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class STDBConnectionManager
    {
        const string SERVER_URL = "http://127.0.0.1:3000";
        const string MODULE_NAME = "stdboffice";

        public void Connect(
            Action<DbConnection, Identity, string> onConnect,
            Action onDisconnect,
            Action<Exception> onError)
        {
            // delete player pref
            // PlayerPrefs.DeleteKey("spacetimedb.identity_token");
            // var key = $"spacetimedb.identity_token - {Application.dataPath}";
            // PlayerPrefs.DeleteKey(key);

            var builder = DbConnection.Builder()
                .WithUri(SERVER_URL)
                .WithModuleName(MODULE_NAME)
                .OnConnect((conn, identity, token) => onConnect?.Invoke(conn, identity, token))
                .OnDisconnect((conn, ex) => onDisconnect?.Invoke())
                .OnConnectError((ex) => onError?.Invoke(ex));

            // AuthToken.SaveToken("spacetimedb.identity_token" + Random.Range(1000, 9999));
            // Debug.Log(AuthToken.Token);
            // builder = builder.WithToken(Guid.NewGuid().ToString());
            if (!string.IsNullOrEmpty(AuthToken.Token))
            {
                builder = builder.WithToken(AuthToken.Token);
            }

            builder.Build();
        }
    }
}