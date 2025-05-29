using UnityEngine;
using Kirurobo;
using System.IO;
using UnityEngine.UI; // For Path operations
using System.Linq;

public class PngDropHandler : MonoBehaviour
{
    private UniWindowController windowController;

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

        if (Physics.Raycast(ray, out hit))
        {
            if (!IsSupportedImageFile(filePaths[0]))
            {
                Debug.LogError("The target object does not have a RawImage or SpriteRenderer component.");
                return;
            }
            
            GameObject targetObject = hit.collider.gameObject;
            
            OfficeProjector officeProjector = targetObject.GetComponentInParent<OfficeProjector>();
            if (officeProjector != null && officeProjector.CompareTag("Chart"))
            {
                officeProjector.ChangeOnce(LoadPNG(filePaths[0]));
            }
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