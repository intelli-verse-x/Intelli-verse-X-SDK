// IVXShareRateUsDemoController.cs
// Production-ready Profile & Social demo for IVX_Share&RateUs scene
// Uses static scene UI (Inspector-assigned references). No runtime UI generation.

using IntelliVerseX.Games.Social;
using IntelliVerseX.Social;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Examples
{
    /// <summary>
    /// Binds and wires Profile + Social demo UI: Profile, Stats, Share, Rate Us.
    /// Assumes static scene UI with buttons assigned in Inspector.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public sealed class IVXShareRateUsDemoController : MonoBehaviour
    {
        private const string LOG_PREFIX = "[IVX-ShareRateUsDemo]";

        [Header("Social Buttons")]
        [SerializeField] private Button _shareTextButton;
        [SerializeField] private Button _shareScreenshotButton;
        [SerializeField] private Button _rateUsButton;

        [Header("Profile (optional)")]
        [SerializeField] private Button _saveProfileButton;

        private void Awake()
        {
            EnsureShareAndRateManagers();
            ResolveReferences();
            ValidateReferences();
            BindListeners();
        }

        private void EnsureShareAndRateManagers()
        {
            if (IVXGShareManager.Instance == null)
            {
                var shareGo = new GameObject("[IVXGShareManager]");
                shareGo.AddComponent<IVXGShareManager>();
                DontDestroyOnLoad(shareGo);
            }
            if (IVXGRateAppManager.Instance == null)
            {
                var rateGo = new GameObject("[IVXGRateAppManager]");
                rateGo.AddComponent<IVXGRateAppManager>();
                DontDestroyOnLoad(rateGo);
            }
        }

        private void ResolveReferences()
        {
            if (_shareTextButton == null) _shareTextButton = FindButton("ShareTextBtn");
            if (_shareScreenshotButton == null) _shareScreenshotButton = FindButton("ShareWithScreenshotBtn");
            if (_rateUsButton == null) _rateUsButton = FindButton("RateUsBtn");
            if (_saveProfileButton == null) _saveProfileButton = FindButton("SaveChangesBtn");
        }

        private Button FindButton(string name)
        {
            var t = transform.Find($"SafeAreaContainer/MainPanel/SocialSection/{name}");
            if (t == null) t = transform.Find($"SafeAreaContainer/MainPanel/ProfileSection/{name}");
            if (t == null) t = transform.Find($"SafeAreaContainer/MainPanel/{name}");
            return t?.GetComponent<Button>();
        }

        private void ValidateReferences()
        {
            if (_shareTextButton == null) Debug.LogError(LOG_PREFIX + " ShareTextButton not assigned. Assign in Inspector or add button named ShareTextBtn.");
            if (_shareScreenshotButton == null) Debug.LogError(LOG_PREFIX + " ShareScreenshotButton not assigned. Assign in Inspector or add button named ShareWithScreenshotBtn.");
            if (_rateUsButton == null) Debug.LogError(LOG_PREFIX + " RateUsButton not assigned. Assign in Inspector or add button named RateUsBtn.");
        }

        private void BindListeners()
        {
            if (_shareTextButton != null) _shareTextButton.onClick.AddListener(OnShareText);
            if (_shareScreenshotButton != null) _shareScreenshotButton.onClick.AddListener(OnShareScreenshot);
            if (_rateUsButton != null) _rateUsButton.onClick.AddListener(OnRateUs);
            if (_saveProfileButton != null) _saveProfileButton.onClick.AddListener(OnSaveProfile);
        }

        private void OnSaveProfile()
        {
            Debug.Log(LOG_PREFIX + " Save profile clicked.");
        }

        private void OnShareText()
        {
            var share = IVXGShareManager.Instance;
            if (share != null)
                share.ShareText("Check out my game! Can you beat my score? 🎮", null, success => Debug.Log(LOG_PREFIX + " Share text: " + (success ? "OK" : "Cancelled")));
            else
                IVXShareService.ShareText("Check out my game! 🎮", null, success => Debug.Log(LOG_PREFIX + " Share: " + (success ? "OK" : "Cancelled")));
        }

        private void OnShareScreenshot()
        {
            var share = IVXGShareManager.Instance;
            if (share != null)
                share.ShareWithScreenshot("I just scored in-game! Can you beat me? 🎮", null, success => Debug.Log(LOG_PREFIX + " Share screenshot: " + (success ? "OK" : "Cancelled")));
            else
                IVXShareService.ShareTextWithScreenshot("Check out my score! 🎮", null, success => Debug.Log(LOG_PREFIX + " Share: " + (success ? "OK" : "Cancelled")));
        }

        private void OnRateUs()
        {
            var rate = IVXGRateAppManager.Instance;
            if (rate != null)
                rate.ForceShowRatePrompt();
            else
                Debug.LogWarning(LOG_PREFIX + " IVXGRateAppManager not found.");
        }
    }
}
