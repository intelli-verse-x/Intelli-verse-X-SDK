using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Analytics
{
    /// <summary>
    /// IVXTodayForYouFeed — Phase 4C (qv-insights-loop).
    ///
    /// Today-for-You feed accessor. Mirrors the SmartNudge pattern:
    /// thin layer over IVXPerModePersonalizationService, plus client-
    /// side dedupe + impression tracking + simple ordering rules:
    ///   1. Cards with kind=="aahaa" come first (engagement-tested),
    ///   2. then kind=="quest", "challenge",
    ///   3. then kind=="content_drop", "weekly_card",
    ///   4. then everything else, in server-supplied order.
    ///
    /// The SDK does NOT render these — that's the per-game UI's job.
    /// We only own the data model + ordering + dedupe.
    /// </summary>
    public class IVXTodayForYouFeed
    {
        public static IVXTodayForYouFeed Instance { get; } = new IVXTodayForYouFeed();

        public event Action<List<IVXPerModePersonalizationService.TodayFeedCard>> OnFeedChanged;

        private readonly HashSet<string> _seenIds = new HashSet<string>();
        private List<IVXPerModePersonalizationService.TodayFeedCard> _current =
            new List<IVXPerModePersonalizationService.TodayFeedCard>();

        public bool IsInitialized => IVXPerModePersonalizationService.Instance.IsInitialized;

        public async Task PrimeAsync()
        {
            if (!IsInitialized) return;
            await IVXPerModePersonalizationService.Instance.GetBundleAsync();
            var raw = IVXPerModePersonalizationService.Instance.GetTodayFeed();
            var ordered = OrderFeed(raw);
            if (!IsSameFeed(ordered, _current))
            {
                _current = ordered;
                try { OnFeedChanged?.Invoke(_current); }
                catch (Exception e)
                {
                    Debug.LogWarning($"[IVXTodayForYouFeed] OnFeedChanged subscriber threw: {e.Message}");
                }
            }
        }

        public List<IVXPerModePersonalizationService.TodayFeedCard> GetCurrent() => _current;

        public bool WasShown(string id) =>
            !string.IsNullOrEmpty(id) && _seenIds.Contains(id);

        public void MarkShown(string id)
        {
            if (!string.IsNullOrEmpty(id)) _seenIds.Add(id);
        }

        public async Task RefreshAsync()
        {
            if (!IsInitialized) return;
            IVXPerModePersonalizationService.Instance.Invalidate();
            await PrimeAsync();
        }

        private static List<IVXPerModePersonalizationService.TodayFeedCard> OrderFeed(
            List<IVXPerModePersonalizationService.TodayFeedCard> raw)
        {
            var ordered = new List<IVXPerModePersonalizationService.TodayFeedCard>();
            if (raw == null) return ordered;
            // Stable, priority-class ordering. We deliberately keep the
            // server's intra-class order so the briefer can A/B test
            // ordering server-side without touching the SDK.
            string[] priorityOrder = new[]
            {
                "aahaa", "quest", "challenge", "content_drop", "weekly_card",
            };
            foreach (var cls in priorityOrder)
            {
                foreach (var c in raw)
                {
                    if (c?.Kind == cls) ordered.Add(c);
                }
            }
            // Append any remaining (unknown-kind) cards last.
            foreach (var c in raw)
            {
                if (c == null) continue;
                if (Array.IndexOf(priorityOrder, c.Kind) < 0) ordered.Add(c);
            }
            return ordered;
        }

        private static bool IsSameFeed(
            List<IVXPerModePersonalizationService.TodayFeedCard> a,
            List<IVXPerModePersonalizationService.TodayFeedCard> b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                var ai = a[i]; var bi = b[i];
                if (ai?.Id != bi?.Id) return false;
            }
            return true;
        }
    }
}
