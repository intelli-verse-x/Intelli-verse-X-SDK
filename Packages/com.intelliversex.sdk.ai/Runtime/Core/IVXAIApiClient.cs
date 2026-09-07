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

        /// <summary>Create a new API client bound to the given config and coroutine host.</summary>
        /// <param name="config">AI configuration asset.</param>
        /// <param name="coroutineHost">MonoBehaviour used to run HTTP coroutines.</param>
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

        /// <summary>Create a new voice session.</summary>
        /// <param name="request">Session creation payload (persona, user, topic, language).</param>
        /// <param name="onSuccess">Callback with the created session response.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void CreateVoiceSession(IVXAICreateVoiceSessionRequest request, Action<IVXAICreateVoiceSessionResponse> onSuccess, Action<string> onError)
        {
            Post(_config.VoiceSessionsEndpoint, request, onSuccess, onError);
        }

        /// <summary>End (delete) an existing voice session.</summary>
        /// <param name="sessionId">Session to terminate.</param>
        /// <param name="onSuccess">Callback with the end-session response (includes analytics).</param>
        /// <param name="onError">Callback with an error message.</param>
        public void EndVoiceSession(string sessionId, Action<IVXAIEndSessionResponse> onSuccess, Action<string> onError)
        {
            Delete(_config.GetVoiceSessionEndpoint(sessionId), onSuccess, onError);
        }

        /// <summary>Get the current status of a voice session.</summary>
        /// <param name="sessionId">Session to query.</param>
        /// <param name="onSuccess">Callback with session status.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void GetVoiceSessionStatus(string sessionId, Action<IVXAISessionStatusResponse> onSuccess, Action<string> onError)
        {
            Get(_config.GetVoiceSessionEndpoint(sessionId), onSuccess, onError);
        }

        /// <summary>Poll for new messages in a voice session (HTTP fallback).</summary>
        /// <param name="sessionId">Session to poll.</param>
        /// <param name="onSuccess">Callback with queued messages.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void PollVoiceMessages(string sessionId, Action<IVXAIPollMessagesResponse> onSuccess, Action<string> onError)
        {
            Get(_config.GetMessagesEndpoint(sessionId), onSuccess, onError);
        }

        /// <summary>Send a user text message in a voice session.</summary>
        /// <param name="sessionId">Target session.</param>
        /// <param name="text">User text to send.</param>
        /// <param name="onSuccess">Callback on success.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void SendVoiceText(string sessionId, string text, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetTextEndpoint(sessionId), new IVXAISendTextRequest { Text = text }, onSuccess, onError);
        }

        /// <summary>Send PCM16 audio data (base64-encoded) in a voice session.</summary>
        /// <param name="sessionId">Target session.</param>
        /// <param name="pcmData">Raw PCM16 audio bytes.</param>
        /// <param name="onSuccess">Callback on success.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void SendVoiceAudio(string sessionId, byte[] pcmData, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            string base64 = Convert.ToBase64String(pcmData);
            Post(_config.GetAudioEndpoint(sessionId), new IVXAISendAudioRequest { Audio = base64 }, onSuccess, onError);
        }

        /// <summary>Commit the audio input buffer, signalling end of user speech.</summary>
        /// <param name="sessionId">Target session.</param>
        /// <param name="onSuccess">Callback on success.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void CommitVoiceAudio(string sessionId, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetAudioCommitEndpoint(sessionId), new { }, onSuccess, onError);
        }

        /// <summary>Trigger the AI to speak a specific prompt in a voice session.</summary>
        /// <param name="sessionId">Target session.</param>
        /// <param name="prompt">Text the AI should speak.</param>
        /// <param name="onSuccess">Callback on success.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void TriggerVoiceSpeech(string sessionId, string prompt, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetTriggerEndpoint(sessionId), new IVXAITriggerSpeechRequest { Prompt = prompt }, onSuccess, onError);
        }

        #endregion

        #region Host Session Endpoints

        /// <summary>Create a new AI host commentary session.</summary>
        /// <param name="request">Host session configuration payload.</param>
        /// <param name="onSuccess">Callback with the created session response.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void CreateHostSession(IVXAICreateHostSessionRequest request, Action<IVXAICreateHostSessionResponse> onSuccess, Action<string> onError)
        {
            Post(_config.HostSessionsEndpoint, request, onSuccess, onError);
        }

        /// <summary>End (delete) an existing host session.</summary>
        /// <param name="sessionId">Session to terminate.</param>
        /// <param name="onSuccess">Callback on success.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void EndHostSession(string sessionId, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Delete(_config.GetHostSessionEndpoint(sessionId), onSuccess, onError);
        }

        /// <summary>Poll for new messages in a host session.</summary>
        /// <param name="sessionId">Session to poll.</param>
        /// <param name="onSuccess">Callback with queued messages.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void PollHostMessages(string sessionId, Action<IVXAIPollMessagesResponse> onSuccess, Action<string> onError)
        {
            Get(_config.GetHostMessagesEndpoint(sessionId), onSuccess, onError);
        }

        /// <summary>Send a player text message in a host session.</summary>
        /// <param name="sessionId">Target session.</param>
        /// <param name="playerId">Identifier of the sending player.</param>
        /// <param name="text">Text to send.</param>
        /// <param name="onSuccess">Callback on success.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void SendHostText(string sessionId, string playerId, string text, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetHostTextEndpoint(sessionId), new IVXAISendTextRequest { Text = text, PlayerId = playerId }, onSuccess, onError);
        }

        /// <summary>Send a game event to the AI host.</summary>
        /// <param name="sessionId">Target session.</param>
        /// <param name="eventType">Type of game event.</param>
        /// <param name="state">Serialised game state.</param>
        /// <param name="data">Optional additional payload.</param>
        /// <param name="onSuccess">Callback on success.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void SendHostGameEvent(string sessionId, string eventType, string state, string data, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetHostEventsEndpoint(sessionId), new IVXAIHostGameEventRequest { EventType = eventType, State = state, Data = data }, onSuccess, onError);
        }

        /// <summary>Submit a player's answer to the AI host.</summary>
        /// <param name="sessionId">Target session.</param>
        /// <param name="playerId">Identifier of the answering player.</param>
        /// <param name="answerIndex">Zero-based index of the selected answer.</param>
        /// <param name="onSuccess">Callback on success.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void SubmitHostAnswer(string sessionId, string playerId, int answerIndex, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetHostAnswersEndpoint(sessionId), new IVXAIHostPlayerAnswerRequest { PlayerId = playerId, AnswerIndex = answerIndex }, onSuccess, onError);
        }

        /// <summary>Trigger the AI host to speak a specific prompt.</summary>
        /// <param name="sessionId">Target session.</param>
        /// <param name="prompt">Text the AI host should speak.</param>
        /// <param name="onSuccess">Callback on success.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void TriggerHostSpeech(string sessionId, string prompt, Action<IVXAISimpleResponse> onSuccess, Action<string> onError)
        {
            Post(_config.GetHostTriggerEndpoint(sessionId), new IVXAITriggerSpeechRequest { Prompt = prompt }, onSuccess, onError);
        }

        #endregion

        #region Entitlement & Products

        /// <summary>Check whether a user is entitled to use a persona.</summary>
        /// <param name="userId">User whose entitlements to check.</param>
        /// <param name="personaId">Optional persona to check access for.</param>
        /// <param name="onSuccess">Callback with the entitlement response.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void CheckEntitlement(string userId, string personaId, Action<IVXAIEntitlementResponse> onSuccess, Action<string> onError)
        {
            string url = _config.GetEntitlementEndpoint(userId);
            if (!string.IsNullOrEmpty(personaId))
                url += $"?persona={UnityWebRequest.EscapeURL(personaId)}";
            Get(url, onSuccess, onError);
        }

        /// <summary>Fetch available purchasable products.</summary>
        /// <param name="onSuccess">Callback with the products list.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void GetProducts(Action<IVXAIProductsResponse> onSuccess, Action<string> onError)
        {
            Get(_config.ProductsEndpoint, onSuccess, onError);
        }

        /// <summary>Fetch the list of available AI personas.</summary>
        /// <param name="onSuccess">Callback with the personas list.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void GetPersonas(Action<IVXAIPersonasResponse> onSuccess, Action<string> onError)
        {
            Get(_config.PersonasEndpoint, onSuccess, onError);
        }

        /// <summary>Submit a purchase (receipt validation) to the backend.</summary>
        /// <param name="request">Purchase receipt payload.</param>
        /// <param name="onSuccess">Callback with the purchase response.</param>
        /// <param name="onError">Callback with an error message.</param>
        public void Purchase(IVXAIPurchaseRequest request, Action<IVXAIPurchaseResponse> onSuccess, Action<string> onError)
        {
            Post(_config.PurchaseEndpoint, request, onSuccess, onError);
        }

        #endregion

        #region Health

        /// <summary>Ping the AI service health endpoint.</summary>
        /// <param name="onSuccess">Callback with the health-check response.</param>
        /// <param name="onError">Callback with an error message.</param>
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
