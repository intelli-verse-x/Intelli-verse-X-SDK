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
        #region Animation Constants
        
        private const float PANEL_ANIMATION_DURATION = 0.3f;
        private const float SLOT_ANIMATION_DURATION = 0.2f;
        private const float SLOT_STAGGER_DELAY = 0.05f;
        private const bool ENABLE_SLOT_ANIMATIONS = true;
        
        #endregion

        #region Panel Animations

        /// <summary>
        /// Animates a panel opening with fade and scale.
        /// </summary>
        public static void AnimatePanelOpen(CanvasGroup canvasGroup, RectTransform rectTransform, Action onComplete = null)
        {
            if (canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

#if DOTWEEN
            // Kill any existing animations first
            canvasGroup.DOKill();
            if (rectTransform != null) rectTransform.DOKill();

            canvasGroup.alpha = 0f;
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * 0.9f;
            }

            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1f, PANEL_ANIMATION_DURATION).SetEase(Ease.OutQuad));
            
            if (rectTransform != null)
            {
                sequence.Join(rectTransform.DOScale(1f, PANEL_ANIMATION_DURATION).SetEase(Ease.OutBack));
            }

            sequence.OnComplete(() => onComplete?.Invoke());
            sequence.SetAutoKill(true);
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
            // Kill any existing animations first
            canvasGroup.DOKill();
            if (rectTransform != null) rectTransform.DOKill();

            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(0f, PANEL_ANIMATION_DURATION * 0.7f).SetEase(Ease.InQuad));
            
            if (rectTransform != null)
            {
                sequence.Join(rectTransform.DOScale(0.9f, PANEL_ANIMATION_DURATION * 0.7f).SetEase(Ease.InBack));
            }

            sequence.OnComplete(() => onComplete?.Invoke());
            sequence.SetAutoKill(true);
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
            if (rectTransform == null)
            {
                if (canvasGroup != null) canvasGroup.alpha = 1f;
                return;
            }

            if (!ENABLE_SLOT_ANIMATIONS)
            {
                if (canvasGroup != null) canvasGroup.alpha = 1f;
                return;
            }

#if DOTWEEN
            // Kill existing animations first
            rectTransform.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();

            float delay = index * SLOT_STAGGER_DELAY;
            Vector2 originalPosition = rectTransform.anchoredPosition;

            // Start off-screen to the right
            rectTransform.anchoredPosition = new Vector2(originalPosition.x + 100f, originalPosition.y);
            if (canvasGroup != null) canvasGroup.alpha = 0f;

            // Animate in
            rectTransform.DOAnchorPos(originalPosition, SLOT_ANIMATION_DURATION)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);

            if (canvasGroup != null)
            {
                canvasGroup.DOFade(1f, SLOT_ANIMATION_DURATION)
                    .SetDelay(delay)
                    .SetEase(Ease.OutQuad)
                    .SetAutoKill(true);
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
            if (rectTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

#if DOTWEEN
            // Kill existing animations first
            rectTransform.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();

            var sequence = DOTween.Sequence();

            if (canvasGroup != null)
            {
                sequence.Append(canvasGroup.DOFade(0f, SLOT_ANIMATION_DURATION).SetEase(Ease.InQuad));
            }

            sequence.Join(rectTransform.DOScale(0.8f, SLOT_ANIMATION_DURATION).SetEase(Ease.InBack));
            sequence.Join(rectTransform.DOAnchorPosX(rectTransform.anchoredPosition.x - 50f, SLOT_ANIMATION_DURATION));

            sequence.OnComplete(() => onComplete?.Invoke());
            sequence.SetAutoKill(true);
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
        /// Robust implementation that handles all edge cases.
        /// </summary>
        public static void AnimateTabSwitch(
            CanvasGroup outgoingContent,
            CanvasGroup incomingContent,
            Action onSwitchPoint = null,
            Action onComplete = null)
        {
            // Edge case: Both null - just invoke callbacks immediately
            if (outgoingContent == null && incomingContent == null)
            {
                onSwitchPoint?.Invoke();
                onComplete?.Invoke();
                return;
            }

#if DOTWEEN
            // Kill any existing animations on both canvas groups first
            if (outgoingContent != null) outgoingContent.DOKill();
            if (incomingContent != null) incomingContent.DOKill();

            var sequence = DOTween.Sequence();
            
            // Check if outgoing content exists and is visible
            bool hasOutgoing = outgoingContent != null && 
                               outgoingContent.gameObject != null && 
                               outgoingContent.gameObject.activeInHierarchy &&
                               outgoingContent.alpha > 0f;
            
            // Check if incoming content exists (we'll activate it regardless of current state)
            bool hasIncoming = incomingContent != null && incomingContent.gameObject != null;

            // Fade out current content (if active and visible)
            if (hasOutgoing)
            {
                sequence.Append(outgoingContent.DOFade(0f, 0.15f).SetEase(Ease.InQuad));
            }
            else
            {
                // No outgoing content or already hidden, just add a tiny delay
                sequence.AppendInterval(0.01f);
            }

            // Call switch point callback at the midpoint
            sequence.AppendCallback(() => onSwitchPoint?.Invoke());

            // Fade in new content
            if (hasIncoming)
            {
                // Ensure starting alpha is 0 for incoming
                incomingContent.alpha = 0f;
                
                // Ensure GameObject is active (belt and suspenders)
                if (!incomingContent.gameObject.activeInHierarchy)
                {
                    incomingContent.gameObject.SetActive(true);
                }
                
                sequence.Append(incomingContent.DOFade(1f, 0.15f).SetEase(Ease.OutQuad));
            }
            else
            {
                // No incoming content, add small interval for timing
                sequence.AppendInterval(0.01f);
            }

            // OnComplete callback - ensure final state is correct
            sequence.OnComplete(() =>
            {
                // Ensure outgoing is fully hidden
                if (outgoingContent != null && outgoingContent.gameObject != null)
                {
                    outgoingContent.alpha = 0f;
                }
                
                // Ensure incoming is fully visible
                if (incomingContent != null && incomingContent.gameObject != null)
                {
                    incomingContent.alpha = 1f;
                }
                
                onComplete?.Invoke();
            });
            
            // Safety: ensure completion happens even if killed
            sequence.SetAutoKill(true);
#else
            // Non-DOTween fallback - immediate switch
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

        /// <summary>
        /// Kills all DOTween animations on a RectTransform.
        /// </summary>
        public static void KillAnimations(RectTransform target)
        {
            if (target == null) return;

#if DOTWEEN
            target.DOKill();
#endif
        }

        #endregion
    }
}
