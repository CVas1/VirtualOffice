using System;
using Assets.Scripts.Networking;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using UnityEngine;
using UnityEngine.UI;

public class OfficeProjector : MonoBehaviour
{
    private BuildingPart buildingPart;
    private string projectorId = null;
    private bool isProjecting = false;

    [SerializeField] private Renderer projectorImageRenderer;
    private Texture2D texture;
    private int width = 1920;
    private int height = 1080;

    private void Awake()
    {
        buildingPart = GetComponent<BuildingPart>();
        if (projectorImageRenderer == null)
        {
            Debug.LogError("Projector image renderer is not set.");
        }

        texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        projectorImageRenderer.material.mainTexture = texture;
        projectorImageRenderer.material.SetTexture("_MainTex", texture);
        projectorImageRenderer.material.SetTextureScale("_MainTex", new Vector2(1, 1));
        projectorImageRenderer.material.SetTextureOffset("_MainTex", new Vector2(0, 0));
    }

    public void SetProjectorId(string id)
    {
        projectorId = id;
    }

    private string GetProjectorId()
    {
        if (projectorId != null)
        {
            return projectorId;
        }

        foreach (var item in buildingPart.Properties)
        {
            if (item.StartsWith("ProjectorId"))
            {
                projectorId = item.Substring(11);
                return projectorId;
            }
        }

        return null;
    }    public void ChangeOnce()
    {
        if (NativeScreenCapture.Instance == null)
        {
            Debug.LogError("NativeScreenCapture instance is null, cannot capture screen.");
            return;
        }
        
        NativeScreenCapture.ScreenCaptureDTO textureDTO = NativeScreenCapture.Instance.GetScreenImage();
        SetImageRaw(textureDTO);

        if (GetProjectorId() == null)
        {
            Debug.LogError("Projector ID is null.");
            return;
        }

        STDBBackendManager.Instance.imageManager.SendImage(GetProjectorId(), texture.EncodeToJPG(50), width, height);
    }    public void ChangeOnce(Texture2D inputTexture)
    {
        if (inputTexture == null)
        {
            Debug.LogError("Input texture is null.");
            return;
        }

        // Create a readable copy of the texture in RGBA32 format
        Texture2D readableTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.RGBA32, false);
        
        // Use Graphics.CopyTexture or RenderTexture to ensure we can read the texture
        RenderTexture renderTexture = RenderTexture.GetTemporary(inputTexture.width, inputTexture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(inputTexture, renderTexture);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        readableTexture.ReadPixels(new Rect(0, 0, inputTexture.width, inputTexture.height), 0, 0);
        readableTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        SetImageRaw(new NativeScreenCapture.ScreenCaptureDTO(readableTexture.width, readableTexture.height, readableTexture.GetRawTextureData()));

        if (GetProjectorId() == null)
        {
            Debug.LogError("Projector ID is null.");
            return;
        }

        STDBBackendManager.Instance.imageManager.SendImage(GetProjectorId(), readableTexture.EncodeToJPG(50), readableTexture.width, readableTexture.height);
        
        // Clean up the temporary texture
        DestroyImmediate(readableTexture);
    }

    public void Broadcast()
    {
        if (isProjecting)
        {
            StopProjection();
            isProjecting = false;
        }
        else
        {
            StartProjection();
            isProjecting = true;
        }
    }

    public void StopBroadcast()
    {
        if (isProjecting)
        {
            StopProjection();
            isProjecting = false;
        }
    }

    public void SetImageRaw(NativeScreenCapture.ScreenCaptureDTO textureDTO)
    {
        if (projectorImageRenderer != null)
        {
            // Calculate expected data size for RGBA32 format (4 bytes per pixel)
            int expectedDataSize = textureDTO.Width * textureDTO.Height * 4;
            
            if (textureDTO.ImageData == null)
            {
                Debug.LogError("Image data is null.");
                return;
            }
            
            if (textureDTO.ImageData.Length != expectedDataSize)
            {
                Debug.LogError($"Image data size mismatch. Expected: {expectedDataSize}, Actual: {textureDTO.ImageData.Length}");
                return;
            }
            
            texture = new Texture2D(textureDTO.Width, textureDTO.Height, TextureFormat.RGBA32, false);
            width = textureDTO.Width;
            height = textureDTO.Height;
            texture.LoadRawTextureData(textureDTO.ImageData);
            texture.Apply();
            projectorImageRenderer.material.mainTexture = texture;
            projectorImageRenderer.material.SetTexture("_MainTex", texture);
            projectorImageRenderer.material.SetTextureScale("_MainTex", new Vector2(1, 1));
            projectorImageRenderer.material.SetTextureOffset("_MainTex", new Vector2(0, 0));
        }
        else
        {
            Debug.LogError("Projector image renderer is not set.");
        }
    }

    public void SetImage(byte[] imageData)
    {
        if (projectorImageRenderer != null)
        {
            if (texture == null || texture.width != width || texture.height != height)
            {
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            }

            texture.LoadImage(imageData);
            texture.Apply();
            projectorImageRenderer.material.mainTexture = texture;
            projectorImageRenderer.material.SetTexture("_MainTex", texture);
            projectorImageRenderer.material.SetTextureScale("_MainTex", new Vector2(1, 1));
            projectorImageRenderer.material.SetTextureOffset("_MainTex", new Vector2(0, 0));
        }
        else
        {
            Debug.LogError("Projector image renderer is not set.");
        }
    }
    private void StartProjection()
    {
        if (NativeScreenCapture.Instance == null)
        {
            Debug.LogError("NativeScreenCapture instance is null, cannot start projection.");
            return;
        }
        
        STDBBackendManager.Instance.imageManager.SendLockImageBroadcast(GetProjectorId());
        NativeScreenCapture.Instance.OnTextureChanged.TryAdd(this.GetHashCode(), OnTextureChanged);
        NativeScreenCapture.Instance.StartCapture();
    }

    private void StopProjection()
    {
        if (NativeScreenCapture.Instance != null)
        {
            NativeScreenCapture.Instance.OnTextureChanged.Remove(this.GetHashCode());
        }
    }

    private void OnDestroy()
    {
        if (NativeScreenCapture.Instance != null)
        {
            NativeScreenCapture.Instance.OnTextureChanged.Remove(this.GetHashCode());
        }
    }    private void OnTextureChanged(NativeScreenCapture.ScreenCaptureDTO textureDTO)
    {
        if (this == null || gameObject == null)
        {
            return; // Object has been destroyed
        }
        
        if (projectorImageRenderer != null)
        {
            SetImageRaw(textureDTO);
            STDBBackendManager.Instance.imageManager.SendImage(GetProjectorId(), texture.EncodeToJPG(30), width,
                height);
        }
        else
        {
            Debug.LogError("Projector image renderer is not set.");
        }
    }
}