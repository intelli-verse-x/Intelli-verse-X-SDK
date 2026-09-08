using System;
using System.Collections.Generic;
using IntelliVerseX.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// UI Toolkit shell for Control Center (CreateGUI). Binds to Connect wizard session methods.
    /// </summary>
    public sealed partial class IVXControlCenter
    {
        private const string UssAssetPath =
            "Packages/com.intelliversex.sdk/Editor/UIToolkit/IVXControlCenter.uss";

        private VisualElement _root;
        private VisualElement _contentHost;
        private VisualElement _homePanel;
        private VisualElement _trafficPanel;
        private VisualElement _apisPanel;
        private ScrollView _homeScrollView;
        private ScrollView _trafficScrollView;
        private ScrollView _apisScrollView;
        private VisualElement _overlayHost;
        private VisualElement _signupCard;
        private VisualElement _forgotCard;
        private VisualElement _busyBar;

        private Button _tabHome;
        private Button _tabTraffic;
        private Button _tabApis;

        private Label _chipConfig;
        private Label _chipAccount;
        private Label _chipGameId;
        private Label _progressTip;
        private VisualElement _focusBanner;
        private VisualElement _connectBody;
        private VisualElement _accountHost;
        private VisualElement _createHost;
        private VisualElement _existingHost;
        private VisualElement _successHost;
        private VisualElement _serverHost;
        private VisualElement _wizardStatusBox;
        private Label _wizardStatusLabel;
        private Label _busyLabel;

        private TextField _loginEmailField;
        private TextField _loginPasswordField;
        private TextField _createGameNameField;
        private TextField _existingGameIdField;
        private TextField _existingGameNameField;
        private TextField _renameGameNameField;
        private TextField _serverHostField;
        private IntegerField _serverPortField;
        private TextField _serverKeyField;
        private Toggle _serverSslToggle;
        private DropdownField _recentGamesDropdown;
        private Button _modeCreateBtn;
        private Button _modeExistingBtn;
        private Label _activeGameIdLabel;
        private VisualElement _activeGameIdStrip;
        private VisualElement _checkRows;
        private VisualElement _playRows;
        private Label _playWarn;
        private Label _serverPingLabel;
        private Label _trafficMeta;
        private VisualElement _trafficList;
        private TextField _apiFilterField;
        private VisualElement _apiList;
        private Label _apiSourceLabel;

        // Signup overlay fields
        private TextField _signupEmailField;
        private TextField _signupPasswordField;
        private TextField _signupUserNameField;
        private TextField _signupFirstNameField;
        private TextField _signupLastNameField;
        private TextField _signupOtpField;
        private VisualElement _signupStepInitiate;
        private VisualElement _signupStepConfirm;
        private Label _signupStatusLabel;

        // Forgot overlay fields
        private TextField _forgotEmailField;
        private TextField _forgotOtpField;
        private TextField _forgotNewPasswordField;
        private VisualElement _forgotStepRequest;
        private VisualElement _forgotStepReset;
        private Label _forgotStatusLabel;

        private bool _uiBuilt;
        private bool _suppressFieldCallbacks;
        private IVisualElementScheduledItem _panelTransitionJob;

        public void CreateGUI()
        {
            _root = rootVisualElement;
            _root.Clear();
            _root.AddToClassList("ivx-root");

            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssAssetPath);
            if (uss != null)
                _root.styleSheets.Add(uss);

            BuildChrome();
            BuildHomePanel();
            BuildTrafficPanel();
            BuildApisPanel();
            BuildOverlays();

            _uiBuilt = true;
            SelectTab(_tab, animate: false);
            RefreshAllUi();
        }

        private void NotifyUiChanged()
        {
            if (!_uiBuilt || _root == null)
            {
                Repaint();
                return;
            }

            RefreshAllUi();
            Repaint();
        }

        private void BuildChrome()
        {
            var subtitle = new Label("Check · Connect · Play");
            subtitle.AddToClassList("ivx-subtitle");
            _root.Add(subtitle);

            var tabs = new VisualElement();
            tabs.AddToClassList("ivx-tabs");
            _tabHome = MakeTabButton("Home", () => SelectTab(Tab.Home));
            _tabTraffic = MakeTabButton("Traffic", () => SelectTab(Tab.Traffic));
            _tabApis = MakeTabButton("APIs", () => SelectTab(Tab.Apis));
            tabs.Add(_tabHome);
            tabs.Add(_tabTraffic);
            tabs.Add(_tabApis);
            _root.Add(tabs);

            _contentHost = new VisualElement { name = "ivx-content" };
            _contentHost.AddToClassList("ivx-content");
            _root.Add(_contentHost);

            _busyBar = new VisualElement();
            _busyBar.AddToClassList("ivx-busy-bar");
            _busyBar.style.display = DisplayStyle.None;
            _busyLabel = new Label();
            _busyLabel.AddToClassList("ivx-hint");
            _busyLabel.style.flexGrow = 1;
            _busyLabel.style.marginBottom = 0;
            var cancelBtn = MakeButton("Cancel", () =>
            {
                CancelWizardWork();
                SetWizardStatus("Cancelled.", MessageType.Warning);
            }, "ivx-btn");
            cancelBtn.style.width = 72;
            _busyBar.Add(_busyLabel);
            _busyBar.Add(cancelBtn);
            _root.Add(_busyBar);
        }

        private void BuildHomePanel()
        {
            _homePanel = new VisualElement { name = "home-panel" };
            _homePanel.AddToClassList("ivx-panel");
            _homeScrollView = new ScrollView(ScrollViewMode.Vertical);
            _homeScrollView.AddToClassList("ivx-scroll");
            _homePanel.Add(_homeScrollView);
            _contentHost.Add(_homePanel);

            // Check
            var check = MakeSection("1. Check");
            _checkRows = new VisualElement();
            check.Add(_checkRows);
            var checkActions = new VisualElement();
            checkActions.AddToClassList("ivx-row");
            checkActions.Add(MakeButton("Fix project settings", OnFixProjectSettings, "ivx-btn", "ivx-btn--primary"));
            checkActions.Add(MakeButton("Install dependencies", () => IVXAdvancedSetup.ShowWindow(), "ivx-btn"));
            check.Add(checkActions);
            _homeScrollView.Add(check);

            // Connect
            var connect = MakeSection("2. Connect");
            var progress = new VisualElement();
            progress.AddToClassList("ivx-row");
            _chipConfig = MakeChip("Config");
            _chipAccount = MakeChip("Account");
            _chipGameId = MakeChip("Game ID");
            progress.Add(_chipConfig);
            progress.Add(_chipAccount);
            progress.Add(_chipGameId);
            var grow = new VisualElement();
            grow.AddToClassList("ivx-grow");
            progress.Add(grow);
            progress.Add(MakeButton("Portal", () => Application.OpenURL(IVXConnectWizardValidation.DevelopersUrl), "ivx-btn", "ivx-btn--ghost"));
            connect.Add(progress);

            _progressTip = new Label();
            _progressTip.AddToClassList("ivx-hint");
            connect.Add(_progressTip);

            _focusBanner = MakeHelp("First-run: pick Create new (sign in + game name) or Use existing (paste / recent).", HelpBoxMessageType.Info);
            _focusBanner.style.display = DisplayStyle.None;
            connect.Add(_focusBanner);

            _activeGameIdStrip = new VisualElement();
            _activeGameIdStrip.AddToClassList("ivx-section");
            _activeGameIdStrip.style.marginBottom = 8;
            _activeGameIdStrip.style.display = DisplayStyle.None;
            var activeTitle = new Label("Active Game ID");
            activeTitle.AddToClassList("ivx-section-title");
            _activeGameIdStrip.Add(activeTitle);
            var gidRow = new VisualElement();
            gidRow.AddToClassList("ivx-row");
            _activeGameIdLabel = new Label();
            _activeGameIdLabel.style.flexGrow = 1;
            _activeGameIdLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            gidRow.Add(_activeGameIdLabel);
            gidRow.Add(MakeButton("Copy", () =>
            {
                if (_config != null && !string.IsNullOrWhiteSpace(_config.GameId))
                {
                    EditorGUIUtility.systemCopyBuffer = _config.GameId.Trim();
                    SetWizardStatus("Game ID copied.", MessageType.Info);
                }
            }, "ivx-btn"));
            _activeGameIdStrip.Add(gidRow);
            connect.Add(_activeGameIdStrip);

            _connectBody = new VisualElement();
            connect.Add(_connectBody);

            // no config
            var noConfig = new VisualElement { name = "no-config" };
            noConfig.Add(MakeHelp(
                "Create a bootstrap connection file once. Your Game ID is stored there.",
                HelpBoxMessageType.Info));
            noConfig.Add(MakeButton("Create connection file", () =>
            {
                CreateConfigAsset();
                PrefillCreateGameNameIfEmpty();
                NotifyUiChanged();
            }, "ivx-btn", "ivx-btn--primary"));
            _connectBody.Add(noConfig);

            // Mode first — one path: Create new OR Use existing (not two parallel wizards).
            var modeRow = new VisualElement { name = "connect-mode-row" };
            modeRow.AddToClassList("ivx-mode-toggle");
            _modeCreateBtn = MakeButton("Create new", () => SetConnectMode(ConnectSourceMode.CreateNew), "ivx-btn");
            _modeExistingBtn = MakeButton("Use existing", () => SetConnectMode(ConnectSourceMode.UseExisting), "ivx-btn");
            modeRow.Add(_modeCreateBtn);
            modeRow.Add(_modeExistingBtn);
            _connectBody.Add(modeRow);

            _accountHost = new VisualElement { name = "account-host" };
            _accountHost.AddToClassList("ivx-section");
            _connectBody.Add(_accountHost);

            _createHost = new VisualElement { name = "create-host" };
            _createHost.AddToClassList("ivx-section");
            _connectBody.Add(_createHost);

            _existingHost = new VisualElement { name = "existing-host" };
            _existingHost.AddToClassList("ivx-section");
            _connectBody.Add(_existingHost);

            _wizardStatusBox = new VisualElement();
            _wizardStatusBox.style.display = DisplayStyle.None;
            _wizardStatusLabel = new Label();
            _wizardStatusLabel.AddToClassList("ivx-help");
            _wizardStatusLabel.style.whiteSpace = WhiteSpace.Normal;
            _wizardStatusBox.Add(_wizardStatusLabel);
            _connectBody.Add(_wizardStatusBox);

            _successHost = new VisualElement { name = "success-host" };
            _successHost.AddToClassList("ivx-section");
            _successHost.style.display = DisplayStyle.None;
            _connectBody.Add(_successHost);

            _serverHost = new VisualElement { name = "server-host" };
            // Intentionally empty — Nakama host/key are not shown in Control Center (security).
            // Maintainers: Advanced Setup → Backend, or Bootstrap Config inspector foldout.
            _serverHost.style.display = DisplayStyle.None;
            _connectBody.Add(_serverHost);

            BuildAccountUi();
            BuildCreateUi();
            BuildExistingUi();
            BuildSuccessUi();
            // BuildServerUi omitted from consumer Connect surface.

            _homeScrollView.Add(connect);

            // Play
            var play = MakeSection("3. Play");
            _playWarn = new Label();
            _playWarn.AddToClassList("ivx-help");
            _playWarn.AddToClassList("ivx-help--warn");
            _playWarn.style.display = DisplayStyle.None;
            play.Add(_playWarn);
            _playRows = new VisualElement();
            play.Add(_playRows);
            play.Add(MakeButton("Add bootstrap to this scene", AddBootstrapToScene, "ivx-btn", "ivx-btn--primary"));
            var playActions = new VisualElement();
            playActions.AddToClassList("ivx-row");
            playActions.Add(MakeButton("Install demo scenes", () => IVXConsumerAssetInstaller.InstallDemoScenesOnly(), "ivx-btn"));
            playActions.Add(MakeButton("Advanced setup", () => IVXAdvancedSetup.ShowWindow(), "ivx-btn"));
            play.Add(playActions);
            play.Add(MakeHint("Press Unity Play. Traffic tab shows IVXRequestBus calls. Photon is optional."));
            _homeScrollView.Add(play);

            var footer = new Label(
                "Unity CLI (preferred over MCP)\nunity open .   unity test --mode EditMode   unity build --target Android");
            footer.AddToClassList("ivx-footer");
            _homeScrollView.Add(footer);
        }

        private void BuildAccountUi()
        {
            _accountHost.Clear();
            var title = new Label("IntelliVerse account (Auth V2)");
            title.AddToClassList("ivx-section-title");
            _accountHost.Add(title);
            _accountHost.AddToClassList("ivx-section");

            // Dynamic content rebuilt on refresh via RefreshAccountUi — placeholder shell fields:
            _loginEmailField = new TextField("Email") { name = "login-email" };
            _loginEmailField.AddToClassList("ivx-field");
            _loginEmailField.RegisterValueChangedCallback(evt =>
            {
                if (_suppressFieldCallbacks) return;
                _loginEmail = evt.newValue ?? "";
            });

            _loginPasswordField = new TextField("Password") { name = "login-password", isPasswordField = true };
            _loginPasswordField.AddToClassList("ivx-field");
            _loginPasswordField.RegisterValueChangedCallback(evt =>
            {
                if (_suppressFieldCallbacks) return;
                _loginPassword = evt.newValue ?? "";
            });
        }

        private void BuildCreateUi()
        {
            _createHost.Clear();
            var title = new Label("Create unique App ID");
            title.AddToClassList("ivx-section-title");
            _createHost.Add(title);
            _createHost.Add(MakeHint(
                "Calls unique-appid with your Auth V2 access token, then writes the UUID into bootstrap + IVXURLs."));

            _createGameNameField = new TextField("Game name") { name = "create-game-name" };
            _createGameNameField.tooltip = "2–80 characters";
            _createGameNameField.AddToClassList("ivx-field");
            _createGameNameField.RegisterValueChangedCallback(evt =>
            {
                if (_suppressFieldCallbacks) return;
                _createGameName = evt.newValue ?? "";
                RefreshCreateEnabled();
            });
            _createHost.Add(_createGameNameField);

            var createBtn = MakeButton("Create unique App ID", () => _ = CreateUniqueAppIdWizardAsync(), "ivx-btn", "ivx-btn--success");
            createBtn.name = "create-appid-btn";
            _createHost.Add(createBtn);
        }

        private void BuildExistingUi()
        {
            _existingHost.Clear();
            var title = new Label("Use an existing Game ID");
            title.AddToClassList("ivx-section-title");
            _existingHost.Add(title);
            _existingHost.Add(MakeHint("Paste a UUID, apply a recent ID from this Editor, or edit the bootstrap fields."));

            _recentGamesDropdown = new DropdownField("Apply recent");
            _recentGamesDropdown.RegisterValueChangedCallback(evt =>
            {
                if (_suppressFieldCallbacks) return;
                ApplyRecentFromDropdown(evt.newValue);
            });
            _existingHost.Add(_recentGamesDropdown);

            _existingGameIdField = new TextField("Game ID");
            _existingGameIdField.AddToClassList("ivx-field");
            _existingGameIdField.RegisterValueChangedCallback(evt =>
            {
                if (_suppressFieldCallbacks) return;
                WriteConfigString("_gameId", evt.newValue);
            });
            _existingHost.Add(_existingGameIdField);

            _existingGameNameField = new TextField("Game name");
            _existingGameNameField.AddToClassList("ivx-field");
            _existingGameNameField.RegisterValueChangedCallback(evt =>
            {
                if (_suppressFieldCallbacks) return;
                WriteConfigString("_gameName", evt.newValue);
            });
            _existingHost.Add(_existingGameNameField);

            var row = new VisualElement();
            row.AddToClassList("ivx-row");
            row.Add(MakeButton("Copy", () =>
            {
                string gid = _existingGameIdField?.value ?? "";
                if (!string.IsNullOrWhiteSpace(gid))
                    EditorGUIUtility.systemCopyBuffer = gid.Trim();
            }, "ivx-btn"));
            row.Add(MakeButton("Paste from clipboard", PasteGameIdFromClipboard, "ivx-btn"));
            row.Add(MakeButton("Apply & verify", () =>
            {
                string gid = _existingGameIdField?.value ?? "";
                if (IVXConnectWizardValidation.IsValidUuid(gid))
                {
                    string name = _existingGameNameField?.value ?? "";
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
            }, "ivx-btn", "ivx-btn--primary"));
            _existingHost.Add(row);
        }

        private void BuildSuccessUi()
        {
            _successHost.Clear();
            var title = new Label("Just created");
            title.AddToClassList("ivx-section-title");
            _successHost.Add(title);

            var idLabel = new Label { name = "last-created-id" };
            _successHost.Add(idLabel);

            _successHost.Add(MakeHint("Display name (local bootstrap only)"));
            _renameGameNameField = new TextField();
            _renameGameNameField.RegisterValueChangedCallback(evt =>
            {
                if (_suppressFieldCallbacks) return;
                _renameGameName = evt.newValue ?? "";
            });
            _successHost.Add(_renameGameNameField);

            var row = new VisualElement();
            row.AddToClassList("ivx-row");
            row.Add(MakeButton("Save display name", () =>
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
            }, "ivx-btn"));
            row.Add(MakeButton("Copy Game ID", () =>
            {
                EditorGUIUtility.systemCopyBuffer = _lastCreatedAppId;
                SetWizardStatus("Game ID copied.", MessageType.Info);
            }, "ivx-btn"));
            row.Add(MakeButton("Select config", () =>
            {
                Selection.activeObject = _config;
                EditorGUIUtility.PingObject(_config);
            }, "ivx-btn"));
            row.Add(MakeButton("Portal", () => Application.OpenURL(IVXConnectWizardValidation.DevelopersUrl), "ivx-btn"));
            _successHost.Add(row);
        }

        /// <summary>
        /// Intentionally empty — Nakama host/port/key stay out of Control Center.
        /// Maintainers: Advanced Setup → Backend, or Bootstrap Config inspector.
        /// </summary>
        private void BuildServerUi()
        {
        }

        private void BuildTrafficPanel()
        {
            _trafficPanel = new VisualElement { name = "traffic-panel" };
            _trafficPanel.AddToClassList("ivx-panel");
            _trafficPanel.style.display = DisplayStyle.None;

            _trafficScrollView = new ScrollView(ScrollViewMode.Vertical);
            _trafficScrollView.AddToClassList("ivx-scroll");

            var head = MakeSection("Traffic");
            _trafficMeta = new Label();
            _trafficMeta.AddToClassList("ivx-hint");
            head.Add(_trafficMeta);
            var actions = new VisualElement();
            actions.AddToClassList("ivx-row");
            actions.Add(MakeButton("Clear history", () =>
            {
                IVXRequestBus.ClearTrafficHistory();
                RefreshTrafficUi();
            }, "ivx-btn"));
            actions.Add(MakeButton("Refresh", RefreshTrafficUi, "ivx-btn"));
            head.Add(actions);

            var header = new VisualElement();
            header.AddToClassList("ivx-traffic-header");
            header.Add(ColLabel("UTC", "ivx-col-utc"));
            header.Add(ColLabel("RPC", "ivx-col-rpc"));
            header.Add(ColLabel("#", "ivx-col-n"));
            header.Add(ColLabel("Status", "ivx-col-status"));
            header.Add(ColLabel("ms", "ivx-col-ms"));
            header.Add(ColLabel("Error / retry", "ivx-col-err"));
            head.Add(header);
            _trafficScrollView.Add(head);

            _trafficList = new VisualElement { name = "traffic-list" };
            _trafficScrollView.Add(_trafficList);
            _trafficPanel.Add(_trafficScrollView);
            _contentHost.Add(_trafficPanel);
        }

        private void BuildApisPanel()
        {
            _apisPanel = new VisualElement { name = "apis-panel" };
            _apisPanel.AddToClassList("ivx-panel");
            _apisPanel.style.display = DisplayStyle.None;

            _apisScrollView = new ScrollView(ScrollViewMode.Vertical);
            _apisScrollView.AddToClassList("ivx-scroll");

            var head = MakeSection("APIs");
            _apiSourceLabel = new Label();
            _apiSourceLabel.AddToClassList("ivx-hint");
            head.Add(_apiSourceLabel);

            var filterRow = new VisualElement();
            filterRow.AddToClassList("ivx-row");
            _apiFilterField = new TextField("Filter");
            _apiFilterField.style.flexGrow = 1;
            _apiFilterField.RegisterValueChangedCallback(evt =>
            {
                _apiFilter = evt.newValue ?? "";
                RefreshApisUi();
            });
            filterRow.Add(_apiFilterField);
            filterRow.Add(MakeButton("Reload", () =>
            {
                IVXRpcIndex.Invalidate();
                RefreshApisUi();
            }, "ivx-btn"));
            head.Add(filterRow);
            _apisScrollView.Add(head);

            _apiList = new VisualElement { name = "api-list" };
            _apisScrollView.Add(_apiList);

            var canon = MakeSection("Canonical public types");
            canon.Add(MakeHint("Wallet → IVXNWalletManager"));
            canon.Add(MakeHint("Leaderboard → IVXNLeaderbordManager"));
            canon.Add(MakeHint("Optional → com.intelliversex.sdk.ai / .discord / .photon"));
            _apisScrollView.Add(canon);

            _apisPanel.Add(_apisScrollView);
            _contentHost.Add(_apisPanel);
        }

        private void BuildOverlays()
        {
            _overlayHost = new VisualElement { name = "overlay-host" };
            _overlayHost.AddToClassList("ivx-overlay-host");
            var backdrop = new VisualElement();
            backdrop.AddToClassList("ivx-overlay-backdrop");
            backdrop.RegisterCallback<ClickEvent>(_ => CloseAuthOverlays());
            _overlayHost.Add(backdrop);

            _signupCard = BuildSignupOverlay();
            _forgotCard = BuildForgotOverlay();
            _overlayHost.Add(_signupCard);
            _overlayHost.Add(_forgotCard);
            _contentHost.Add(_overlayHost);
        }

        private VisualElement BuildSignupOverlay()
        {
            var card = new VisualElement { name = "signup-card" };
            card.AddToClassList("ivx-overlay-card");
            card.style.display = DisplayStyle.None;

            var title = new Label("Create account (OTP)");
            title.AddToClassList("ivx-overlay-title");
            card.Add(title);
            card.Add(MakeHint("In-wizard Auth V2 signup. Password is never saved to disk."));

            _signupStepInitiate = new VisualElement();
            _signupEmailField = new TextField("Email");
            _signupPasswordField = new TextField("Password") { isPasswordField = true };
            _signupUserNameField = new TextField("Username");
            _signupFirstNameField = new TextField("First name");
            _signupLastNameField = new TextField("Last name");
            _signupStepInitiate.Add(_signupEmailField);
            _signupStepInitiate.Add(_signupPasswordField);
            _signupStepInitiate.Add(_signupUserNameField);
            _signupStepInitiate.Add(_signupFirstNameField);
            _signupStepInitiate.Add(_signupLastNameField);
            _signupStepInitiate.Add(MakeButton("Send OTP", () =>
            {
                SyncSignupFieldsFromUi();
                _ = SignupInitiateWizardAsync();
            }, "ivx-btn", "ivx-btn--primary"));
            card.Add(_signupStepInitiate);

            _signupStepConfirm = new VisualElement();
            _signupStepConfirm.style.display = DisplayStyle.None;
            _signupOtpField = new TextField("OTP code");
            _signupStepConfirm.Add(_signupOtpField);
            _signupStepConfirm.Add(MakeButton("Confirm signup", () =>
            {
                SyncSignupFieldsFromUi();
                _ = SignupConfirmWizardAsync();
            }, "ivx-btn", "ivx-btn--success"));
            _signupStepConfirm.Add(MakeButton("Resend OTP", () =>
            {
                SyncSignupFieldsFromUi();
                _ = SignupInitiateWizardAsync();
            }, "ivx-btn", "ivx-btn--ghost"));
            card.Add(_signupStepConfirm);

            _signupStatusLabel = new Label();
            _signupStatusLabel.AddToClassList("ivx-hint");
            _signupStatusLabel.style.whiteSpace = WhiteSpace.Normal;
            card.Add(_signupStatusLabel);

            var footer = new VisualElement();
            footer.AddToClassList("ivx-row");
            footer.Add(MakeButton("Back to sign in", CloseAuthOverlays, "ivx-btn"));
            card.Add(footer);
            return card;
        }

        private VisualElement BuildForgotOverlay()
        {
            var card = new VisualElement { name = "forgot-card" };
            card.AddToClassList("ivx-overlay-card");
            card.style.display = DisplayStyle.None;

            var title = new Label("Reset password (OTP)");
            title.AddToClassList("ivx-overlay-title");
            card.Add(title);
            card.Add(MakeHint("Request a reset code, then set a new password. Nothing is persisted to disk."));

            _forgotStepRequest = new VisualElement();
            _forgotEmailField = new TextField("Email");
            _forgotStepRequest.Add(_forgotEmailField);
            _forgotStepRequest.Add(MakeButton("Send reset OTP", () =>
            {
                SyncForgotFieldsFromUi();
                _ = ForgotPasswordWizardAsync();
            }, "ivx-btn", "ivx-btn--primary"));
            card.Add(_forgotStepRequest);

            _forgotStepReset = new VisualElement();
            _forgotStepReset.style.display = DisplayStyle.None;
            _forgotOtpField = new TextField("OTP code");
            _forgotNewPasswordField = new TextField("New password") { isPasswordField = true };
            _forgotStepReset.Add(_forgotOtpField);
            _forgotStepReset.Add(_forgotNewPasswordField);
            _forgotStepReset.Add(MakeButton("Reset password", () =>
            {
                SyncForgotFieldsFromUi();
                _ = ResetPasswordWizardAsync();
            }, "ivx-btn", "ivx-btn--success"));
            _forgotStepReset.Add(MakeButton("Resend OTP", () =>
            {
                SyncForgotFieldsFromUi();
                _ = ForgotPasswordWizardAsync();
            }, "ivx-btn", "ivx-btn--ghost"));
            card.Add(_forgotStepReset);

            _forgotStatusLabel = new Label();
            _forgotStatusLabel.AddToClassList("ivx-hint");
            _forgotStatusLabel.style.whiteSpace = WhiteSpace.Normal;
            card.Add(_forgotStatusLabel);

            var footer = new VisualElement();
            footer.AddToClassList("ivx-row");
            footer.Add(MakeButton("Back to sign in", CloseAuthOverlays, "ivx-btn"));
            card.Add(footer);
            return card;
        }

        private void SelectTab(Tab tab, bool animate = true)
        {
            _tab = tab;
            _tabHome.EnableInClassList("ivx-tab--active", tab == Tab.Home);
            _tabTraffic.EnableInClassList("ivx-tab--active", tab == Tab.Traffic);
            _tabApis.EnableInClassList("ivx-tab--active", tab == Tab.Apis);

            ShowPanel(_homePanel, tab == Tab.Home, animate);
            ShowPanel(_trafficPanel, tab == Tab.Traffic, animate);
            ShowPanel(_apisPanel, tab == Tab.Apis, animate);

            if (tab == Tab.Traffic)
                RefreshTrafficUi();
            if (tab == Tab.Apis)
                RefreshApisUi();
        }

        private void ShowPanel(VisualElement panel, bool show, bool animate)
        {
            if (panel == null)
                return;

            _panelTransitionJob?.Pause();

            // Always collapse hidden panels immediately so they don't steal flex space
            // (that caused empty Traffic/APIs chrome and overlapping section bars).
            if (!show)
            {
                panel.RemoveFromClassList("ivx-panel--exit");
                panel.style.display = DisplayStyle.None;
                return;
            }

            panel.style.display = DisplayStyle.Flex;
            if (!animate)
            {
                panel.RemoveFromClassList("ivx-panel--exit");
                return;
            }

            panel.AddToClassList("ivx-panel--exit");
            panel.schedule.Execute(() => panel.RemoveFromClassList("ivx-panel--exit")).StartingIn(16);
        }

        private void OpenSignupOverlay()
        {
            _authOverlay = AuthOverlay.Signup;
            if (!_signupAwaitingOtp)
            {
                _signupStatus = string.Empty;
                _signupEmail = string.IsNullOrWhiteSpace(_signupEmail) ? _loginEmail : _signupEmail;
            }
            if (_signupEmailField != null) _signupEmailField.SetValueWithoutNotify(_signupEmail ?? "");
            ShowOverlayCard(_signupCard);
            RefreshSignupOverlayUi();
        }

        private void OpenForgotOverlay()
        {
            _authOverlay = AuthOverlay.Forgot;
            if (!_forgotAwaitingOtp)
            {
                _forgotStatus = string.Empty;
                _forgotEmail = string.IsNullOrWhiteSpace(_forgotEmail) ? _loginEmail : _forgotEmail;
            }
            if (_forgotEmailField != null) _forgotEmailField.SetValueWithoutNotify(_forgotEmail ?? "");
            ShowOverlayCard(_forgotCard);
            RefreshForgotOverlayUi();
        }

        private void CloseAuthOverlays()
        {
            _authOverlay = AuthOverlay.None;
            HideOverlayCard(_signupCard);
            HideOverlayCard(_forgotCard);
            _overlayHost?.RemoveFromClassList("ivx-overlay-host--open");
            ClearSignupSecrets();
            ClearForgotSecrets();
        }

        private void ShowOverlayCard(VisualElement card)
        {
            if (_overlayHost == null || card == null)
                return;

            _signupCard.style.display = DisplayStyle.None;
            _forgotCard.style.display = DisplayStyle.None;
            _signupCard.RemoveFromClassList("ivx-overlay--visible");
            _forgotCard.RemoveFromClassList("ivx-overlay--visible");

            _overlayHost.AddToClassList("ivx-overlay-host--open");
            card.style.display = DisplayStyle.Flex;
            card.schedule.Execute(() => card.AddToClassList("ivx-overlay--visible")).StartingIn(16);
        }

        private void HideOverlayCard(VisualElement card)
        {
            if (card == null)
                return;
            card.RemoveFromClassList("ivx-overlay--visible");
            card.schedule.Execute(() =>
            {
                if (_authOverlay == AuthOverlay.None)
                    card.style.display = DisplayStyle.None;
            }).StartingIn(220);
        }

        private void RefreshAllUi()
        {
            if (!_uiBuilt)
                return;

            if (_pendingClearExpiredSession)
            {
                _pendingClearExpiredSession = false;
                ClearExpiredSessionFields();
            }

            RefreshCheckUi();
            RefreshConnectUi();
            RefreshPlayUi();
            RefreshBusyUi();
            SyncOverlayVisibilityFromState();
            RefreshSignupOverlayUi();
            RefreshForgotOverlayUi();

            if (_tab == Tab.Traffic)
                RefreshTrafficUi();
            if (_tab == Tab.Apis)
                RefreshApisUi();
        }

        private void SyncOverlayVisibilityFromState()
        {
            if (_overlayHost == null)
                return;

            if (_authOverlay == AuthOverlay.None)
            {
                if (_overlayHost.ClassListContains("ivx-overlay-host--open"))
                {
                    HideOverlayCard(_signupCard);
                    HideOverlayCard(_forgotCard);
                    _overlayHost.RemoveFromClassList("ivx-overlay-host--open");
                }
                return;
            }

            if (_authOverlay == AuthOverlay.Signup)
            {
                if (_signupCard != null && _signupCard.style.display == DisplayStyle.None)
                    ShowOverlayCard(_signupCard);
            }
            else if (_authOverlay == AuthOverlay.Forgot)
            {
                if (_forgotCard != null && _forgotCard.style.display == DisplayStyle.None)
                    ShowOverlayCard(_forgotCard);
            }
        }

        private void RefreshCheckUi()
        {
            if (_checkRows == null)
                return;

            _checkRows.Clear();
            bool newtonsoft = TypeExists("Newtonsoft.Json.JsonConvert");
            bool nakama = TypeExists("Nakama.Client");
            var validation = IVXProjectSetup.RunValidation();
            int failed = 0;
            for (int i = 0; i < validation.Count; i++)
            {
                if (!validation[i].Passed && !validation[i].IsWarning)
                    failed++;
            }

            _checkRows.Add(MakeStatusRow("JSON (Newtonsoft)", newtonsoft));
            _checkRows.Add(MakeStatusRow("Nakama client", nakama));
            _checkRows.Add(MakeStatusRow("Project settings", failed == 0));
            _checkRows.Add(MakeStatusRow("Photon (optional)", TypeExists("Photon.Pun.PhotonNetwork"), optional: true));
        }

        private void RefreshConnectUi()
        {
            bool hasConfig = _config != null;
            bool signedIn = HasUsableWizardSession;
            bool hasGameId = HasValidBootstrapGameId;

            SetChip(_chipConfig, hasConfig);
            SetChip(_chipAccount, signedIn || hasGameId);
            SetChip(_chipGameId, hasGameId);

            if (_progressTip != null)
            {
                if (!hasConfig)
                    _progressTip.text = "Create the connection file to continue.";
                else if (!hasGameId && _connectMode == ConnectSourceMode.CreateNew && !signedIn)
                    _progressTip.text = "Sign in, enter a game name, then create a unique App ID.";
                else if (!hasGameId && _connectMode == ConnectSourceMode.CreateNew)
                    _progressTip.text = "Enter a game name and create a unique App ID.";
                else if (!hasGameId)
                    _progressTip.text = "Paste a UUID, pick a recent ID, or switch to Create new.";
                else
                    _progressTip.text = "Game ID is set. Optionally ping Nakama, then continue to Play.";
            }

            if (_focusBanner != null)
                _focusBanner.style.display = (_focusConnectBanner && !hasGameId) ? DisplayStyle.Flex : DisplayStyle.None;

            if (_activeGameIdStrip != null)
            {
                _activeGameIdStrip.style.display = hasGameId ? DisplayStyle.Flex : DisplayStyle.None;
                if (hasGameId && _activeGameIdLabel != null)
                    _activeGameIdLabel.text = _config.GameId.Trim();
            }

            var noConfig = _connectBody?.Q("no-config");
            if (noConfig != null)
                noConfig.style.display = hasConfig ? DisplayStyle.None : DisplayStyle.Flex;

            bool showWizard = hasConfig;
            var modeRow = _connectBody?.Q("connect-mode-row");
            if (modeRow != null)
                modeRow.style.display = showWizard ? DisplayStyle.Flex : DisplayStyle.None;
            if (_modeCreateBtn != null)
            {
                _modeCreateBtn.EnableInClassList("ivx-tab--active", _connectMode == ConnectSourceMode.CreateNew);
                _modeExistingBtn.EnableInClassList("ivx-tab--active", _connectMode == ConnectSourceMode.UseExisting);
            }

            // Auth is only for Create new; Use existing is paste/recent only.
            bool showAccount = showWizard && _connectMode == ConnectSourceMode.CreateNew;
            if (_accountHost != null)
                _accountHost.style.display = showAccount ? DisplayStyle.Flex : DisplayStyle.None;

            if (_createHost != null)
                _createHost.style.display = showWizard && _connectMode == ConnectSourceMode.CreateNew
                    ? DisplayStyle.Flex : DisplayStyle.None;
            if (_existingHost != null)
                _existingHost.style.display = showWizard && _connectMode == ConnectSourceMode.UseExisting
                    ? DisplayStyle.Flex : DisplayStyle.None;
            // Never expose Nakama host/port/key on the consumer Control Center surface.
            if (_serverHost != null)
                _serverHost.style.display = DisplayStyle.None;

            RefreshAccountUi();
            RefreshCreateEnabled();
            RefreshExistingFields();
            RefreshSuccessUi();
            RefreshServerFields();
            RefreshWizardStatusUi();
        }

        private void RefreshAccountUi()
        {
            if (_accountHost == null)
                return;

            _accountHost.Clear();
            var title = new Label("IntelliVerse account (Auth V2)");
            title.AddToClassList("ivx-section-title");
            _accountHost.Add(title);

            if (IsWizardLoggedIn && IsWizardTokenExpired)
            {
                if (CanRefreshWizardToken)
                {
                    _accountHost.Add(MakeHelp(
                        "Access token expired. Refresh to continue without re-entering your password.",
                        HelpBoxMessageType.Warning));
                    var refreshBtn = MakeButton("Refresh session", () => _ = RefreshWizardTokenAsync(force: true), "ivx-btn", "ivx-btn--primary");
                    refreshBtn.SetEnabled(!_wizardBusy);
                    _accountHost.Add(refreshBtn);
                }
                else
                {
                    _accountHost.Add(MakeHelp("Session expired. Sign in again.", HelpBoxMessageType.Warning));
                    _pendingClearExpiredSession = true;
                }
                return;
            }

            if (HasUsableWizardSession)
            {
                string who = string.IsNullOrWhiteSpace(_wizardDisplayName)
                    ? _wizardUserLabel
                    : _wizardDisplayName + " · " + _wizardUserLabel;
                if (string.IsNullOrWhiteSpace(who))
                    who = "Signed in";

                _accountHost.Add(MakeHelp("Signed in as " + who + ".", HelpBoxMessageType.Info));
                if (!string.IsNullOrWhiteSpace(_wizardGroupsLabel))
                    _accountHost.Add(MakeHint("Roles: " + _wizardGroupsLabel));

                var row = new VisualElement();
                row.AddToClassList("ivx-row");
                var signOut = MakeButton("Sign out", SignOutWizard, "ivx-btn");
                signOut.SetEnabled(!_wizardBusy);
                row.Add(signOut);
                var refresh = MakeButton("Refresh token", () => _ = RefreshWizardTokenAsync(force: true), "ivx-btn");
                refresh.SetEnabled(!_wizardBusy && CanRefreshWizardToken);
                row.Add(refresh);
                var grow = new VisualElement();
                grow.AddToClassList("ivx-grow");
                row.Add(grow);
                if (_wizardTokenExpiryEpoch > 0)
                {
                    var left = TimeSpan.FromSeconds(Math.Max(0, _wizardTokenExpiryEpoch - DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    row.Add(MakeHint("Token ~" + FormatRemaining(left)));
                }
                _accountHost.Add(row);
                return;
            }

            _accountHost.Add(MakeHint("Same login your game uses at runtime. Password is never saved to disk."));

            _suppressFieldCallbacks = true;
            if (_loginEmailField == null)
            {
                _loginEmailField = new TextField("Email");
                _loginEmailField.RegisterValueChangedCallback(evt =>
                {
                    if (_suppressFieldCallbacks) return;
                    _loginEmail = evt.newValue ?? "";
                });
            }
            if (_loginPasswordField == null)
            {
                _loginPasswordField = new TextField("Password") { isPasswordField = true };
                _loginPasswordField.RegisterValueChangedCallback(evt =>
                {
                    if (_suppressFieldCallbacks) return;
                    _loginPassword = evt.newValue ?? "";
                });
            }

            _loginEmailField.SetValueWithoutNotify(_loginEmail ?? "");
            _loginPasswordField.SetValueWithoutNotify(_loginPassword ?? "");
            _suppressFieldCallbacks = false;

            _loginEmailField.SetEnabled(!_wizardBusy);
            _loginPasswordField.SetEnabled(!_wizardBusy);
            _accountHost.Add(_loginEmailField);
            _accountHost.Add(_loginPasswordField);

            var signIn = MakeButton(
                _wizardBusy && (_wizardBusyLabel ?? "").StartsWith("Sign", StringComparison.Ordinal)
                    ? BusyLabelWithPulse("Signing in")
                    : "Sign in",
                () => _ = SignInWizardAsync(),
                "ivx-btn", "ivx-btn--primary");
            signIn.SetEnabled(!_wizardBusy && IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out _));
            _accountHost.Add(signIn);

            var links = new VisualElement();
            links.AddToClassList("ivx-row");
            links.Add(MakeButton("Create account", OpenSignupOverlay, "ivx-btn", "ivx-btn--ghost"));
            links.Add(MakeButton("Forgot password", OpenForgotOverlay, "ivx-btn", "ivx-btn--ghost"));
            _accountHost.Add(links);

            if (!IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out string blockReason) && !_wizardBusy)
                _accountHost.Add(MakeHint(blockReason));
        }

        private void RefreshCreateEnabled()
        {
            if (_createHost == null)
                return;

            _suppressFieldCallbacks = true;
            _createGameNameField?.SetValueWithoutNotify(_createGameName ?? "");
            _suppressFieldCallbacks = false;

            var btn = _createHost.Q<Button>("create-appid-btn");
            bool canCreate = HasUsableWizardSession && !_wizardBusy;
            btn?.SetEnabled(canCreate && IVXConnectWizardValidation.TryValidateGameName(_createGameName, out _));
            if (btn != null)
            {
                btn.text = _wizardBusy && (_wizardBusyLabel ?? "").IndexOf("unique", StringComparison.OrdinalIgnoreCase) >= 0
                    ? BusyLabelWithPulse("Creating unique App ID")
                    : "Create unique App ID";
            }

            // hints
            var oldHints = _createHost.Query<Label>(className: "ivx-create-dyn").ToList();
            foreach (var h in oldHints)
                h.RemoveFromHierarchy();

            Label hint = null;
            if (!HasUsableWizardSession)
                hint = MakeHint("Sign in above first (Create new path).");
            else if (!IVXConnectWizardValidation.TryValidateGameName(_createGameName, out string nameError))
                hint = MakeHint(nameError);
            else if (HasValidBootstrapGameId)
                hint = MakeHint("A Game ID is already set — create will ask before replacing it.");

            if (hint != null)
            {
                hint.AddToClassList("ivx-create-dyn");
                _createHost.Add(hint);
            }

            int len = (_createGameName ?? string.Empty).Trim().Length;
            var lenLabel = MakeHint(len + " / " + IVXConnectWizardValidation.MaxGameNameLength);
            lenLabel.AddToClassList("ivx-create-dyn");
            _createHost.Add(lenLabel);
        }

        private void RefreshExistingFields()
        {
            if (_config == null || _existingGameIdField == null)
                return;

            if (_configSo == null)
                _configSo = new SerializedObject(_config);
            _configSo.Update();

            _suppressFieldCallbacks = true;
            _existingGameIdField.SetValueWithoutNotify(_configSo.FindProperty("_gameId")?.stringValue ?? "");
            _existingGameNameField.SetValueWithoutNotify(_configSo.FindProperty("_gameName")?.stringValue ?? "");

            var recent = IVXConnectWizardValidation.LoadRecentGames();
            var labels = new List<string>(IVXConnectWizardValidation.RecentGamePopupLabels(recent));
            _recentGamesDropdown.choices = labels;
            _recentGamesDropdown.SetValueWithoutNotify(labels.Count > 0 ? labels[0] : "");
            _suppressFieldCallbacks = false;
        }

        private void RefreshSuccessUi()
        {
            if (_successHost == null)
                return;

            bool show = !string.IsNullOrWhiteSpace(_lastCreatedAppId) && HasValidBootstrapGameId;
            _successHost.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
                return;

            var idLabel = _successHost.Q<Label>("last-created-id");
            if (idLabel != null)
                idLabel.text = _lastCreatedAppId;

            _suppressFieldCallbacks = true;
            _renameGameNameField?.SetValueWithoutNotify(_renameGameName ?? "");
            _suppressFieldCallbacks = false;
        }

        private void RefreshServerFields()
        {
            if (_config == null || _serverHostField == null)
                return;
            if (_configSo == null)
                _configSo = new SerializedObject(_config);
            _configSo.Update();

            _suppressFieldCallbacks = true;
            _serverHostField.SetValueWithoutNotify(_configSo.FindProperty("_serverHost")?.stringValue ?? "");
            _serverPortField.SetValueWithoutNotify(_configSo.FindProperty("_serverPort")?.intValue ?? 7350);
            _serverKeyField.SetValueWithoutNotify(_configSo.FindProperty("_serverKey")?.stringValue ?? "");
            _serverSslToggle.SetValueWithoutNotify(_configSo.FindProperty("_useSSL")?.boolValue ?? false);
            _suppressFieldCallbacks = false;
            RefreshServerPingUi();
        }

        private void RefreshServerPingUi()
        {
            if (_serverPingLabel == null)
                return;
            _serverPingLabel.text = _serverPing ?? "";
            _serverPingLabel.style.display = string.IsNullOrEmpty(_serverPing) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void RefreshWizardStatusUi()
        {
            if (_wizardStatusBox == null)
                return;

            bool show = !string.IsNullOrEmpty(_wizardStatus);
            _wizardStatusBox.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
                return;

            _wizardStatusLabel.text = _wizardStatus;
            _wizardStatusLabel.RemoveFromClassList("ivx-help--warn");
            _wizardStatusLabel.RemoveFromClassList("ivx-help--err");
            _wizardStatusLabel.RemoveFromClassList("ivx-help--ok");
            switch (_wizardStatusType)
            {
                case MessageType.Warning:
                    _wizardStatusLabel.AddToClassList("ivx-help--warn");
                    break;
                case MessageType.Error:
                    _wizardStatusLabel.AddToClassList("ivx-help--err");
                    break;
                case MessageType.Info:
                    _wizardStatusLabel.AddToClassList("ivx-help--ok");
                    break;
            }
        }

        private void RefreshBusyUi()
        {
            if (_busyBar == null)
                return;
            _busyBar.style.display = _wizardBusy ? DisplayStyle.Flex : DisplayStyle.None;
            if (_busyLabel != null)
                _busyLabel.text = BusyLabelWithPulse(_wizardBusyLabel ?? "Working");
        }

        private void RefreshPlayUi()
        {
            if (_playRows == null)
                return;

            if (_playWarn != null)
            {
                if (_config == null || string.IsNullOrWhiteSpace(_config.GameId))
                {
                    _playWarn.text = "Finish Connect first — the SDK needs a Game ID before Play is useful.";
                    _playWarn.style.display = DisplayStyle.Flex;
                }
                else if (!IVXConnectWizardValidation.IsValidUuid(_config.GameId))
                {
                    _playWarn.text = "Game ID does not look like a UUID. Fix it in Connect → Use existing.";
                    _playWarn.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _playWarn.style.display = DisplayStyle.None;
                }
            }

            _playRows.Clear();
            bool hasBootstrap = FindBootstrapInOpenScenes() != null;
            _playRows.Add(MakeStatusRow("Bootstrap in this scene", hasBootstrap));
        }

        private void RefreshTrafficUi()
        {
            if (_trafficList == null)
                return;

            _trafficMeta.text = "Live IVXRequestBus activity (last " + IVXRequestBus.MaxTrafficHistory +
                                "). In-flight: " + IVXRequestBus.GetInFlightCount() + ".";

            _trafficList.Clear();
            var rows = IVXRequestBus.GetRecentTraffic();
            if (rows.Length == 0)
            {
                _trafficList.Add(MakeHelp(
                    "No RPCs recorded yet. Press Play or call managers that use IVXRequestBus.",
                    HelpBoxMessageType.Info));
                return;
            }

            for (int i = 0; i < rows.Length; i++)
            {
                var e = rows[i];
                var row = new VisualElement();
                row.AddToClassList("ivx-traffic-row");
                row.Add(ColLabel(e.Utc.ToString("HH:mm:ss"), "ivx-col-utc"));
                row.Add(ColLabel(e.RpcId ?? "", "ivx-col-rpc"));
                row.Add(ColLabel(e.Attempt.ToString(), "ivx-col-n"));
                row.Add(ColLabel(e.Status ?? "", "ivx-col-status"));
                row.Add(ColLabel(e.LatencyMs.ToString(), "ivx-col-ms"));
                string detail = e.Error ?? "";
                if (e.RetryAfterMs.HasValue)
                    detail = (string.IsNullOrEmpty(detail) ? "" : detail + " · ") + "retry_after_ms=" + e.RetryAfterMs.Value;
                row.Add(ColLabel(detail, "ivx-col-err"));
                _trafficList.Add(row);
            }
        }

        private void RefreshApisUi()
        {
            if (_apiList == null)
                return;

            var entries = IVXRpcIndex.GetEntries();
            string source = IVXRpcIndex.LoadedFrom;
            _apiSourceLabel.text = entries.Length > 0
                ? "Generated RPC index (" + entries.Length + " ids). Filter by name or module."
                : "RPC index missing — run tools/generate-rpc-index.ps1 from the repo root.";
            if (!string.IsNullOrEmpty(source))
                _apiSourceLabel.text += "\nSource: " + source;

            _apiList.Clear();
            string filter = (_apiFilter ?? "").Trim().ToLowerInvariant();
            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e == null || string.IsNullOrEmpty(e.id))
                    continue;
                if (!string.IsNullOrEmpty(filter)
                    && e.id.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0
                    && (e.module == null || e.module.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0))
                    continue;

                var row = new VisualElement();
                row.AddToClassList("ivx-api-row");
                var id = new Label(e.id);
                id.style.width = 260;
                id.style.overflow = Overflow.Hidden;
                row.Add(id);
                var mod = new Label(e.module ?? "");
                mod.style.width = 90;
                row.Add(mod);
                var auth = new Label(e.authRequired ? "auth" : "open");
                auth.style.width = 40;
                row.Add(auth);
                string copyId = e.id;
                row.Add(MakeButton("Copy", () => EditorGUIUtility.systemCopyBuffer = copyId, "ivx-btn"));
                _apiList.Add(row);
            }
        }

        private void RefreshSignupOverlayUi()
        {
            if (_signupCard == null)
                return;

            bool open = _authOverlay == AuthOverlay.Signup;
            if (open)
                _overlayHost?.AddToClassList("ivx-overlay-host--open");

            if (_signupStepInitiate != null)
                _signupStepInitiate.style.display = _signupAwaitingOtp ? DisplayStyle.None : DisplayStyle.Flex;
            if (_signupStepConfirm != null)
                _signupStepConfirm.style.display = _signupAwaitingOtp ? DisplayStyle.Flex : DisplayStyle.None;

            if (_signupStatusLabel != null)
                _signupStatusLabel.text = _signupStatus ?? "";
        }

        private void RefreshForgotOverlayUi()
        {
            if (_forgotCard == null)
                return;

            if (_forgotStepRequest != null)
                _forgotStepRequest.style.display = _forgotAwaitingOtp ? DisplayStyle.None : DisplayStyle.Flex;
            if (_forgotStepReset != null)
                _forgotStepReset.style.display = _forgotAwaitingOtp ? DisplayStyle.Flex : DisplayStyle.None;

            if (_forgotStatusLabel != null)
                _forgotStatusLabel.text = _forgotStatus ?? "";
        }

        private void SyncSignupFieldsFromUi()
        {
            _signupEmail = _signupEmailField?.value ?? _signupEmail;
            _signupPassword = _signupPasswordField?.value ?? _signupPassword;
            _signupUserName = _signupUserNameField?.value ?? _signupUserName;
            _signupFirstName = _signupFirstNameField?.value ?? _signupFirstName;
            _signupLastName = _signupLastNameField?.value ?? _signupLastName;
            _signupOtp = _signupOtpField?.value ?? _signupOtp;
        }

        private void SyncForgotFieldsFromUi()
        {
            _forgotEmail = _forgotEmailField?.value ?? _forgotEmail;
            _forgotOtp = _forgotOtpField?.value ?? _forgotOtp;
            _forgotNewPassword = _forgotNewPasswordField?.value ?? _forgotNewPassword;
        }

        private void SetConnectMode(ConnectSourceMode mode)
        {
            _connectMode = mode;
            EditorPrefs.SetInt(PrefConnectMode, (int)mode);
            NotifyUiChanged();
        }

        private void ApplyRecentFromDropdown(string label)
        {
            var recent = IVXConnectWizardValidation.LoadRecentGames();
            string[] labels = IVXConnectWizardValidation.RecentGamePopupLabels(recent);
            int next = Array.IndexOf(labels, label);
            if (recent.Length == 0 || next <= 0 || next > recent.Length)
                return;

            var entry = recent[next - 1];
            string smoke;
            bool ok = ApplyGameIdToConfig(entry.id, entry.name, remember: true, out smoke);
            SetWizardStatus(
                (ok ? "Applied recent Game ID.\n" : "Applied with issues.\n") + entry.id + "\n" + smoke,
                ok ? MessageType.Info : MessageType.Warning);
        }

        private void WriteConfigString(string prop, string value)
        {
            if (_config == null)
                return;
            if (_configSo == null)
                _configSo = new SerializedObject(_config);
            _configSo.Update();
            var p = _configSo.FindProperty(prop);
            if (p == null)
                return;
            p.stringValue = value ?? "";
            _configSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_config);
            if (prop == "_gameId" && IVXConnectWizardValidation.IsValidUuid(value))
            {
                ApplyGameIdToRuntime(value);
                IVXConnectWizardValidation.RememberRecentGame(value, _config.GameName);
            }
        }

        private void WriteConfigInt(string prop, int value)
        {
            if (_config == null)
                return;
            if (_configSo == null)
                _configSo = new SerializedObject(_config);
            _configSo.Update();
            var p = _configSo.FindProperty(prop);
            if (p == null)
                return;
            p.intValue = value;
            _configSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_config);
        }

        private void WriteConfigBool(string prop, bool value)
        {
            if (_config == null)
                return;
            if (_configSo == null)
                _configSo = new SerializedObject(_config);
            _configSo.Update();
            var p = _configSo.FindProperty(prop);
            if (p == null)
                return;
            p.boolValue = value;
            _configSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_config);
        }

        private void OnFixProjectSettings()
        {
            var validation = IVXProjectSetup.RunValidation();
            int failed = 0;
            for (int i = 0; i < validation.Count; i++)
            {
                if (!validation[i].Passed && !validation[i].IsWarning)
                    failed++;
            }

            IVXProjectSetup.QuickValidate();
            EditorUtility.DisplayDialog(
                WindowTitle,
                failed == 0
                    ? "Project checks passed."
                    : "Some checks failed. Open Advanced Setup > Platform Validation to apply fixes.",
                "OK");
            NotifyUiChanged();
        }

        private static Button MakeTabButton(string text, Action onClick)
        {
            var btn = new Button(onClick) { text = text };
            btn.AddToClassList("ivx-tab");
            return btn;
        }

        private static Button MakeButton(string text, Action onClick, params string[] classes)
        {
            var btn = new Button(onClick) { text = text };
            if (classes != null)
            {
                for (int i = 0; i < classes.Length; i++)
                    btn.AddToClassList(classes[i]);
            }
            return btn;
        }

        private static VisualElement MakeSection(string title)
        {
            var section = new VisualElement();
            section.AddToClassList("ivx-section");
            var label = new Label(title);
            label.AddToClassList("ivx-section-title");
            section.Add(label);
            return section;
        }

        private static Label MakeHint(string text)
        {
            var label = new Label(text);
            label.AddToClassList("ivx-hint");
            return label;
        }

        private static Label MakeChip(string text)
        {
            var label = new Label("○ " + text);
            label.AddToClassList("ivx-chip");
            return label;
        }

        private static void SetChip(Label chip, bool done)
        {
            if (chip == null)
                return;
            string baseText = chip.text;
            int space = baseText.IndexOf(' ');
            string name = space >= 0 ? baseText.Substring(space + 1) : baseText;
            chip.text = (done ? "● " : "○ ") + name;
            chip.EnableInClassList("ivx-chip--done", done);
        }

        private static VisualElement MakeHelp(string text, HelpBoxMessageType type)
        {
            var label = new Label(text);
            label.AddToClassList("ivx-help");
            if (type == HelpBoxMessageType.Warning)
                label.AddToClassList("ivx-help--warn");
            else if (type == HelpBoxMessageType.Error)
                label.AddToClassList("ivx-help--err");
            return label;
        }

        private static VisualElement MakeStatusRow(string label, bool ok, bool optional = false)
        {
            var row = new VisualElement();
            row.AddToClassList("ivx-row");
            var status = new Label(ok ? "Ready" : (optional ? "Optional" : "Needs work"));
            status.AddToClassList(ok || optional ? "ivx-status-ok" : "ivx-status-bad");
            if (!ok && optional)
                status.style.color = new StyleColor(new Color(0.6f, 0.62f, 0.68f));
            var name = new Label(label);
            name.style.flexGrow = 1;
            row.Add(status);
            row.Add(name);
            return row;
        }

        private static Label ColLabel(string text, string className)
        {
            var label = new Label(text ?? "");
            label.AddToClassList(className);
            return label;
        }
    }
}
