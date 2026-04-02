using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Queues and plays back PCM16 audio clips received from the AI backend.
    /// Handles base64 decoding, sample conversion, and sequential clip playback.
    /// </summary>
    public sealed class IVXAIAudioPlayer : MonoBehaviour
    {
        #region Private Fields

        private AudioSource _source;
        private int _sampleRate;
        private int _channels;
        private readonly Queue<AudioClip> _clipQueue = new Queue<AudioClip>();
        private bool _isPlaying;
        private int _clipCounter;

        #endregion

        #region Events

        /// <summary>Fired when audio playback starts.</summary>
        public event Action OnPlaybackStarted;

        /// <summary>Fired when all queued audio has finished playing.</summary>
        public event Action OnPlaybackFinished;

        #endregion

        #region Properties

        public bool IsPlaying => _isPlaying;

        #endregion

        #region Initialization

        /// <summary>
        /// Configure the player with an IVXAIConfig and an optional external AudioSource.
        /// If no AudioSource is provided, one is created on this GameObject.
        /// </summary>
        public void Initialize(IVXAIConfig config, AudioSource externalSource = null)
        {
            _sampleRate = config.AudioSampleRate;
            _channels = config.AudioChannels;

            if (externalSource != null)
            {
                _source = externalSource;
            }
            else
            {
                _source = GetComponent<AudioSource>();
                if (_source == null)
                    _source = gameObject.AddComponent<AudioSource>();
            }

            _source.playOnAwake = false;
        }

        #endregion

        #region Public Methods

        /// <summary>Maximum clips allowed in the queue. Configurable via <see cref="SetMaxQueueSize"/>.</summary>
        private int _maxQueueSize = 30;

        /// <summary>Sets the max audio queue size (from IVXAIConfig.MaxAudioQueueSize).</summary>
        public void SetMaxQueueSize(int max) => _maxQueueSize = Mathf.Max(1, max);

        /// <summary>Enqueue raw PCM16 byte data for playback.</summary>
        public void EnqueuePcm(byte[] pcm16Data)
        {
            if (pcm16Data == null || pcm16Data.Length < 2) return;

            while (_clipQueue.Count >= _maxQueueSize)
            {
                var overflow = _clipQueue.Dequeue();
                Destroy(overflow);
            }

            float[] samples = Pcm16ToFloat(pcm16Data);
            var clip = AudioClip.Create($"IVX_AI_{_clipCounter++}", samples.Length, _channels, _sampleRate, false);
            clip.SetData(samples, 0);

            _clipQueue.Enqueue(clip);

            if (!_isPlaying)
                PlayNext();
        }

        /// <summary>Decode a base64 string to PCM16 and enqueue for playback.</summary>
        public void EnqueueBase64(string base64Audio)
        {
            if (string.IsNullOrEmpty(base64Audio)) return;

            try
            {
                byte[] pcm = Convert.FromBase64String(base64Audio);
                EnqueuePcm(pcm);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(IVXAIAudioPlayer)}] Base64 decode failed: {ex.Message}");
            }
        }

        /// <summary>Stop playback and clear the queue, destroying all buffered clips.</summary>
        public void StopAll()
        {
            while (_clipQueue.Count > 0)
                Destroy(_clipQueue.Dequeue());

            if (_source != null)
            {
                _source.Stop();
                if (_source.clip != null)
                {
                    Destroy(_source.clip);
                    _source.clip = null;
                }
            }

            _isPlaying = false;
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (!_isPlaying) return;

            if (_source != null && !_source.isPlaying)
            {
                PlayNext();
            }
        }

        #endregion

        #region Internal

        private void PlayNext()
        {
            if (_clipQueue.Count == 0)
            {
                _isPlaying = false;
                OnPlaybackFinished?.Invoke();
                return;
            }

            if (_source.clip != null)
                Destroy(_source.clip);

            var clip = _clipQueue.Dequeue();
            _source.clip = clip;
            _source.Play();

            if (!_isPlaying)
            {
                _isPlaying = true;
                OnPlaybackStarted?.Invoke();
            }
        }

        private static float[] Pcm16ToFloat(byte[] pcm)
        {
            int sampleCount = pcm.Length / 2;
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                short raw = (short)(pcm[i * 2] | (pcm[i * 2 + 1] << 8));
                samples[i] = raw / 32768f;
            }
            return samples;
        }

        #endregion
    }
}
