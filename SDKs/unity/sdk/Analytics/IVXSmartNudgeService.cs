using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Analytics
{
    /// <summary>
    /// IVXSmartNudgeService — Phase 4C (qv-insights-loop).
    ///
    /// Tiny wrapper over IVXPerModePersonalizationService that exposes
    /// a single piece of state: "do I have a nudge to show, and what
    /// is it?". Subscribers can poll or subscribe to OnNudgeChanged
    /// for refresh notifications.
    ///
    /// Lifecycle:
    ///   - Initialize() at session start (after IVXPerModePersonalizationService).
    ///   - PrimeAsync() once when the home/lobby scene loads.
    ///   - GetCurrent() any time to read the current nudge.
    ///   - MarkShown(id) when the UI renders it (so we can dedupe).
    ///   - MarkEngaged(id) when the user taps the CTA.
    ///   - PromoteCacheRefresh() to drop the cache + re-fetch.
    ///
    /// Why a separate class? Two reasons:
    ///   1. Game scripts shouldn't take a hard dependency on the
    ///      personalization service for the simple "show the nudge"
    ///      use case. Single-responsibility.
    ///   2. We layer in client-side dedupe + impression analytics
    ///      here, so the underlying personalization service stays
    ///      side-effect-free (good for tests).
    /// </summary>
    public class IVXSmartNudgeService
    {
        public static IVXSmartNudgeService Instance { get; } = new IVXSmartNudgeService();

        public event Action<IVXPerModePersonalizationService.SmartNudge> OnNudgeChanged;

        private IVXPerModePersonalizationService.SmartNudge _current;
        private string _lastShownId;

        public bool IsInitialized => IVXPerModePersonalizationService.Instance.IsInitialized;

        /// <summary>
        /// Pre-fetch the bundle so the home/lobby UI can read the
        /// nudge synchronously. Call once per scene load. Safe to
        /// call multiple times — coalesces under the hood.
        /// </summary>
        public async Task PrimeAsync()
        {
            if (!IsInitialized) return;
            await IVXPerModePersonalizationService.Instance.GetBundleAsync();
            var fresh = IVXPerModePersonalizationService.Instance.GetSmartNudge();
            if (!IsSameNudge(fresh, _current))
            {
                _current = fresh;
                try { OnNudgeChanged?.Invoke(_current); }
                catch (Exception e)
                {
                    Debug.LogWarning($"[IVXSmartNudgeService] OnNudgeChanged subscriber threw: {e.Message}");
                }
            }
        }

        public IVXPerModePersonalizationService.SmartNudge GetCurrent() => _current;

        /// <summary>
        /// Mark a nudge as shown — used to dedupe back-to-back shows
        /// of the same nudge across scene transitions.
        /// </summary>
        public void MarkShown(string nudgeId)
        {
            if (string.IsNullOrEmpty(nudgeId)) return;
            _lastShownId = nudgeId;
        }

        public bool WasJustShown(string nudgeId) =>
            !string.IsNullOrEmpty(nudgeId) && nudgeId == _lastShownId;

        /// <summary>
        /// Drop the local cache + bundle and re-fetch from the AI svc.
        /// Use sparingly — typically only after a major user action
        /// that we expect changes the cohort (e.g. just made first
        /// purchase).
        /// </summary>
        public async Task RefreshAsync()
        {
            if (!IsInitialized) return;
            IVXPerModePersonalizationService.Instance.Invalidate();
            await PrimeAsync();
        }

        private static bool IsSameNudge(
            IVXPerModePersonalizationService.SmartNudge a,
            IVXPerModePersonalizationService.SmartNudge b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.Title == b.Title && a.Body == b.Body && a.CtaActionId == b.CtaActionId;
        }
    }
}
