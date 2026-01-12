# AI Voice Integration Guide

**IntelliVerse-X SDK**  
**Date:** January 3, 2026  
**Version:** 1.0  
**Module:** AI Voice Personas with IAP Monetization

---

## 📋 Overview

This guide shows how to integrate the AI Voice system into QuizVerse. The system provides 10 AI-powered voice personas with a complete IAP monetization layer.

### ✨ Key Features

- **10 AI Personas**: Fortune Teller, Relationship Coach, AI Teacher, Matchmaker, Party Host, Trivia Host, Motivational Coach, Career Coach, Story Teller, Health Advisor
- **Real-time Voice**: Powered by xAI Grok for natural conversations
- **IAP Integration**: Session packs, subscriptions, and consumables
- **Revenue Optimized**: Free trial → conversion → retention flow
- **Multi-language**: 18+ languages supported
- **HTTP Polling**: WebGL compatible (no WebSocket required)

### 🎯 Revenue Tiers

| Tier | Personas | Est. ARPU | IAP Model |
|------|----------|-----------|-----------|
| **Tier 1** | Fortune Teller, Relationship Coach, AI Teacher | $5-15 | Session packs + Subscription |
| **Tier 2** | Party Host, Trivia Host, Motivational Coach, Story Teller | $2-6 | Battle Pass + Packs |
| **Premium** | Matchmaker, Career Coach, Health Advisor | $8-30 | Premium sessions + Monthly |

---

## 🚀 Quick Start (5 Minutes)

### Step 1: Add Required Scripts

Create the following files in your Unity project:

**`Assets/_QuizVerse/Scripts/AIVoice/AIVoiceManager.cs`**

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Trivia.AIVoice
{
    public enum AIPersonaType
    {
        FortuneTeller,
        RelationshipCoach,
        AITeacher,
        Matchmaker,
        PartyHost,
        TriviaHost,
        MotivationalCoach,
        CareerCoach,
        StoryTeller,
        HealthAdvisor
    }

    [Serializable]
    public class SessionConfig
    {
        public string language;
        public int durationSeconds;
        public bool isPremium;
        public bool freeTrialAvailable;
        public int remainingFreeSessions;
    }

    [Serializable]
    public class SocialProof
    {
        public int readingsToday;
        public int activeUsers;
        public float averageRating;
    }

    [Serializable]
    public class CreateSessionResponse
    {
        public bool success;
        public string sessionId;
        public string persona;
        public bool isPremium;
        public int durationSeconds;
        public SessionConfig config;
        public SocialProof socialProof;
        public string error;
    }

    [Serializable]
    public class MessageResponse
    {
        public string type;
        public string audio;
        public string text;
        public long timestamp;
        public string error;
    }

    [Serializable]
    public class MessagesWrapper
    {
        public bool success;
        public MessageResponse[] messages;
        public int count;
    }

    [Serializable]
    public class EntitlementResponse
    {
        public bool success;
        public string userId;
        public bool hasSubscription;
        public bool freeTrialUsed;
        public int freeSessionsRemaining;
        public int totalSessionsCompleted;
        public bool canAccessPersona;
        public string reason;
    }

    public class AIVoiceManager : MonoBehaviour
    {
        public static AIVoiceManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string apiBaseUrl = "https://api.intelli-verse-x.ai/api/ai";
        [SerializeField] private float pollingInterval = 0.5f;

        [Header("State")]
        public string CurrentSessionId { get; private set; }
        public AIPersonaType CurrentPersona { get; private set; }
        public bool IsSessionActive { get; private set; }
        public bool IsPremiumSession { get; private set; }
        public int SessionDurationSeconds { get; private set; }

        // Events
        public event Action<string> OnVoiceAudioReceived;
        public event Action<string> OnCaptionReceived;
        public event Action<string> OnCaptionComplete;
        public event Action OnSessionStarted;
        public event Action OnSessionEnded;
        public event Action<SocialProof> OnSocialProofReceived;
        public event Action<string> OnError;
        public event Action<string> OnUpsellMessage;

        private Coroutine pollingCoroutine;
        private string userId;
        private string authToken;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize with user credentials
        /// </summary>
        public void Initialize(string userId, string authToken = null)
        {
            this.userId = userId;
            this.authToken = authToken;
            Debug.Log($"[AIVoiceManager] Initialized for user: {userId}");
        }

        /// <summary>
        /// Check if user can access a persona
        /// </summary>
        public void CheckEntitlement(AIPersonaType persona, Action<EntitlementResponse> callback)
        {
            StartCoroutine(CheckEntitlementCoroutine(persona, callback));
        }

        private IEnumerator CheckEntitlementCoroutine(AIPersonaType persona, Action<EntitlementResponse> callback)
        {
            string url = $"{apiBaseUrl}/ai-voice/entitlements/{userId}?persona={persona}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                SetAuthHeaders(request);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<EntitlementResponse>(request.downloadHandler.text);
                    callback?.Invoke(response);
                }
                else
                {
                    callback?.Invoke(new EntitlementResponse { success = false, reason = request.error });
                }
            }
        }

        /// <summary>
        /// Start an AI Voice session
        /// </summary>
        public void StartSession(
            AIPersonaType persona,
            string userName,
            string topic = null,
            string language = "en",
            Action<CreateSessionResponse> callback = null)
        {
            if (IsSessionActive)
            {
                Debug.LogWarning("[AIVoiceManager] Session already active");
                return;
            }

            StartCoroutine(CreateSessionCoroutine(persona, userName, topic, language, callback));
        }

        private IEnumerator CreateSessionCoroutine(
            AIPersonaType persona,
            string userName,
            string topic,
            string language,
            Action<CreateSessionResponse> callback)
        {
            string url = $"{apiBaseUrl}/ai-voice/sessions";

            var requestBody = new
            {
                persona = persona.ToString(),
                userId = userId,
                userName = userName,
                topic = topic,
                language = language
            };

            string jsonBody = JsonUtility.ToJson(new CreateSessionRequest
            {
                persona = persona.ToString(),
                userId = userId,
                userName = userName,
                topic = topic ?? "",
                language = language
            });

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                SetAuthHeaders(request);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<CreateSessionResponse>(request.downloadHandler.text);
                    
                    if (response.success)
                    {
                        CurrentSessionId = response.sessionId;
                        CurrentPersona = persona;
                        IsSessionActive = true;
                        IsPremiumSession = response.isPremium;
                        SessionDurationSeconds = response.durationSeconds;

                        // Start polling for messages
                        pollingCoroutine = StartCoroutine(PollMessages());

                        // Notify listeners
                        OnSessionStarted?.Invoke();
                        OnSocialProofReceived?.Invoke(response.socialProof);

                        Debug.Log($"[AIVoiceManager] Session started: {CurrentSessionId}");
                    }

                    callback?.Invoke(response);
                }
                else
                {
                    var errorResponse = new CreateSessionResponse
                    {
                        success = false,
                        error = request.error
                    };
                    callback?.Invoke(errorResponse);
                    OnError?.Invoke(request.error);
                }
            }
        }

        /// <summary>
        /// Send text message to persona
        /// </summary>
        public void SendText(string text)
        {
            if (!IsSessionActive) return;
            StartCoroutine(SendTextCoroutine(text));
        }

        private IEnumerator SendTextCoroutine(string text)
        {
            string url = $"{apiBaseUrl}/ai-voice/sessions/{CurrentSessionId}/text";

            string jsonBody = JsonUtility.ToJson(new TextRequest { text = text });

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                SetAuthHeaders(request);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[AIVoiceManager] Failed to send text: {request.error}");
                }
            }
        }

        /// <summary>
        /// Send audio to persona (for voice input)
        /// </summary>
        public void SendAudio(byte[] pcmData)
        {
            if (!IsSessionActive) return;
            StartCoroutine(SendAudioCoroutine(pcmData));
        }

        private IEnumerator SendAudioCoroutine(byte[] pcmData)
        {
            string url = $"{apiBaseUrl}/ai-voice/sessions/{CurrentSessionId}/audio";

            string base64Audio = Convert.ToBase64String(pcmData);
            string jsonBody = JsonUtility.ToJson(new AudioRequest { audio = base64Audio });

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                SetAuthHeaders(request);

                yield return request.SendWebRequest();
            }
        }

        /// <summary>
        /// Commit audio buffer (signal end of speech)
        /// </summary>
        public void CommitAudio()
        {
            if (!IsSessionActive) return;
            StartCoroutine(CommitAudioCoroutine());
        }

        private IEnumerator CommitAudioCoroutine()
        {
            string url = $"{apiBaseUrl}/ai-voice/sessions/{CurrentSessionId}/audio/commit";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                SetAuthHeaders(request);

                yield return request.SendWebRequest();
            }
        }

        /// <summary>
        /// End the current session
        /// </summary>
        public void EndSession()
        {
            if (!IsSessionActive) return;
            StartCoroutine(EndSessionCoroutine());
        }

        private IEnumerator EndSessionCoroutine()
        {
            if (pollingCoroutine != null)
            {
                StopCoroutine(pollingCoroutine);
                pollingCoroutine = null;
            }

            string url = $"{apiBaseUrl}/ai-voice/sessions/{CurrentSessionId}";

            using (UnityWebRequest request = UnityWebRequest.Delete(url))
            {
                SetAuthHeaders(request);
                yield return request.SendWebRequest();
            }

            IsSessionActive = false;
            CurrentSessionId = null;
            OnSessionEnded?.Invoke();

            Debug.Log("[AIVoiceManager] Session ended");
        }

        /// <summary>
        /// Poll for messages from the AI
        /// </summary>
        private IEnumerator PollMessages()
        {
            while (IsSessionActive)
            {
                string url = $"{apiBaseUrl}/ai-voice/sessions/{CurrentSessionId}/messages";

                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    SetAuthHeaders(request);
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var wrapper = JsonUtility.FromJson<MessagesWrapper>(request.downloadHandler.text);
                        
                        if (wrapper.success && wrapper.messages != null)
                        {
                            foreach (var msg in wrapper.messages)
                            {
                                ProcessMessage(msg);
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(pollingInterval);
            }
        }

        private void ProcessMessage(MessageResponse msg)
        {
            switch (msg.type)
            {
                case "voice_audio":
                    OnVoiceAudioReceived?.Invoke(msg.audio);
                    break;

                case "voice_caption":
                    OnCaptionReceived?.Invoke(msg.text);
                    break;

                case "voice_caption_complete":
                    OnCaptionComplete?.Invoke(msg.text);
                    break;

                case "session_ending":
                    OnUpsellMessage?.Invoke("Session ending - want more?");
                    break;

                case "session_complete":
                    EndSession();
                    break;

                case "error":
                    OnError?.Invoke(msg.error);
                    break;

                case "social_proof":
                    // Already handled at session start
                    break;

                case "scarcity_message":
                    // Handle scarcity/urgency
                    break;
            }
        }

        private void SetAuthHeaders(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
        }

        private void OnDestroy()
        {
            if (IsSessionActive)
            {
                // Fire and forget cleanup
                StartCoroutine(EndSessionCoroutine());
            }
        }
    }

    // Helper classes for JSON serialization
    [Serializable]
    public class CreateSessionRequest
    {
        public string persona;
        public string userId;
        public string userName;
        public string topic;
        public string language;
    }

    [Serializable]
    public class TextRequest
    {
        public string text;
    }

    [Serializable]
    public class AudioRequest
    {
        public string audio;
    }
}
```

### Step 2: Create IAP Manager

**`Assets/_QuizVerse/Scripts/AIVoice/AIVoiceIAPManager.cs`**

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Trivia.AIVoice
{
    [Serializable]
    public class IAPProduct
    {
        public string productId;
        public string displayName;
        public string description;
        public float price;
        public string currency;
        public string type;
        public string persona;
        public int sessionsIncluded;
        public int durationDays;
        public string badge;
        public bool isPopular;
        public int discountPercent;
    }

    [Serializable]
    public class ProductsResponse
    {
        public bool success;
        public IAPProduct[] products;
    }

    [Serializable]
    public class PurchaseResponse
    {
        public bool success;
        public string message;
        public EntitlementResponse entitlement;
        public string error;
    }

    public class AIVoiceIAPManager : MonoBehaviour
    {
        public static AIVoiceIAPManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string apiBaseUrl = "https://api.intelli-verse-x.ai/api/ai";

        // Cached products
        public List<IAPProduct> AllProducts { get; private set; } = new List<IAPProduct>();
        
        // Events
        public event Action<List<IAPProduct>> OnProductsLoaded;
        public event Action<PurchaseResponse> OnPurchaseComplete;
        public event Action<string> OnPurchaseError;

        private string userId;
        private string authToken;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(string userId, string authToken = null)
        {
            this.userId = userId;
            this.authToken = authToken;
            
            // Load products on init
            LoadProducts();
        }

        /// <summary>
        /// Load all IAP products from server
        /// </summary>
        public void LoadProducts(AIPersonaType? persona = null)
        {
            StartCoroutine(LoadProductsCoroutine(persona));
        }

        private IEnumerator LoadProductsCoroutine(AIPersonaType? persona)
        {
            string url = $"{apiBaseUrl}/ai-voice/products";
            if (persona.HasValue)
            {
                url += $"?persona={persona.Value}";
            }

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                SetAuthHeaders(request);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<ProductsResponse>(request.downloadHandler.text);
                    if (response.success)
                    {
                        AllProducts = new List<IAPProduct>(response.products);
                        OnProductsLoaded?.Invoke(AllProducts);
                        Debug.Log($"[AIVoiceIAPManager] Loaded {AllProducts.Count} products");
                    }
                }
                else
                {
                    Debug.LogError($"[AIVoiceIAPManager] Failed to load products: {request.error}");
                }
            }
        }

        /// <summary>
        /// Get products for a specific persona
        /// </summary>
        public List<IAPProduct> GetProductsForPersona(AIPersonaType persona)
        {
            return AllProducts.FindAll(p => 
                p.persona == persona.ToString() || string.IsNullOrEmpty(p.persona));
        }

        /// <summary>
        /// Get recommended products (popular/best value)
        /// </summary>
        public List<IAPProduct> GetRecommendedProducts()
        {
            return AllProducts.FindAll(p => p.isPopular || !string.IsNullOrEmpty(p.badge));
        }

        /// <summary>
        /// Process a purchase
        /// </summary>
        public void PurchaseProduct(string productId, string receiptData = null)
        {
            StartCoroutine(PurchaseCoroutine(productId, receiptData));
        }

        private IEnumerator PurchaseCoroutine(string productId, string receiptData)
        {
            string url = $"{apiBaseUrl}/ai-voice/purchase";

            var requestBody = new PurchaseRequest
            {
                userId = userId,
                productId = productId,
                receiptData = receiptData ?? "",
                platform = Application.platform == RuntimePlatform.IPhonePlayer ? "ios" : "android"
            };

            string jsonBody = JsonUtility.ToJson(requestBody);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                SetAuthHeaders(request);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<PurchaseResponse>(request.downloadHandler.text);
                    
                    if (response.success)
                    {
                        OnPurchaseComplete?.Invoke(response);
                        Debug.Log($"[AIVoiceIAPManager] Purchase successful: {productId}");
                    }
                    else
                    {
                        OnPurchaseError?.Invoke(response.message);
                    }
                }
                else
                {
                    OnPurchaseError?.Invoke(request.error);
                }
            }
        }

        private void SetAuthHeaders(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
        }
    }

    [Serializable]
    public class PurchaseRequest
    {
        public string userId;
        public string productId;
        public string receiptData;
        public string platform;
    }
}
```

### Step 3: Create UI Controller

**`Assets/_QuizVerse/Scripts/AIVoice/AIVoiceUIController.cs`**

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Trivia.AIVoice
{
    public class AIVoiceUIController : MonoBehaviour
    {
        [Header("Session UI")]
        [SerializeField] private GameObject sessionPanel;
        [SerializeField] private TextMeshProUGUI captionText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI personaNameText;
        [SerializeField] private Button endSessionButton;
        [SerializeField] private TMP_InputField userInputField;
        [SerializeField] private Button sendButton;

        [Header("Social Proof UI")]
        [SerializeField] private TextMeshProUGUI activeUsersText;
        [SerializeField] private TextMeshProUGUI ratingsText;

        [Header("Upsell UI")]
        [SerializeField] private GameObject upsellPanel;
        [SerializeField] private TextMeshProUGUI upsellMessageText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button dismissButton;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        private float sessionStartTime;
        private int sessionDuration;
        private Coroutine timerCoroutine;

        private void Start()
        {
            // Subscribe to events
            var voiceManager = AIVoiceManager.Instance;
            if (voiceManager != null)
            {
                voiceManager.OnSessionStarted += OnSessionStarted;
                voiceManager.OnSessionEnded += OnSessionEnded;
                voiceManager.OnCaptionReceived += OnCaptionReceived;
                voiceManager.OnCaptionComplete += OnCaptionComplete;
                voiceManager.OnVoiceAudioReceived += OnVoiceAudioReceived;
                voiceManager.OnSocialProofReceived += OnSocialProofReceived;
                voiceManager.OnUpsellMessage += OnUpsellMessage;
                voiceManager.OnError += OnError;
            }

            // Setup button listeners
            if (endSessionButton) endSessionButton.onClick.AddListener(OnEndSessionClicked);
            if (sendButton) sendButton.onClick.AddListener(OnSendClicked);
            if (upgradeButton) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (dismissButton) dismissButton.onClick.AddListener(OnDismissClicked);

            // Hide panels initially
            if (sessionPanel) sessionPanel.SetActive(false);
            if (upsellPanel) upsellPanel.SetActive(false);
        }

        private void OnSessionStarted()
        {
            sessionPanel?.SetActive(true);
            
            var manager = AIVoiceManager.Instance;
            sessionDuration = manager.SessionDurationSeconds;
            sessionStartTime = Time.time;
            
            if (personaNameText)
            {
                personaNameText.text = manager.CurrentPersona.ToString();
            }

            // Start timer
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(UpdateTimer());
        }

        private void OnSessionEnded()
        {
            sessionPanel?.SetActive(false);
            
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }

        private void OnCaptionReceived(string text)
        {
            if (captionText)
            {
                captionText.text += text;
            }
        }

        private void OnCaptionComplete(string text)
        {
            if (captionText)
            {
                captionText.text = text;
            }
        }

        private void OnVoiceAudioReceived(string base64Audio)
        {
            // Convert base64 to audio and play
            // Note: Implement PCM16 to AudioClip conversion
            PlayAudioFromBase64(base64Audio);
        }

        private void PlayAudioFromBase64(string base64Audio)
        {
            byte[] audioData = System.Convert.FromBase64String(base64Audio);
            
            // Convert PCM16 to float samples
            float[] samples = new float[audioData.Length / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = System.BitConverter.ToInt16(audioData, i * 2);
                samples[i] = sample / 32768f;
            }

            // Create AudioClip
            AudioClip clip = AudioClip.Create("VoiceResponse", samples.Length, 1, 16000, false);
            clip.SetData(samples, 0);

            // Play
            if (audioSource)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        private void OnSocialProofReceived(SocialProof proof)
        {
            if (activeUsersText)
            {
                activeUsersText.text = $"{proof.activeUsers} people online";
            }
            if (ratingsText)
            {
                ratingsText.text = $"⭐ {proof.averageRating:F1}/5";
            }
        }

        private void OnUpsellMessage(string message)
        {
            // Don't show for premium users
            if (AIVoiceManager.Instance.IsPremiumSession) return;

            if (upsellPanel)
            {
                upsellPanel.SetActive(true);
                if (upsellMessageText) upsellMessageText.text = message;
            }
        }

        private void OnError(string error)
        {
            Debug.LogError($"[AIVoiceUI] Error: {error}");
            // Show error toast/notification
        }

        private void OnEndSessionClicked()
        {
            AIVoiceManager.Instance?.EndSession();
        }

        private void OnSendClicked()
        {
            if (userInputField && !string.IsNullOrEmpty(userInputField.text))
            {
                AIVoiceManager.Instance?.SendText(userInputField.text);
                userInputField.text = "";
            }
        }

        private void OnUpgradeClicked()
        {
            upsellPanel?.SetActive(false);
            // Show IAP store
            ShowIAPStore();
        }

        private void OnDismissClicked()
        {
            upsellPanel?.SetActive(false);
        }

        private void ShowIAPStore()
        {
            // Implement your IAP store UI
            Debug.Log("Show IAP Store");
        }

        private IEnumerator UpdateTimer()
        {
            while (AIVoiceManager.Instance.IsSessionActive)
            {
                float elapsed = Time.time - sessionStartTime;
                float remaining = sessionDuration - elapsed;

                if (remaining <= 0)
                {
                    if (timerText) timerText.text = "0:00";
                    yield break;
                }

                int minutes = Mathf.FloorToInt(remaining / 60);
                int seconds = Mathf.FloorToInt(remaining % 60);
                
                if (timerText)
                {
                    timerText.text = $"{minutes}:{seconds:D2}";
                    
                    // Warning color when low
                    if (remaining <= 15)
                    {
                        timerText.color = Color.red;
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void OnDestroy()
        {
            var voiceManager = AIVoiceManager.Instance;
            if (voiceManager != null)
            {
                voiceManager.OnSessionStarted -= OnSessionStarted;
                voiceManager.OnSessionEnded -= OnSessionEnded;
                voiceManager.OnCaptionReceived -= OnCaptionReceived;
                voiceManager.OnCaptionComplete -= OnCaptionComplete;
                voiceManager.OnVoiceAudioReceived -= OnVoiceAudioReceived;
                voiceManager.OnSocialProofReceived -= OnSocialProofReceived;
                voiceManager.OnUpsellMessage -= OnUpsellMessage;
                voiceManager.OnError -= OnError;
            }
        }
    }
}
```

---

## 📦 IAP Product Configuration

### Configure in Unity IAP

Add these products to your IAP configuration:

```yaml
# IVXIAPConfig.asset - AI Voice Products

Products:
  # Fortune Teller (Tier 1)
  - productId: fortune_single_reading
    displayName: "1 Fortune Reading"
    price: 0.99
    type: Consumable
    
  - productId: fortune_5_pack
    displayName: "5 Readings Pack"
    price: 2.99
    badge: "POPULAR"
    
  - productId: fortune_mystic_pass
    displayName: "Mystic Pass"
    price: 4.99
    type: Subscription
    duration: Monthly

  # Matchmaker (Premium)
  - productId: matchmaker_consultation
    displayName: "1 Love Consultation"
    price: 4.99
    
  - productId: matchmaker_monthly
    displayName: "Love Expert Monthly"
    price: 19.99
    type: Subscription
    badge: "PREMIUM"

  # All-Access
  - productId: ai_voice_all_access
    displayName: "All-Access Pass"
    price: 14.99
    type: Subscription
    badge: "BEST VALUE"
```

---

## 🎯 Usage Examples

### Start a Fortune Teller Session

```csharp
using Trivia.AIVoice;

public class FortuneTellerButton : MonoBehaviour
{
    public void OnFortuneTellerClicked()
    {
        var manager = AIVoiceManager.Instance;
        
        // Check entitlement first
        manager.CheckEntitlement(AIPersonaType.FortuneTeller, (entitlement) =>
        {
            if (entitlement.canAccessPersona)
            {
                // Start session
                manager.StartSession(
                    AIPersonaType.FortuneTeller,
                    "John",  // User's name
                    "Will I find love this year?",  // Topic/question
                    "en",  // Language
                    (response) =>
                    {
                        if (response.success)
                        {
                            Debug.Log($"Session started: {response.sessionId}");
                            // Show session UI
                        }
                        else
                        {
                            Debug.LogError($"Failed: {response.error}");
                        }
                    }
                );
            }
            else
            {
                // Show paywall
                ShowPaywall(AIPersonaType.FortuneTeller, entitlement.reason);
            }
        });
    }
    
    private void ShowPaywall(AIPersonaType persona, string reason)
    {
        // Show your IAP store filtered by this persona
        AIVoiceIAPManager.Instance.LoadProducts(persona);
        // Open store UI
    }
}
```

### Handle Voice Responses with Audio

```csharp
public class VoiceAudioHandler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    
    private void Start()
    {
        AIVoiceManager.Instance.OnVoiceAudioReceived += PlayAudio;
    }
    
    private void PlayAudio(string base64Audio)
    {
        byte[] audioData = Convert.FromBase64String(base64Audio);
        
        // PCM16 to float conversion
        float[] samples = new float[audioData.Length / 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short sample = BitConverter.ToInt16(audioData, i * 2);
            samples[i] = sample / 32768f;
        }
        
        AudioClip clip = AudioClip.Create("Voice", samples.Length, 1, 16000, false);
        clip.SetData(samples, 0);
        
        audioSource.clip = clip;
        audioSource.Play();
    }
}
```

---

## 🔧 API Reference

### REST Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/ai/ai-voice/personas` | List available personas |
| GET | `/api/ai/ai-voice/products?persona=X` | Get IAP products |
| GET | `/api/ai/ai-voice/entitlements/{userId}` | Check user access |
| POST | `/api/ai/ai-voice/purchase` | Process purchase |
| POST | `/api/ai/ai-voice/sessions` | Create session |
| GET | `/api/ai/ai-voice/sessions/{id}` | Get session status |
| GET | `/api/ai/ai-voice/sessions/{id}/messages` | Poll messages |
| POST | `/api/ai/ai-voice/sessions/{id}/text` | Send text |
| POST | `/api/ai/ai-voice/sessions/{id}/audio` | Send audio |
| DELETE | `/api/ai/ai-voice/sessions/{id}` | End session |

### Message Types

| Type | Description |
|------|-------------|
| `voice_audio` | Base64 PCM16 audio from AI |
| `voice_caption` | Partial text caption |
| `voice_caption_complete` | Complete text response |
| `session_ending` | Session time running out |
| `session_complete` | Session ended |
| `social_proof` | Active users/ratings data |
| `scarcity_message` | Urgency/limited time message |
| `error` | Error occurred |

---

## 📊 Revenue Optimization Checklist

### ✅ Free Trial Flow
- [ ] First session FREE for all users
- [ ] Show value before asking for money
- [ ] Social proof displayed (active users, ratings)
- [ ] Scarcity messaging (limited time offers)

### ✅ Conversion Points
- [ ] Upsell at 45 seconds into free session
- [ ] Time warning at 15 seconds remaining
- [ ] Session end with "want more?" prompt
- [ ] Daily notification for return users

### ✅ Pricing Strategy
- [ ] $0.99 impulse buy (single session)
- [ ] $2.99-4.99 value packs (5-10 sessions)
- [ ] $4.99-9.99/month subscriptions
- [ ] $14.99/month all-access bundle

### ✅ Retention Hooks
- [ ] Daily free session (ad-supported)
- [ ] Push notifications for fortunes
- [ ] Personalization (remember name, preferences)
- [ ] Streak rewards

---

## 📈 Expected Revenue

| MAU | Conversion Rate | ARPU | Monthly Revenue |
|-----|-----------------|------|-----------------|
| 1,000 | 3% | $5 | $150 |
| 10,000 | 3% | $8 | $2,400 |
| 50,000 | 4% | $10 | $20,000 |
| 100,000 | 5% | $12 | $60,000 |

---

## 🐛 Troubleshooting

### "Session not found"
- Check session ID is correct
- Session may have expired (timeout)
- Ensure polling is active

### "No entitlement"
- User needs to purchase or use free trial
- Check subscription expiry
- Verify user ID matches

### Audio not playing
- Ensure AudioSource is configured
- Check audio format (PCM16, 16kHz)
- Verify base64 decoding

---

## 📚 Additional Resources

- [IAP Integration Guide](./IAP_INTEGRATION_GUIDE.md)
- [xAI Realtime API Docs](https://docs.x.ai/realtime)
- [Unity IAP Documentation](https://docs.unity3d.com/Manual/UnityIAP.html)

---

**Status:** 🚀 Production Ready  
**Integration Time:** 2-4 hours  
**Revenue Potential:** $2,000 - $60,000+/month (based on MAU)

🎤 **Start monetizing with AI Voice today!**

