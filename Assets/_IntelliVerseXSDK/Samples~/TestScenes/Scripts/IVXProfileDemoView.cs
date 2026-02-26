using System;
using System.Collections;
using System.Text;
using IntelliVerseX.Backend.Nakama;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.Samples.TestScenes
{
    /// <summary>
    /// Simple profile demo view using IMGUI for zero-setup sample usage.
    /// </summary>
    public sealed class IVXProfileDemoView : MonoBehaviour
    {
        [Header("Runtime Status")]
        [SerializeField] private string statusMessage = "Idle";
        [SerializeField] private string debugMessage = string.Empty;
        [SerializeField] private string profileSnapshot = string.Empty;
        [SerializeField] private string portfolioSnapshot = string.Empty;

        [Header("Editable Profile Fields")]
        [SerializeField] private string firstName = string.Empty;
        [SerializeField] private string lastName = string.Empty;
        [SerializeField] private string city = string.Empty;
        [SerializeField] private string region = string.Empty;
        [SerializeField] private string country = string.Empty;
        [SerializeField] private string countryCode = string.Empty;
        [SerializeField] private string locale = "en-us";
        [SerializeField] private string avatarUrl = string.Empty;

        [Header("OnGUI")]
        [SerializeField] private bool renderOnGui = true;
        [SerializeField] private Vector2 scrollPosition = Vector2.zero;
        [SerializeField] private string avatarPreviewStatus = "No avatar loaded.";

        private Texture2D _avatarTexture;
        private Coroutine _avatarLoadCoroutine;

        public event Action OnRefreshClicked;
        public event Action OnSaveClicked;
        public event Action OnPortfolioClicked;
        public event Action OnPreviewAvatarClicked;

        public IVXNProfileManager.IVXNProfileUpdateRequest BuildUpdateRequest()
        {
            return new IVXNProfileManager.IVXNProfileUpdateRequest
            {
                FirstName = firstName,
                LastName = lastName,
                City = city,
                Region = region,
                Country = country,
                CountryCode = countryCode,
                Locale = locale,
                AvatarUrl = avatarUrl
            };
        }

        public void ApplyProfile(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            if (snapshot == null)
            {
                SetStatus("Profile is null.", true);
                return;
            }

            firstName = snapshot.FirstName ?? string.Empty;
            lastName = snapshot.LastName ?? string.Empty;
            city = snapshot.City ?? string.Empty;
            region = snapshot.Region ?? string.Empty;
            country = snapshot.Country ?? string.Empty;
            countryCode = snapshot.CountryCode ?? string.Empty;
            locale = string.IsNullOrWhiteSpace(snapshot.Locale) ? locale : snapshot.Locale;
            avatarUrl = snapshot.AvatarUrl ?? string.Empty;
            RequestAvatarPreview(avatarUrl);

            var sb = new StringBuilder(512);
            sb.AppendLine("Profile Snapshot");
            sb.AppendLine($"UserId: {snapshot.UserId}");
            sb.AppendLine($"Name: {snapshot.FirstName} {snapshot.LastName}".Trim());
            sb.AppendLine($"Location: {snapshot.City}, {snapshot.Region}, {snapshot.Country} ({snapshot.CountryCode})");
            sb.AppendLine($"Locale: {snapshot.Locale}");
            sb.AppendLine($"Platform: {snapshot.Platform}");
            sb.AppendLine($"Schema/Profile Version: {snapshot.SchemaVersion}/{snapshot.ProfileVersion}");
            sb.AppendLine($"TraceId: {snapshot.TraceId}");
            sb.AppendLine($"RequestId: {snapshot.RequestId}");
            profileSnapshot = sb.ToString();
        }

        public void ApplyPortfolio(IVXNProfileManager.IVXNProfilePortfolioSnapshot portfolio)
        {
            if (portfolio == null)
            {
                portfolioSnapshot = "Portfolio unavailable.";
                return;
            }

            var sb = new StringBuilder(512);
            sb.AppendLine("Portfolio Snapshot");
            sb.AppendLine($"UserId: {portfolio.UserId}");
            sb.AppendLine($"TotalGames: {portfolio.TotalGames}");
            sb.AppendLine($"GlobalWalletBalance: {portfolio.GlobalWalletBalance}");
            sb.AppendLine("Games:");

            for (var i = 0; i < portfolio.Games.Count; i++)
            {
                var game = portfolio.Games[i];
                sb.AppendLine($" - {game.GameId}: plays={game.PlayCount}, sessions={game.SessionCount}, wallet={game.WalletBalance}");
            }

            portfolioSnapshot = sb.ToString();
        }

        public void SetStatus(string message, bool isError = false)
        {
            statusMessage = isError ? $"ERROR: {message}" : message;
        }

        public void SetDebug(string message)
        {
            debugMessage = message ?? string.Empty;
        }

        private void OnDisable()
        {
            if (_avatarLoadCoroutine != null)
            {
                StopCoroutine(_avatarLoadCoroutine);
                _avatarLoadCoroutine = null;
            }

            if (_avatarTexture != null)
            {
                Destroy(_avatarTexture);
                _avatarTexture = null;
            }
        }

        private void RequestAvatarPreview(string rawUrl)
        {
            if (_avatarLoadCoroutine != null)
            {
                StopCoroutine(_avatarLoadCoroutine);
                _avatarLoadCoroutine = null;
            }

            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                SetAvatarTexture(null, "No avatar URL set.");
                return;
            }

            var url = rawUrl.Trim();
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                SetAvatarTexture(null, "Avatar URL must be http/https.");
                return;
            }

            avatarPreviewStatus = "Loading avatar...";
            _avatarLoadCoroutine = StartCoroutine(LoadAvatarPreviewCoroutine(url));
        }

        private IEnumerator LoadAvatarPreviewCoroutine(string url)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url, true))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    SetAvatarTexture(null, "Failed to load avatar preview.");
                    yield break;
                }

                var texture = DownloadHandlerTexture.GetContent(request);
                SetAvatarTexture(texture, "Avatar loaded.");
            }
        }

        private void SetAvatarTexture(Texture2D texture, string status)
        {
            if (_avatarTexture != null && _avatarTexture != texture)
            {
                Destroy(_avatarTexture);
            }

            _avatarTexture = texture;
            avatarPreviewStatus = status ?? string.Empty;
        }

        private void OnGUI()
        {
            if (!renderOnGui)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, Mathf.Min(Screen.width - 32f, 760f), Screen.height - 32f), GUI.skin.box);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("IntelliVerseX Profile Test Scene", GUI.skin.label);
            GUILayout.Space(8f);

            GUILayout.Label($"Status: {statusMessage}");
            if (!string.IsNullOrEmpty(debugMessage))
            {
                GUILayout.Label($"Debug: {debugMessage}");
            }

            GUILayout.Space(10f);
            GUILayout.Label("Edit Profile");
            firstName = DrawTextField("First Name", firstName);
            lastName = DrawTextField("Last Name", lastName);
            city = DrawTextField("City", city);
            region = DrawTextField("Region", region);
            country = DrawTextField("Country", country);
            countryCode = DrawTextField("Country Code", countryCode);
            locale = DrawTextField("Locale", locale);
            avatarUrl = DrawTextField("Avatar Url", avatarUrl);

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Profile", GUILayout.Height(30f)))
            {
                OnRefreshClicked?.Invoke();
            }
            if (GUILayout.Button("Save Profile", GUILayout.Height(30f)))
            {
                OnSaveClicked?.Invoke();
            }
            if (GUILayout.Button("Fetch Portfolio", GUILayout.Height(30f)))
            {
                OnPortfolioClicked?.Invoke();
            }
            if (GUILayout.Button("Preview Avatar", GUILayout.Height(30f)))
            {
                RequestAvatarPreview(avatarUrl);
                OnPreviewAvatarClicked?.Invoke();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("Avatar Preview: " + avatarPreviewStatus);
            if (_avatarTexture != null)
            {
                GUILayout.Box(_avatarTexture, GUILayout.Width(120f), GUILayout.Height(120f));
            }
            GUILayout.Space(8f);
            GUILayout.Label(profileSnapshot);
            GUILayout.Space(8f);
            GUILayout.Label(portfolioSnapshot);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static string DrawTextField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(130f));
            var updated = GUILayout.TextField(value ?? string.Empty, GUILayout.MinWidth(200f));
            GUILayout.EndHorizontal();
            return updated;
        }
    }
}
