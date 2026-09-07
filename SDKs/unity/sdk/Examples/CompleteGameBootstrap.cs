using IntelliVerseX.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IntelliVerseX.Examples
{
    /// <summary>
    /// Example bootstrap: requires <see cref="IVXBootstrap"/> + <see cref="IVXBootstrapConfig"/>.
    /// Do not use IntelliVerseXManager.
    /// </summary>
    public class CompleteGameBootstrap : MonoBehaviour
    {
        [Header("SDK Configuration")]
        [SerializeField]
        [Tooltip("Create via Control Center or Assets → Create → IntelliVerseX → Bootstrap Config")]
        private IVXBootstrapConfig bootstrapConfig;

        [Header("Scene Management")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private async void Awake()
        {
            if (bootstrapConfig == null)
            {
                Debug.LogError("[Bootstrap] Assign IVXBootstrapConfig (IntelliVerseX → Control Center).");
                return;
            }

            if (!bootstrapConfig.Validate())
            {
                Debug.LogError("[Bootstrap] IVXBootstrapConfig invalid — paste a Game ID.");
                return;
            }

            var bootstrap = FindObjectOfType<IVXBootstrap>();
            if (bootstrap == null)
            {
                var go = new GameObject("IVXBootstrap");
                bootstrap = go.AddComponent<IVXBootstrap>();
                DontDestroyOnLoad(go);
            }

            bootstrap.ApplyConfig(bootstrapConfig);

            if (showDebugLogs)
                Debug.Log("[Bootstrap] Starting IVXBootstrap.InitializeAsync for '" + bootstrapConfig.GameName + "'...");

            bool ok = await bootstrap.InitializeAsync();
            if (ok)
                OnSDKReady();
            else
                Debug.LogError("[Bootstrap] IVXBootstrap failed (offline or auth error). Check Advanced Setup → Dependencies.");
        }

        private void OnSDKReady()
        {
            if (showDebugLogs)
                Debug.Log("[Bootstrap] SDK ready. GameId=" + bootstrapConfig.GameId);

            if (!string.IsNullOrEmpty(mainMenuSceneName)
                && Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
