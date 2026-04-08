using UnityEngine;
using IntelliVerseX.AI;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Hiro;

/// <summary>
/// Example game bootstrap for IntelliVerseX SDK integration.
/// Shows the minimal code needed to integrate any game with the SDK.
///
/// Steps to integrate your game:
/// 1. Add the IVX_Bootstrap.prefab to your first scene (Assets → _IntelliVerseXSDK → Bootstrap)
/// 2. Configure IVXBootstrapConfig with your Nakama server details
/// 3. (Optional) Create an IVXAIConfig asset for AI features
/// 4. Attach this script to a GameObject in your first scene
/// 5. Implement your game logic in StartGame()
///
/// That's it! Your game now has:
/// - Unified identity (Nakama + optional Cognito)
/// - Hiro economy, leaderboards, and inventory
/// - AI NPC Dialog, Moderation, Content Generation
/// - Discord Social SDK integration
/// - Analytics via Satori
/// - Multiplayer via Game Modes
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("AI Configuration (Optional)")]
    [Tooltip("Assign your IVXAIConfig asset to enable AI features")]
    [SerializeField] private IVXAIConfig aiConfig;

    [Header("Debug")]
    [SerializeField] private bool showSDKInfo = true;

    private void OnEnable()
    {
        IVXBootstrap.Instance.OnBootstrapComplete += OnSDKReady;
    }

    private void OnDisable()
    {
        if (IVXBootstrap.Instance != null)
            IVXBootstrap.Instance.OnBootstrapComplete -= OnSDKReady;
    }

    private void OnSDKReady(bool success)
    {
        Debug.Log("[GameBootstrap] SDK Ready!");

        if (showSDKInfo)
            PrintSDKInfo();

        if (aiConfig != null)
            InitializeAI();

        StartGame();
    }

    private void InitializeAI()
    {
        string token = IVXBootstrap.Instance.AuthToken;
        string userId = IVXBootstrap.Instance.UserId;

        IVXAINPCDialogManager.Instance?.Initialize(aiConfig);
        IVXAINPCDialogManager.Instance?.SetAuthToken(token);

        IVXAIAssistant.Instance?.Initialize(aiConfig);
        IVXAIAssistant.Instance?.SetAuthToken(token);

        IVXAIModerator.Instance?.Initialize(aiConfig);
        IVXAIModerator.Instance?.SetAuthToken(token);

        IVXAIContentGenerator.Instance?.Initialize(aiConfig);
        IVXAIContentGenerator.Instance?.SetAuthToken(token);

        if (!string.IsNullOrEmpty(userId))
        {
            IVXAIProfiler.Instance?.Initialize(aiConfig, userId);
            IVXAIProfiler.Instance?.SetAuthToken(token);
            IVXAIProfiler.Instance?.StartAutoTracking();
        }

        IVXAIVoiceServices.Instance?.Initialize(aiConfig);
        IVXAIVoiceServices.Instance?.SetAuthToken(token);

        Debug.Log("[GameBootstrap] AI managers initialized.");
    }

    private void StartGame()
    {
        Debug.Log("=== GAME STARTING ===");
        Debug.Log($"Welcome, Player {IVXBootstrap.Instance.UserId}!");

        // Your game logic here:
        // SceneManager.LoadScene("MainMenu");
    }

    private void PrintSDKInfo()
    {
        var bootstrap = IVXBootstrap.Instance;
        Debug.Log("=== IntelliVerseX SDK v5.8.0 ===");
        Debug.Log($"User ID:    {bootstrap.UserId}");
        Debug.Log($"User Name:  {bootstrap.UserName}");
        Debug.Log($"AI Config:  {(aiConfig != null ? aiConfig.Provider.ToString() : "None")}");
        Debug.Log($"Mock Mode:  {(aiConfig != null ? aiConfig.MockMode.ToString() : "N/A")}");
        Debug.Log("================================");
    }

    // Example: Submit score to leaderboard (replace leaderboard id with your Hiro leaderboard id)
    public async void SubmitScore(long score)
    {
        var hiro = IVXHiroCoordinator.Instance;
        if (hiro == null || !hiro.IsInitialized) return;
        string gameId = string.IsNullOrEmpty(IVXBootstrap.Instance.GameId) ? null : IVXBootstrap.Instance.GameId;
        var result = await hiro.Leaderboards.SubmitScoreAsync("default", score, 0, null, null, gameId);
        if (result != null)
            Debug.Log($"Score {score} submitted! (rank {result.rank})");
    }

    // Example: Moderate chat before sending
    public void SendChat(string message)
    {
        if (IVXAIModerator.Instance == null || !IVXAIModerator.Instance.IsEnabled)
        {
            BroadcastChat(message);
            return;
        }

        IVXAIModerator.Instance.FilterMessage(message, filtered =>
        {
            if (!string.IsNullOrEmpty(filtered))
                BroadcastChat(filtered);
        });
    }

    private void BroadcastChat(string message)
    {
        Debug.Log($"[Chat] {message}");
    }
}
