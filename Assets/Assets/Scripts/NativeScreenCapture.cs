using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assets.Scripts;
using Assets.Scripts.Networking;
using UnityEngine;
using UnityEngine.Events;

public class NativeScreenCapture : MonoBehaviourSingleton<NativeScreenCapture>
{
    [DllImport("ScreenCaptureDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool CaptureScreen(IntPtr buffer, int width, int height);

    [DllImport("ScreenCaptureDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void GetScreenSize(ref int width, ref int height);

    [DllImport("ScreenCaptureDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ReleaseScreenCaptureResources();

    public class ScreenCaptureDTO
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] ImageData { get; set; }

        public ScreenCaptureDTO(int width, int height, byte[] imageData)
        {
            Width = width;
            Height = height;
            ImageData = imageData;
        }
    }

    private int screenWidth;
    private int screenHeight;
    private byte[] buffer;
    private GCHandle handle;
    private IntPtr pointer;

    public int captureFPS = 10;
    private float captureInterval;
    private Coroutine captureRoutine;

    [NonSerialized] public bool isCapturing = false;

    // public UnityEvent<Texture2D> OnTextureChanged;
    public Dictionary<int, Action<ScreenCaptureDTO>> OnTextureChanged = new Dictionary<int, Action<ScreenCaptureDTO>>();

    private void Start()
    {
        STDBRoomManager.OnRoomLeave += StopCapture;

        // Get screen size
        GetScreenSize(ref screenWidth, ref screenHeight);
        Debug.Log("Screen size: " + screenWidth + " x " + screenHeight);

        // Init texture and buffer
        //screenTex = new Texture2D(screenWidth, screenHeight, TextureFormat.BGRA32, false);
        buffer = new byte[screenWidth * screenHeight * 4];
        handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        pointer = handle.AddrOfPinnedObject();

        UpdateCaptureInterval();
    }

    private void OnValidate()
    {
        UpdateCaptureInterval();
    }

    private void UpdateCaptureInterval()
    {
        captureInterval = 1f / Mathf.Max(1, captureFPS);
    }

    public ScreenCaptureDTO GetScreenImage()
    {
        // Make sure the latest capture is in the texture
        bool success = CaptureScreen(pointer, screenWidth, screenHeight);
        if (success)
        {
            // Texture2D screenTex = new Texture2D(screenWidth, screenHeight, TextureFormat.BGRA32, false);
            // screenTex.LoadRawTextureData(buffer);
            // screenTex.Apply();
            // return screenTex; //
            return new ScreenCaptureDTO(screenWidth, screenHeight, buffer);
        }
        else
        {
            Debug.LogError("Screen capture failed!");
            return null;
        }
    }

    public void StartCapture()
    {
        if (!isCapturing)
        {
            isCapturing = true;
            captureRoutine = StartCoroutine(CaptureLoop());
        }
    }

    public void StopCapture()
    {
        if (isCapturing)
        {
            isCapturing = false;
            if (captureRoutine != null)
            {
                StopCoroutine(captureRoutine);
                captureRoutine = null;
            }
        }
    }

    private System.Collections.IEnumerator CaptureLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(captureInterval);

        while (isCapturing)
        {
            // check if the OnTextureChanged event has any subscribers else stop capturing
            if (OnTextureChanged == null || OnTextureChanged.Count == 0)
            {
                StopCapture();
                yield break;
            }


            bool success = CaptureScreen(pointer, screenWidth, screenHeight);
            if (success)
            {
                ScreenCaptureDTO screenCaptureDTO = new ScreenCaptureDTO(screenWidth, screenHeight, buffer);
                foreach (var action in OnTextureChanged.Values)
                {
                    // action?.Invoke(buffer);
                    action?.Invoke(screenCaptureDTO);
                }
            }
            else
            {
                Debug.LogError("Screen capture failed!");
            }

            yield return wait;
        }
    }

    private void OnDestroy()
    {
        if (handle.IsAllocated)
            handle.Free();

        ReleaseScreenCaptureResources();
    }
}