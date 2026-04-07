using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Generates quests, stories, items, and dialogue via the IVX content API.
    /// </summary>
    public sealed class IVXAIContentGenerator : MonoBehaviour
    {
        #region Constants

        private const string ContentTypeQuest = "quest";
        private const string ContentTypeStory = "story";
        private const string ContentTypeItem = "item";
        private const string ContentTypeDialogue = "dialogue";
        private const string ContentTypeTemplate = "template";

        #endregion

        #region Singleton

        private static IVXAIContentGenerator _instance;

        /// <summary>Singleton instance (lazy-resolved if not yet registered).</summary>
        public static IVXAIContentGenerator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<IVXAIContentGenerator>();
                return _instance;
            }
        }

        #endregion

        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private IVXAIConfig _config;

        #endregion

        #region Private Fields

        private bool _initialized;
        private string _authToken;
        private Coroutine _generationRoutine;
        private UnityWebRequest _activeRequest;

        #endregion

        #region Events

        /// <summary>Raised when generation completes successfully; argument is raw JSON content.</summary>
        public event Action<string> OnContentGenerated;

        /// <summary>Raised for streaming backends; not used for single-shot HTTP responses.</summary>
        public event Action<string> OnStreamingChunk;

        /// <summary>Raised when a request fails or JSON parsing fails.</summary>
        public event Action<string> OnError;

        #endregion

        #region Properties

        /// <summary>True while an HTTP generation request is in flight.</summary>
        public bool IsGenerating { get; private set; }

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

        #region Initialization

        /// <summary>
        /// Binds API configuration. Required before calling generate methods.
        /// </summary>
        /// <param name="config">ScriptableObject configuration; must not be null.</param>
        public void Initialize(IVXAIConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (!_config.Validate(out string err))
            {
                Debug.LogError($"[{nameof(IVXAIContentGenerator)}] Invalid config: {err}");
                _initialized = false;
                return;
            }

            _initialized = true;
        }

        /// <summary>Sets the bearer token applied to content generation HTTP requests (<c>Authorization</c> header).</summary>
        public void SetAuthToken(string token)
        {
            _authToken = token;
        }

        #endregion

        #region Public API — Generation

        /// <summary>
        /// Generates a quest from a template and optional player-specific context.
        /// </summary>
        public void GenerateQuest(IVXQuestTemplate template, string playerContext = null,
            Action<IVXGeneratedQuest> onComplete = null)
        {
            if (!_initialized || _config == null)
            {
                Fail("Not initialized", onComplete);
                return;
            }

            string prompt = BuildQuestPrompt(template);
            var request = new IVXContentGenRequest
            {
                Type = ContentTypeQuest,
                Prompt = prompt,
                Template = template,
                Context = playerContext,
                MaxTokens = 2048,
                Temperature = 0.75f
            };

            RunGenerate(request,
                json =>
                {
                    try
                    {
                        IVXGeneratedQuest q = JsonConvert.DeserializeObject<IVXGeneratedQuest>(json);
                        onComplete?.Invoke(q);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(ex.Message);
                        onComplete?.Invoke(null);
                    }
                });
        }

        /// <summary>
        /// Generates a short story or narrative passage.
        /// </summary>
        public void GenerateStory(string prompt, string genre = "fantasy", int maxWords = 500,
            Action<IVXGeneratedStory> onComplete = null)
        {
            if (!_initialized || _config == null)
            {
                Fail("Not initialized", onComplete);
                return;
            }

            string ctx = $"genre:{genre}; max_words:{maxWords}";
            var request = new IVXContentGenRequest
            {
                Type = ContentTypeStory,
                Prompt = prompt ?? string.Empty,
                Template = null,
                Context = ctx,
                MaxTokens = Mathf.Clamp(maxWords * 2, 256, 4096),
                Temperature = 0.8f
            };

            RunGenerate(request,
                json =>
                {
                    try
                    {
                        IVXGeneratedStory s = JsonConvert.DeserializeObject<IVXGeneratedStory>(json);
                        onComplete?.Invoke(s);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(ex.Message);
                        onComplete?.Invoke(null);
                    }
                });
        }

        /// <summary>
        /// Generates item flavor text, description, and optional stats.
        /// </summary>
        public void GenerateItemDescription(string itemName, string itemType, string rarity,
            Action<IVXGeneratedItem> onComplete = null)
        {
            if (!_initialized || _config == null)
            {
                Fail("Not initialized", onComplete);
                return;
            }

            string prompt =
                $"Create an RPG item. Name: {itemName}. Type: {itemType}. Rarity: {rarity}.";
            var request = new IVXContentGenRequest
            {
                Type = ContentTypeItem,
                Prompt = prompt,
                Template = null,
                Context = null,
                MaxTokens = 512,
                Temperature = 0.7f
            };

            RunGenerate(request,
                json =>
                {
                    try
                    {
                        IVXGeneratedItem item = JsonConvert.DeserializeObject<IVXGeneratedItem>(json);
                        onComplete?.Invoke(item);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(ex.Message);
                        onComplete?.Invoke(null);
                    }
                });
        }

        /// <summary>
        /// Generates a short dialogue script for the given scenario and cast.
        /// </summary>
        public void GenerateDialogue(string scenario, string[] characters,
            Action<IVXGeneratedDialogue> onComplete = null)
        {
            if (!_initialized || _config == null)
            {
                Fail("Not initialized", onComplete);
                return;
            }

            string cast = characters == null ? string.Empty : string.Join(",", characters);
            var request = new IVXContentGenRequest
            {
                Type = ContentTypeDialogue,
                Prompt = scenario ?? string.Empty,
                Template = null,
                Context = $"characters:{cast}",
                MaxTokens = 1536,
                Temperature = 0.75f
            };

            RunGenerate(request,
                json =>
                {
                    try
                    {
                        IVXGeneratedDialogue d = JsonConvert.DeserializeObject<IVXGeneratedDialogue>(json);
                        onComplete?.Invoke(d);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(ex.Message);
                        onComplete?.Invoke(null);
                    }
                });
        }

        /// <summary>
        /// Fills a structured template using AI; completed text is passed as JSON or plain string.
        /// </summary>
        public void GenerateFromTemplate(string template, Dictionary<string, string> variables,
            Action<string> onComplete = null)
        {
            if (!_initialized || _config == null)
            {
                Fail("Not initialized", onComplete);
                return;
            }

            string ctx = variables == null ? null : JsonConvert.SerializeObject(variables);
            var request = new IVXContentGenRequest
            {
                Type = ContentTypeTemplate,
                Prompt = template ?? string.Empty,
                Template = null,
                Context = ctx,
                MaxTokens = 2048,
                Temperature = 0.5f
            };

            RunGenerate(request,
                json =>
                {
                    onComplete?.Invoke(json);
                });
        }

        /// <summary>
        /// Aborts the active generation request, if any.
        /// </summary>
        public void CancelGeneration()
        {
            if (_generationRoutine != null)
            {
                StopCoroutine(_generationRoutine);
                _generationRoutine = null;
            }

            if (_activeRequest != null)
            {
                try
                {
                    _activeRequest.Abort();
                }
                catch (Exception)
                {
                    // ignored
                }

                _activeRequest.Dispose();
                _activeRequest = null;
            }

            IsGenerating = false;
        }

        #endregion

        #region Private — HTTP

        private string ContentGenerateUrl() => $"{TrimBase(_config.ApiBaseUrl)}/content/generate";

        private static string TrimBase(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return string.Empty;
            return baseUrl.TrimEnd('/');
        }

        private void RunGenerate(IVXContentGenRequest request, Action<string> onParsed)
        {
            CancelGeneration();
            IsGenerating = true;
            _generationRoutine = StartCoroutine(GenerateCoroutine(request, onParsed));
        }

        private IEnumerator GenerateCoroutine(IVXContentGenRequest request, Action<string> onParsed)
        {
            string url = ContentGenerateUrl();
            string json = JsonConvert.SerializeObject(request, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var web = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            web.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(_authToken))
                web.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            if (!string.IsNullOrEmpty(_config.ApiKey))
                web.SetRequestHeader("X-API-Key", _config.ApiKey);
            web.timeout = (int)_config.RequestTimeout;

            _activeRequest = web;
            yield return web.SendWebRequest();

            _activeRequest = null;
            IsGenerating = false;
            _generationRoutine = null;

            if (web.result != UnityWebRequest.Result.Success)
            {
                string msg = $"{web.method} {web.url} failed: {web.error}";
                Debug.LogWarning($"[{nameof(IVXAIContentGenerator)}] {msg}");
                OnError?.Invoke(msg);
                web.Dispose();
                yield break;
            }

            try
            {
                string body = web.downloadHandler.text;
                IVXContentGenResponse response = JsonConvert.DeserializeObject<IVXContentGenResponse>(body);
                if (response == null || string.IsNullOrEmpty(response.Content))
                {
                    OnError?.Invoke("Empty content response");
                    web.Dispose();
                    yield break;
                }

                OnContentGenerated?.Invoke(response.Content);
                onParsed?.Invoke(response.Content);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
            }
            finally
            {
                web.Dispose();
            }
        }

        private static string BuildQuestPrompt(IVXQuestTemplate template)
        {
            if (template == null)
                return "Generate a quest.";
            var sb = new StringBuilder();
            sb.Append("Generate a quest. ");
            if (!string.IsNullOrEmpty(template.Genre))
                sb.Append("Genre: ").Append(template.Genre).Append(". ");
            if (!string.IsNullOrEmpty(template.Difficulty))
                sb.Append("Difficulty: ").Append(template.Difficulty).Append(". ");
            if (template.RequiredElements != null && template.RequiredElements.Length > 0)
                sb.Append("Elements: ").Append(string.Join(", ", template.RequiredElements)).Append(". ");
            sb.Append("Duration minutes: ").Append(template.EstimatedDurationMinutes).Append(". ");
            if (!string.IsNullOrEmpty(template.CustomPrompt))
                sb.Append(template.CustomPrompt);
            return sb.ToString();
        }

        private void Fail<T>(string message, Action<T> onComplete)
        {
            Debug.LogWarning($"[{nameof(IVXAIContentGenerator)}] {message}");
            OnError?.Invoke(message);
            onComplete?.Invoke(default);
        }

        private void Fail(string message, Action<string> onComplete)
        {
            Debug.LogWarning($"[{nameof(IVXAIContentGenerator)}] {message}");
            OnError?.Invoke(message);
            onComplete?.Invoke(null);
        }

        #endregion
    }
}
