using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;
using IntelliVerseX.Backend;
using IntelliVerseX.Analytics;

namespace {{game_name}}.Bootstrap
{
    /// <summary>
    /// Entry point for {{game_name}}. Initializes the IntelliVerseX SDK,
    /// authenticates the player, loads Hiro systems, and transitions to
    /// the appropriate scene (Login or MainMenu).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Scene Navigation")]
        [SerializeField] private string _loginScene = "Login";
        [SerializeField] private string _mainMenuScene = "MainMenu";

        [Header("Debug")]
        [SerializeField] private bool _autoGuestLogin = true;
        [SerializeField] private bool _verboseLogging = true;

        #endregion

        #region Private Fields

        private bool _isInitialized;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            await InitializeAsync();
        }

        #endregion

        #region Initialization

        private async Task InitializeAsync()
        {
            try
            {
                Log("Initializing IntelliVerseX SDK...");

                IVXBootstrap.Instance.OnBootstrapComplete += OnBootstrapReady;
                await WaitForBootstrap();

                Log("SDK initialized. Checking auth state...");

                if (IVXAuthManager.Instance.IsAuthenticated)
                {
                    Log("Session restored — going to main menu");
                    await LoadHiroSystems();
                    InitializeSatori();
                    SceneManager.LoadScene(_mainMenuScene);
                }
                else if (_autoGuestLogin)
                {
                    Log("No session — auto guest login...");
                    await IVXAuthManager.Instance.SignInGuestAsync();
                    await LoadHiroSystems();
                    InitializeSatori();
                    SceneManager.LoadScene(_mainMenuScene);
                }
                else
                {
                    Log("No session — showing login screen");
                    SceneManager.LoadScene(_loginScene);
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameBootstrap] Init failed: {ex.Message}");
            }
        }

        private Task WaitForBootstrap()
        {
            if (IVXBootstrap.Instance.IsReady)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();
            void Handler() { tcs.TrySetResult(true); IVXBootstrap.Instance.OnBootstrapComplete -= Handler; }
            IVXBootstrap.Instance.OnBootstrapComplete += Handler;
            return tcs.Task;
        }

        private async Task LoadHiroSystems()
        {
            Log("Loading Hiro economy, achievements, streaks, energy, progression...");
            var coordinator = IVXHiroCoordinator.Instance;
            if (coordinator != null)
                await coordinator.InitializeAllAsync();
            Log("Hiro systems loaded");
        }

        private void InitializeSatori()
        {
            Log("Initializing Satori analytics + feature flags...");
            IVXSatoriClient.Instance?.IdentifyAsync();
            IVXSatoriClient.Instance?.TrackEvent("app_launch", new()
            {
                { "game_id", "{{game_id}}" },
                { "engine", "unity" },
                { "platform", Application.platform.ToString() },
            });
            Log("Satori initialized");
        }

        private void OnBootstrapReady()
        {
            Log("Bootstrap complete callback received");
        }

        #endregion

        #region Helpers

        private void Log(string message)
        {
            if (_verboseLogging)
                Debug.Log($"[GameBootstrap] {message}");
        }

        #endregion
    }
}
