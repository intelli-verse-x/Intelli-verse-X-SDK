// IVXIntroController.cs
// IntelliVerseX SDK - Universal Intro Scene Controller
// DOTween is optional - uses fallback if not installed

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.Core;

#if DOTWEEN || DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace IntelliVerseX.UI
{
    /// <summary>
    /// Universal intro scene controller for IntelliVerse-X SDK.
    /// Automatically uses game configuration for branding and navigation.
    /// 
    /// Setup:
    /// 1. Add IVX_IntroScene prefab to your first scene
    /// 2. Configure nextSceneName (e.g., "LoginScene" or "MainMenu")
    /// 3. Optional: Customize colors, durations, logo sprites
    /// 
    /// Features:
    /// - Skip with any key press or mouse click
    /// - Automatic game branding from IntelliVerseXConfig
    /// - Smooth animations with DOTween (optional)
    /// - Audio support (optional)
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/UI/Intro Controller")]
    public class IVXIntroController : MonoBehaviour
    {
        [Header("Scene Navigation")]
        [Tooltip("Name of the scene to load after intro (e.g., 'LoginScene', 'MainMenu')")]
        public string nextSceneName = "LoginScene";
        
        [Header("UI Elements")]
        [Tooltip("IntelliVerse-X logo (animated)")]
        public RectTransform xLogo;
        
        [Tooltip("IntelliVerse-X logo canvas group (for fading)")]
        public CanvasGroup xLogoGroup;
        
        [Tooltip("IntelliVerse text/image")]
        public RectTransform intelliVerseText;
        
        [Tooltip("IntelliVerse canvas group (for fading)")]
        public CanvasGroup intelliVerseGroup;
        
        [Tooltip("Tagline canvas group (for fading)")]
        public CanvasGroup taglineGroup;
        
        [Tooltip("Game name text (auto-populated from config)")]
        public TextMeshProUGUI gameNameText;
        
        [Tooltip("Optional extra effects")]
        public GameObject extraEffect;
        
        [Header("Visual Effects")]
        [Tooltip("Black overlay for fade in/out")]
        public CanvasGroup blackOverlayGroup;
        
        [Tooltip("Glow effect")]
        public GameObject glowEffect;
        
        [Header("Audio (Optional)")]
        [Tooltip("Audio source for intro music")]
        public AudioSource sfxSource;
        
        [Tooltip("Intro music clip")]
        public AudioClip introMusic;
        
        [Header("Timing")]
        [Tooltip("Total intro duration in seconds")]
        public float totalIntroDuration = 9f;
        
        [Tooltip("Enable skip with any key/click")]
        public bool allowSkip = true;
        
        [Header("Animation Durations")]
        public float xDropDuration = 1.4f;
        public float xSlideRightDuration = 0.8f;
        public float intelliVerseDelay = 2.4f;
        public float intelliVerseSlideDuration = 0.6f;
        public float intelliVerseFadeDuration = 0.4f;
        public float taglineDelay = 0.3f;
        public float taglineFadeDuration = 0.6f;
        public float xPulseStartTime = 7.0f;
        public float pulseScale = 1.12f;
        public float textOffset = 0f;
        
        private Vector2 xStartPos;
        private Vector2 xMidPos;
        private Vector2 xFinalPos;
        private Vector2 textFinalPos;
        private bool hasSkipped = false;

#if DOTWEEN || DOTWEEN_ENABLED
        private Sequence introSequence;
#endif

        void Start()
        {
#if DOTWEEN || DOTWEEN_ENABLED
            DOTween.Init();
#endif
            
            // Load game configuration
            var config = UnityEngine.Resources.Load<IntelliVerseXConfig>("IntelliVerseX/QuizVerseConfig");
            if (config != null && gameNameText != null)
            {
                gameNameText.text = config.gameName;
            }
            
            if (!ValidateReferences())
            {
                Debug.LogError("[IVX Intro] Missing UI references. Please assign all required elements.");
#if !DOTWEEN && !DOTWEEN_ENABLED
                StartCoroutine(FallbackLoadScene());
#endif
                return;
            }
            
            InitializePositions();
            SetupInitialState();
            PlayIntroMusic();
            
#if DOTWEEN || DOTWEEN_ENABLED
            AnimateIntro();
#else
            Debug.LogWarning("[IVX Intro] DOTween not installed. Using simple fallback.");
            StartCoroutine(FallbackIntro());
#endif
        }

        void Update()
        {
            if (allowSkip && !hasSkipped && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            {
                SkipIntro();
            }
        }

        bool ValidateReferences()
        {
            return xLogo != null && xLogoGroup != null && 
                   intelliVerseText != null && intelliVerseGroup != null && 
                   taglineGroup != null;
        }

        void InitializePositions()
        {
            xFinalPos = xLogo.anchoredPosition;
            xMidPos = new Vector2(0, 0);
            xStartPos = new Vector2(0, Screen.height * 0.6f);
            textFinalPos = intelliVerseText.anchoredPosition;
        }

        void SetupInitialState()
        {
            // X Logo
            xLogo.anchoredPosition = xStartPos;
            xLogo.localEulerAngles = new Vector3(0, 0, -180f);
            xLogoGroup.alpha = 0;
            
            // IntelliVerse Text
            intelliVerseText.anchoredPosition = xFinalPos + new Vector2(textOffset, 0);
            intelliVerseGroup.alpha = 0;
            
            // Tagline
            taglineGroup.alpha = 0;
            
            // Effects
            if (extraEffect) extraEffect.SetActive(false);
            if (glowEffect) glowEffect.SetActive(false);
            if (blackOverlayGroup) blackOverlayGroup.alpha = 1f;
        }

        void PlayIntroMusic()
        {
            if (sfxSource && introMusic)
            {
                sfxSource.clip = introMusic;
                sfxSource.loop = false;
                sfxSource.Play();
            }
        }

#if !DOTWEEN && !DOTWEEN_ENABLED
        private System.Collections.IEnumerator FallbackIntro()
        {
            // Simple fallback without DOTween
            float elapsed = 0f;
            
            // Fade in
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 1f;
                if (blackOverlayGroup) blackOverlayGroup.alpha = 1f - t;
                xLogoGroup.alpha = t;
                yield return null;
            }
            
            // Show logo
            xLogo.anchoredPosition = xFinalPos;
            xLogo.localEulerAngles = Vector3.zero;
            xLogoGroup.alpha = 1f;
            
            yield return new WaitForSeconds(1f);
            
            // Show text
            intelliVerseText.anchoredPosition = textFinalPos;
            intelliVerseGroup.alpha = 1f;
            intelliVerseText.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(1f);
            
            // Show tagline
            taglineGroup.alpha = 1f;
            
            yield return new WaitForSeconds(totalIntroDuration - 3f);
            
            // Fade out
            elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 1f;
                if (blackOverlayGroup) blackOverlayGroup.alpha = t;
                yield return null;
            }
            
            LoadNextScene();
        }
        
        private System.Collections.IEnumerator FallbackLoadScene()
        {
            yield return new WaitForSeconds(2f);
            LoadNextScene();
        }
#endif

#if DOTWEEN || DOTWEEN_ENABLED
        void AnimateIntro()
        {
            introSequence = DOTween.Sequence();
            
            // Fade in from black
            if (blackOverlayGroup)
                introSequence.Insert(0f, blackOverlayGroup.DOFade(0f, 1f).SetEase(Ease.OutQuad));
            
            // 1. Drop X into center
            introSequence.Append(xLogo.DOAnchorPos(xMidPos, xDropDuration).SetEase(Ease.OutQuart));
            introSequence.Join(xLogoGroup.DOFade(1f, xDropDuration * 0.6f));
            introSequence.Join(xLogo.DOLocalRotate(Vector3.zero, xDropDuration, RotateMode.FastBeyond360));
            
            // 2. Slide to final position
            introSequence.Append(xLogo.DOAnchorPos(xFinalPos, xSlideRightDuration).SetEase(Ease.InOutSine));
            
            // 3. Show IntelliVerse text
            introSequence.AppendInterval(intelliVerseDelay - (xDropDuration + xSlideRightDuration));
            introSequence.AppendCallback(() => intelliVerseText.gameObject.SetActive(true));
            introSequence.Append(intelliVerseText.DOAnchorPos(textFinalPos, intelliVerseSlideDuration).SetEase(Ease.OutExpo));
            introSequence.Join(intelliVerseGroup.DOFade(1f, intelliVerseFadeDuration));
            introSequence.Join(intelliVerseText.DOScale(1.05f, 0.25f).SetLoops(2, LoopType.Yoyo));
            
            // 4. Fade in tagline
            introSequence.AppendInterval(taglineDelay);
            introSequence.Append(taglineGroup.DOFade(1f, taglineFadeDuration));
            
            // 5. Pulse + glow
            introSequence.AppendInterval(xPulseStartTime - (intelliVerseDelay + intelliVerseSlideDuration + taglineDelay + taglineFadeDuration));
            introSequence.AppendCallback(() =>
            {
                if (extraEffect) extraEffect.SetActive(true);
                if (glowEffect) glowEffect.SetActive(true);
                
                xLogo.DOScale(pulseScale, 0.4f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
                
                if (glowEffect)
                {
                    var glowGroup = glowEffect.GetComponent<CanvasGroup>();
                    if (glowGroup) glowGroup.DOFade(0f, 1f).From(1f);
                }
            });
            
            // 6. Floating motion
            introSequence.InsertCallback(xPulseStartTime + 0.8f, () =>
            {
                xLogo.DOAnchorPos(xFinalPos + new Vector2(0, 8f), 1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            });
            
            // 7. Fade out to black
            if (blackOverlayGroup)
                introSequence.Insert(totalIntroDuration - 1f, blackOverlayGroup.DOFade(1f, 1f).SetEase(Ease.InOutQuad));
            
            // 8. Load next scene
            float remaining = totalIntroDuration - xPulseStartTime - 0.8f - 1f;
            if (remaining > 0f) introSequence.AppendInterval(remaining);
            
            introSequence.AppendCallback(LoadNextScene);
        }
#endif

        void SkipIntro()
        {
            if (hasSkipped) return;
            
            hasSkipped = true;
            Debug.Log("[IVX Intro] Skipping intro...");
            
#if DOTWEEN || DOTWEEN_ENABLED
            if (introSequence != null && introSequence.IsActive())
            {
                introSequence.Kill();
            }
#else
            StopAllCoroutines();
#endif
            
            LoadNextScene();
        }

        void LoadNextScene()
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError("[IVX Intro] Next scene name is not set!");
                return;
            }
            
            Debug.Log($"[IVX Intro] Loading scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }

        void OnDestroy()
        {
#if DOTWEEN || DOTWEEN_ENABLED
            introSequence?.Kill();
#endif
        }
    }
}
