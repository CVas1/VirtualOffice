using System;
using UnityEngine;
using UnityEngine.UI;

public class OfficeProjector : MonoBehaviour
{
    private Renderer projectorImageRenderer;
    
    
    public void OnClick()
    {
        Debug.Log("Projector clicked!"); 
        StartProjection();
    }
    private void StartProjection()
    {
        NativeScreenCapture.Instance.StartCapture();
        NativeScreenCapture.Instance.OnTextureChanged.AddListener(OnTextureChanged);
    }
    private void StopProjection()
    {
        NativeScreenCapture.Instance.StopCapture();
        NativeScreenCapture.Instance.OnTextureChanged.RemoveListener(OnTextureChanged);
    }
    private void OnDestroy()
    {
        StopProjection();
    }
    

    private void OnTextureChanged(Texture2D arg0)
    {        
        if (projectorImageRenderer != null)
        {
            projectorImageRenderer.material.mainTexture = arg0;
            projectorImageRenderer.material.SetTexture("_MainTex", arg0);
            projectorImageRenderer.material.SetTextureScale("_MainTex", new Vector2(1, 1));
            projectorImageRenderer.material.SetTextureOffset("_MainTex", new Vector2(0, 0));
        }
        else
        {
            Debug.LogError("Projector image renderer is not set.");
        }
    }

    private void Start()
    {
        projectorImageRenderer = GetComponent<Renderer>();
        
    }
}
