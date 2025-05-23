using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.IO.Compression;
using Assets.Scripts.Networking;

namespace Assets.Scripts.VoiceRecorder
{
    public class VoiceChatRecorder : MonoBehaviour
    {
        public int sampleRate = 8000; // Lower sample rate for bandwidth savings
        public KeyCode pushToTalkKey = KeyCode.C;
        public float chunkInterval = 0.5f;

        private AudioClip recordingClip;
        private string microphoneDevice;
        private Coroutine recordingCoroutine;
        private int lastSamplePosition;
        private bool isRecording;
        private bool shiftPressed;

        void Start()
        {
            microphoneDevice = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        }

        void Update()
        {
            shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (shiftPressed && Input.GetKeyDown(pushToTalkKey))
            {
                StartRecording();
            }

            if (Input.GetKeyUp(pushToTalkKey))
            {
                StopRecording();
            }
        }

        public void StartRecording()
        {
            if (isRecording || microphoneDevice == null) return;

            recordingClip = Microphone.Start(microphoneDevice, true, 1, sampleRate);
            lastSamplePosition = 0;
            isRecording = true;
            recordingCoroutine = StartCoroutine(RecordingRoutine());
        }

        public void StopRecording()
        {
            if (!isRecording) return;

            Microphone.End(microphoneDevice);
            isRecording = false;
            if (recordingCoroutine != null) StopCoroutine(recordingCoroutine);
        }

        private IEnumerator RecordingRoutine()
        {
            while (isRecording)
            {
                yield return new WaitForSecondsRealtime(chunkInterval);
                ProcessChunk();
            }
        }

        private void ProcessChunk()
        {
            int currentPosition = Microphone.GetPosition(microphoneDevice);
            int sampleCount = currentPosition - lastSamplePosition;

            if (sampleCount < 0)
            {
                sampleCount += recordingClip.samples;
            }

            if (sampleCount > 0)
            {
                float[] samples = new float[sampleCount * recordingClip.channels];
                recordingClip.GetData(samples, lastSamplePosition);

                // Convert and compress
                byte[] rawData = new byte[samples.Length * sizeof(float)];
                Buffer.BlockCopy(samples, 0, rawData, 0, rawData.Length);

                // Compress the data
                byte[] compressedData = Compress(rawData);


                STDBBackendManager.Instance.voiceManager.SendVoiceClip(compressedData);
                // VoiceChatPlayer.Instance.EnqueueAudio(compressedData, 0); // Pass the identity parameter
                //OnChunkRecorded?.Invoke(compressedData);
            }

            lastSamplePosition = currentPosition % recordingClip.samples;
        }

        private byte[] Compress(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream compressStream = new GZipStream(output, CompressionMode.Compress))
                {
                    compressStream.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }

        void OnDestroy()
        {
            StopRecording();
        }
    }
}