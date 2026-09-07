using IntelliVerseX.Bootstrap;
using UnityEngine;

/// <summary>
/// Minimal scene helper: validates <see cref="IVXBootstrapConfig"/> is assigned.
/// Prefer attaching <see cref="IVXBootstrap"/> (Control Center → Add bootstrap).
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("SDK Configuration")]
    [SerializeField]
    [Tooltip("Create via Control Center or Assets → Create → IntelliVerseX → Bootstrap Config")]
    private IVXBootstrapConfig _config;

    [SerializeField] private bool _showSdkInfo = true;

    private void Awake()
    {
        if (_config == null)
        {
            Debug.LogError("[GameBootstrap] Assign IVXBootstrapConfig (IntelliVerseX → Control Center).");
            return;
        }

        if (!_config.Validate())
        {
            Debug.LogError("[GameBootstrap] IVXBootstrapConfig is invalid. Paste a Game ID.");
            return;
        }

        if (_showSdkInfo)
        {
            Debug.Log("[GameBootstrap] Config OK: " + _config.GameName + " / " + _config.GameId
                      + ". Ensure an IVXBootstrap component is in the scene.");
        }

        var bootstrap = FindObjectOfType<IVXBootstrap>();
        if (bootstrap == null)
        {
            Debug.LogWarning("[GameBootstrap] No IVXBootstrap in scene. Use Control Center → Add bootstrap to this scene.");
        }
    }
}
