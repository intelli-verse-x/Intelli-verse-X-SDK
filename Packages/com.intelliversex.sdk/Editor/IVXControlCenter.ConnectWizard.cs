using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Identity;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Control Center Connect: Auth V2 → unique App ID → bootstrap Game ID → runtime IVXURLs.
    /// Passwords / bearer tokens stay in memory only.
    /// </summary>
    public sealed partial class IVXControlCenter
    {
        private enum ConnectSourceMode
        {
            CreateNew = 0,
            UseExisting = 1
        }

        private enum AuthOverlay
        {
            None = 0,
            Signup = 1,
            Forgot = 2
        }

        private const string PrefConnectMode = "IVX.ControlCenter.ConnectMode";

        // Auth V2 + unique-appid wizard state (editor-only; never persist secrets)
        private string _loginEmail = "";
        private string _loginPassword = "";
        private string _wizardAccessToken = "";
        private string _wizardRefreshToken = "";
        private string _wizardIdpUsername = "";
        private string _wizardUserLabel = "";
        private string _wizardDisplayName = "";
        private string _wizardGroupsLabel = "";
        private long _wizardTokenExpiryEpoch;
        private string _createGameName = "";
        private bool _wizardBusy;
        private string _wizardBusyLabel = "";
        private string _wizardStatus = "";
        private MessageType _wizardStatusType = MessageType.None;
        private CancellationTokenSource _wizardCts;
        private string _lastCreatedAppId = "";
        private bool _focusConnectBanner;
        private int _recentGamesPopupIndex;
        private string _renameGameName = "";
        private ConnectSourceMode _connectMode = ConnectSourceMode.CreateNew;
        private bool _pendingClearExpiredSession;

        // In-wizard OTP signup / forgot-password overlay state (memory only)
        private AuthOverlay _authOverlay = AuthOverlay.None;
        private string _signupEmail = "";
        private string _signupPassword = "";
        private string _signupUserName = "";
        private string _signupFirstName = "";
        private string _signupLastName = "";
        private string _signupOtp = "";
        private bool _signupAwaitingOtp;
        private string _signupStatus = "";
        private string _forgotEmail = "";
        private string _forgotOtp = "";
        private string _forgotNewPassword = "";
        private bool _forgotAwaitingOtp;
        private string _forgotStatus = "";

        private bool IsWizardLoggedIn => !string.IsNullOrWhiteSpace(_wizardAccessToken);

        private bool IsWizardTokenExpired
        {
            get
            {
                if (_wizardTokenExpiryEpoch <= 0)
                    return false;
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return now + 30 >= _wizardTokenExpiryEpoch;
            }
        }

        private bool HasUsableWizardSession => IsWizardLoggedIn && !IsWizardTokenExpired;

        private bool CanRefreshWizardToken =>
            !string.IsNullOrWhiteSpace(_wizardRefreshToken)
            && !string.IsNullOrWhiteSpace(_wizardIdpUsername);

        private bool HasValidBootstrapGameId =>
            _config != null && IVXConnectWizardValidation.IsValidUuid(_config.GameId);

        private void DrawConnect()
        {
            if (_pendingClearExpiredSession)
            {
                _pendingClearExpiredSession = false;
                ClearExpiredSessionFields();
            }

            EditorGUILayout.LabelField("2. Connect", EditorStyles.boldLabel);
            DrawConnectProgress();

            if (_focusConnectBanner && !HasValidBootstrapGameId)
            {
                EditorGUILayout.HelpBox(
                    "First-run: pick Create new (sign in + game name) or Use existing (paste / recent).",
                    MessageType.Info);
            }

            if (_config == null)
            {
                EditorGUILayout.HelpBox(
                    "Create a bootstrap connection file once. Your Game ID is stored there.",
                    MessageType.Info);
                if (GUILayout.Button("Create connection file", GUILayout.Height(34)))
                {
                    CreateConfigAsset();
                    PrefillCreateGameNameIfEmpty();
                }
                return;
            }

            if (_configSo == null)
                _configSo = new SerializedObject(_config);

            HandleConnectKeyboardShortcuts();
            DrawCurrentGameIdStrip();

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawConnectAccountCard();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            DrawConnectSourceModeToolbar();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            switch (_connectMode)
            {
                case ConnectSourceMode.CreateNew:
                    DrawConnectRegisterCard();
                    break;
                case ConnectSourceMode.UseExisting:
                    DrawUseExistingCard();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EditorGUILayout.EndVertical();

            DrawWizardStatusAndBusy();

            if (!string.IsNullOrWhiteSpace(_lastCreatedAppId) && HasValidBootstrapGameId)
            {
                EditorGUILayout.Space(6);
                DrawCreatedSuccessActions();
            }

            // Nakama host/port/key intentionally omitted from Control Center (consumer security).
            // Maintainers: IntelliVerseX → Advanced Setup → Backend, or Bootstrap Config inspector.
        }

        private void DrawConnectProgress()
        {
            bool hasConfig = _config != null;
            bool signedIn = HasUsableWizardSession;
            bool hasGameId = HasValidBootstrapGameId;

            EditorGUILayout.BeginHorizontal();
            DrawProgressChip("Config", hasConfig);
            DrawProgressChip("Account", signedIn || hasGameId);
            DrawProgressChip("Game ID", hasGameId);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Portal", EditorStyles.miniButton, GUILayout.Width(56)))
                Application.OpenURL(IVXConnectWizardValidation.DevelopersUrl);
            EditorGUILayout.EndHorizontal();

            string tip;
            if (!hasConfig)
                tip = "Create the connection file to continue.";
            else if (!hasGameId && _connectMode == ConnectSourceMode.CreateNew && !signedIn)
                tip = "Sign in, enter a game name, then create a unique App ID.";
            else if (!hasGameId && _connectMode == ConnectSourceMode.CreateNew)
                tip = "Enter a game name and create a unique App ID.";
            else if (!hasGameId)
                tip = "Paste a UUID, pick a recent ID, or switch to Create new.";
            else
                tip = "Game ID is set. Optionally ping Nakama, then continue to Play.";

            EditorGUILayout.LabelField(tip, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4);
        }

        private static void DrawProgressChip(string label, bool done)
        {
            var prev = GUI.color;
            GUI.color = done ? new Color(0.55f, 0.9f, 0.6f) : new Color(0.72f, 0.72f, 0.72f);
            GUILayout.Label((done ? "● " : "○ ") + label, EditorStyles.miniLabel, GUILayout.Width(72));
            GUI.color = prev;
        }

        private void DrawCurrentGameIdStrip()
        {
            if (!HasValidBootstrapGameId)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Active Game ID", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(_config.GameId.Trim(), EditorStyles.textField, GUILayout.Height(18));
            if (GUILayout.Button("Copy", GUILayout.Width(52), GUILayout.Height(20)))
            {
                EditorGUIUtility.systemCopyBuffer = _config.GameId.Trim();
                SetWizardStatus("Game ID copied.", MessageType.Info);
            }
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrWhiteSpace(_config.GameName))
                EditorGUILayout.LabelField("Name: " + _config.GameName, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Runtime IVXURLs.GameId is kept in sync when you edit this config.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawConnectSourceModeToolbar()
        {
            EditorGUI.BeginChangeCheck();
            int mode = GUILayout.Toolbar(
                (int)_connectMode,
                new[] { "Create new", "Use existing" },
                GUILayout.Height(26));
            if (EditorGUI.EndChangeCheck())
            {
                _connectMode = (ConnectSourceMode)mode;
                EditorPrefs.SetInt(PrefConnectMode, mode);
            }
        }

        private void DrawWizardStatusAndBusy()
        {
            if (!string.IsNullOrEmpty(_wizardStatus))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(_wizardStatus, _wizardStatusType);
            }

            if (!_wizardBusy)
                return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(BusyLabelWithPulse(_wizardBusyLabel), EditorStyles.miniLabel);
            if (GUILayout.Button("Cancel", GUILayout.Width(72), GUILayout.Height(22)))
            {
                CancelWizardWork();
                SetWizardStatus("Cancelled.", MessageType.Warning);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawConnectAccountCard()
        {
            EditorGUILayout.LabelField("IntelliVerse account (Auth V2)", EditorStyles.boldLabel);

            if (IsWizardLoggedIn && IsWizardTokenExpired)
            {
                if (CanRefreshWizardToken)
                {
                    EditorGUILayout.HelpBox(
                        "Access token expired. Refresh to continue without re-entering your password.",
                        MessageType.Warning);
                    EditorGUI.BeginDisabledGroup(_wizardBusy);
                    if (GUILayout.Button("Refresh session", GUILayout.Height(28)))
                        _ = RefreshWizardTokenAsync(force: true);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Session expired. Sign in again.",
                        MessageType.Warning);
                    _pendingClearExpiredSession = true;
                }
            }
            else if (HasUsableWizardSession)
            {
                string who = string.IsNullOrWhiteSpace(_wizardDisplayName)
                    ? _wizardUserLabel
                    : _wizardDisplayName + " · " + _wizardUserLabel;
                if (string.IsNullOrWhiteSpace(who))
                    who = "Signed in";

                EditorGUILayout.HelpBox("Signed in as " + who + ".", MessageType.Info);
                if (!string.IsNullOrWhiteSpace(_wizardGroupsLabel))
                    EditorGUILayout.LabelField("Roles: " + _wizardGroupsLabel, EditorStyles.miniLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(_wizardBusy);
                if (GUILayout.Button("Sign out", GUILayout.Height(26)))
                    SignOutWizard();
                EditorGUI.BeginDisabledGroup(!CanRefreshWizardToken);
                if (GUILayout.Button("Refresh token", GUILayout.Height(26)))
                    _ = RefreshWizardTokenAsync(force: true);
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();
                GUILayout.FlexibleSpace();
                if (_wizardTokenExpiryEpoch > 0)
                {
                    var left = TimeSpan.FromSeconds(Math.Max(0, _wizardTokenExpiryEpoch - DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    EditorGUILayout.LabelField("Token ~" + FormatRemaining(left), EditorStyles.miniLabel, GUILayout.Width(110));
                }
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.LabelField(
                "Same login your game uses at runtime. Password is never saved to disk.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUI.BeginDisabledGroup(_wizardBusy);
            _loginEmail = EditorGUILayout.TextField(new GUIContent("Email"), _loginEmail);
            _loginPassword = EditorGUILayout.PasswordField(new GUIContent("Password"), _loginPassword);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(_wizardBusy || !IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out _));
            GUI.backgroundColor = new Color(0.45f, 0.75f, 1f);
            string signLabel = _wizardBusy && _wizardBusyLabel.StartsWith("Sign", StringComparison.Ordinal)
                ? BusyLabelWithPulse("Signing in")
                : "Sign in";
            if (GUILayout.Button(signLabel, GUILayout.Height(32)))
                _ = SignInWizardAsync();
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create account", EditorStyles.miniButton))
                OpenSignupOverlay();
            if (GUILayout.Button("Forgot password", EditorStyles.miniButton))
                OpenForgotOverlay();
            EditorGUILayout.EndHorizontal();

            if (!IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out string blockReason) && !_wizardBusy)
                EditorGUILayout.LabelField(blockReason, EditorStyles.miniLabel);
        }

        private void DrawConnectRegisterCard()
        {
            EditorGUILayout.LabelField("Create unique App ID", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Calls unique-appid with your Auth V2 access token, then writes the UUID into bootstrap + IVXURLs.",
                EditorStyles.wordWrappedMiniLabel);

            bool canCreate = HasUsableWizardSession && !_wizardBusy;
            EditorGUI.BeginDisabledGroup(!canCreate);
            _createGameName = EditorGUILayout.TextField(
                new GUIContent("Game name", "2–80 characters"),
                _createGameName);

            int len = (_createGameName ?? string.Empty).Trim().Length;
            EditorGUILayout.LabelField(len + " / " + IVXConnectWizardValidation.MaxGameNameLength, EditorStyles.miniLabel);

            GUI.backgroundColor = canCreate && IVXConnectWizardValidation.TryValidateGameName(_createGameName, out _)
                ? new Color(0.45f, 0.9f, 0.55f)
                : Color.white;
            string createLabel = _wizardBusy && _wizardBusyLabel.IndexOf("unique", StringComparison.OrdinalIgnoreCase) >= 0
                ? BusyLabelWithPulse("Creating unique App ID")
                : "Create unique App ID";
            if (GUILayout.Button(createLabel, GUILayout.Height(34)))
                _ = CreateUniqueAppIdWizardAsync();
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();

            if (!HasUsableWizardSession)
                EditorGUILayout.HelpBox("Sign in above first (Create new path).", MessageType.Warning);
            else if (!IVXConnectWizardValidation.TryValidateGameName(_createGameName, out string nameError))
                EditorGUILayout.LabelField(nameError, EditorStyles.miniLabel);
            else if (HasValidBootstrapGameId)
                EditorGUILayout.HelpBox("A Game ID is already set — create will ask before replacing it.", MessageType.None);
        }

        private void DrawUseExistingCard()
        {
            EditorGUILayout.LabelField("Use an existing Game ID", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Paste a UUID, apply a recent ID from this Editor, or edit the bootstrap fields.",
                EditorStyles.wordWrappedMiniLabel);

            DrawRecentGamesPicker();
            EditorGUILayout.Space(4);

            _configSo.Update();
            EditorGUILayout.PropertyField(
                _configSo.FindProperty("_gameId"),
                new GUIContent("Game ID", "UUID from dashboard or a previous create."));
            EditorGUILayout.PropertyField(_configSo.FindProperty("_gameName"), new GUIContent("Game name"));

            string gid = _configSo.FindProperty("_gameId")?.stringValue ?? "";
            if (!string.IsNullOrWhiteSpace(gid) && !IVXConnectWizardValidation.IsValidUuid(gid))
            {
                EditorGUILayout.HelpBox(
                    "Game ID should look like a UUID (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).",
                    MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(gid));
            if (GUILayout.Button("Copy", GUILayout.Width(64)))
                EditorGUIUtility.systemCopyBuffer = gid.Trim();
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Paste from clipboard", GUILayout.Width(150)))
                PasteGameIdFromClipboard();
            if (GUILayout.Button("Apply & verify", GUILayout.Width(110)))
            {
                if (IVXConnectWizardValidation.IsValidUuid(gid))
                {
                    string name = _configSo.FindProperty("_gameName")?.stringValue ?? "";
                    string smoke;
                    bool ok = ApplyGameIdToConfig(gid.Trim(), name, remember: true, out smoke);
                    SetWizardStatus(
                        (ok ? "Game ID applied.\n" : "Applied with issues.\n") + gid.Trim() + "\n" + smoke,
                        ok ? MessageType.Info : MessageType.Warning);
                }
                else
                {
                    SetWizardStatus("Enter a valid UUID Game ID first.", MessageType.Error);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_configSo.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_config);
                if (IVXConnectWizardValidation.IsValidUuid(_config.GameId))
                {
                    ApplyGameIdToRuntime(_config.GameId);
                    IVXConnectWizardValidation.RememberRecentGame(_config.GameId, _config.GameName);
                }
            }
        }

        private void DrawRecentGamesPicker()
        {
            var recent = IVXConnectWizardValidation.LoadRecentGames();
            EditorGUILayout.LabelField("Recent on this machine", EditorStyles.miniBoldLabel);
            string[] labels = IVXConnectWizardValidation.RecentGamePopupLabels(recent);
            EditorGUI.BeginChangeCheck();
            int next = EditorGUILayout.Popup("Apply recent", _recentGamesPopupIndex, labels);
            if (!EditorGUI.EndChangeCheck())
                return;

            _recentGamesPopupIndex = next;
            if (recent.Length == 0 || next <= 0 || next > recent.Length)
                return;

            var entry = recent[next - 1];
            string smoke;
            bool ok = ApplyGameIdToConfig(entry.id, entry.name, remember: true, out smoke);
            _recentGamesPopupIndex = 0;
            SetWizardStatus(
                (ok ? "Applied recent Game ID.\n" : "Applied with issues.\n") + entry.id + "\n" + smoke,
                ok ? MessageType.Info : MessageType.Warning);
        }

        private void DrawCreatedSuccessActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Just created", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(_lastCreatedAppId, EditorStyles.textField, GUILayout.Height(18));

            EditorGUILayout.LabelField("Display name (local bootstrap only)", EditorStyles.miniBoldLabel);
            _renameGameName = EditorGUILayout.TextField(_renameGameName);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save display name", GUILayout.Height(26)))
            {
                if (IVXConnectWizardValidation.TryValidateGameName(_renameGameName, out string err))
                {
                    ApplyGameNameOnly(_renameGameName.Trim());
                    IVXConnectWizardValidation.RememberRecentGame(_lastCreatedAppId, _renameGameName.Trim());
                    SetWizardStatus(
                        "Local display name saved. Cloud title edits are in the Developers portal.",
                        MessageType.Info);
                }
                else
                {
                    SetWizardStatus(err, MessageType.Error);
                }
            }
            if (GUILayout.Button("Copy Game ID", GUILayout.Height(26)))
            {
                EditorGUIUtility.systemCopyBuffer = _lastCreatedAppId;
                SetWizardStatus("Game ID copied.", MessageType.Info);
            }
            if (GUILayout.Button("Select config", GUILayout.Height(26)))
            {
                Selection.activeObject = _config;
                EditorGUIUtility.PingObject(_config);
            }
            if (GUILayout.Button("Portal", GUILayout.Height(26)))
                Application.OpenURL(IVXConnectWizardValidation.DevelopersUrl);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void PasteGameIdFromClipboard()
        {
            string clip = (EditorGUIUtility.systemCopyBuffer ?? "").Trim();
            if (!IVXConnectWizardValidation.IsValidUuid(clip))
            {
                SetWizardStatus("Clipboard does not contain a UUID Game ID.", MessageType.Warning);
                return;
            }

            string name = _configSo?.FindProperty("_gameName")?.stringValue ?? "";
            string smoke;
            bool ok = ApplyGameIdToConfig(clip, name, remember: true, out smoke);
            SetWizardStatus(
                (ok ? "Pasted Game ID.\n" : "Pasted with issues.\n") + clip + "\n" + smoke,
                ok ? MessageType.Info : MessageType.Warning);
        }

        private void HandleConnectKeyboardShortcuts()
        {
            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown)
                return;
            if (e.keyCode != KeyCode.Return && e.keyCode != KeyCode.KeypadEnter)
                return;
            if (_wizardBusy)
                return;

            if (!HasUsableWizardSession)
            {
                if (IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out _))
                {
                    _ = SignInWizardAsync();
                    e.Use();
                }
                return;
            }

            if (_connectMode == ConnectSourceMode.CreateNew
                && IVXConnectWizardValidation.TryValidateGameName(_createGameName, out _))
            {
                _ = CreateUniqueAppIdWizardAsync();
                e.Use();
            }
        }

        private void LoadConnectWizardPrefs()
        {
            if (string.IsNullOrWhiteSpace(_loginEmail))
                _loginEmail = EditorPrefs.GetString(IVXConnectWizardValidation.PrefEmail, string.Empty) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_createGameName))
                _createGameName = EditorPrefs.GetString(IVXConnectWizardValidation.PrefGameName, string.Empty) ?? string.Empty;
            _connectMode = (ConnectSourceMode)Mathf.Clamp(
                EditorPrefs.GetInt(PrefConnectMode, (int)ConnectSourceMode.CreateNew), 0, 1);
            _focusConnectBanner = EditorPrefs.GetBool(IVXConnectWizardValidation.PrefFocusConnect, false);
            if (_focusConnectBanner)
                EditorPrefs.DeleteKey(IVXConnectWizardValidation.PrefFocusConnect);

            if (_focusConnectBanner && !HasValidBootstrapGameId)
                _connectMode = ConnectSourceMode.CreateNew;
        }

        private void PrefillCreateGameNameIfEmpty()
        {
            if (!string.IsNullOrWhiteSpace(_createGameName))
                return;
            if (_config != null && !string.IsNullOrWhiteSpace(_config.GameName))
            {
                _createGameName = _config.GameName.Trim();
                return;
            }

            string product = Application.productName;
            if (!string.IsNullOrWhiteSpace(product)
                && !string.Equals(product, "My project", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(product, "My Project", StringComparison.OrdinalIgnoreCase))
            {
                _createGameName = product.Trim();
            }
        }

        private void TryHydrateWizardSessionFromMemory()
        {
            if (!string.IsNullOrWhiteSpace(_wizardAccessToken))
                return;
            if (!UserSessionManager.HasSession)
                return;

            _wizardAccessToken = UserSessionManager.AccessToken;
            _wizardTokenExpiryEpoch = UserSessionManager.Current?.accessTokenExpiryEpoch ?? 0;
            _wizardRefreshToken = UserSessionManager.Current?.refreshToken ?? "";
            _wizardIdpUsername = UserSessionManager.Current?.idpUsername ?? "";
            var session = UserSessionManager.Current;
            if (session == null)
                return;

            _wizardUserLabel = !string.IsNullOrWhiteSpace(session.email) ? session.email : session.userId;
            _wizardDisplayName = BuildDisplayName(session.firstName, session.lastName, session.userName);
            _wizardGroupsLabel = TryReadJwtGroups(_wizardAccessToken);
            if (!string.IsNullOrWhiteSpace(session.email))
                _loginEmail = session.email;

            if (CanRefreshWizardToken)
                ConfigureWizardUserAuth();
        }

        private void CancelWizardWork()
        {
            try { _wizardCts?.Cancel(); } catch { /* ignore */ }
            try { _wizardCts?.Dispose(); } catch { /* ignore */ }
            _wizardCts = null;
            _wizardBusy = false;
            _wizardBusyLabel = string.Empty;
        }

        private void ClearExpiredSessionFields()
        {
            _wizardAccessToken = string.Empty;
            _wizardTokenExpiryEpoch = 0;
        }

        private void SignOutWizard()
        {
            CancelWizardWork();
            _wizardAccessToken = string.Empty;
            _wizardRefreshToken = string.Empty;
            _wizardIdpUsername = string.Empty;
            _wizardUserLabel = string.Empty;
            _wizardDisplayName = string.Empty;
            _wizardGroupsLabel = string.Empty;
            _wizardTokenExpiryEpoch = 0;
            _loginPassword = string.Empty;
            _lastCreatedAppId = string.Empty;
            try { APIManager.ClearUserAuth(); } catch { /* optional */ }
            SetWizardStatus("Signed out. Password and tokens cleared from memory.", MessageType.Info);
            NotifyUiChanged();
        }

        private void SetWizardStatus(string message, MessageType type)
        {
            _wizardStatus = message ?? string.Empty;
            _wizardStatusType = type;
            NotifyUiChanged();
        }

        private void ClearSignupSecrets()
        {
            _signupPassword = string.Empty;
            _signupOtp = string.Empty;
        }

        private void ClearForgotSecrets()
        {
            _forgotNewPassword = string.Empty;
            _forgotOtp = string.Empty;
        }

        private async Task SignupInitiateWizardAsync()
        {
            if (_wizardBusy)
                return;

            string email = (_signupEmail ?? string.Empty).Trim();
            string password = _signupPassword ?? string.Empty;
            if (!IVXConnectWizardValidation.IsValidEmail(email))
            {
                _signupStatus = "Enter a valid email.";
                NotifyUiChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                _signupStatus = "Password must be at least 8 characters (upper, lower, digit, special).";
                NotifyUiChanged();
                return;
            }

            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Sending signup OTP";
            _signupStatus = "Contacting Auth V2 signup…";
            NotifyUiChanged();

            try
            {
                var response = await APIManager.SignupInitiateAsync(email, password, "user", ct);
                if (response == null || !response.status)
                {
                    _signupStatus = HumanizeApiMessage(response?.message, "Could not send signup OTP.");
                    return;
                }

                _signupAwaitingOtp = true;
                _signupStatus = HumanizeApiMessage(response.message, "OTP sent. Check your email, then confirm below.");
                if (string.IsNullOrWhiteSpace(_signupUserName))
                {
                    int at = email.IndexOf('@');
                    _signupUserName = at > 0 ? email.Substring(0, at) : email;
                }
            }
            catch (OperationCanceledException)
            {
                _signupStatus = "Signup cancelled.";
            }
            catch (Exception ex)
            {
                _signupStatus = HumanizeException(ex, "signup initiate");
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                NotifyUiChanged();
            }
        }

        private async Task SignupConfirmWizardAsync()
        {
            if (_wizardBusy)
                return;

            string email = (_signupEmail ?? string.Empty).Trim();
            string otp = (_signupOtp ?? string.Empty).Trim();
            string password = _signupPassword ?? string.Empty;
            string userName = (_signupUserName ?? string.Empty).Trim();

            if (!IVXConnectWizardValidation.IsValidEmail(email))
            {
                _signupStatus = "Enter a valid email.";
                NotifyUiChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(otp))
            {
                _signupStatus = "Enter the OTP from your email.";
                NotifyUiChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(userName) || userName.Length < 3)
            {
                _signupStatus = "Username must be at least 3 characters.";
                NotifyUiChanged();
                return;
            }

            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Confirming signup";
            _signupStatus = "Confirming OTP…";
            NotifyUiChanged();

            try
            {
                var request = new APIManager.SignupConfirmRequest
                {
                    email = email,
                    otp = otp,
                    password = password,
                    userName = userName,
                    firstName = (_signupFirstName ?? string.Empty).Trim(),
                    lastName = (_signupLastName ?? string.Empty).Trim(),
                    role = "user",
                    fromDevice = "unity"
                };

                var response = await APIManager.SignupConfirmAsync(
                    request,
                    configureUserAuthOnSuccess: true,
                    ct: ct);

                if (response == null || !response.status || response.data == null)
                {
                    _signupStatus = HumanizeApiMessage(response?.message, "Signup confirm failed.");
                    return;
                }

                string access = !string.IsNullOrWhiteSpace(response.data.token)
                    ? response.data.token
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(access))
                {
                    _signupStatus = "Signup succeeded but no access token came back. Sign in with your new account.";
                    ClearSignupSecrets();
                    _signupAwaitingOtp = false;
                    _authOverlay = AuthOverlay.None;
                    _loginEmail = email;
                    SetWizardStatus("Account created — sign in with your new credentials.", MessageType.Info);
                    return;
                }

                _wizardAccessToken = access.Trim();
                _wizardRefreshToken = response.data.refreshToken?.Trim() ?? "";
                _wizardIdpUsername = response.data.user?.idpUsername?.Trim() ?? "";
                _wizardTokenExpiryEpoch = ComputeTokenExpiryEpoch(response.data.expiresIn, _wizardAccessToken);
                _wizardGroupsLabel = TryReadJwtGroups(_wizardAccessToken);

                if (response.data.user != null)
                {
                    _wizardUserLabel = !string.IsNullOrWhiteSpace(response.data.user.email)
                        ? response.data.user.email
                        : response.data.user.userName;
                    _wizardDisplayName = BuildDisplayName(
                        response.data.user.firstName,
                        response.data.user.lastName,
                        response.data.user.userName);
                }
                else
                {
                    _wizardUserLabel = email;
                    _wizardDisplayName = string.Empty;
                }

                _loginEmail = email;
                EditorPrefs.SetString(IVXConnectWizardValidation.PrefEmail, email);
                ClearSignupSecrets();
                _signupAwaitingOtp = false;
                _authOverlay = AuthOverlay.None;
                _connectMode = ConnectSourceMode.CreateNew;
                EditorPrefs.SetInt(PrefConnectMode, (int)_connectMode);
                PrefillCreateGameNameIfEmpty();
                SetWizardStatus("Account created and signed in. Enter a game name and create a unique App ID.", MessageType.Info);
            }
            catch (OperationCanceledException)
            {
                _signupStatus = "Signup confirm cancelled.";
            }
            catch (Exception ex)
            {
                _signupStatus = HumanizeException(ex, "signup confirm");
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                NotifyUiChanged();
            }
        }

        private async Task ForgotPasswordWizardAsync()
        {
            if (_wizardBusy)
                return;

            string email = (_forgotEmail ?? string.Empty).Trim();
            if (!IVXConnectWizardValidation.IsValidEmail(email))
            {
                _forgotStatus = "Enter a valid email.";
                NotifyUiChanged();
                return;
            }

            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Sending reset OTP";
            _forgotStatus = "Requesting password reset…";
            NotifyUiChanged();

            try
            {
                var response = await APIManager.ForgotPasswordAsync(email, ct);
                if (response == null || !response.status)
                {
                    _forgotStatus = HumanizeApiMessage(response?.message, "Could not send reset OTP.");
                    return;
                }

                _forgotAwaitingOtp = true;
                _forgotStatus = HumanizeApiMessage(response.message, "OTP sent. Enter the code and your new password.");
            }
            catch (OperationCanceledException)
            {
                _forgotStatus = "Forgot-password cancelled.";
            }
            catch (Exception ex)
            {
                _forgotStatus = HumanizeException(ex, "forgot password");
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                NotifyUiChanged();
            }
        }

        private async Task ResetPasswordWizardAsync()
        {
            if (_wizardBusy)
                return;

            string email = (_forgotEmail ?? string.Empty).Trim();
            string otp = (_forgotOtp ?? string.Empty).Trim();
            string newPassword = _forgotNewPassword ?? string.Empty;

            if (!IVXConnectWizardValidation.IsValidEmail(email))
            {
                _forgotStatus = "Enter a valid email.";
                NotifyUiChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(otp))
            {
                _forgotStatus = "Enter the OTP from your email.";
                NotifyUiChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                _forgotStatus = "New password must be at least 8 characters (upper, lower, digit, special).";
                NotifyUiChanged();
                return;
            }

            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Resetting password";
            _forgotStatus = "Resetting password…";
            NotifyUiChanged();

            try
            {
                var response = await APIManager.ResetPasswordAsync(email, otp, newPassword, ct);
                if (response == null || !response.status)
                {
                    _forgotStatus = HumanizeApiMessage(response?.message, "Password reset failed.");
                    return;
                }

                _loginEmail = email;
                _loginPassword = string.Empty;
                ClearForgotSecrets();
                _forgotAwaitingOtp = false;
                _authOverlay = AuthOverlay.None;
                SetWizardStatus(
                    HumanizeApiMessage(response.message, "Password reset. Sign in with your new password."),
                    MessageType.Info);
            }
            catch (OperationCanceledException)
            {
                _forgotStatus = "Password reset cancelled.";
            }
            catch (Exception ex)
            {
                _forgotStatus = HumanizeException(ex, "password reset");
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                NotifyUiChanged();
            }
        }

        private void ConfigureWizardUserAuth()
        {
            if (!CanRefreshWizardToken || string.IsNullOrWhiteSpace(_wizardAccessToken))
                return;

            APIManager.ConfigureUserAuth(
                useUserAuthToken: true,
                idpUsername: _wizardIdpUsername,
                refreshToken: _wizardRefreshToken,
                initialAccessToken: _wizardAccessToken,
                accessTokenExpiresInEpoch: _wizardTokenExpiryEpoch > 0
                    ? _wizardTokenExpiryEpoch
                    : DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1800);
        }

        /// <summary>Ensures a usable access token; refreshes when expired if possible.</summary>
        private async Task<bool> EnsureFreshAccessTokenAsync(CancellationToken ct)
        {
            if (HasUsableWizardSession)
                return true;

            if (!CanRefreshWizardToken)
                return false;

            ConfigureWizardUserAuth();
            await APIManager.RefreshUserTokenAsync(ct);
            string access = await APIManager.GetUserAccessTokenAsync(ct);
            if (string.IsNullOrWhiteSpace(access))
                return false;

            ApplyAccessToken(access.Trim());
            return HasUsableWizardSession;
        }

        private void ApplyAccessToken(string access)
        {
            _wizardAccessToken = access;
            _wizardGroupsLabel = TryReadJwtGroups(_wizardAccessToken);
            if (TryReadJwtExp(_wizardAccessToken, out long exp) && exp > 0)
                _wizardTokenExpiryEpoch = exp;
            else
                _wizardTokenExpiryEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1800;
            ConfigureWizardUserAuth();
        }

        private async Task SignInWizardAsync()
        {
            if (_wizardBusy)
                return;

            if (!IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out string block))
            {
                SetWizardStatus(block, MessageType.Error);
                return;
            }

            string email = _loginEmail.Trim();
            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Signing in";
            SetWizardStatus("Contacting Auth V2…", MessageType.Info);

            try
            {
                string routeGameId = HasValidBootstrapGameId ? _config.GameId.Trim() : IVXURLs.DefaultGameId;
                var request = new APIManager.LoginRequest
                {
                    email = email,
                    password = _loginPassword,
                    fromDevice = "unity",
                    gameId = routeGameId
                };

                var response = await APIManager.LoginAsync(
                    request,
                    configureUserAuthOnSuccess: true,
                    persistSession: false,
                    ct: ct);

                if (response == null || !response.status || response.data == null)
                {
                    SetWizardStatus(HumanizeApiMessage(response?.message, "Sign-in failed. Check email and password."), MessageType.Error);
                    return;
                }

                string access = !string.IsNullOrWhiteSpace(response.data.accessToken)
                    ? response.data.accessToken
                    : response.data.token;

                if (string.IsNullOrWhiteSpace(access))
                {
                    SetWizardStatus("Sign-in succeeded but no access token came back.", MessageType.Error);
                    return;
                }

                _wizardAccessToken = access.Trim();
                _wizardRefreshToken = response.data.refreshToken?.Trim() ?? "";
                _wizardIdpUsername = response.data.user?.idpUsername?.Trim() ?? "";
                _wizardTokenExpiryEpoch = ComputeTokenExpiryEpoch(response.data.expiresIn, _wizardAccessToken);
                _wizardGroupsLabel = TryReadJwtGroups(_wizardAccessToken);

                if (response.data.user != null)
                {
                    _wizardUserLabel = !string.IsNullOrWhiteSpace(response.data.user.email)
                        ? response.data.user.email
                        : response.data.user.userName;
                    _wizardDisplayName = BuildDisplayName(
                        response.data.user.firstName,
                        response.data.user.lastName,
                        response.data.user.userName);
                }
                else
                {
                    _wizardUserLabel = email;
                    _wizardDisplayName = string.Empty;
                }

                _loginPassword = string.Empty;
                EditorPrefs.SetString(IVXConnectWizardValidation.PrefEmail, email);
                PrefillCreateGameNameIfEmpty();
                _focusConnectBanner = false;
                _connectMode = ConnectSourceMode.CreateNew;
                EditorPrefs.SetInt(PrefConnectMode, (int)_connectMode);
                SetWizardStatus("Signed in. Enter a game name and create a unique App ID.", MessageType.Info);
            }
            catch (OperationCanceledException)
            {
                SetWizardStatus("Sign-in cancelled.", MessageType.Warning);
            }
            catch (Exception ex)
            {
                SetWizardStatus(HumanizeException(ex, "sign-in"), MessageType.Error);
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                NotifyUiChanged();
            }
        }

        private async Task RefreshWizardTokenAsync(bool force)
        {
            if (_wizardBusy)
                return;
            if (!CanRefreshWizardToken)
            {
                SetWizardStatus("No refresh credentials. Sign in again.", MessageType.Error);
                return;
            }

            if (!force && !IsWizardTokenExpired)
                return;

            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Refreshing session";
            SetWizardStatus("Refreshing access token…", MessageType.Info);

            try
            {
                if (!await EnsureFreshAccessTokenAsync(ct))
                    throw new Exception("Refresh returned an empty access token.");
                SetWizardStatus("Session refreshed.", MessageType.Info);
            }
            catch (OperationCanceledException)
            {
                SetWizardStatus("Refresh cancelled.", MessageType.Warning);
            }
            catch (Exception ex)
            {
                _wizardAccessToken = string.Empty;
                _wizardTokenExpiryEpoch = 0;
                SetWizardStatus(HumanizeException(ex, "token refresh") + " Sign in again.", MessageType.Error);
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                NotifyUiChanged();
            }
        }

        private async Task CreateUniqueAppIdWizardAsync()
        {
            if (_wizardBusy)
                return;

            if (!IVXConnectWizardValidation.TryValidateGameName(_createGameName, out string nameError))
            {
                SetWizardStatus(nameError, MessageType.Error);
                return;
            }

            if (_config == null)
            {
                SetWizardStatus("Create a connection file first.", MessageType.Error);
                return;
            }

            string gameName = _createGameName.Trim();

            if (HasValidBootstrapGameId)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Replace Game ID?",
                    "Bootstrap already has:\n" + _config.GameId +
                    "\n\nCreating a new unique App ID will replace it in this project’s config.\n" +
                    "The previous ID is not deleted on the server.",
                    "Replace",
                    "Cancel");
                if (!replace)
                    return;
            }

            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Creating unique App ID";
            SetWizardStatus("Registering \"" + gameName + "\"…", MessageType.Info);

            try
            {
                if (!await EnsureFreshAccessTokenAsync(ct))
                {
                    SetWizardStatus("Sign in first — then create a unique App ID.", MessageType.Error);
                    return;
                }

                APIManager.UniqueAppIdResponse response;
                try
                {
                    response = await APIManager.CreateUniqueAppIdAsync(gameName, _wizardAccessToken, ct);
                }
                catch (Exception firstEx) when (IsUnauthorizedMessage(firstEx.Message) && CanRefreshWizardToken)
                {
                    SetWizardStatus("Unauthorized — refreshing token and retrying once…", MessageType.Warning);
                    if (!await EnsureFreshAccessTokenAsync(ct))
                        throw;
                    response = await APIManager.CreateUniqueAppIdAsync(gameName, _wizardAccessToken, ct);
                }

                string uniqueId = response.data.uniqueAppId.Trim();
                if (!IVXConnectWizardValidation.IsValidUuid(uniqueId))
                {
                    SetWizardStatus("Server returned an unexpected App ID format. Nothing was saved.", MessageType.Error);
                    return;
                }

                string smoke;
                bool ok = ApplyGameIdToConfig(uniqueId, gameName, remember: true, out smoke);
                _lastCreatedAppId = uniqueId;
                _renameGameName = gameName;
                EditorPrefs.SetString(IVXConnectWizardValidation.PrefGameName, gameName);
                _focusConnectBanner = false;

                if (HasUsableWizardSession)
                {
                    SetWizardStatus("Running online Game ID verify…", MessageType.Info);
                    var online = await APIManager.VerifyGameIdOnlineAsync(uniqueId, _wizardAccessToken, ct);
                    smoke += "\n• Online: " + online.message;
                    if (!online.OverallOk && online.localOk && online.authOk && !online.gameAccepted)
                        ok = false;
                }

                SetWizardStatus(
                    (ok ? "Unique App ID created and verified.\n" : "Unique App ID created with verification issues.\n")
                    + uniqueId + "\n" + smoke,
                    ok ? MessageType.Info : MessageType.Warning);
            }
            catch (OperationCanceledException)
            {
                SetWizardStatus("Create cancelled.", MessageType.Warning);
            }
            catch (Exception ex)
            {
                string msg = HumanizeException(ex, "unique App ID");
                if (IsUnauthorizedMessage(msg))
                {
                    _wizardAccessToken = string.Empty;
                    _wizardTokenExpiryEpoch = 0;
                    msg = "Session expired or unauthorized. Sign in again, then retry.";
                }

                SetWizardStatus(msg, MessageType.Error);
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                NotifyUiChanged();
            }
        }

        private bool ApplyGameIdToConfig(string gameId, string gameName, bool remember, out string smokeDetail)
        {
            smokeDetail = string.Empty;
            if (_config == null || !IVXConnectWizardValidation.IsValidUuid(gameId))
            {
                smokeDetail = "• Invalid Game ID or missing config.";
                return false;
            }

            if (_configSo == null)
                _configSo = new SerializedObject(_config);

            _configSo.Update();
            SerializedProperty gameIdProp = _configSo.FindProperty("_gameId");
            SerializedProperty gameNameProp = _configSo.FindProperty("_gameName");
            if (gameIdProp != null)
                gameIdProp.stringValue = gameId.Trim();
            if (gameNameProp != null && !string.IsNullOrWhiteSpace(gameName))
                gameNameProp.stringValue = gameName.Trim();
            _configSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();

            ApplyGameIdToRuntime(gameId);
            if (remember)
                IVXConnectWizardValidation.RememberRecentGame(gameId, gameName);

            return EvaluateSmokeCheck(gameId, out smokeDetail);
        }

        private void ApplyGameNameOnly(string gameName)
        {
            if (_config == null)
                return;
            if (_configSo == null)
                _configSo = new SerializedObject(_config);
            _configSo.Update();
            SerializedProperty gameNameProp = _configSo.FindProperty("_gameName");
            if (gameNameProp != null)
                gameNameProp.stringValue = gameName;
            _configSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
        }

        private static void ApplyGameIdToRuntime(string gameId)
        {
            if (!IVXConnectWizardValidation.IsValidUuid(gameId))
                return;
            IVXURLs.GameId = gameId.Trim();
        }

        private bool EvaluateSmokeCheck(string gameId, out string detail)
        {
            var messages = new StringBuilder();
            bool ok = true;
            string trimmed = gameId.Trim();

            if (!IVXConnectWizardValidation.IsValidUuid(trimmed))
            {
                ok = false;
                messages.AppendLine("• Game ID is not a valid UUID.");
            }

            if (_config == null || !string.Equals(_config.GameId?.Trim(), trimmed, StringComparison.OrdinalIgnoreCase))
            {
                ok = false;
                messages.AppendLine("• Bootstrap config was not updated.");
            }
            else
            {
                messages.AppendLine("• Bootstrap Game ID updated.");
            }

            if (!string.Equals(IVXURLs.GameId, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                ok = false;
                messages.AppendLine("• Runtime IVXURLs.GameId was not applied.");
            }
            else
            {
                messages.AppendLine("• Runtime IVXURLs.GameId synced.");
            }

            if (_config != null && _config.Validate(logWarnings: false))
                messages.AppendLine("• Bootstrap Validate() passed.");
            else
            {
                ok = false;
                messages.AppendLine("• Bootstrap Validate() failed.");
            }

            detail = messages.ToString().TrimEnd();
            return ok;
        }

        private static string BusyLabelWithPulse(string baseLabel)
        {
            int ticks = (int)(EditorApplication.timeSinceStartup * 4) % 4;
            return baseLabel + new string('.', ticks + 1);
        }

        private static string FormatRemaining(TimeSpan span)
        {
            if (span.TotalHours >= 1)
                return ((int)span.TotalHours) + "h " + span.Minutes + "m left";
            if (span.TotalMinutes >= 1)
                return span.Minutes + "m left";
            return Math.Max(0, (int)span.TotalSeconds) + "s left";
        }

        private static string BuildDisplayName(string first, string last, string fallback)
        {
            string f = (first ?? string.Empty).Trim();
            string l = (last ?? string.Empty).Trim();
            if (f.Length == 0 && l.Length == 0)
                return (fallback ?? string.Empty).Trim();
            return (f + " " + l).Trim();
        }

        private static long ComputeTokenExpiryEpoch(int expiresInSeconds, string jwt)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (expiresInSeconds > 0)
                return now + expiresInSeconds;
            if (TryReadJwtExp(jwt, out long exp) && exp > now)
                return exp;
            return now + 1800;
        }

        private static bool TryReadJwtExp(string jwt, out long exp)
        {
            exp = 0;
            if (string.IsNullOrWhiteSpace(jwt))
                return false;

            string[] parts = jwt.Split('.');
            if (parts.Length < 2)
                return false;

            try
            {
                string payload = parts[1].Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                string json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                Match m = Regex.Match(json, "\"exp\"\\s*:\\s*(\\d+)");
                if (!m.Success)
                    return false;
                return long.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out exp);
            }
            catch
            {
                return false;
            }
        }

        private static string TryReadJwtGroups(string jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
                return string.Empty;

            string[] parts = jwt.Split('.');
            if (parts.Length < 2)
                return string.Empty;

            try
            {
                string payload = parts[1].Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                string json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                Match array = Regex.Match(json, "\"cognito:groups\"\\s*:\\s*\\[([^\\]]*)\\]");
                if (!array.Success)
                    return string.Empty;

                var matches = Regex.Matches(array.Groups[1].Value, "\"([^\"]+)\"");
                if (matches.Count == 0)
                    return string.Empty;

                var sb = new StringBuilder();
                for (int i = 0; i < matches.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(matches[i].Groups[1].Value);
                }

                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsUnauthorizedMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return false;
            return msg.IndexOf("401", StringComparison.Ordinal) >= 0
                   || msg.IndexOf("Unauthorized", StringComparison.OrdinalIgnoreCase) >= 0
                   || msg.IndexOf("invalid token", StringComparison.OrdinalIgnoreCase) >= 0
                   || msg.IndexOf("token expired", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string HumanizeApiMessage(string message, string fallback)
        {
            if (string.IsNullOrWhiteSpace(message))
                return fallback;
            string m = message.Trim();
            if (m.Length > 280)
                m = m.Substring(0, 280) + "…";
            return m;
        }

        private static string HumanizeException(Exception ex, string action)
        {
            if (ex == null)
                return "Unknown error during " + action + ".";

            string raw = ex.Message ?? string.Empty;
            Match jsonMsg = Regex.Match(raw, "\"message\"\\s*:\\s*\"([^\"]+)\"");
            if (jsonMsg.Success)
                return HumanizeApiMessage(jsonMsg.Groups[1].Value, raw);

            if (raw.IndexOf("HTTP 401", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Unauthorized (401). Sign in again.";
            if (raw.IndexOf("HTTP 403", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Forbidden (403). This account may lack permission to create games.";
            if (raw.IndexOf("HTTP 409", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Conflict (409). Try a different game name.";
            if (raw.IndexOf("HTTP 429", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Too many requests. Wait a moment and try again.";
            if (raw.IndexOf("HTTP 5", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Server error. Try again in a minute.";
            if (raw.IndexOf("Cannot resolve", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("ConnectionError", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("No connection", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Network problem during " + action + ". Check internet connectivity.";

            return HumanizeApiMessage(raw, "Something went wrong during " + action + ".");
        }
    }
}
