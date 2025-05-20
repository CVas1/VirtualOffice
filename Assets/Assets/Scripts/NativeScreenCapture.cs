using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class NativeScreenCapture : MonoBehaviourSingleton<NativeScreenCapture>
{
    [DllImport("ScreenCaptureDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool CaptureScreen(IntPtr buffer, int width, int height);

    [DllImport("ScreenCaptureDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void GetScreenSize(ref int width, ref int height);

    private Texture2D screenTex;
    
    private int screenWidth;
    private int screenHeight;
    private byte[] buffer;
    private GCHandle handle;
    private IntPtr pointer;
    private bool isCapturing = false;
    private float captureInterval;
    private float captureTimer = 0f;

    public int captureFPS = 10;

    public UnityEvent<Texture2D> OnTextureChanged;
    void Start()
    {
        // Retrieve the current screen dimensions
        GetScreenSize(ref screenWidth, ref screenHeight);
        Debug.Log("Screen size: " + screenWidth + " x " + screenHeight);

        // Create a Texture2D with the screen dimensions
        screenTex = new Texture2D(screenWidth, screenHeight, TextureFormat.BGRA32, false);
        buffer = new byte[screenWidth * screenHeight * 4];

        // Pin the managed array so we can pass a pointer to the native function
        handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        pointer = handle.AddrOfPinnedObject();

        captureInterval = 1f / captureFPS;
    }

    void Update()
    {
        captureInterval = 1f / captureFPS;

        if (!isCapturing) return;

        captureTimer += Time.deltaTime;
        if (captureTimer >= captureInterval)
        {
            captureTimer = 0f;
            bool success = CaptureScreen(pointer, screenWidth, screenHeight);
            if (success)
            {
                // Load the texture data
                screenTex.LoadRawTextureData(buffer);
                screenTex.Apply();
                
                // Flip the texture vertically
                FlipTextureVertically(screenTex);
                OnTextureChanged?.Invoke(screenTex);
            }
            else
            {
                Debug.LogError("Screen capture failed!");
            }
        }
    }

    // Method to flip a texture vertically
    private void FlipTextureVertically(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        Color[] pixelsFlipped = new Color[pixels.Length];
        
        for (int y = 0; y < screenHeight; y++)
        {
            for (int x = 0; x < screenWidth; x++)
            {
                pixelsFlipped[x + y * screenWidth] = pixels[x + (screenHeight - y - 1) * screenWidth];
            }
        }
        
        texture.SetPixels(pixelsFlipped);
        texture.Apply();
    }

    public void StartCapture()
    {
        isCapturing = true;
        captureTimer = 0f;
    }

    public void StopCapture()
    {
        isCapturing = false;
    }

    void OnDestroy()
    {
        if (handle.IsAllocated)
        {
            handle.Free();
        }
    }
}