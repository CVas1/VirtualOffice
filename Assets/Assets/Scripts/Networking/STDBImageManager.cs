using System.Linq;
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
        }

        private void OnImagesInsert(EventContext ctx, Images image)
        {
            byte[] imageData = image.ImageData.ToArray();

            Debug.Log($"Received image data: {imageData.Length} bytes");
        }

        private void OnImagesUpdate(EventContext ctx, Images oldImage, Images newImage)
        {
            // Handle image updates if necessary
            // Debug.Log($"Updated image data: {image.ImageData.Length} bytes");
        }

        // Method to send an image to the server
        public void SendImage(string buildingIdentifier, byte[] imageData)
        {
            if (conn != null && conn.IsActive)
            {
                conn.Reducers.SendImage(buildingIdentifier, imageData.ToList());
            }
            else
            {
                Debug.LogWarning("Cannot send image: not connected to server");
            }
        }
    }
}