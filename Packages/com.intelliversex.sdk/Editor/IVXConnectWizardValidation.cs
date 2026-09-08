using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Pure helpers for the Control Center Connect wizard (validation + recent Game IDs).
    /// Kept free of EditorWindow state so EditMode tests can cover them.
    /// </summary>
    public static class IVXConnectWizardValidation
    {
        public const int MinGameNameLength = 2;
        public const int MaxGameNameLength = 80;
        public const int MaxRecentGames = 8;

        public const string PrefEmail = "IVX.ControlCenter.Email";
        public const string PrefGameName = "IVX.ControlCenter.LastGameName";
        public const string PrefManualFoldout = "IVX.ControlCenter.ManualFoldout";
        public const string PrefServerFoldout = "IVX.ControlCenter.ServerFoldout";
        public const string PrefRecentGames = "IVX.ControlCenter.RecentGames";
        public const string PrefFocusConnect = "IVX.ControlCenter.FocusConnectOnce";

        public const string DevelopersUrl = "https://intelli-verse-x.ai/developers";
        public const string SignupUrl = "https://intelli-verse-x.ai/developers";
        public const string ForgotPasswordUrl = "https://intelli-verse-x.ai/developers";

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex UuidRegex = new Regex(
            @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
            RegexOptions.Compiled);

        [Serializable]
        public class RecentGameEntry
        {
            public string id;
            public string name;
            public long at;
        }

        [Serializable]
        private class RecentGamesBlob
        {
            public RecentGameEntry[] items;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            return EmailRegex.IsMatch(email.Trim());
        }

        public static bool IsValidUuid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return UuidRegex.IsMatch(value.Trim());
        }

        public static bool TryValidateGameName(string name, out string reason)
        {
            string trimmed = (name ?? string.Empty).Trim();
            if (trimmed.Length < MinGameNameLength)
            {
                reason = "Use at least " + MinGameNameLength + " characters.";
                return false;
            }

            if (trimmed.Length > MaxGameNameLength)
            {
                reason = "Keep the name under " + MaxGameNameLength + " characters.";
                return false;
            }

            if (trimmed.IndexOfAny(new[] { '\n', '\r', '\t' }) >= 0)
            {
                reason = "Remove line breaks from the game name.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public static bool TryValidateSignIn(string email, string password, out string reason)
        {
            string e = (email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(e))
            {
                reason = "Enter your IntelliVerse email.";
                return false;
            }

            if (!IsValidEmail(e))
            {
                reason = "Email looks invalid.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                reason = "Enter your password.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public static RecentGameEntry[] LoadRecentGames()
        {
            string raw = EditorPrefs.GetString(PrefRecentGames, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
                return Array.Empty<RecentGameEntry>();

            try
            {
                var blob = JsonUtility.FromJson<RecentGamesBlob>(raw);
                if (blob?.items == null)
                    return Array.Empty<RecentGameEntry>();

                var list = new List<RecentGameEntry>(blob.items.Length);
                for (int i = 0; i < blob.items.Length; i++)
                {
                    var e = blob.items[i];
                    if (e == null || !IsValidUuid(e.id))
                        continue;
                    list.Add(e);
                }

                return list.ToArray();
            }
            catch
            {
                return Array.Empty<RecentGameEntry>();
            }
        }

        public static void RememberRecentGame(string id, string name)
        {
            if (!IsValidUuid(id))
                return;

            string trimmedId = id.Trim();
            string trimmedName = (name ?? string.Empty).Trim();
            var existing = new List<RecentGameEntry>(LoadRecentGames());
            for (int i = existing.Count - 1; i >= 0; i--)
            {
                if (string.Equals(existing[i].id, trimmedId, StringComparison.OrdinalIgnoreCase))
                    existing.RemoveAt(i);
            }

            existing.Insert(0, new RecentGameEntry
            {
                id = trimmedId,
                name = trimmedName,
                at = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            while (existing.Count > MaxRecentGames)
                existing.RemoveAt(existing.Count - 1);

            var blob = new RecentGamesBlob { items = existing.ToArray() };
            EditorPrefs.SetString(PrefRecentGames, JsonUtility.ToJson(blob));
        }

        public static string[] RecentGamePopupLabels(RecentGameEntry[] entries)
        {
            if (entries == null || entries.Length == 0)
                return new[] { "(no recent Game IDs in this Editor)" };

            var labels = new string[entries.Length + 1];
            labels[0] = "Select a recent Game ID…";
            for (int i = 0; i < entries.Length; i++)
            {
                string n = string.IsNullOrWhiteSpace(entries[i].name) ? "Unnamed" : entries[i].name;
                string shortId = entries[i].id.Length > 8 ? entries[i].id.Substring(0, 8) + "…" : entries[i].id;
                labels[i + 1] = n + "  (" + shortId + ")";
            }

            return labels;
        }
    }
}
