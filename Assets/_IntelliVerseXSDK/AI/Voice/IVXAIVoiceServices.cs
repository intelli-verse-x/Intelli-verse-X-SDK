using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.AI
{
    #region Client data models

    /// <summary>
    /// Describes a TTS voice available from the voice service.
    /// </summary>
    [Serializable]
    public class IVXAIVoice
    {
        /// <summary>Backend voice identifier.</summary>
        public string VoiceId;
        /// <summary>Display name for UI.</summary>
        public string DisplayName;
        /// <summary>ISO language code.</summary>
        public string Language;
        /// <summary>Gender label from the provider.</summary>
        public string Gender;
        /// <summary>Optional preview audio URL.</summary>
        public string PreviewUrl;
        /// <summary>Arbitrary tags (e.g. style).</summary>
        public string[] Tags;
    }

    /// <summary>
    /// Result of speech-to-text, one-shot or final streaming utterance.
    /// </summary>
    [Serializable]
    public class IVXTranscriptionResult
    {
        /// <summary>Recognized text.</summary>
        public string Text;
        /// <summary>Detected or requested language code.</summary>
        public string Language;
        /// <summary>Confidence 0–1.</summary>
        public float Confidence;
        /// <summary>True when the utterance is finalized.</summary>
        public bool IsFinal;
    }

    #endregion

    #region API models

    /// <summary>
    /// Speech-to-text request payload.
    /// </summary>
    public class IVXSTTRequest
    {
        /// <summary>PCM16 audio, base64-encoded.</summary>
        [JsonProperty("audio_base64")]
        public string AudioBase64 { get; set; }

        /// <summary>Optional BCP-47 / ISO language hint.</summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>Sample rate in Hz.</summary>
        [JsonProperty("sample_rate")]
        public int SampleRate { get; set; }
    }

    /// <summary>
    /// Speech-to-text response.
    /// </summary>
    public class IVXSTTResponse
    {
        /// <summary>Transcribed text.</summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>Detected language.</summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>Confidence score.</summary>
        [JsonProperty("confidence")]
        public float Confidence { get; set; }
    }

    /// <summary>
    /// Text-to-speech request payload.
    /// </summary>
    public class IVXTTSRequest
    {
        /// <summary>Text to synthesize.</summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>Voice id; optional if server has a default.</summary>
        [JsonProperty("voice_id")]
        public string VoiceId { get; set; }

        /// <summary>Language code.</summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>Output sample rate.</summary>
        [JsonProperty("sample_rate")]
        public int SampleRate { get; set; }
    }

    /// <summary>
    /// Text-to-speech response with embedded audio.
    /// </summary>
    public class IVXTTSResponse
    {
        /// <summary>PCM16 audio, base64-encoded.</summary>
        [JsonProperty("audio_base64")]
        public string AudioBase64 { get; set; }

        /// <summary>Audio duration in seconds.</summary>
        [JsonProperty("duration_seconds")]
        public float DurationSeconds { get; set; }
    }

    /// <summary>
    /// One row in the voices list returned by the API.
    /// </summary>
    public class IVXVoiceListItemDto
    {
        /// <summary>Backend voice identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Human-readable voice name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>ISO or BCP-47 language code.</summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>Gender label from the provider.</summary>
        [JsonProperty("gender")]
        public string Gender { get; set; }

        /// <summary>HTTPS URL for sample audio.</summary>
        [JsonProperty("preview_url")]
        public string PreviewUrl { get; set; }

        /// <summary>Optional style or capability tags.</summary>
        [JsonProperty("tags")]
        public string[] Tags { get; set; }
    }

    /// <summary>
    /// Voices list API response.
    /// </summary>
    public class IVXVoicesResponse
    {
        /// <summary>Available voices.</summary>
        [JsonProperty("voices")]
        public List<IVXVoiceListItemDto> Voices { get; set; }
    }

    /// <summary>
    /// Language detection API response.
    /// </summary>
    public class IVXLanguageDetectResponse
    {
        /// <summary>Detected language code.</summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>Confidence 0–1.</summary>
        [JsonProperty("confidence")]
        public float Confidence { get; set; }
    }

    /// <summary>
    /// Language detection request (same shape as STT without requiring transcript).
    /// </summary>
    internal class IVXLanguageDetectRequest
    {
        /// <summary>PCM16 audio, base64-encoded.</summary>
        [JsonProperty("audio_base64")]
        public string AudioBase64 { get; set; }

        /// <summary>Sample rate in Hz.</summary>
        [JsonProperty("sample_rate")]
        public int SampleRate { get; set; }
    }

    #endregion

    /// <summary>
    /// HTTP and WebSocket helpers for speech-to-text, text-to-speech, and voice listing.
    /// </summary>
    public sealed class IVXAIVoiceServices : MonoBehaviour
    {
        #region Singleton

        private static IVXAIVoiceServices _instance;

        /// <summary>Singleton instance (lazy-resolved).</summary>
        public static IVXAIVoiceServices Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<IVXAIVoiceServices>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Fired for a finalized or one-shot transcription result.</summary>
        public event Action<IVXTranscriptionResult> OnTranscriptionResult;

        /// <summary>Streaming partial hypothesis text.</summary>
        public event Action<string> OnPartialTranscription;

        /// <summary>PCM16 audio from TTS.</summary>
        public event Action<byte[]> OnSpeechSynthesized;

        /// <summary>Errors from HTTP or WebSocket layers.</summary>
        public event Action<string> OnError;

        #endregion

        #region Properties

        /// <summary>True while streaming STT is active and the socket is connected.</summary>
        public bool IsTranscribing { get; private set; }

        /// <summary>True after <see cref="Initialize(IVXAIConfig)"/> completes successfully.</summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>Voices returned by the last <see cref="ListVoices"/> call.</summary>
        public IReadOnlyList<IVXAIVoice> AvailableVoices => _voicesReadOnly;

        #endregion

        #region Private Fields

        private IVXAIConfig _config;
        private MonoBehaviour _coroutineHost;
        private IVXAIWebSocketClient _webSocketClient;
        private readonly List<IVXAIVoice> _cachedVoices = new List<IVXAIVoice>();
        private IReadOnlyList<IVXAIVoice> _voicesReadOnly = Array.Empty<IVXAIVoice>();
        private string _authToken;
        private bool _isInitialized;
        private GameObject _wsHost;
        private bool _streamSessionRequested;
        private int _pendingStreamSampleRate;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _coroutineHost = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            StopStreamingTranscriptionInternal();

            if (_wsHost != null)
                Destroy(_wsHost);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Binds AI configuration; required before other calls.
        /// </summary>
        /// <param name="config">Configuration asset.</param>
        public void Initialize(IVXAIConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (!_config.Validate(out string err))
            {
                Debug.LogError($"[{nameof(IVXAIVoiceServices)}] Config validation failed: {err}");
                return;
            }

            _coroutineHost = this;
            _isInitialized = true;

            if (_config.DebugLogging)
                Debug.Log($"[{nameof(IVXAIVoiceServices)}] Initialized");
        }

        /// <summary>Sets the bearer token applied to voice HTTP requests.</summary>
        public void SetAuthToken(string token)
        {
            _authToken = token;
        }

        /// <summary>
        /// One-shot speech-to-text over HTTP.
        /// </summary>
        /// <param name="pcmData">PCM16 mono samples.</param>
        /// <param name="sampleRate">Sample rate (default 16000).</param>
        /// <param name="onComplete">Callback with transcript.</param>
        public void TranscribeAudio(byte[] pcmData, int sampleRate = 16000, Action<IVXTranscriptionResult> onComplete = null)
        {
            if (!_isInitialized)
            {
                ReportError("TranscribeAudio called before Initialize.");
                onComplete?.Invoke(null);
                return;
            }

            if (pcmData == null || pcmData.Length == 0)
            {
                ReportError("TranscribeAudio: empty audio.");
                onComplete?.Invoke(null);
                return;
            }

            var req = new IVXSTTRequest
            {
                AudioBase64 = Convert.ToBase64String(pcmData),
                Language = null,
                SampleRate = sampleRate
            };

            string url = $"{TrimApiBase(_config.ApiBaseUrl)}/voice/transcribe";
            _coroutineHost.StartCoroutine(PostJsonCoroutine<IVXSTTRequest, IVXSTTResponse>(url, req, (IVXSTTResponse r) =>
            {
                var result = new IVXTranscriptionResult
                {
                    Text = r?.Text ?? string.Empty,
                    Language = r?.Language ?? _config.DefaultLanguage,
                    Confidence = r?.Confidence ?? 0f,
                    IsFinal = true
                };
                OnTranscriptionResult?.Invoke(result);
                onComplete?.Invoke(result);
            }, err =>
            {
                ReportError(err);
                onComplete?.Invoke(null);
            }));
        }

        /// <summary>
        /// One-shot text-to-speech over HTTP; returns PCM16 bytes via callback and <see cref="OnSpeechSynthesized"/>.
        /// </summary>
        /// <param name="text">Text to speak.</param>
        /// <param name="voiceId">Optional voice id.</param>
        /// <param name="onAudio">PCM16 payload.</param>
        public void SynthesizeSpeech(string text, string voiceId = null, Action<byte[]> onAudio = null)
        {
            if (!_isInitialized)
            {
                ReportError("SynthesizeSpeech called before Initialize.");
                onAudio?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                ReportError("SynthesizeSpeech: empty text.");
                onAudio?.Invoke(null);
                return;
            }

            var req = new IVXTTSRequest
            {
                Text = text,
                VoiceId = voiceId,
                Language = _config.DefaultLanguage,
                SampleRate = _config.AudioSampleRate
            };

            string url = $"{TrimApiBase(_config.ApiBaseUrl)}/voice/synthesize";
            _coroutineHost.StartCoroutine(PostJsonCoroutine<IVXTTSRequest, IVXTTSResponse>(url, req, (IVXTTSResponse r) =>
            {
                byte[] pcm = null;
                if (!string.IsNullOrEmpty(r?.AudioBase64))
                {
                    try
                    {
                        pcm = Convert.FromBase64String(r.AudioBase64);
                    }
                    catch (Exception ex)
                    {
                        ReportError($"TTS base64 decode failed: {ex.Message}");
                    }
                }

                if (pcm != null)
                {
                    OnSpeechSynthesized?.Invoke(pcm);
                    onAudio?.Invoke(pcm);
                }
                else
                    onAudio?.Invoke(null);
            }, err =>
            {
                ReportError(err);
                onAudio?.Invoke(null);
            }));
        }

        /// <summary>
        /// Fetches the catalog of voices and caches <see cref="AvailableVoices"/>.
        /// </summary>
        /// <param name="onComplete">Voices list.</param>
        public void ListVoices(Action<List<IVXAIVoice>> onComplete = null)
        {
            if (!_isInitialized)
            {
                ReportError("ListVoices called before Initialize.");
                onComplete?.Invoke(new List<IVXAIVoice>());
                return;
            }

            string url = $"{TrimApiBase(_config.ApiBaseUrl)}/voice/voices";
            _coroutineHost.StartCoroutine(GetJsonCoroutine<IVXVoicesResponse>(url, response =>
            {
                _cachedVoices.Clear();
                if (response?.Voices != null)
                {
                    foreach (var v in response.Voices)
                    {
                        if (v == null)
                            continue;
                        _cachedVoices.Add(new IVXAIVoice
                        {
                            VoiceId = v.Id,
                            DisplayName = v.Name,
                            Language = v.Language,
                            Gender = v.Gender,
                            PreviewUrl = v.PreviewUrl,
                            Tags = v.Tags ?? Array.Empty<string>()
                        });
                    }
                }

                _voicesReadOnly = new ReadOnlyCollection<IVXAIVoice>(_cachedVoices);
                onComplete?.Invoke(new List<IVXAIVoice>(_cachedVoices));
            }, err =>
            {
                ReportError(err);
                onComplete?.Invoke(new List<IVXAIVoice>());
            }));
        }

        /// <summary>
        /// Detects spoken language from a short PCM16 sample.
        /// </summary>
        /// <param name="pcmData">Audio bytes.</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="onResult">Language code and confidence.</param>
        public void DetectLanguage(byte[] pcmData, int sampleRate = 16000, Action<string, float> onResult = null)
        {
            if (!_isInitialized)
            {
                ReportError("DetectLanguage called before Initialize.");
                onResult?.Invoke(null, 0f);
                return;
            }

            if (pcmData == null || pcmData.Length == 0)
            {
                ReportError("DetectLanguage: empty audio.");
                onResult?.Invoke(null, 0f);
                return;
            }

            var req = new IVXLanguageDetectRequest
            {
                AudioBase64 = Convert.ToBase64String(pcmData),
                SampleRate = sampleRate
            };

            string url = $"{TrimApiBase(_config.ApiBaseUrl)}/voice/detect-language";
            _coroutineHost.StartCoroutine(PostJsonCoroutine<IVXLanguageDetectRequest, IVXLanguageDetectResponse>(url, req, (IVXLanguageDetectResponse r) =>
            {
                onResult?.Invoke(r?.Language, r?.Confidence ?? 0f);
            }, err =>
            {
                ReportError(err);
                onResult?.Invoke(null, 0f);
            }));
        }

        /// <summary>
        /// Opens a WebSocket to the streaming STT endpoint and begins listening for JSON messages.
        /// </summary>
        /// <param name="sampleRate">Sample rate advertised to the server (for metadata only).</param>
        public void StartStreamingTranscription(int sampleRate = 16000)
        {
            if (!_isInitialized)
            {
                ReportError("StartStreamingTranscription called before Initialize.");
                return;
            }

            StopStreamingTranscriptionInternal();

            if (_wsHost == null)
            {
                _wsHost = new GameObject($"{nameof(IVXAIWebSocketClient)}_VoiceStream");
                _wsHost.transform.SetParent(transform, false);
                _webSocketClient = _wsHost.AddComponent<IVXAIWebSocketClient>();
            }

            string wsUrl = BuildVoiceStreamWebSocketUrl(_config);
            _webSocketClient.Initialize(wsUrl, headers: BuildWsHeaders());
            _webSocketClient.SetDebugLogging(_config.DebugLogging);
            _webSocketClient.OnConnected += VoiceStreamConnected;
            _webSocketClient.OnMessageReceived += HandleStreamingMessage;
            _webSocketClient.OnError += HandleWsError;
            _webSocketClient.OnDisconnected += HandleWsDisconnected;

            _streamSessionRequested = true;
            _pendingStreamSampleRate = sampleRate;
            IsTranscribing = false;

            _webSocketClient.Connect();
        }

        /// <summary>
        /// Stops streaming STT and closes the socket.
        /// </summary>
        public void StopStreamingTranscription()
        {
            StopStreamingTranscriptionInternal();
        }

        /// <summary>
        /// Sends a PCM16 chunk to the active streaming session (binary frame).
        /// </summary>
        /// <param name="pcmChunk">Raw PCM bytes.</param>
        public void FeedAudioChunk(byte[] pcmChunk)
        {
            if (!IsTranscribing || _webSocketClient == null || !_webSocketClient.IsConnected)
                return;

            if (pcmChunk == null || pcmChunk.Length == 0)
                return;

            _webSocketClient.SendBinary(pcmChunk);
        }

        #endregion

        #region Private Methods — WebSocket

        private void StopStreamingTranscriptionInternal()
        {
            _streamSessionRequested = false;

            if (_webSocketClient != null)
            {
                _webSocketClient.OnConnected -= VoiceStreamConnected;
                _webSocketClient.OnMessageReceived -= HandleStreamingMessage;
                _webSocketClient.OnError -= HandleWsError;
                _webSocketClient.OnDisconnected -= HandleWsDisconnected;
                _webSocketClient.Disconnect(false);
            }

            IsTranscribing = false;
        }

        private void VoiceStreamConnected()
        {
            if (!_streamSessionRequested || _webSocketClient == null || _config == null)
                return;

            string meta = JsonConvert.SerializeObject(new
            {
                type = "start",
                sample_rate = _pendingStreamSampleRate,
                language = _config.DefaultLanguage
            });
            _webSocketClient.Send(meta);
            IsTranscribing = true;
        }

        private void HandleWsDisconnected(string reason)
        {
            IsTranscribing = false;
            if (_config != null && _config.DebugLogging)
                Debug.Log($"[{nameof(IVXAIVoiceServices)}] WS disconnected: {reason}");
        }

        private void HandleWsError(string err)
        {
            ReportError(err);
            IsTranscribing = false;
        }

        private void HandleStreamingMessage(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            try
            {
                var jo = JObject.Parse(json);
                string type = (jo["type"] ?? jo["event"])?.ToString()?.ToLowerInvariant();

                if (type == "partial" || type == "transcript_partial")
                {
                    string text = jo["text"]?.ToString();
                    if (!string.IsNullOrEmpty(text))
                        OnPartialTranscription?.Invoke(text);
                    return;
                }

                if (type == "final" || type == "transcript" || type == "transcript_final")
                {
                    var result = new IVXTranscriptionResult
                    {
                        Text = jo["text"]?.ToString() ?? string.Empty,
                        Language = jo["language"]?.ToString() ?? _config.DefaultLanguage,
                        Confidence = jo["confidence"]?.Value<float>() ?? 0f,
                        IsFinal = true
                    };
                    OnTranscriptionResult?.Invoke(result);
                    return;
                }

                if (type == "error")
                {
                    ReportError(jo["message"]?.ToString() ?? "Streaming STT error");
                }
            }
            catch (Exception ex)
            {
                ReportError($"Streaming parse: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds <c>{ApiBaseUrl as ws}/voice/stream</c> (same host as REST <c>/voice/*</c>).
        /// </summary>
        private static string BuildVoiceStreamWebSocketUrl(IVXAIConfig config)
        {
            string baseUrl = config.ApiBaseUrl.TrimEnd('/');
            string ws = baseUrl.Replace("https://", "wss://", StringComparison.OrdinalIgnoreCase)
                .Replace("http://", "ws://", StringComparison.OrdinalIgnoreCase);
            return $"{ws}/voice/stream";
        }

        private Dictionary<string, string> BuildWsHeaders()
        {
            var h = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(_config.ApiKey))
                h["X-API-Key"] = _config.ApiKey;
            return h;
        }

        #endregion

        #region Private Methods — HTTP

        private IEnumerator PostJsonCoroutine<TReq, TRes>(string url, TReq body, Action<TRes> onSuccess, Action<string> onError)
            where TRes : class
        {
            string json = JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            using (var request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                ApplyAuthHeaders(request);
                request.timeout = (int)_config.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"{request.method} {url}: {request.error}");
                    yield break;
                }

                try
                {
                    var result = JsonConvert.DeserializeObject<TRes>(request.downloadHandler.text);
                    onSuccess?.Invoke(result);
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"Deserialize: {ex.Message}");
                }
            }
        }

        private IEnumerator GetJsonCoroutine<T>(string url, Action<T> onSuccess, Action<string> onError) where T : class
        {
            using (var request = UnityWebRequest.Get(url))
            {
                ApplyAuthHeaders(request);
                request.timeout = (int)_config.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"{request.method} {url}: {request.error}");
                    yield break;
                }

                try
                {
                    var result = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                    onSuccess?.Invoke(result);
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"Deserialize: {ex.Message}");
                }
            }
        }

        private void ApplyAuthHeaders(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(_authToken))
                request.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            if (!string.IsNullOrEmpty(_config.ApiKey))
                request.SetRequestHeader("X-API-Key", _config.ApiKey);
        }

        private static string TrimApiBase(string apiBaseUrl)
        {
            if (string.IsNullOrEmpty(apiBaseUrl))
                return string.Empty;
            return apiBaseUrl.TrimEnd('/');
        }

        private void ReportError(string message)
        {
            Debug.LogWarning($"[{nameof(IVXAIVoiceServices)}] {message}");
            OnError?.Invoke(message);
        }

        #endregion
    }
}
