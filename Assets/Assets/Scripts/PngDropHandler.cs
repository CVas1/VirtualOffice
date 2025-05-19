using UnityEngine;
using Kirurobo;
using System.IO;
using UnityEngine.UI; // For Path operations
using System.Linq;

public class PngDropHandler : MonoBehaviour
{
    private UniWindowController windowController;
    public LayerMask targetLayerMask; // Layer mask to filter the target objects

    void Start()
    {
        // Get the UniWindowController instance
        windowController = UniWindowController.current;

        // Subscribe to the FilesDropped event
        windowController.OnDropFiles += OnFilesDropped;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (windowController != null)
        {
            windowController.OnDropFiles -= OnFilesDropped;
        }
    }

    private void OnFilesDropped(string[] filePaths)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayerMask))
        {
            GameObject targetObject = hit.collider.gameObject;
            Renderer renderer = targetObject.GetComponentInChildren<Renderer>();

            if (renderer == null || !IsSupportedImageFile(filePaths[0]))
            {
                Debug.LogError("The target object does not have a RawImage or SpriteRenderer component.");
                return;
            }

            Texture2D newTexture = LoadPNG(filePaths[0]);
            renderer.material.mainTexture = newTexture;
        }
    }

    private bool IsSupportedImageFile(string filePath)
    {
        string[] supportedExtensions = { ".png", ".jpg", ".jpeg" };
        string fileExtension = Path.GetExtension(filePath).ToLower();
        return supportedExtensions.Contains(fileExtension);
    }

    private Texture2D LoadPNG(string filePath)
    {
        Texture2D texture = null;

        if (File.Exists(filePath))
        {
            // Read the file bytes
            byte[] fileData = File.ReadAllBytes(filePath);

            // Create a texture and load the image data
            texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(fileData); // Automatically resizes the texture
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }

        return texture;
    }
}