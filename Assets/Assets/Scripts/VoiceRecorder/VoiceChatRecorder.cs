using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpacetimeDB;
using UnityEngine;

namespace Assets.Scripts.VoiceRecorder
{
    public class VoiceChatRecorder : MonoBehaviour
    {
        [Tooltip("Length of each clip in seconds")]
        public float chunkLength = 0.2f;

        [Tooltip("Sample rate for recording")] public int sampleRate = 16000;

        private AudioClip micClip;
        private int lastSamplePos = 0;
        private int samplesPerChunk;
        private bool isRecording = false;

        private void Start()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("No microphone found.");
                enabled = false;
                return;
            }

            samplesPerChunk = Mathf.CeilToInt(sampleRate * chunkLength);
            // Start the mic in a looped 1-second buffer
            micClip = Microphone.Start(null, true, 1, sampleRate);
            StartCoroutine(CaptureAndSendLoop());
        }

        private void Update()
        {
            // Start recording when Shift + C is pressed
            if (!isRecording && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.C))
            {
                isRecording = true;
                lastSamplePos = Microphone.GetPosition(null);
                Debug.Log("Voice recording started");
            }
            // // Stop when either key is released
            // else if (isRecording && (!Input.GetKey(KeyCode.LeftShift) || !Input.GetKey(KeyCode.C)))
            // {
            //     isRecording = false;
            //     Debug.Log("Voice recording stopped");
            // }
        }

        private IEnumerator CaptureAndSendLoop()
        {
            // wait until mic actually starts
            while (!(Microphone.GetPosition(null) > 0)) yield return null;

            var wait = new WaitForSeconds(chunkLength);
            while (true)
            {
                yield return wait;

                if (!isRecording) continue;

                int curPos = Microphone.GetPosition(null);
                int count = curPos - lastSamplePos;
                if (count < 0) count += micClip.samples;

                // read exactly samplesPerChunk floats (wrapping if needed)
                float[] buffer = new float[samplesPerChunk];
                micClip.GetData(buffer, lastSamplePos);

                // wrap into an AudioClip to feed WavUtility
                AudioClip chunkClip = AudioClip.Create("chunk", samplesPerChunk, 1, sampleRate, false);
                chunkClip.SetData(buffer, 0);

                // encode to WAV byte[]
                byte[] wavBytes = WavUtility.FromAudioClip(chunkClip);

                // send into spacetime
                // GameManager.Conn.Reducers.SendVoice(GameManager.CurrentRoomId, wavBytes.ToList());
                AudioClip ac = WavUtility.ToAudioClip(wavBytes, 0);

                // hand off to the voice manager
                VoiceChatPlayer.Instance.EnqueueClip(GameManager.LocalIdentity, ac);

                lastSamplePos = (lastSamplePos + samplesPerChunk) % micClip.samples;
            }
        }
    }
}