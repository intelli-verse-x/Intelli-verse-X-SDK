// LogoIntroController.cs
// IntelliVerseX SDK - Logo Intro Animation
// Requires DOTween package to be installed

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

#if DOTWEEN || DOTWEEN_ENABLED
using DG.Tweening;
#endif

public class LogoIntroController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform xLogo;
    public CanvasGroup xLogoGroup;
    public RectTransform intelliVerseText;
    public CanvasGroup intelliVerseGroup;
    public CanvasGroup taglineGroup;
    public GameObject extraEffect;

    [Header("Visual Effects")]
    public CanvasGroup blackOverlayGroup;
    public GameObject glowEffect;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip introMusic;

    [Header("Scene Settings")]
    public string nextSceneName = "Main";
    public float totalIntroDuration = 9f;

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

#if DOTWEEN || DOTWEEN_ENABLED
    private Sequence introSequence;
#endif

    void Start()
    {
#if DOTWEEN || DOTWEEN_ENABLED
        DOTween.Init();

        if (!xLogo || !xLogoGroup || !intelliVerseText || !intelliVerseGroup || !taglineGroup)
        {
            Debug.LogError("[LogoIntroController] Missing UI references.");
            return;
        }

        xFinalPos = xLogo.anchoredPosition;
        xMidPos = new Vector2(0, 0);
        xStartPos = new Vector2(0, Screen.height * 0.6f);
        textFinalPos = intelliVerseText.anchoredPosition;

        xLogo.anchoredPosition = xStartPos;
        xLogo.localEulerAngles = new Vector3(0, 0, -180f);
        xLogoGroup.alpha = 0;

        intelliVerseText.anchoredPosition = xFinalPos + new Vector2(textOffset, 0);
        intelliVerseGroup.alpha = 0;
        taglineGroup.alpha = 0;

        if (extraEffect) extraEffect.SetActive(false);
        if (glowEffect) glowEffect.SetActive(false);
        if (blackOverlayGroup) blackOverlayGroup.alpha = 1f;

        if (sfxSource && introMusic)
        {
            sfxSource.clip = introMusic;
            sfxSource.loop = false;
            sfxSource.Play();
        }

        AnimateIntro();
#else
        Debug.LogWarning("[LogoIntroController] DOTween not installed. Using fallback - loading scene directly.");
        StartCoroutine(FallbackLoadScene());
#endif
    }

#if !DOTWEEN && !DOTWEEN_ENABLED
    private System.Collections.IEnumerator FallbackLoadScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(nextSceneName);
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

        // 3. Show IntelliVerse image from inside X
        introSequence.AppendInterval(intelliVerseDelay - (xDropDuration + xSlideRightDuration));
        introSequence.AppendCallback(() => { intelliVerseText.gameObject.SetActive(true); });
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

            CanvasGroup glowGroup = glowEffect.GetComponent<CanvasGroup>();
            if (glowGroup)
                glowGroup.DOFade(0f, 1f).From(1f);
        });

        // 6. Add floating motion after pulse
        introSequence.InsertCallback(xPulseStartTime + 0.8f, () =>
        {
            xLogo.DOAnchorPos(xFinalPos + new Vector2(0, 8f), 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        });

        // 7. Fade out to black
        if (blackOverlayGroup)
            introSequence.Insert(totalIntroDuration - 1f, blackOverlayGroup.DOFade(1f, 1f).SetEase(Ease.InOutQuad));

        // 8. Load scene at end
        float remaining = totalIntroDuration - xPulseStartTime - 0.8f - 1f;
        if (remaining > 0f)
            introSequence.AppendInterval(remaining);

        introSequence.AppendCallback(() => SceneManager.LoadScene(nextSceneName));
    }

    void OnDestroy()
    {
        introSequence?.Kill();
    }
#endif
}
