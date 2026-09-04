using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Hiro;

/// <summary>
/// Example scene bootstrap for the Git UPM package (<c>com.intelliversex.sdk</c>).
/// Uses modules that ship under <c>Assets/Intelli-verse-X-SDK</c> (Core + Hiro, etc.).
/// </summary>
/// <remarks>
/// AI and legacy <c>IVXBootstrap</c> live in the monorepo under <c>Assets/_IntelliVerseXSDK</c>;
/// they are not part of the UPM subtree. For the same flow with AI, open the full SDK project
/// or copy those assemblies into your game explicitly.
///
/// Setup:
/// 1. Create <see cref="IntelliVerseXConfig"/> (Assets → Create → IntelliVerse-X → Game Configuration).
/// 2. Assign it on this component.
/// 3. Optionally add <see cref="IVXHiroCoordinator"/> to the scene and initialize Hiro after auth.
/// </remarks>
public class GameBootstrap : MonoBehaviour
{
    [Header("SDK Configuration")]
    [SerializeField]
    [Tooltip("Create via: Assets → Create → IntelliVerse-X → Game Configuration")]
    private IntelliVerseXConfig _sdkConfig;

    [Header("Debug")]
    [SerializeField] private bool _showSdkInfo = true;

    private void Awake()
    {
        if (_sdkConfig == null)
        {
            Debug.LogError("[GameBootstrap] Assign IntelliVerseXConfig (see also CompleteGameBootstrap).");
            return;
        }

        if (!_sdkConfig.IsValid())
        {
            Debug.LogError("[GameBootstrap] IntelliVerseXConfig is invalid. Check Game ID and Game Name.");
            return;
        }

        IntelliVerseXManager.Initialize(_sdkConfig);
        IntelliVerseXManager.Instance.OnReady += OnSdkReady;
        IntelliVerseXManager.Instance.OnError += OnSdkError;
    }

    private void OnDestroy()
    {
        if (IntelliVerseXManager.Instance != null)
        {
            IntelliVerseXManager.Instance.OnReady -= OnSdkReady;
            IntelliVerseXManager.Instance.OnError -= OnSdkError;
        }
    }

    private void OnSdkError(string message)
    {
        Debug.LogError($"[GameBootstrap] SDK error: {message}");
    }

    private void OnSdkReady()
    {
        Debug.Log("[GameBootstrap] SDK ready.");

        if (_showSdkInfo)
            PrintSdkInfo();

        StartGame();
    }

    private void StartGame()
    {
        Debug.Log("=== GAME STARTING ===");
        Debug.Log($"Welcome, {IntelliVerseXIdentity.Username} (GameId: {IntelliVerseXIdentity.GameId})");
    }

    private void PrintSdkInfo()
    {
        Debug.Log("=== IntelliVerseX SDK (UPM sample) ===");
        Debug.Log($"SDK Version: {IntelliVerseXManager.SDKVersion}");
        Debug.Log($"Config:    {_sdkConfig.gameName} ({_sdkConfig.gameId})");
        Debug.Log($"Username:  {IntelliVerseXIdentity.Username}");
        Debug.Log($"DeviceId:  {IntelliVerseXIdentity.DeviceId}");
        Debug.Log($"GameId:    {IntelliVerseXIdentity.GameId}");
        Debug.Log("======================================");
    }

    /// <summary>
    /// Example: submit a score to a Hiro leaderboard (requires <see cref="IVXHiroCoordinator"/> initialized).
    /// </summary>
    public async void SubmitScore(long score)
    {
        var hiro = IVXHiroCoordinator.Instance;
        if (hiro == null || !hiro.IsInitialized)
            return;

        string gameId = string.IsNullOrEmpty(IntelliVerseXIdentity.GameId) ? null : IntelliVerseXIdentity.GameId;
        var result = await hiro.Leaderboards.SubmitScoreAsync("default", score, 0, null, null, gameId);
        if (result != null)
            Debug.Log($"[GameBootstrap] Score {score} submitted (rank {result.rank}).");
    }

    /// <summary>Example chat send (no moderation — add your own filter if needed).</summary>
    public void SendChat(string message)
    {
        BroadcastChat(message);
    }

    private static void BroadcastChat(string message)
    {
        Debug.Log($"[Chat] {message}");
    }
}
