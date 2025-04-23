using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Assets.Scripts.VoiceRecorder
{
    public class VoiceChatPlayer : MonoBehaviour
    {
        public int sampleRate = 8000;
        public int channels = 1;
        public int maxParallelSources = 10;

        private Dictionary<uint, VoiceChatSpeaker> speakers = new Dictionary<uint, VoiceChatSpeaker>();
        private List<AudioSource> audioSourcePool = new List<AudioSource>();

        public static VoiceChatPlayer Instance;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            InitializeAudioSourcePool();
        }

        public void EnqueueAudio(byte[] compressedData, uint identity)
        {
            if (!speakers.ContainsKey(identity))
            {
                speakers[identity] = new VoiceChatSpeaker();
            }

            // Decompress the data
            byte[] rawData = Decompress(compressedData);
            float[] audioSamples = new float[rawData.Length / sizeof(float)];
            Buffer.BlockCopy(rawData, 0, audioSamples, 0, rawData.Length);

            lock (speakers[identity].audioQueue)
            {
                speakers[identity].audioQueue.Enqueue(audioSamples);
            }
        }

        void Update()
        {
            foreach (var speaker in speakers)
            {
                if (speaker.Value.audioQueue.Count > 0 && speaker.Value.currentSource == null)
                {
                    PlayNextChunk(speaker.Key);
                }
            }
        }

        private void PlayNextChunk(uint identity)
        {
            lock (speakers[identity].audioQueue)
            {
                if (speakers[identity].audioQueue.Count == 0) return;

                float[] audioSamples = speakers[identity].audioQueue.Dequeue();
                AudioSource source = GetAvailableAudioSource();

                if (source == null) return;

                AudioClip clip = AudioClip.Create($"VoiceClip_{identity}",
                    audioSamples.Length / channels,
                    channels,
                    sampleRate,
                    false);

                clip.SetData(audioSamples, 0);
                source.clip = clip;
                source.Play();

                speakers[identity].currentSource = source;
                StartCoroutine(ReleaseAudioSourceAfterPlay(source, identity));
            }
        }

        private AudioSource GetAvailableAudioSource()
        {
            foreach (AudioSource source in audioSourcePool)
            {
                if (!source.isPlaying) return source;
            }

            return null;
        }

        private IEnumerator ReleaseAudioSourceAfterPlay(AudioSource source, uint identity)
        {
            yield return new WaitWhile(() => source.isPlaying);
            speakers[identity].currentSource = null;
        }

        private byte[] Decompress(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream decompressStream = new GZipStream(input, CompressionMode.Decompress))
                {
                    decompressStream.CopyTo(output);
                }

                return output.ToArray();
            }
        }

        private void InitializeAudioSourcePool()
        {
            for (int i = 0; i < maxParallelSources; i++)
            {
                GameObject child = new GameObject($"AudioSource_{i}");
                child.transform.parent = transform;
                AudioSource source = child.AddComponent<AudioSource>();
                audioSourcePool.Add(source);
            }
        }
    }

    public class VoiceChatSpeaker
    {
        public Queue<float[]> audioQueue = new Queue<float[]>();
        public AudioSource currentSource;
    }
}