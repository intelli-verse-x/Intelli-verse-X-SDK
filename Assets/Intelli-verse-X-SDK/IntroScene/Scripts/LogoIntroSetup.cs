// LogoIntroSetup.cs
// IntelliVerseX SDK - Logo Intro Setup Helper
// DOTween is optional - script works without it

using UnityEngine;
using UnityEngine.UI;

#if DOTWEEN || DOTWEEN_ENABLED
using DG.Tweening;
#endif

public class LogoIntroSetup : MonoBehaviour
{
    public Sprite xSprite;
    public Sprite titleSprite;
    public Sprite taglineSprite;

    void Start()
    {
        SetupIntroUI();
    }

    void SetupIntroUI()
    {
        // 1. Canvas
        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 2. LogoIntroManager
        GameObject managerGO = new GameObject("LogoIntroManager");
        managerGO.transform.SetParent(canvasGO.transform, false);

        // 3. X Logo
        GameObject xGO = new GameObject("XLogo", typeof(RectTransform), typeof(Image));
        xGO.transform.SetParent(managerGO.transform, false);
        RectTransform xRT = xGO.GetComponent<RectTransform>();
        xRT.anchorMin = xRT.anchorMax = new Vector2(0.5f, 0.5f);
        xRT.pivot = new Vector2(0.5f, 0.5f);
        xRT.sizeDelta = new Vector2(500, 500);
        xRT.anchoredPosition = new Vector2(0, 0);
        Image xImg = xGO.GetComponent<Image>();
        xImg.sprite = xSprite;
        xImg.SetNativeSize();

        // 4. IntelliVerse Title
        GameObject titleGO = new GameObject("IntelliVerseText", typeof(RectTransform), typeof(Image));
        titleGO.transform.SetParent(managerGO.transform, false);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.sizeDelta = new Vector2(700, 150);
        titleRT.anchoredPosition = Vector2.zero;
        Image titleImg = titleGO.GetComponent<Image>();
        titleImg.sprite = titleSprite;
        titleImg.SetNativeSize();
        titleGO.SetActive(false);

        // 5. Tagline (sprite)
        GameObject tagGO = new GameObject("Tagline", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        tagGO.transform.SetParent(managerGO.transform, false);
        RectTransform tagRT = tagGO.GetComponent<RectTransform>();
        tagRT.anchorMin = tagRT.anchorMax = new Vector2(0.5f, 0.1f);
        tagRT.pivot = new Vector2(0.5f, 0.5f);
        tagRT.sizeDelta = new Vector2(1000, 200);
        tagRT.anchoredPosition = Vector2.zero;
        Image tagImg = tagGO.GetComponent<Image>();
        tagImg.sprite = taglineSprite;
        tagImg.SetNativeSize();
        CanvasGroup tagGroup = tagGO.GetComponent<CanvasGroup>();
        tagGroup.alpha = 0;

        // 6. Extra effect placeholder
        GameObject pulseGO = new GameObject("ExtraEffect", typeof(RectTransform), typeof(Image));
        pulseGO.transform.SetParent(managerGO.transform, false);
        RectTransform pulseRT = pulseGO.GetComponent<RectTransform>();
        pulseRT.anchorMin = pulseRT.anchorMax = new Vector2(0.5f, 0.5f);
        pulseRT.pivot = new Vector2(0.5f, 0.5f);
        pulseRT.sizeDelta = new Vector2(800, 800);
        pulseRT.anchoredPosition = Vector2.zero;
        Image pulseImg = pulseGO.GetComponent<Image>();
        pulseImg.color = new Color(1f, 1f, 1f, 0.05f);
        pulseGO.SetActive(false);

        // 7. Add animation controller
        LogoIntroController controller = managerGO.AddComponent<LogoIntroController>();
        controller.xLogo = xRT;
        controller.intelliVerseText = titleRT;
        controller.taglineGroup = tagGroup;
        controller.extraEffect = pulseGO;
        controller.nextSceneName = "Main";

#if DOTWEEN || DOTWEEN_ENABLED
        Debug.Log("✅ Logo Intro UI set up with DOTween animations!");
#else
        Debug.Log("✅ Logo Intro UI set up (DOTween not installed - using fallback)");
#endif
    }
}
