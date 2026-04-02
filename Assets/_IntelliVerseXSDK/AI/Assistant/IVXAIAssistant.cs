using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.AI
{
    #region Data Models

    /// <summary>
    /// Serializable snapshot of in-game state sent to the assistant API for grounded answers.
    /// </summary>
    [Serializable]
    public class IVXAIGameContext
    {
        #region Fields

        /// <summary>Current level or map identifier.</summary>
        public string CurrentLevel;

        /// <summary>What the player is trying to accomplish right now.</summary>
        public string CurrentObjective;

        /// <summary>Coarse game phase label, e.g. tutorial or mid_game.</summary>
        public string GamePhase;

        /// <summary>Inventory item ids or display names.</summary>
        public string[] Inventory;

        /// <summary>Numeric stats (health, score, etc.) keyed by name.</summary>
        public Dictionary<string, float> PlayerStats;

        /// <summary>Additional JSON payload for custom integrations.</summary>
        public string CustomContext;

        #endregion

        /// <summary>Creates an empty context with initialized collections.</summary>
        public IVXAIGameContext()
        {
            Inventory = Array.Empty<string>();
            PlayerStats = new Dictionary<string, float>();
        }
    }

    /// <summary>JSON response from the general <c>assistant/ask</c> endpoint.</summary>
    [Serializable]
    public class IVXAIAssistantResponse
    {
        #region API Fields

        /// <summary>Full natural-language answer.</summary>
        [JsonProperty("response")]
        public string Response;

        /// <summary>Optional citation or document ids supporting the answer.</summary>
        [JsonProperty("sources")]
        public string[] Sources;

        /// <summary>Model self-reported confidence in [0,1] if provided.</summary>
        [JsonProperty("confidence")]
        public float Confidence;

        /// <summary>True when the reply was produced in streaming mode on the server.</summary>
        [JsonProperty("is_streaming")]
        public bool IsStreaming;

        #endregion
    }

    /// <summary>JSON response from the contextual hint endpoint.</summary>
    [Serializable]
    public class IVXAIHintResponse
    {
        #region API Fields

        /// <summary>Short hint text.</summary>
        [JsonProperty("hint")]
        public string Hint;

        /// <summary>Difficulty or spoiler level of the hint.</summary>
        [JsonProperty("difficulty_level")]
        public string DifficultyLevel;

        /// <summary>Whether another hint can be requested.</summary>
        [JsonProperty("next_hint_available")]
        public bool NextHintAvailable;

        #endregion
    }

    /// <summary>One step in a guided tutorial sequence.</summary>
    [Serializable]
    public class IVXAITutorialStep
    {
        #region API Fields

        /// <summary>1-based or 0-based step index from the server.</summary>
        [JsonProperty("step_number")]
        public int StepNumber;

        /// <summary>Short title for the step.</summary>
        [JsonProperty("title")]
        public string Title;

        /// <summary>Body copy explaining the step.</summary>
        [JsonProperty("description")]
        public string Description;

        /// <summary>Optional UI or input action id required to advance.</summary>
        [JsonProperty("action_required")]
        public string ActionRequired;

        #endregion
    }

    /// <summary>JSON response describing a full tutorial flow for a feature.</summary>
    [Serializable]
    public class IVXAITutorialResponse
    {
        #region API Fields

        /// <summary>Feature or screen identifier.</summary>
        [JsonProperty("feature_id")]
        public string FeatureId;

        /// <summary>Ordered tutorial steps.</summary>
        [JsonProperty("steps")]
        public List<IVXAITutorialStep> Steps;

        /// <summary>Estimated duration in seconds.</summary>
        [JsonProperty("estimated_time_seconds")]
        public int EstimatedTimeSeconds;

        #endregion
    }

    #endregion

    /// <summary>
    /// In-game AI assistant: hints, tutorials, Q&amp;A, and knowledge search against the IVX assistant HTTP API.
    /// </summary>
    public sealed class IVXAIAssistant : MonoBehaviour
    {
        #region Singleton

        private static IVXAIAssistant _instance;

        /// <summary>Singleton instance (lazy-resolved if not yet assigned).</summary>
        public static IVXAIAssistant Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<IVXAIAssistant>();
                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private IVXAIConfig _config;
        private string _authToken;
        private readonly List<string> _conversationLines = new List<string>();
        private bool _isProcessing;

        #endregion

        #region Events

        /// <summary>Fired when a full assistant response is available.</summary>
        public event Action<string> OnResponseReceived;

        /// <summary>Fired when the server marks the reply as streaming (<see cref="IVXAIAssistantResponse.IsStreaming"/>); emits the same text as the full response for single-shot HTTP responses.</summary>
        public event Action<string> OnStreamingChunk;

        /// <summary>Fired on network or parse errors.</summary>
        public event Action<string> OnError;

        #endregion

        #region Properties

        /// <summary>True while a request is in flight.</summary>
        public bool IsProcessing => _isProcessing;

        /// <summary>Optional system prompt overriding default assistant behaviour (sent on each ask).</summary>
        public string SystemPrompt { get; set; }

        /// <summary>True after <see cref="Initialize"/> succeeds.</summary>
        public bool IsInitialized => _config != null;

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
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region Public Methods — Initialization

        /// <summary>
        /// Binds AI configuration for assistant HTTP calls (base URL, keys, timeouts, auth headers).
        /// </summary>
        /// <param name="config">ScriptableObject with API base URL, keys, and timeouts.</param>
        public void Initialize(IVXAIConfig config)
        {
            if (config == null)
            {
                Debug.LogError($"[{nameof(IVXAIAssistant)}] Config is null.");
                return;
            }

            if (!config.Validate(out string err))
            {
                Debug.LogError($"[{nameof(IVXAIAssistant)}] Invalid config: {err}");
                return;
            }

            _config = config;
        }

        /// <summary>Sets the bearer token applied to assistant HTTP requests (<c>Authorization</c> header).</summary>
        public void SetAuthToken(string token)
        {
            _authToken = token;
        }

        /// <summary>Clears locally tracked conversation lines used with <see cref="Ask"/>.</summary>
        public void ClearHistory()
        {
            _conversationLines.Clear();
        }

        /// <summary>Sets <see cref="SystemPrompt"/> (same as assigning the property).</summary>
        public void SetSystemPrompt(string prompt)
        {
            SystemPrompt = prompt;
        }

        #endregion

        #region Public Methods — API

        /// <summary>
        /// Asks a general question with optional game context; invokes the callback with the parsed response.
        /// </summary>
        public void Ask(string question, IVXAIGameContext context = null, Action<IVXAIAssistantResponse> onComplete = null)
        {
            if (!EnsureReady())
                return;

            if (string.IsNullOrEmpty(question))
            {
                RaiseError("Ask: question is empty.");
                return;
            }

            var body = new IVXAIAssistantAskRequest
            {
                Question = question,
                GameContext = context,
                SystemPrompt = SystemPrompt,
                Conversation = _conversationLines.Count > 0 ? _conversationLines.ToArray() : null
            };

            string url = _config.ApiBaseUrl.TrimEnd('/') + "/assistant/ask";
            _isProcessing = true;
            PostJson(url, body, (IVXAIAssistantResponse res) =>
            {
                _isProcessing = false;
                if (res != null && !string.IsNullOrEmpty(res.Response))
                {
                    if (res.IsStreaming)
                        OnStreamingChunk?.Invoke(res.Response);
                    _conversationLines.Add($"user: {question}");
                    _conversationLines.Add($"assistant: {res.Response}");
                    OnResponseReceived?.Invoke(res.Response);
                }

                onComplete?.Invoke(res);
            }, err =>
            {
                _isProcessing = false;
                RaiseError(err);
            });
        }

        /// <summary>
        /// Requests a contextual hint for the current level/objective.
        /// </summary>
        public void GetHint(string levelId, string objectiveId, IVXAIGameContext context = null, Action<IVXAIHintResponse> onComplete = null)
        {
            if (!EnsureReady())
                return;

            var body = new IVXAIHintRequest
            {
                LevelId = levelId,
                ObjectiveId = objectiveId,
                GameContext = context,
                SystemPrompt = SystemPrompt
            };

            string url = _config.ApiBaseUrl.TrimEnd('/') + "/assistant/hint";
            _isProcessing = true;
            PostJson(url, body, (IVXAIHintResponse res) =>
            {
                _isProcessing = false;
                onComplete?.Invoke(res);
            }, err =>
            {
                _isProcessing = false;
                RaiseError(err);
            });
        }

        /// <summary>
        /// Fetches a structured tutorial for a feature id.
        /// </summary>
        public void GetTutorial(string featureId, Action<IVXAITutorialResponse> onComplete = null)
        {
            if (!EnsureReady())
                return;

            if (string.IsNullOrEmpty(featureId))
            {
                RaiseError("GetTutorial: featureId is empty.");
                return;
            }

            var body = new IVXAITutorialRequest
            {
                FeatureId = featureId,
                SystemPrompt = SystemPrompt
            };

            string url = _config.ApiBaseUrl.TrimEnd('/') + "/assistant/tutorial";
            _isProcessing = true;
            PostJson(url, body, (IVXAITutorialResponse res) =>
            {
                _isProcessing = false;
                onComplete?.Invoke(res);
            }, err =>
            {
                _isProcessing = false;
                RaiseError(err);
            });
        }

        /// <summary>
        /// Performs a knowledge-base (RAG) search and returns result snippets or ids.
        /// </summary>
        public void SearchKnowledgeBase(string query, Action<string[]> onResults = null)
        {
            if (!EnsureReady())
                return;

            if (string.IsNullOrEmpty(query))
            {
                RaiseError("SearchKnowledgeBase: query is empty.");
                return;
            }

            var body = new IVXAIAssistantSearchRequest
            {
                Query = query,
                SystemPrompt = SystemPrompt
            };

            string url = _config.ApiBaseUrl.TrimEnd('/') + "/assistant/search";
            _isProcessing = true;
            PostJson(url, body, (IVXAIAssistantSearchResponse res) =>
            {
                _isProcessing = false;
                string[] results = res?.Results ?? Array.Empty<string>();
                onResults?.Invoke(results);
            }, err =>
            {
                _isProcessing = false;
                RaiseError(err);
            });
        }

        #endregion

        #region Private Methods — HTTP

        private void PostJson<T>(string url, object body, Action<T> onSuccess, Action<string> onError) where T : class
        {
            string json = JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            StartCoroutine(RequestCoroutine(request, onSuccess, onError));
        }

        private IEnumerator RequestCoroutine<T>(UnityWebRequest request, Action<T> onSuccess, Action<string> onError) where T : class
        {
            ApplyHeaders(request);
            request.timeout = (int)_config.RequestTimeout;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{request.method} {request.url} failed: {request.error}");
                request.Dispose();
                yield break;
            }

            try
            {
                string text = request.downloadHandler.text;
                if (_config.DebugLogging)
                    Debug.Log($"[{nameof(IVXAIAssistant)}] {request.method} {request.url} → {text}");

                var result = JsonConvert.DeserializeObject<T>(text);
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

            if (_config != null && !string.IsNullOrEmpty(_config.ApiKey))
                request.SetRequestHeader("X-API-Key", _config.ApiKey);
        }

        private bool EnsureReady()
        {
            if (_config != null)
                return true;
            RaiseError($"{nameof(IVXAIAssistant)} is not initialized. Call Initialize first.");
            return false;
        }

        private void RaiseError(string message)
        {
            Debug.LogWarning($"[{nameof(IVXAIAssistant)}] {message}");
            OnError?.Invoke(message);
        }

        #endregion

        #region Request DTOs

        [Serializable]
        private class IVXAIAssistantAskRequest
        {
            [JsonProperty("question")] public string Question;
            [JsonProperty("game_context")] public IVXAIGameContext GameContext;
            [JsonProperty("system_prompt")] public string SystemPrompt;
            [JsonProperty("conversation")] public string[] Conversation;
        }

        [Serializable]
        private class IVXAIHintRequest
        {
            [JsonProperty("level_id")] public string LevelId;
            [JsonProperty("objective_id")] public string ObjectiveId;
            [JsonProperty("game_context")] public IVXAIGameContext GameContext;
            [JsonProperty("system_prompt")] public string SystemPrompt;
        }

        [Serializable]
        private class IVXAITutorialRequest
        {
            [JsonProperty("feature_id")] public string FeatureId;
            [JsonProperty("system_prompt")] public string SystemPrompt;
        }

        [Serializable]
        private class IVXAIAssistantSearchRequest
        {
            [JsonProperty("query")] public string Query;
            [JsonProperty("system_prompt")] public string SystemPrompt;
        }

        [Serializable]
        private class IVXAIAssistantSearchResponse
        {
            [JsonProperty("results")] public string[] Results;
        }

        #endregion
    }
}
