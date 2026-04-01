using System;
using UnityEngine;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Captures microphone audio, converts it to PCM16 chunks, and delivers
    /// them via a callback for transmission to the AI backend.
    /// </summary>
    public sealed class IVXAIAudioRecorder : MonoBehaviour
    {
        #region Constants

        private const int RECORDING_LENGTH_SEC = 300;
        private const int CHUNK_SAMPLES = 2048;

        #endregion

        #region Private Fields

        private int _sampleRate;
        private int _channels;
        private Action<byte[]> _onAudioChunk;
        private AudioClip _micClip;
        private int _lastReadPos;
        private bool _isRecording;
        private string _micDevice;

        #endregion

        #region Properties

        public bool IsRecording => _isRecording;

        #endregion

        #region Events

        /// <summary>Fired when recording starts.</summary>
        public event Action OnRecordingStarted;

        /// <summary>Fired when recording stops.</summary>
        public event Action OnRecordingStopped;

        #endregion

        #region Initialization

        /// <summary>
        /// Configure the recorder.
        /// <paramref name="onAudioChunk"/> receives PCM16 byte arrays each frame while recording.
        /// </summary>
        public void Initialize(IVXAIConfig config, Action<byte[]> onAudioChunk)
        {
            _sampleRate = config.AudioSampleRate;
            _channels = config.AudioChannels;
            _onAudioChunk = onAudioChunk;
        }

        #endregion

        #region Public Methods

        /// <summary>Begin capturing from the default microphone.</summary>
        public void StartRecording()
        {
            if (_isRecording) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.LogWarning($"[{nameof(IVXAIAudioRecorder)}] Microphone recording not supported on WebGL.");
            return;
#else
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning($"[{nameof(IVXAIAudioRecorder)}] No microphone found.");
                return;
            }

            _micDevice = Microphone.devices[0];
            _micClip = Microphone.Start(_micDevice, true, RECORDING_LENGTH_SEC, _sampleRate);
            _lastReadPos = 0;
            _isRecording = true;
            OnRecordingStarted?.Invoke();
#endif
        }

        /// <summary>Stop capturing.</summary>
        public void StopRecording()
        {
            if (!_isRecording) return;

#if !UNITY_WEBGL || UNITY_EDITOR
            Microphone.End(_micDevice);
#endif
            _isRecording = false;
            _micClip = null;
            OnRecordingStopped?.Invoke();
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (!_isRecording || _micClip == null) return;

#if !UNITY_WEBGL || UNITY_EDITOR
            int currentPos = Microphone.GetPosition(_micDevice);
            if (currentPos == _lastReadPos) return;

            int samplesToRead;
            if (currentPos > _lastReadPos)
            {
                samplesToRead = currentPos - _lastReadPos;
            }
            else
            {
                samplesToRead = (_micClip.samples - _lastReadPos) + currentPos;
            }

            if (samplesToRead < CHUNK_SAMPLES) return;

            float[] samples = new float[samplesToRead * _channels];
            _micClip.GetData(samples, _lastReadPos);
            _lastReadPos = currentPos;

            byte[] pcm = FloatToPcm16(samples);
            _onAudioChunk?.Invoke(pcm);
#endif
        }

        private void OnDestroy()
        {
            if (_isRecording) StopRecording();
        }

        #endregion

        #region Internal

        private static byte[] FloatToPcm16(float[] samples)
        {
            byte[] pcm = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                float clamped = Mathf.Clamp(samples[i], -1f, 1f);
                short val = (short)(clamped * 32767f);
                pcm[i * 2] = (byte)(val & 0xFF);
                pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
            }
            return pcm;
        }

        #endregion
    }
}
