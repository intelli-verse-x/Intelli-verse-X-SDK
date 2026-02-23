using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if DOTWEEN_ENABLED || DOTWEEN
using DG.Tweening;
#endif

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// Referral code popup panel for registration flow.
    /// Allows users to enter a referral code that will be applied during registration.
    /// </summary>
    public class IVXPanelReferral : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Root")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Input")]
        [SerializeField] private TMP_InputField _referralCodeInput;

        [Header("Input Focus Visuals")]
        [SerializeField] private Graphic _focusFrame;
        [SerializeField] private Color _focusFrameColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _idleFrameColor = new Color(1f, 1f, 1f, 0.16f);
        [SerializeField] private Color _caretFocusColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _caretIdleColor = new Color(1f, 1f, 1f, 0.85f);
#pragma warning disable CS0414
        [SerializeField] private float _focusFrameScale = 1.01f;
        [SerializeField] private float _focusAnimDuration = 0.12f;
#pragma warning restore CS0414

        [Header("Buttons")]
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _closeButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Validation")]
        [SerializeField] private int _minCodeLength = 3;
        [SerializeField] private int _maxCodeLength = 20;

        #endregion

        #region Private Fields

        private IVXCanvasAuth _canvasAuth;

        #endregion

        #region Events

        /// <summary>
        /// Fired when a referral code is submitted
        /// </summary>
        public event Action<string> OnSubmitted;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvasAuth = GetComponentInParent<IVXCanvasAuth>();
            
            EnsureCanvasGroup();
            SetupButtons();
            SetupFocusVisuals();
        }

        private void OnEnable()
        {
            ClearError();
        }

        private void OnDestroy()
        {
#if DOTWEEN_ENABLED || DOTWEEN
            if (_canvasGroup != null) _canvasGroup.DOKill();
            if (_panel != null) _panel.transform.DOKill();
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Bind to parent canvas auth
        /// </summary>
        public void Bind(IVXCanvasAuth owner) => _canvasAuth = owner;

        /// <summary>
        /// Open the referral panel with optional prefill code
        /// </summary>
        public void Open(string prefillCode = "")
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            FadeIn();
            ClearError();
            SetStatus("");

            if (_referralCodeInput != null)
            {
                _referralCodeInput.text = prefillCode ?? "";
                _referralCodeInput.ActivateInputField();
            }
        }

        /// <summary>
        /// Close the referral panel
        /// </summary>
        public void Close()
        {
            if (_panel == null) return;
            FadeOut(() => _panel.SetActive(false));
        }

        #endregion

        #region Setup Methods

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup == null && _panel != null)
            {
                _canvasGroup = _panel.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = _panel.AddComponent<CanvasGroup>();
            }
        }

        private void SetupButtons()
        {
            _submitButton?.onClick.AddListener(OnClickSubmit);
            _clearButton?.onClick.AddListener(OnClickClear);
            _closeButton?.onClick.AddListener(OnClickClose);
        }

        private void SetupFocusVisuals()
        {
            WireInputFocus(_referralCodeInput, _focusFrame);
        }

        #endregion

        #region Button Handlers

        private void OnClickSubmit()
        {
            AnimateButton(_submitButton);

            string code = _referralCodeInput?.text?.Trim().ToUpperInvariant() ?? "";

            if (string.IsNullOrEmpty(code))
            {
                ShowError("Please enter a referral code.");
                _referralCodeInput?.ActivateInputField();
                return;
            }

            var validation = ValidateReferralCode(code);
            if (!validation.isValid)
            {
                ShowError(validation.errorMessage);
                _referralCodeInput?.ActivateInputField();
                return;
            }

            OnSubmitted?.Invoke(code);
            _canvasAuth?.NotifyReferralSubmitted(code);
            SetStatus("Referral code applied!");
            Close();
        }

        private void OnClickClear()
        {
            AnimateButton(_clearButton);

            if (_referralCodeInput != null)
                _referralCodeInput.text = "";

            OnSubmitted?.Invoke("");
            _canvasAuth?.NotifyReferralSubmitted("");
            SetStatus("Referral code cleared.");
            Close();
        }

        private void OnClickClose()
        {
            AnimateButton(_closeButton);
            Close();
        }

        #endregion

        #region Validation

        private (bool isValid, string errorMessage) ValidateReferralCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Referral code cannot be empty.");

            if (code.Length < _minCodeLength)
                return (false, $"Referral code must be at least {_minCodeLength} characters.");

            if (code.Length > _maxCodeLength)
                return (false, $"Referral code must be {_maxCodeLength} characters or less.");

            foreach (char c in code)
            {
                if (!char.IsLetterOrDigit(c))
                    return (false, "Referral code can only contain letters and numbers.");
            }

            return (true, null);
        }

        #endregion

        #region UI Helpers

        private void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
            ShakePanel();
        }

        private void ClearError()
        {
            if (_errorText != null)
            {
                _errorText.text = "";
                _errorText.gameObject.SetActive(false);
            }
        }

        private void SetStatus(string msg)
        {
            if (_statusText != null)
                _statusText.text = msg ?? "";
        }

        private void ShakePanel()
        {
#if DOTWEEN_ENABLED || DOTWEEN
            if (_panel != null)
            {
                var t = _panel.transform;
                t.DOKill();
                t.DOShakePosition(0.2f, 6f, 15, 90f, false, true);
            }
#endif
        }

        private void FadeIn()
        {
#if DOTWEEN_ENABLED || DOTWEEN
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, 0.15f);
            }
            if (_panel != null)
            {
                _panel.transform.localScale = Vector3.one * 0.95f;
                _panel.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
            }
#endif
        }

        private void FadeOut(Action onComplete)
        {
#if DOTWEEN_ENABLED || DOTWEEN
            Tween t1 = null;
            if (_canvasGroup != null) t1 = _canvasGroup.DOFade(0f, 0.12f);
            var t2 = _panel != null ? _panel.transform.DOScale(0.95f, 0.12f).SetEase(Ease.InBack) : null;

            if (t1 != null) t1.OnComplete(() => onComplete?.Invoke());
            else if (t2 != null) t2.OnComplete(() => onComplete?.Invoke());
            else onComplete?.Invoke();
#else
            onComplete?.Invoke();
#endif
        }

        private void AnimateButton(Button b)
        {
#if DOTWEEN_ENABLED || DOTWEEN
            if (b == null) return;
            var tr = b.transform;
            tr.DOKill();
            tr.DOPunchScale(Vector3.one * -0.05f, 0.12f, 8, 1f);
#endif
        }

        #endregion

        #region Focus Visuals

        private void WireInputFocus(TMP_InputField field, Graphic frame)
        {
            if (field == null) return;

            SetFieldFocusVisual(field, frame, focused: false, instant: true);

            field.onSelect.AddListener(_ => SetFieldFocusVisual(field, frame, focused: true, instant: false));
            field.onDeselect.AddListener(_ => SetFieldFocusVisual(field, frame, focused: false, instant: false));
            field.onEndEdit.AddListener(_ => SetFieldFocusVisual(field, frame, focused: false, instant: false));
        }

        private void SetFieldFocusVisual(TMP_InputField field, Graphic frame, bool focused, bool instant)
        {
#if DOTWEEN_ENABLED || DOTWEEN
            if (frame != null)
            {
                frame.DOKill();
                if (!instant)
                    frame.DOColor(focused ? _focusFrameColor : _idleFrameColor, _focusAnimDuration);
                else
                    frame.color = focused ? _focusFrameColor : _idleFrameColor;

                var rt = frame.rectTransform;
                if (rt != null)
                {
                    rt.DOKill();
                    if (!instant)
                        rt.DOScale(focused ? _focusFrameScale : 1f, _focusAnimDuration).SetEase(Ease.OutQuad);
                    else
                        rt.localScale = Vector3.one * (focused ? _focusFrameScale : 1f);
                }
            }
#else
            if (frame != null)
                frame.color = focused ? _focusFrameColor : _idleFrameColor;
#endif

            if (field != null)
            {
                field.caretColor = focused ? _caretFocusColor : _caretIdleColor;
                var sel = focused ? _caretFocusColor : _caretIdleColor;
                sel.a = 0.28f;
                field.selectionColor = sel;
                field.caretWidth = focused ? 2 : 1;
            }
        }

        #endregion
    }
}
