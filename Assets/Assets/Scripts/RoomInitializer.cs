using System;
using Assets.Scripts.Networking;
using UnityEngine;

namespace Assets.Scripts
{
    public class RoomInitializer : MonoBehaviour
    {
        private void Start()
        {
            STDBBackendManager.Instance.roomManager.InitilizeAfterWorldLoad();
        }
    }
}