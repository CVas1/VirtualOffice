using System.Linq;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class STDBImageManager
    {
        private DbConnection conn;
        private SubscriptionHandle currentImageSub;

        public void Init(DbConnection connection)
        {
            conn = connection;
            conn.Db.Images.OnInsert += OnImagesInsert;
            conn.Db.Images.OnUpdate += OnImagesUpdate;

            conn.Db.ImageBroadcastLock.OnInsert += OnImageLockInsert;
            conn.Db.ImageBroadcastLock.OnUpdate += OnImageLockUpdate;
            conn.Db.ImageBroadcastLock.OnDelete += OnImageLockDelete;
        }

        private void OnImagesInsert(EventContext ctx, Images image)
        {
            byte[] imageData = image.ImageData.ToArray();
            RoomBuildingManager.Instance.SetImageOfProjector(image.BuildingIdentifier, imageData, image.Width,
                image.Height);
        }

        private void OnImagesUpdate(EventContext ctx, Images oldImage, Images newImage)
        {
            byte[] newImageData = newImage.ImageData.ToArray();
            RoomBuildingManager.Instance.SetImageOfProjector(newImage.BuildingIdentifier, newImageData, newImage.Width,
                newImage.Height);
        }

        private void OnImageLockInsert(EventContext ctx, ImageBroadcastLock lockData)
        {
        }

        private void OnImageLockUpdate(EventContext ctx, ImageBroadcastLock oldLock, ImageBroadcastLock newLock)
        {
            Debug.Log(newLock);
        }

        private void OnImageLockDelete(EventContext ctx, ImageBroadcastLock lockData)
        {
            Debug.Log(lockData);
        }

        public void ProjectorCreatedLocally(string projectorId)
        {
        }

        public void ProjectorDestroyedLocally(string projectorId)
        {
        }

        // Method to send an image to the server
        public void SendImage(string buildingIdentifier, byte[] imageData, int width, int height)
        {
            if (conn != null && conn.IsActive)
            {
                conn.Reducers.SendImage(buildingIdentifier, imageData.ToList(), width, height);
            }
            else
            {
                Debug.LogWarning("Cannot send image: not connected to server");
            }
        }

        public void SendLockImageBroadcast(string buildingIdentifier)
        {
            if (conn != null && conn.IsActive)
            {
                conn.Reducers.LockImageBroadcast(buildingIdentifier);
            }
            else
            {
                Debug.LogWarning("Cannot send image lock: not connected to server");
            }
        }
    }
}