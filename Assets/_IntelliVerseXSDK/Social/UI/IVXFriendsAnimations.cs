using System;
using System.Collections.Generic;
using UnityEngine;

#if DOTWEEN
using DG.Tweening;
#endif

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// DOTween animation helpers for the Friends UI.
    /// Gracefully degrades when DOTween is not installed.
    /// </summary>
    public static class IVXFriendsAnimations
    {
        private static IVXFriendsConfig Config => IVXFriendsConfig.Instance;

        #region Panel Animations

        /// <summary>
        /// Animates a panel opening with fade and scale.
        /// </summary>
        public static void AnimatePanelOpen(CanvasGroup canvasGroup, RectTransform rectTransform, Action onComplete = null)
        {
            if (canvasGroup == null) return;

#if DOTWEEN
            canvasGroup.alpha = 0f;
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * 0.9f;
            }

            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1f, Config.panelAnimationDuration).SetEase(Ease.OutQuad));
            
            if (rectTransform != null)
            {
                sequence.Join(rectTransform.DOScale(1f, Config.panelAnimationDuration).SetEase(Ease.OutBack));
            }

            if (onComplete != null)
            {
                sequence.OnComplete(() => onComplete());
            }
#else
            canvasGroup.alpha = 1f;
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
            onComplete?.Invoke();
#endif
        }

        /// <summary>
        /// Animates a panel closing with fade and scale.
        /// </summary>
        public static void AnimatePanelClose(CanvasGroup canvasGroup, RectTransform rectTransform, Action onComplete = null)
        {
            if (canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

#if DOTWEEN
            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(0f, Config.panelAnimationDuration * 0.7f).SetEase(Ease.InQuad));
            
            if (rectTransform != null)
            {
                sequence.Join(rectTransform.DOScale(0.9f, Config.panelAnimationDuration * 0.7f).SetEase(Ease.InBack));
            }

            if (onComplete != null)
            {
                sequence.OnComplete(() => onComplete());
            }
#else
            canvasGroup.alpha = 0f;
            onComplete?.Invoke();
#endif
        }

        #endregion

        #region Slot Animations

        /// <summary>
        /// Animates a slot appearing with slide-in effect.
        /// </summary>
        public static void AnimateSlotAppear(RectTransform rectTransform, CanvasGroup canvasGroup, int index)
        {
            if (!Config.enableSlotAnimations)
            {
                if (canvasGroup != null) canvasGroup.alpha = 1f;
                return;
            }

#if DOTWEEN
            float delay = index * Config.slotStaggerDelay;
            Vector2 originalPosition = rectTransform.anchoredPosition;

            // Start off-screen to the right
            rectTransform.anchoredPosition = new Vector2(originalPosition.x + 100f, originalPosition.y);
            if (canvasGroup != null) canvasGroup.alpha = 0f;

            // Animate in
            rectTransform.DOAnchorPos(originalPosition, Config.slotAnimationDuration)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad);

            if (canvasGroup != null)
            {
                canvasGroup.DOFade(1f, Config.slotAnimationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutQuad);
            }
#else
            if (canvasGroup != null) canvasGroup.alpha = 1f;
#endif
        }

        /// <summary>
        /// Animates a slot disappearing (e.g., when removed or action taken).
        /// </summary>
        public static void AnimateSlotDisappear(RectTransform rectTransform, CanvasGroup canvasGroup, Action onComplete = null)
        {
#if DOTWEEN
            var sequence = DOTween.Sequence();

            if (canvasGroup != null)
            {
                sequence.Append(canvasGroup.DOFade(0f, Config.slotAnimationDuration).SetEase(Ease.InQuad));
            }

            sequence.Join(rectTransform.DOScale(0.8f, Config.slotAnimationDuration).SetEase(Ease.InBack));
            sequence.Join(rectTransform.DOAnchorPosX(rectTransform.anchoredPosition.x - 50f, Config.slotAnimationDuration));

            if (onComplete != null)
            {
                sequence.OnComplete(() => onComplete());
            }
#else
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            onComplete?.Invoke();
#endif
        }

        /// <summary>
        /// Animates multiple slots appearing with staggered timing.
        /// </summary>
        public static void AnimateSlotsAppear(List<RectTransform> slots, List<CanvasGroup> canvasGroups)
        {
            if (slots == null) return;

            for (int i = 0; i < slots.Count; i++)
            {
                var cg = (canvasGroups != null && i < canvasGroups.Count) ? canvasGroups[i] : null;
                AnimateSlotAppear(slots[i], cg, i);
            }
        }

        #endregion

        #region Button Animations

        /// <summary>
        /// Animates a button press with scale punch.
        /// </summary>
        public static void AnimateButtonPress(RectTransform buttonRect)
        {
            if (buttonRect == null) return;

#if DOTWEEN
            buttonRect.DOKill();
            buttonRect.localScale = Vector3.one;
            buttonRect.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);
#endif
        }

        /// <summary>
        /// Animates a success action (e.g., friend added).
        /// </summary>
        public static void AnimateSuccess(RectTransform targetRect, Action onComplete = null)
        {
            if (targetRect == null)
            {
                onComplete?.Invoke();
                return;
            }

#if DOTWEEN
            var sequence = DOTween.Sequence();
            sequence.Append(targetRect.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad));
            sequence.Append(targetRect.DOScale(1f, 0.1f).SetEase(Ease.InQuad));
            
            if (onComplete != null)
            {
                sequence.OnComplete(() => onComplete());
            }
#else
            onComplete?.Invoke();
#endif
        }

        #endregion

        #region Loading Animations

        /// <summary>
        /// Starts a pulsing animation for loading indicators.
        /// </summary>
        public static void StartLoadingPulse(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null) return;

#if DOTWEEN
            canvasGroup.DOKill();
            canvasGroup.alpha = 1f;
            canvasGroup.DOFade(0.3f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
#endif
        }

        /// <summary>
        /// Stops the loading pulse animation.
        /// </summary>
        public static void StopLoadingPulse(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null) return;

#if DOTWEEN
            canvasGroup.DOKill();
            canvasGroup.alpha = 1f;
#else
            canvasGroup.alpha = 1f;
#endif
        }

        /// <summary>
        /// Animates a spinner rotation.
        /// </summary>
        public static void StartSpinnerRotation(RectTransform spinnerRect)
        {
            if (spinnerRect == null) return;

#if DOTWEEN
            spinnerRect.DOKill();
            spinnerRect.DORotate(new Vector3(0, 0, -360f), 1f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
#endif
        }

        /// <summary>
        /// Stops the spinner rotation.
        /// </summary>
        public static void StopSpinnerRotation(RectTransform spinnerRect)
        {
            if (spinnerRect == null) return;

#if DOTWEEN
            spinnerRect.DOKill();
            spinnerRect.rotation = Quaternion.identity;
#else
            spinnerRect.rotation = Quaternion.identity;
#endif
        }

        #endregion

        #region Tab Animations

        /// <summary>
        /// Animates tab switching with content fade.
        /// </summary>
        public static void AnimateTabSwitch(
            CanvasGroup outgoingContent,
            CanvasGroup incomingContent,
            Action onSwitchPoint = null,
            Action onComplete = null)
        {
#if DOTWEEN
            var sequence = DOTween.Sequence();

            // Fade out current content
            if (outgoingContent != null)
            {
                sequence.Append(outgoingContent.DOFade(0f, 0.15f).SetEase(Ease.InQuad));
            }

            // Call switch point callback
            if (onSwitchPoint != null)
            {
                sequence.AppendCallback(() => onSwitchPoint());
            }

            // Fade in new content
            if (incomingContent != null)
            {
                incomingContent.alpha = 0f;
                sequence.Append(incomingContent.DOFade(1f, 0.15f).SetEase(Ease.OutQuad));
            }

            if (onComplete != null)
            {
                sequence.OnComplete(() => onComplete());
            }
#else
            if (outgoingContent != null) outgoingContent.alpha = 0f;
            onSwitchPoint?.Invoke();
            if (incomingContent != null) incomingContent.alpha = 1f;
            onComplete?.Invoke();
#endif
        }

        #endregion

        #region Utility

        /// <summary>
        /// Kills all DOTween animations on a transform.
        /// </summary>
        public static void KillAnimations(Transform target)
        {
            if (target == null) return;

#if DOTWEEN
            target.DOKill();
#endif
        }

        /// <summary>
        /// Kills all DOTween animations on a CanvasGroup.
        /// </summary>
        public static void KillAnimations(CanvasGroup target)
        {
            if (target == null) return;

#if DOTWEEN
            target.DOKill();
#endif
        }

        #endregion
    }
}
