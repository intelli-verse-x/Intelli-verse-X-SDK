using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Low-level HTTP client for the IVX AI REST endpoints (ai-voice and ai-host).
    /// Not intended to be used directly by game code — use <see cref="IVXAISessionManager"/> instead.
    /// </summary>
    public sealed class IVXAIApiClient
    {
        #region Private Fields

        private readonly IVXAIConfig _config;
        private string _authToken;
        private readonly MonoBehaviour _coroutineHost;

        #endregion

        #region Constructor

        public IVXAIApiClient(IVXAIConfig config, MonoBehaviour coroutineHost)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _coroutineHost = coroutineHost ?? throw new ArgumentNullException(nameof(coroutineHost));
        }

        #endregion

        #region Auth

        /// <summary>Set or update the bearer token used for all requests.</summary>
        public void SetAuthToken(string token) => _authToken = token;

        #endregion

        #region Voice Session Endpoints

        public void CreateVoiceSession(IVXAICreateVoiceSessionRequest request, Action<IVXAICreateVoiceSessionResponse> onSuccess, Action<string> onError)
        {
            Post(_config.VoiceSessionsEndpoint, request, onSuccess, onError);
        }

        public void EndVoiceSession(string sessionId, Action<IVXAIEndSessionResponse> onSuccess, Action<string> onError)
        {
            Delete(_config.GetVoiceSessionEndpoint(sessionId), onSuccess, onError);
        }

        public void GetVoiceSessionStatus(string sessionId, Action<IVXAISessionStatusResponse> onSuccess, Action<string> onError)
        {
            Get(_config.GetVoiceSessionEndpoint(sessionId), onSuccess, onError);
        }

        public void PollVoiceMessages(string sessionId, Action<IVXAIPollMessagesResponse> onSuccess, Action<string> onError)
        {
            Get(_config.GetMessagesEndpoint(sessionId), onSuccess, onError);
        }

        public void SendVoiceText(string sessionId, string text, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetTextEndpoint(sessionId), new IVXAISendTextRequest { Text = text }, onSuccess, onError);
        }

        public void SendVoiceAudio(string sessionId, byte[] pcmData, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            string base64 = Convert.ToBase64String(pcmData);
            Post(_config.GetAudioEndpoint(sessionId), new IVXAISendAudioRequest { Audio = base64 }, onSuccess, onError);
        }

        public void CommitVoiceAudio(string sessionId, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetAudioCommitEndpoint(sessionId), new { }, onSuccess, onError);
        }

        public void TriggerVoiceSpeech(string sessionId, string prompt, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetTriggerEndpoint(sessionId), new IVXAITriggerSpeechRequest { Prompt = prompt }, onSuccess, onError);
        }

        #endregion

        #region Host Session Endpoints

        public void CreateHostSession(IVXAICreateHostSessionRequest request, Action<IVXAICreateHostSessionResponse> onSuccess, Action<string> onError)
        {
            Post(_config.HostSessionsEndpoint, request, onSuccess, onError);
        }

        public void EndHostSession(string sessionId, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Delete(_config.GetHostSessionEndpoint(sessionId), onSuccess, onError);
        }

        public void PollHostMessages(string sessionId, Action<IVXAIPollMessagesResponse> onSuccess, Action<string> onError)
        {
            Get(_config.GetHostMessagesEndpoint(sessionId), onSuccess, onError);
        }

        public void SendHostText(string sessionId, string playerId, string text, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetHostTextEndpoint(sessionId), new IVXAISendTextRequest { Text = text, PlayerId = playerId }, onSuccess, onError);
        }

        public void SendHostGameEvent(string sessionId, string eventType, string state, string data, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetHostEventsEndpoint(sessionId), new IVXAIHostGameEventRequest { EventType = eventType, State = state, Data = data }, onSuccess, onError);
        }

        public void SubmitHostAnswer(string sessionId, string playerId, int answerIndex, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetHostAnswersEndpoint(sessionId), new IVXAIHostPlayerAnswerRequest { PlayerId = playerId, AnswerIndex = answerIndex }, onSuccess, onError);
        }

        public void TriggerHostSpeech(string sessionId, string prompt, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetHostTriggerEndpoint(sessionId), new IVXAITriggerSpeechRequest { Prompt = prompt }, onSuccess, onError);
        }

        #endregion

        #region Entitlement & Products

        public void CheckEntitlement(string userId, string personaId, Action<IVXAIEntitlementResponse> onSuccess, Action<string> onError)
        {
            string url = _config.GetEntitlementEndpoint(userId);
            if (!string.IsNullOrEmpty(personaId))
                url += $"?persona={UnityWebRequest.EscapeURL(personaId)}";
            Get(url, onSuccess, onError);
        }

        public void GetProducts(Action<IVXAIProductsResponse> onSuccess, Action<string> onError)
        {
            Get(_config.ProductsEndpoint, onSuccess, onError);
        }

        public void GetPersonas(Action<IVXAIPersonasResponse> onSuccess, Action<string> onError)
        {
            Get(_config.PersonasEndpoint, onSuccess, onError);
        }

        public void Purchase(IVXAIPurchaseRequest request, Action<IVXAIPurchaseResponse> onSuccess, Action<string> onError)
        {
            Post(_config.PurchaseEndpoint, request, onSuccess, onError);
        }

        #endregion

        #region Health

        public void CheckHealth(Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Get(_config.HealthEndpoint, onSuccess, onError);
        }

        #endregion

        #region HTTP Helpers

        private void Get<T>(string url, Action<T> onSuccess, Action<string> onError) where T : class
        {
            _coroutineHost.StartCoroutine(RequestCoroutine(UnityWebRequest.Get(url), onSuccess, onError));
        }

        private void Post<T>(string url, object body, Action<T> onSuccess, Action<string> onError) where T : class
        {
            string json = JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            _coroutineHost.StartCoroutine(RequestCoroutine(request, onSuccess, onError));
        }

        private void Delete<T>(string url, Action<T> onSuccess, Action<string> onError) where T : class
        {
            var request = UnityWebRequest.Delete(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            _coroutineHost.StartCoroutine(RequestCoroutine(request, onSuccess, onError));
        }

        private IEnumerator RequestCoroutine<T>(UnityWebRequest request, Action<T> onSuccess, Action<string> onError) where T : class
        {
            ApplyHeaders(request);
            request.timeout = (int)_config.RequestTimeout;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string error = $"{request.method} {request.url} failed: {request.error}";
                Log(error, true);
                onError?.Invoke(error);
                request.Dispose();
                yield break;
            }

            try
            {
                string responseText = request.downloadHandler.text;
                LogVerbose($"{request.method} {request.url} → {responseText}");
                var result = JsonConvert.DeserializeObject<T>(responseText);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Deserialization error: {ex.Message}");
            }
            finally
            {
                request.Dispose();
            }
        }

        private void ApplyHeaders(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(_authToken))
                request.SetRequestHeader("Authorization", $"Bearer {_authToken}");

            if (!string.IsNullOrEmpty(_config.ApiKey))
                request.SetRequestHeader("X-API-Key", _config.ApiKey);
        }

        #endregion

        #region Logging

        private void Log(string msg, bool isWarning = false)
        {
            if (isWarning)
                Debug.LogWarning($"[{nameof(IVXAIApiClient)}] {msg}");
            else if (_config.DebugLogging)
                Debug.Log($"[{nameof(IVXAIApiClient)}] {msg}");
        }

        private void LogVerbose(string msg)
        {
            if (_config.DebugLogging)
                Debug.Log($"[{nameof(IVXAIApiClient)}] {msg}");
        }

        #endregion
    }
}
