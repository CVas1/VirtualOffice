using System;
using UnityEngine;
using System.Collections.Generic;
using SpacetimeDB;

namespace Assets.Scripts.VoiceRecorder
{
    public class VoiceChatPlayer : MonoBehaviour
    {
        class PlayerState
        {
            public readonly Queue<AudioClip> queue = new Queue<AudioClip>();
            public AudioSource current;
            public AudioSource next;
            public bool isPlaying = false;
        }

        // one VoiceChatPlayer in the scene
        public static VoiceChatPlayer Instance { get; private set; }

        [Tooltip("Maximum pitch boost when catching up")]
        public float maxSpeedup = 1.5f;

        [Tooltip("Crossfade length in seconds")]
        public float crossfadeTime = 0.05f;

        // track per‐speaker state
        private readonly Dictionary<Identity, PlayerState> states
            = new Dictionary<Identity, PlayerState>();

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        /// <summary>
        /// Enqueue a new clip for playback, kicking off playback if needed.
        /// </summary>
        public void EnqueueClip(Identity sender, AudioClip clip)
        {
            if (!states.TryGetValue(sender, out var st))
            {
                var go = new GameObject($"Voice_{sender}");
                go.transform.SetParent(transform, false);

                st = new PlayerState
                {
                    current = go.AddComponent<AudioSource>(),
                    next = go.AddComponent<AudioSource>()
                };
                // configure both sources the same
                foreach (var src in new[] { st.current, st.next })
                {
                    src.spatialBlend = 0f;
                    src.loop = false;
                    src.playOnAwake = false;
                }

                states[sender] = st;
            }

            st.queue.Enqueue(clip);

            if (!st.isPlaying)
                StartCoroutine(PlayLoop(sender, st));
        }

        private System.Collections.IEnumerator PlayLoop(Identity sender, PlayerState st)
        {
            st.isPlaying = true;

            while (st.queue.Count > 0)
            {
                // fetch next clip
                var clip = st.queue.Dequeue();

                // if buffer is growing, speed up to catch up
                float speed = 1f;
                if (st.queue.Count > 2)
                    speed = Mathf.Min(maxSpeedup, 1f + st.queue.Count * 0.1f);

                // prepare crossfade: we'll start 'next' just before 'current' ends
                AudioSource playSrc = st.current;
                AudioSource prepSrc = st.next;

                prepSrc.clip = clip;
                prepSrc.pitch = speed;
                prepSrc.volume = 0f;
                prepSrc.Play();

                if (!playSrc.isPlaying)
                {
                    // no current playing yet: just fade in next
                    float t = 0f;
                    while (t < crossfadeTime)
                    {
                        prepSrc.volume = t / crossfadeTime;
                        t += Time.deltaTime;
                        yield return null;
                    }

                    prepSrc.volume = 1f;
                }
                else
                {
                    // schedule crossfade
                    float startFadeTime = clip.length - crossfadeTime;
                    // wait until it's time to fade
                    yield return new WaitForSeconds(startFadeTime / speed);
                    // then fade out current, fade in next
                    float t = 0f;
                    while (t < crossfadeTime)
                    {
                        float α = t / crossfadeTime;
                        prepSrc.volume = α;
                        playSrc.volume = 1f - α;
                        t += Time.deltaTime;
                        yield return null;
                    }

                    prepSrc.volume = 1f;
                    playSrc.Stop();
                }

                // swap sources
                st.current.volume = 1f;
                st.current.pitch = speed;
                (st.current, st.next) = (st.next, st.current);

                // wait for clip to finish if needed
                float remaining = (clip.length - (st.current.clip == clip ? 0 : crossfadeTime)) / speed;
                yield return new WaitForSeconds(remaining);
            }

            st.isPlaying = false;
        }
    }
}