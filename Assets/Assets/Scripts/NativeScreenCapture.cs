using System;
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

    private Texture2D screenTex;
    private int screenWidth;
    private int screenHeight;
    private byte[] buffer;
    private GCHandle handle;
    private IntPtr pointer;

    public int captureFPS = 10;
    private float captureInterval;
    private Coroutine captureRoutine;

    [NonSerialized] public bool isCapturing = false;

    public UnityEvent<Texture2D> OnTextureChanged;

    private void Start()
    {
        STDBRoomManager.OnRoomLeave += StopCapture;

        // Get screen size
        GetScreenSize(ref screenWidth, ref screenHeight);
        Debug.Log("Screen size: " + screenWidth + " x " + screenHeight);

        // Init texture and buffer
        screenTex = new Texture2D(screenWidth, screenHeight, TextureFormat.BGRA32, false);
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
            bool success = CaptureScreen(pointer, screenWidth, screenHeight);
            if (success)
            {
                screenTex.LoadRawTextureData(buffer);
                screenTex.Apply(); // Minimal Apply call
                OnTextureChanged?.Invoke(screenTex);
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