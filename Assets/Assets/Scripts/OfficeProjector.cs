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
    }

    public void ChangeOnce()
    {
        NativeScreenCapture.ScreenCaptureDTO textureDTO = NativeScreenCapture.Instance.GetScreenImage();
        SetImageRaw(textureDTO);

        if (GetProjectorId() == null)
        {
            Debug.LogError("Projector ID is null.");
            return;
        }

        STDBBackendManager.Instance.imageManager.SendImage(GetProjectorId(), texture.EncodeToJPG(50), width, height);
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

    public void SetImageRaw(NativeScreenCapture.ScreenCaptureDTO textureDTO)
    {
        if (projectorImageRenderer != null)
        {
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
        NativeScreenCapture.Instance.OnTextureChanged.TryAdd(this.GetHashCode(), OnTextureChanged);
        NativeScreenCapture.Instance.StartCapture();
    }

    private void StopProjection()
    {
        NativeScreenCapture.Instance.OnTextureChanged.Remove(this.GetHashCode());
    }

    private void OnDestroy()
    {
        NativeScreenCapture.Instance.OnTextureChanged.Remove(this.GetHashCode());
    }

    private void OnTextureChanged(NativeScreenCapture.ScreenCaptureDTO textureDTO)
    {
        if (projectorImageRenderer != null)
        {
            SetImageRaw(textureDTO);
            STDBBackendManager.Instance.imageManager.SendImage(GetProjectorId(), texture.EncodeToJPG(50), width,
                height);
        }
        else
        {
            Debug.LogError("Projector image renderer is not set.");
        }
    }
}