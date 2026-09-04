using System;
using System.Collections.Generic;
using IntelliVerseX.Discord;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Self-contained programmatic UI demo for IntelliVerseX Discord Social SDK features.
    /// Add to any GameObject in play mode or editor; ensures a Canvas and builds a 7-tab interface.
    /// Requires IVXDiscord* manager components (and <see cref="IVXDiscordConfig"/> on the manager) in the scene for live behavior.
    /// </summary>
    public sealed class IVXDiscordSocialDemo : MonoBehaviour
    {
        #region Colors (#1A1A2E / #16213E / #0F3460 / #E94560)

        private static readonly Color COL_BG = new Color(0.102f, 0.102f, 0.180f);
        private static readonly Color COL_PANEL = new Color(0.086f, 0.129f, 0.243f);
        private static readonly Color COL_ACCENT = new Color(0.059f, 0.204f, 0.376f);
        private static readonly Color COL_HIGHLIGHT = new Color(0.914f, 0.271f, 0.376f);
        private static readonly Color COL_TAB_OFF = new Color(0.12f, 0.14f, 0.22f);
        private static readonly Color COL_INPUT = new Color(0.08f, 0.10f, 0.16f);
        private static readonly Color COL_TEXT = new Color(0.92f, 0.93f, 0.95f);
        private static readonly Color COL_DIM = new Color(0.50f, 0.53f, 0.60f);
        private static readonly Color COL_ONLINE = new Color(0.35f, 0.85f, 0.55f);
        private static readonly Color COL_INGAME = new Color(0.45f, 0.65f, 0.95f);

        #endregion

        #region Private Fields — Tabs & panels

        private readonly Image[] _tabImages = new Image[7];
        private readonly RectTransform[] _panels = new RectTransform[7];

        private TextMeshProUGUI _accountStatusLabel;
        private TextMeshProUGUI _friendsPendingLabel;
        private TextMeshProUGUI _inviteInfoLabel;
        private TextMeshProUGUI _moderationResultLabel;
        private TextMeshProUGUI _footerStatusLabel;

        private TMP_InputField _presenceDetails;
        private TMP_InputField _presenceState;
        private TMP_Dropdown _statusDisplayDropdown;

        private TMP_InputField _friendUsernameInput;
        private TMP_InputField _blockUserIdInput;
        private RectTransform _friendsListContent;

        private TMP_InputField _dmRecipientInput;
        private TMP_InputField _dmMessageInput;
        private RectTransform _dmScrollContent;
        private Toggle _dmShowingChatToggle;

        private TMP_InputField _lobbySecretInput;
        private TMP_InputField _lobbyChatInput;
        private RectTransform _lobbyScrollContent;

        private TMP_InputField _inviteTargetInput;
        private TMP_InputField _moderationTestInput;
        private TMP_InputField _vadThresholdInput;
        private TMP_InputField _mobileSchemeInput;

        private Toggle _voiceMuteToggle;
        private Toggle _voiceDeafenToggle;
        private Slider _voiceInputSlider;
        private Slider _voiceOutputSlider;

        private IVXGameInvite _lastInvite;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            EnsureCanvasAndRaycaster();
            BuildUI();
            SelectTab(0);
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        #region Event wiring

        private void SubscribeEvents()
        {
            var mgr = IVXDiscordManager.Instance;
            if (mgr != null)
            {
                mgr.OnAccountLinked += HandleAccountLinked;
                mgr.OnAccountUnlinked += HandleAccountUnlinked;
                mgr.OnError += HandleDiscordError;
                mgr.OnConnected += HandleConnected;
            }

            var friends = IVXDiscordFriends.Instance;
            if (friends != null)
                friends.OnFriendsUpdated += HandleFriendsUpdated;

            var invites = IVXDiscordInvites.Instance;
            if (invites != null)
                invites.OnInviteReceived += HandleInviteReceived;

            var lobby = IVXDiscordLobby.Instance;
            if (lobby != null)
                lobby.OnMessageReceived += HandleLobbyMessage;

            var mod = IVXDiscordModeration.Instance;
            if (mod != null)
                mod.OnModerationDecisionReceived += HandleModerationDecision;
        }

        private void UnsubscribeEvents()
        {
            var mgr = IVXDiscordManager.Instance;
            if (mgr != null)
            {
                mgr.OnAccountLinked -= HandleAccountLinked;
                mgr.OnAccountUnlinked -= HandleAccountUnlinked;
                mgr.OnError -= HandleDiscordError;
                mgr.OnConnected -= HandleConnected;
            }

            var friends = IVXDiscordFriends.Instance;
            if (friends != null)
                friends.OnFriendsUpdated -= HandleFriendsUpdated;

            var invites = IVXDiscordInvites.Instance;
            if (invites != null)
                invites.OnInviteReceived -= HandleInviteReceived;

            var lobby = IVXDiscordLobby.Instance;
            if (lobby != null)
                lobby.OnMessageReceived -= HandleLobbyMessage;

            var mod = IVXDiscordModeration.Instance;
            if (mod != null)
                mod.OnModerationDecisionReceived -= HandleModerationDecision;
        }

        private void HandleAccountLinked(string userId, string username)
        {
            RefreshAccountStatusLabel();
            SetFooterStatus($"Account linked: {username} ({userId})");
        }

        private void HandleAccountUnlinked()
        {
            RefreshAccountStatusLabel();
            SetFooterStatus("Account unlinked.");
        }

        private void HandleDiscordError(string err)
        {
            SetFooterStatus($"Discord error: {err}");
        }

        private void HandleConnected()
        {
            SetFooterStatus("Discord manager connected.");
        }

        private void HandleFriendsUpdated(IReadOnlyList<IVXUnifiedFriend> list)
        {
            RebuildFriendsList(list);
            UpdatePendingRequestLabel();
        }

        private void HandleInviteReceived(IVXGameInvite invite)
        {
            _lastInvite = invite;
            if (_inviteInfoLabel != null)
            {
                _inviteInfoLabel.text =
                    $"From: {invite.InviterName} ({invite.InviterUserId})\n" +
                    $"Join secret: {invite.JoinSecret}\n" +
                    $"Activity: {invite.ActivityDetails}\n" +
                    $"Party: {invite.PartyCurrentSize}/{invite.PartyMaxSize}";
            }

            SetFooterStatus("Invite received.");
        }

        private void HandleLobbyMessage(string sender, string message)
        {
            AppendLine(_lobbyScrollContent, $"{sender}: {message}", COL_TEXT);
        }

        private void HandleModerationDecision(IVXModerationDecision decision)
        {
            if (_moderationResultLabel == null)
                return;
            _moderationResultLabel.text =
                $"Action: {decision.Action}\n" +
                $"Severity: {decision.Severity}\n" +
                $"Reason: {decision.Reason}\n" +
                $"Replacement: {decision.Replacement}\n" +
                $"MsgId: {decision.MessageId}";
        }

        #endregion

        #region Canvas

        private void EnsureCanvasAndRaycaster()
        {
            if (GetComponent<Canvas>() == null)
            {
                var canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            if (GetComponent<EventSystem>() == null && FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.transform.SetParent(transform, false);
            }
        }

        #endregion

        #region UI root

        private void BuildUI()
        {
            var bg = MakePanel(transform, "Background", COL_BG);
            Stretch(bg);

            var root = MakeRect(transform, "DiscordSocialRoot");
            Stretch(root);
            var rootV = AddVLayout(root.gameObject, 8f, new RectOffset(16, 16, 12, 12));
            rootV.childForceExpandHeight = false;

            BuildHeader(root);
            BuildTabBar(root);

            var contentArea = MakeRect(root, "ContentArea");
            contentArea.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;

            for (int i = 0; i < 7; i++)
            {
                _panels[i] = MakePanel(contentArea, $"Panel_{i}", COL_PANEL);
                Stretch(_panels[i]);
                var pad = AddVLayout(_panels[i].gameObject, 8f, new RectOffset(14, 14, 12, 12));
                pad.childForceExpandHeight = false;
                BuildPanel(i, _panels[i]);
                _panels[i].gameObject.SetActive(false);
            }

            var footer = MakePanel(root, "Footer", COL_ACCENT);
            footer.gameObject.AddComponent<LayoutElement>().minHeight = 26f;
            var fv = AddVLayout(footer.gameObject, 0f, new RectOffset(10, 10, 4, 4));
            fv.childAlignment = TextAnchor.MiddleLeft;
            _footerStatusLabel = MakeTMP(footer, "FooterStatus", "Ready.", 12f, FontStyles.Italic, COL_DIM);

            RefreshAccountStatusLabel();
        }

        private void BuildHeader(RectTransform parent)
        {
            var header = MakeRect(parent, "Header");
            header.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            var h = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 12f;
            h.padding = new RectOffset(4, 4, 4, 4);
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = false;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;

            MakeTMP(header, "Title", "DISCORD SOCIAL SDK", 22f, FontStyles.Bold, COL_HIGHLIGHT);
            var sp = MakeRect(header, "Sp");
            sp.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            MakeTMP(header, "Hint", "IVX Discord managers + config required", 11f, FontStyles.Normal, COL_DIM,
                TextAlignmentOptions.MidlineRight);
        }

        private void BuildTabBar(RectTransform parent)
        {
            var bar = MakeRect(parent, "TabBar");
            bar.gameObject.AddComponent<LayoutElement>().minHeight = 42f;
            var hlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            string[] labels =
            {
                "Account", "Presence", "Friends", "DMs", "Lobby/Voice", "Invites", "Moderation"
            };

            for (int i = 0; i < 7; i++)
            {
                int idx = i;
                _tabImages[i] = CreateTabButton(bar, $"Tab_{i}", labels[i], () => SelectTab(idx));
            }
        }

        private void BuildPanel(int index, RectTransform panel)
        {
            switch (index)
            {
                case 0: BuildPanelAccount(panel); break;
                case 1: BuildPanelPresence(panel); break;
                case 2: BuildPanelFriends(panel); break;
                case 3: BuildPanelDM(panel); break;
                case 4: BuildPanelLobbyVoice(panel); break;
                case 5: BuildPanelInvites(panel); break;
                case 6: BuildPanelModeration(panel); break;
            }
        }

        #endregion

        #region Panel 1 — Account

        private void BuildPanelAccount(RectTransform panel)
        {
            MakeTMP(panel, "H", "Account linking & OAuth", 16f, FontStyles.Bold, COL_TEXT);

            var mgr = IVXDiscordManager.Instance;
            var row1 = MakeButtonRow(panel);
            AddButton(row1, "Initialize SDK", () =>
            {
                if (mgr == null) { SetFooterStatus("IVXDiscordManager missing."); return; }
                mgr.Initialize();
                RefreshAccountStatusLabel();
                SetFooterStatus("Initialize() called.");
            });
            AddButton(row1, "Link Discord Account", () =>
            {
                if (mgr == null) { SetFooterStatus("IVXDiscordManager missing."); return; }
                mgr.LinkAccount();
                SetFooterStatus("LinkAccount()");
            });
            AddButton(row1, "Unlink Account", () =>
            {
                if (mgr == null) { SetFooterStatus("IVXDiscordManager missing."); return; }
                mgr.UnlinkAccount();
                RefreshAccountStatusLabel();
            });

            var row2 = MakeButtonRow(panel);
            AddButton(row2, "Create Provisional", () =>
            {
                if (mgr == null) { SetFooterStatus("IVXDiscordManager missing."); return; }
                mgr.CreateProvisionalAccount(ok => SetFooterStatus($"Provisional: {(ok ? "ok" : "fail")}"));
            });
            AddButton(row2, "Register Entry Point", () =>
            {
                if (mgr == null) { SetFooterStatus("IVXDiscordManager missing."); return; }
                mgr.RegisterAuthorizeRequestCallback(() =>
                {
                    SetFooterStatus("Authorize request callback (from Discord).");
                });
                SetFooterStatus("RegisterAuthorizeRequestCallback registered.");
            });
            AddButton(row2, "Remove Entry Callback", () =>
            {
                if (mgr == null) { SetFooterStatus("IVXDiscordManager missing."); return; }
                mgr.RemoveAuthorizeRequestCallback();
                SetFooterStatus("Authorize callback removed.");
            });

            var row3 = MakeButtonRow(panel);
            _mobileSchemeInput = MakeInputField(panel, "MobileScheme", "Redirect scheme (e.g. mygame://)");
            _mobileSchemeInput.text = mgr?.Config?.MobileRedirectScheme ?? "mygame://";
            AddButton(row3, "Mobile OAuth2", () =>
            {
                if (mgr == null) { SetFooterStatus("IVXDiscordManager missing."); return; }
                string scheme = string.IsNullOrEmpty(_mobileSchemeInput.text) ? "mygame://" : _mobileSchemeInput.text;
                mgr.StartMobileOAuth2Flow(scheme, ok => SetFooterStatus($"Mobile OAuth2: {ok}"));
            });

            _accountStatusLabel = MakeTMP(panel, "AccountStatus", "", 13f, FontStyles.Normal, COL_DIM);
            _accountStatusLabel.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            RefreshAccountStatusLabel();
        }

        private void RefreshAccountStatusLabel()
        {
            if (_accountStatusLabel == null)
                return;
            var mgr = IVXDiscordManager.Instance;
            if (mgr == null)
            {
                _accountStatusLabel.text = "IVXDiscordManager: not in scene.";
                return;
            }

            _accountStatusLabel.text =
                $"Linked: {mgr.IsAccountLinked}\n" +
                $"User ID: {mgr.DiscordUserId ?? "(none)"}\n" +
                $"Username: {mgr.DiscordUsername ?? "(none)"}\n" +
                $"Initialized: {mgr.IsInitialized} | Connected: {mgr.IsConnected}";
        }

        #endregion

        #region Panel 2 — Presence

        private void BuildPanelPresence(RectTransform panel)
        {
            MakeTMP(panel, "H", "Rich Presence", 16f, FontStyles.Bold, COL_TEXT);

            _presenceDetails = MakeInputField(panel, "Details", "Details (e.g. Ranked Match)");
            _presenceState = MakeInputField(panel, "State", "State (e.g. In Queue)");
            _presenceDetails.text = "IntelliVerseX Demo";
            _presenceState.text = "Exploring SDK";

            var row1 = MakeButtonRow(panel);
            var pr = IVXDiscordPresence.Instance;
            AddButton(row1, "Set Activity", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.SetActivity(_presenceDetails.text, _presenceState.text);
                SetFooterStatus("SetActivity");
            });
            AddButton(row1, "Start Timer", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.StartTimer();
                SetFooterStatus("StartTimer");
            });
            AddButton(row1, "Stop Timer", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.StopTimer();
                SetFooterStatus("StopTimer");
            });

            var row2 = MakeButtonRow(panel);
            AddButton(row2, "Set Party", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.SetParty("demo_party_ivx", 2, 4, "join_secret_test_abc");
                SetFooterStatus("SetParty (test data)");
            });
            AddButton(row2, "Clear Party", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.ClearParty();
                SetFooterStatus("ClearParty");
            });
            AddButton(row2, "Clear Buttons", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.ClearButtons();
                SetFooterStatus("ClearButtons");
            });

            var row3 = MakeButtonRow(panel);
            AddButton(row3, "Set Field URLs", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.SetFieldUrls("https://discord.com", "https://support.discord.com");
                SetFooterStatus("SetFieldUrls");
            });
            AddButton(row3, "Set Asset URLs", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.SetAssetUrls("https://discord.com", "https://discord.com/channels/@me");
                SetFooterStatus("SetAssetUrls");
            });
            AddButton(row3, "Add Buttons", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.AddButton("Discord", "https://discord.com");
                pr.AddButton("Community", "https://discord.gg/YVPxPFftMQ");
                SetFooterStatus("AddButton x2");
            });

            var rowDrop = MakeRect(panel, "DropRow");
            rowDrop.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            var dh = rowDrop.gameObject.AddComponent<HorizontalLayoutGroup>();
            dh.spacing = 10f;
            dh.childAlignment = TextAnchor.MiddleLeft;
            dh.childControlWidth = false;
            dh.childControlHeight = true;
            MakeTMP(rowDrop, "Dl", "Status display type:", 14f, FontStyles.Normal, COL_TEXT);
            _statusDisplayDropdown = CreateTmpDropdown(rowDrop, "StatusDisplay");
            _statusDisplayDropdown.options.Clear();
            _statusDisplayDropdown.options.Add(new TMP_Dropdown.OptionData("Name"));
            _statusDisplayDropdown.options.Add(new TMP_Dropdown.OptionData("State"));
            _statusDisplayDropdown.options.Add(new TMP_Dropdown.OptionData("Details"));
            _statusDisplayDropdown.value = 0;
            _statusDisplayDropdown.onValueChanged.AddListener(i =>
            {
                if (pr == null) return;
                var t = (IVXStatusDisplayType)i;
                pr.SetStatusDisplayType(t);
                SetFooterStatus($"SetStatusDisplayType({t})");
            });

            var row4 = MakeButtonRow(panel);
            AddButton(row4, "RPC Only Mode", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                long appId = IVXDiscordManager.Instance?.Config?.ApplicationId ?? 0L;
                pr.InitializeRPCOnly(appId);
                SetFooterStatus($"InitializeRPCOnly({appId})");
            });
            AddButton(row4, "Clear Presence", () =>
            {
                if (pr == null) { SetFooterStatus("IVXDiscordPresence missing."); return; }
                pr.ClearPresence();
                SetFooterStatus("ClearPresence");
            });
        }

        #endregion

        #region Panel 3 — Friends

        private void BuildPanelFriends(RectTransform panel)
        {
            MakeTMP(panel, "H", "Friends & relationships", 16f, FontStyles.Bold, COL_TEXT);

            var row = MakeButtonRow(panel);
            var fr = IVXDiscordFriends.Instance;
            AddButton(row, "Refresh Friends", () =>
            {
                if (fr == null) { SetFooterStatus("IVXDiscordFriends missing."); return; }
                fr.Refresh();
                UpdatePendingRequestLabel();
                SetFooterStatus("Refresh()");
            });

            _friendsPendingLabel = MakeTMP(panel, "Pending", "Pending requests: —", 13f, FontStyles.Normal, COL_DIM);

            var scroll = BuildScrollView(panel, "FriendsScroll", out _friendsListContent);
            scroll.gameObject.GetComponent<LayoutElement>().flexibleHeight = 1f;
            scroll.gameObject.GetComponent<LayoutElement>().minHeight = 200f;

            _friendUsernameInput = MakeInputField(panel, "FriendUser", "Username (friend requests)");
            _blockUserIdInput = MakeInputField(panel, "BlockId", "Discord user ID (block / unblock)");

            var row2 = MakeButtonRow(panel);
            AddButton(row2, "Send Game Friend Request", () =>
            {
                if (fr == null) { SetFooterStatus("IVXDiscordFriends missing."); return; }
                fr.SendGameFriendRequest(_friendUsernameInput.text, ok => SetFooterStatus($"Game friend req: {ok}"));
            });
            AddButton(row2, "Send Discord Friend Request", () =>
            {
                if (fr == null) { SetFooterStatus("IVXDiscordFriends missing."); return; }
                fr.SendDiscordFriendRequest(_friendUsernameInput.text, ok => SetFooterStatus($"Discord friend req: {ok}"));
            });

            var row3 = MakeButtonRow(panel);
            AddButton(row3, "Block User", () =>
            {
                if (fr == null) { SetFooterStatus("IVXDiscordFriends missing."); return; }
                if (!ulong.TryParse(_blockUserIdInput.text, out var uid))
                {
                    SetFooterStatus("Invalid user ID.");
                    return;
                }

                fr.BlockUser(uid, ok => SetFooterStatus($"Block: {ok}"));
            });
            AddButton(row3, "Unblock User", () =>
            {
                if (fr == null) { SetFooterStatus("IVXDiscordFriends missing."); return; }
                if (!ulong.TryParse(_blockUserIdInput.text, out var uid))
                {
                    SetFooterStatus("Invalid user ID.");
                    return;
                }

                fr.UnblockUser(uid, ok => SetFooterStatus($"Unblock: {ok}"));
            });

            if (fr != null)
                RebuildFriendsList(fr.Friends);
        }

        private void RebuildFriendsList(IReadOnlyList<IVXUnifiedFriend> friends)
        {
            if (_friendsListContent == null)
                return;

            for (int i = _friendsListContent.childCount - 1; i >= 0; i--)
                Destroy(_friendsListContent.GetChild(i).gameObject);

            if (friends == null || friends.Count == 0)
            {
                var empty = MakeTMP(_friendsListContent, "Empty", "No friends loaded. Tap Refresh.", 13f, FontStyles.Italic,
                    COL_DIM);
                empty.gameObject.AddComponent<LayoutElement>().minHeight = 24f;
                return;
            }

            for (int i = 0; i < friends.Count; i++)
            {
                var f = friends[i];
                var row = MakePanel(_friendsListContent, $"F_{i}", COL_INPUT);
                row.gameObject.AddComponent<LayoutElement>().minHeight = 56f;
                var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
                h.spacing = 10f;
                h.padding = new RectOffset(10, 10, 6, 6);
                h.childAlignment = TextAnchor.MiddleLeft;
                h.childControlWidth = true;
                h.childControlHeight = true;
                h.childForceExpandWidth = true;

                var left = MakeRect(row, "L");
                left.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
                var v = AddVLayout(left.gameObject, 2f, new RectOffset(0, 0, 0, 0));
                v.childForceExpandHeight = false;
                MakeTMP(left, "N", f.DisplayName ?? "?", 15f, FontStyles.Bold, COL_TEXT);
                string meta = $"{f.Source} | DiscordRel: {f.DiscordRelationshipType} | GameRel: {f.GameRelationshipType}";
                if (!string.IsNullOrEmpty(f.ActivityText))
                    meta += $"\n{f.ActivityText}";
                MakeTMP(left, "M", meta, 11f, FontStyles.Normal, COL_DIM);

                var badges = MakeRect(row, "Badges");
                badges.gameObject.AddComponent<LayoutElement>().minWidth = 120f;
                var bv = AddVLayout(badges.gameObject, 4f, new RectOffset(0, 0, 0, 0));
                bv.childAlignment = TextAnchor.MiddleRight;
                string on = f.IsOnline ? "● Online" : "○ Offline";
                MakeTMP(badges, "On", on, 12f, FontStyles.Bold, f.IsOnline ? COL_ONLINE : COL_DIM,
                    TextAlignmentOptions.MidlineRight);
                string ig = f.IsInGame ? "In game" : "Not in game";
                MakeTMP(badges, "Ig", ig, 11f, FontStyles.Normal, f.IsInGame ? COL_INGAME : COL_DIM,
                    TextAlignmentOptions.MidlineRight);
            }
        }

        private void UpdatePendingRequestLabel()
        {
            if (_friendsPendingLabel == null)
                return;
            var fr = IVXDiscordFriends.Instance;
            if (fr == null)
            {
                _friendsPendingLabel.text = "IVXDiscordFriends: missing.";
                return;
            }

            int n = fr.GetPendingRequests().Count;
            _friendsPendingLabel.text = $"Pending requests: {n}";
        }

        #endregion

        #region Panel 4 — DMs

        private void BuildPanelDM(RectTransform panel)
        {
            MakeTMP(panel, "H", "Direct messages", 16f, FontStyles.Bold, COL_TEXT);

            _dmRecipientInput = MakeInputField(panel, "Recv", "Recipient Discord user ID (ulong)");
            _dmRecipientInput.text = "123456789012345678";
            _dmMessageInput = MakeInputField(panel, "Body", "Message text");

            var row = MakeButtonRow(panel);
            var msg = IVXDiscordMessages.Instance;
            AddButton(row, "Send DM", () =>
            {
                if (msg == null) { SetFooterStatus("IVXDiscordMessages missing."); return; }
                if (!ulong.TryParse(_dmRecipientInput.text, out var rid))
                {
                    SetFooterStatus("Invalid recipient ID.");
                    return;
                }

                msg.SendDM(rid, _dmMessageInput.text,
                    id => AppendLine(_dmScrollContent, $"Sent ok (id {id})", COL_TEXT),
                    err => SetFooterStatus(err));
            });
            AddButton(row, "Get DM History", () =>
            {
                if (msg == null) { SetFooterStatus("IVXDiscordMessages missing."); return; }
                if (!ulong.TryParse(_dmRecipientInput.text, out var rid))
                {
                    SetFooterStatus("Invalid recipient ID.");
                    return;
                }

                msg.GetDMHistory(rid, 50, list =>
                {
                    ClearScroll(_dmScrollContent);
                    if (list == null || list.Count == 0)
                    {
                        AppendLine(_dmScrollContent, "(empty history)", COL_DIM);
                        return;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        var m = list[i];
                        AppendLine(_dmScrollContent,
                            $"[{m.AuthorName}] {m.Content} (id {m.MessageId})", COL_TEXT);
                    }
                });
                SetFooterStatus("GetDMHistory");
            });
            AddButton(row, "Get DM Summaries", () =>
            {
                if (msg == null) { SetFooterStatus("IVXDiscordMessages missing."); return; }
                msg.GetDMSummaries(list =>
                {
                    ClearScroll(_dmScrollContent);
                    if (list == null || list.Count == 0)
                    {
                        AppendLine(_dmScrollContent, "(no summaries)", COL_DIM);
                        return;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        var s = list[i];
                        AppendLine(_dmScrollContent,
                            $"{s.DisplayName} (user {s.UserId}) last {s.LastMessageId}", COL_TEXT);
                    }
                });
                SetFooterStatus("GetDMSummaries");
            });

            var toggleRow = MakeRect(panel, "ChatToggleRow");
            toggleRow.gameObject.AddComponent<LayoutElement>().minHeight = 32f;
            var th = toggleRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            th.spacing = 12f;
            th.childAlignment = TextAnchor.MiddleLeft;
            var tgo = new GameObject("ShowingChat");
            tgo.transform.SetParent(toggleRow, false);
            var tgoRt = tgo.AddComponent<RectTransform>();
            tgoRt.sizeDelta = new Vector2(28f, 28f);
            var dmBg = tgo.AddComponent<Image>();
            dmBg.color = COL_INPUT;
            _dmShowingChatToggle = tgo.AddComponent<Toggle>();
            _dmShowingChatToggle.targetGraphic = dmBg;
            var check = new GameObject("Check");
            check.transform.SetParent(tgo.transform, false);
            var checkRt = check.AddComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0, 0.5f);
            checkRt.anchorMax = new Vector2(0, 0.5f);
            checkRt.sizeDelta = new Vector2(22f, 22f);
            checkRt.anchoredPosition = new Vector2(14f, 0f);
            var checkImg = check.AddComponent<Image>();
            checkImg.color = COL_HIGHLIGHT;
            _dmShowingChatToggle.graphic = checkImg;
            _dmShowingChatToggle.isOn = false;
            _dmShowingChatToggle.onValueChanged.AddListener(v =>
            {
                if (msg == null) return;
                msg.SetShowingChat(v);
                SetFooterStatus($"SetShowingChat({v})");
            });
            var tl = MakeRect(toggleRow, "TLab");
            tl.gameObject.AddComponent<LayoutElement>().minWidth = 200f;
            MakeTMP(tl, "LT", "Showing Chat (suppress DM notifications)", 13f, FontStyles.Normal, COL_TEXT);

            var scroll = BuildScrollView(panel, "DMScroll", out _dmScrollContent);
            scroll.gameObject.GetComponent<LayoutElement>().flexibleHeight = 1f;
            scroll.gameObject.GetComponent<LayoutElement>().minHeight = 180f;
        }

        #endregion

        #region Panel 5 — Lobby & Voice

        private void BuildPanelLobbyVoice(RectTransform panel)
        {
            MakeTMP(panel, "H", "Lobbies, chat & voice", 16f, FontStyles.Bold, COL_TEXT);

            _lobbySecretInput = MakeInputField(panel, "Secret", "Lobby secret");
            _lobbySecretInput.text = "demo_lobby_secret";

            var lobby = IVXDiscordLobby.Instance;
            var voice = IVXDiscordVoice.Instance;

            var row1 = MakeButtonRow(panel);
            AddButton(row1, "Create/Join Lobby", () =>
            {
                if (lobby == null) { SetFooterStatus("IVXDiscordLobby missing."); return; }
                lobby.CreateOrJoinLobby(_lobbySecretInput.text, "{\"demo\":true}");
                SetFooterStatus("CreateOrJoinLobby");
            });
            AddButton(row1, "Leave Lobby", () =>
            {
                if (lobby == null) { SetFooterStatus("IVXDiscordLobby missing."); return; }
                lobby.LeaveLobby();
                ClearScroll(_lobbyScrollContent);
                SetFooterStatus("LeaveLobby");
            });
            AddButton(row1, "Join with Metadata", () =>
            {
                if (lobby == null) { SetFooterStatus("IVXDiscordLobby missing."); return; }
                lobby.CreateOrJoinLobbyWithMetadata(
                    _lobbySecretInput.text,
                    "{\"lobby_mode\":\"demo\"}",
                    "{\"display_name\":\"DemoUser\"}",
                    id => SetFooterStatus($"Joined lobby {id}"));
            });

            _lobbyChatInput = MakeInputField(panel, "LChat", "Lobby chat message");
            var rowChat = MakeButtonRow(panel);
            AddButton(rowChat, "Send Lobby Chat", () =>
            {
                if (lobby == null) { SetFooterStatus("IVXDiscordLobby missing."); return; }
                lobby.SendMessage(_lobbyChatInput.text);
                AppendLine(_lobbyScrollContent, $"[local] {_lobbyChatInput.text}", COL_DIM);
            });
            AddButton(rowChat, "Fetch Chat History", () =>
            {
                if (lobby == null) { SetFooterStatus("IVXDiscordLobby missing."); return; }
                lobby.FetchChatHistory(50, list =>
                {
                    ClearScroll(_lobbyScrollContent);
                    if (list == null || list.Count == 0)
                    {
                        AppendLine(_lobbyScrollContent, "(no history)", COL_DIM);
                        return;
                    }

                    for (int i = 0; i < list.Count; i++)
                        AppendLine(_lobbyScrollContent, list[i], COL_TEXT);
                });
            });

            var scroll = BuildScrollView(panel, "LobbyScroll", out _lobbyScrollContent);
            scroll.gameObject.GetComponent<LayoutElement>().flexibleHeight = 1f;
            scroll.gameObject.GetComponent<LayoutElement>().minHeight = 140f;

            MakeTMP(panel, "VH", "Voice", 14f, FontStyles.Bold, COL_HIGHLIGHT);

            var rowV = MakeButtonRow(panel);
            AddButton(rowV, "Join Voice", () =>
            {
                if (voice == null) { SetFooterStatus("IVXDiscordVoice missing."); return; }
                var lob = IVXDiscordLobby.Instance;
                if (lob == null || !lob.IsInLobby)
                {
                    SetFooterStatus("Join a lobby first.");
                    return;
                }

                voice.JoinCall(lob.CurrentLobbyId);
                SetFooterStatus("JoinCall");
            });
            AddButton(rowV, "Leave Voice", () =>
            {
                if (voice == null) { SetFooterStatus("IVXDiscordVoice missing."); return; }
                voice.LeaveCall();
                SetFooterStatus("LeaveCall");
            });
            AddButton(rowV, "End All Calls", () =>
            {
                if (voice == null) { SetFooterStatus("IVXDiscordVoice missing."); return; }
                voice.EndAllCalls(() => SetFooterStatus("EndAllCalls"));
            });

            var toggles = MakeRect(panel, "VoiceToggles");
            toggles.gameObject.AddComponent<LayoutElement>().minHeight = 36f;
            var th = toggles.gameObject.AddComponent<HorizontalLayoutGroup>();
            th.spacing = 24f;
            th.childAlignment = TextAnchor.MiddleLeft;
            _voiceMuteToggle = CreateToggle(toggles, "Mute", "Mute", (v, vo) => vo.SetSelfMute(v));
            _voiceDeafenToggle = CreateToggle(toggles, "Deafen", "Deafen", (v, vo) => vo.SetSelfDeafen(v));

            var sliders = MakeRect(panel, "Sliders");
            sliders.gameObject.AddComponent<LayoutElement>().minHeight = 80f;
            var sv = sliders.gameObject.AddComponent<VerticalLayoutGroup>();
            sv.spacing = 6f;
            sv.childAlignment = TextAnchor.UpperLeft;
            sv.childControlHeight = true;
            sv.childForceExpandHeight = false;

            _voiceInputSlider = CreateLabeledSlider(sliders, "Input volume (0–200)", 100f, v =>
            {
                if (voice != null) voice.SetInputVolume(v);
            });
            _voiceOutputSlider = CreateLabeledSlider(sliders, "Output volume (0–200)", 100f, v =>
            {
                if (voice != null) voice.SetOutputVolume(v);
            });

            _vadThresholdInput = MakeInputField(panel, "Vad", "VAD threshold dB (e.g. -35)");
            _vadThresholdInput.text = "-35";
            var rowVad = MakeButtonRow(panel);
            AddButton(rowVad, "Set VAD Threshold", () =>
            {
                if (voice == null) { SetFooterStatus("IVXDiscordVoice missing."); return; }
                float db = -30f;
                if (!string.IsNullOrEmpty(_vadThresholdInput.text))
                    float.TryParse(_vadThresholdInput.text, out db);
                voice.SetVADThreshold(true, db);
                SetFooterStatus($"SetVADThreshold(true, {db})");
            });
            AddButton(rowVad, "Reset VAD (default)", () =>
            {
                if (voice == null) { SetFooterStatus("IVXDiscordVoice missing."); return; }
                voice.SetVADThreshold(false);
                SetFooterStatus("SetVADThreshold(false)");
            });
        }

        private Toggle CreateToggle(RectTransform parent, string name, string label,
            Action<bool, IVXDiscordVoice> apply)
        {
            var row = MakeRect(parent, name);
            row.gameObject.AddComponent<LayoutElement>().minWidth = 140f;
            var tgo = new GameObject("T");
            tgo.transform.SetParent(row, false);
            var tgoRt = tgo.AddComponent<RectTransform>();
            tgoRt.sizeDelta = new Vector2(28f, 28f);
            var bg = tgo.AddComponent<Image>();
            bg.color = COL_INPUT;
            var t = tgo.AddComponent<Toggle>();
            t.targetGraphic = bg;
            var mark = new GameObject("Check", typeof(RectTransform), typeof(Image));
            mark.transform.SetParent(tgo.transform, false);
            var mrt = mark.GetComponent<RectTransform>();
            mrt.anchorMin = new Vector2(0, 0.5f);
            mrt.anchorMax = new Vector2(0, 0.5f);
            mrt.sizeDelta = new Vector2(20f, 20f);
            mrt.anchoredPosition = new Vector2(12f, 0f);
            var mi = mark.GetComponent<Image>();
            mi.color = COL_HIGHLIGHT;
            t.graphic = mi;
            var voice = IVXDiscordVoice.Instance;
            t.onValueChanged.AddListener(v =>
            {
                if (voice != null)
                    apply(v, voice);
            });
            var lab = MakeRect(row, "Lab");
            lab.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            MakeTMP(lab, "L", label, 13f, FontStyles.Normal, COL_TEXT);
            return t;
        }

        private Slider CreateLabeledSlider(RectTransform parent, string label, float initial, Action<float> onChanged)
        {
            var row = MakeRect(parent, "SlRow");
            row.gameObject.AddComponent<LayoutElement>().minHeight = 36f;
            var v = AddVLayout(row.gameObject, 4f, new RectOffset(0, 0, 0, 0));
            v.childForceExpandHeight = false;
            MakeTMP(row, "Cap", label, 12f, FontStyles.Normal, COL_DIM);
            var sgo = MakeRect(row, "Slider");
            sgo.gameObject.AddComponent<LayoutElement>().minHeight = 28f;
            var bg = sgo.gameObject.AddComponent<Image>();
            bg.color = COL_INPUT;

            var fillArea = MakeRect(sgo, "FillArea");
            Stretch(fillArea);
            fillArea.offsetMin = new Vector2(4f, 4f);
            fillArea.offsetMax = new Vector2(-4f, -4f);
            var fill = MakeRect(fillArea, "Fill");
            var fillRt = fill;
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.sizeDelta = new Vector2(0f, 0f);
            fillRt.anchoredPosition = Vector2.zero;
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color = COL_HIGHLIGHT;

            var handleArea = MakeRect(sgo, "HandleSlideArea");
            Stretch(handleArea);
            handleArea.offsetMin = new Vector2(4f, 4f);
            handleArea.offsetMax = new Vector2(-4f, -4f);
            var handle = MakeRect(handleArea, "Handle");
            var handleRt = handle;
            handleRt.sizeDelta = new Vector2(18f, 18f);
            handleRt.anchorMin = new Vector2(0f, 0.5f);
            handleRt.anchorMax = new Vector2(0f, 0.5f);
            handleRt.pivot = new Vector2(0.5f, 0.5f);
            var handleImg = handle.gameObject.AddComponent<Image>();
            handleImg.color = COL_TEXT;

            var sl = sgo.gameObject.AddComponent<Slider>();
            sl.fillRect = fillRt;
            sl.handleRect = handleRt;
            sl.targetGraphic = handleImg;
            sl.minValue = 0f;
            sl.maxValue = 200f;
            sl.wholeNumbers = true;
            sl.value = initial;
            sl.direction = Slider.Direction.LeftToRight;
            sl.onValueChanged.AddListener(v => onChanged(v));
            return sl;
        }

        #endregion

        #region Panel 6 — Invites

        private void BuildPanelInvites(RectTransform panel)
        {
            MakeTMP(panel, "H", "Game invites", 16f, FontStyles.Bold, COL_TEXT);

            _inviteTargetInput = MakeInputField(panel, "InvTarget", "Target Discord user ID");
            var inv = IVXDiscordInvites.Instance;

            var row = MakeButtonRow(panel);
            AddButton(row, "Send Invite", () =>
            {
                if (inv == null) { SetFooterStatus("IVXDiscordInvites missing."); return; }
                inv.SendInvite(_inviteTargetInput.text, "Join my IntelliVerseX session!");
                SetFooterStatus("SendInvite");
            });

            _inviteInfoLabel = MakeTMP(panel, "InvInfo", "Last received invite: (none yet)", 13f, FontStyles.Normal, COL_DIM);
            _inviteInfoLabel.gameObject.AddComponent<LayoutElement>().minHeight = 72f;

            var row2 = MakeButtonRow(panel);
            AddButton(row2, "Accept", () =>
            {
                if (inv == null) { SetFooterStatus("IVXDiscordInvites missing."); return; }
                if (_lastInvite == null)
                {
                    SetFooterStatus("No invite to accept.");
                    return;
                }

                inv.AcceptInvite(_lastInvite);
                SetFooterStatus("AcceptInvite");
            });
            AddButton(row2, "Decline", () =>
            {
                if (inv == null) { SetFooterStatus("IVXDiscordInvites missing."); return; }
                if (_lastInvite == null)
                {
                    SetFooterStatus("No invite to decline.");
                    return;
                }

                inv.DeclineInvite(_lastInvite);
                SetFooterStatus("DeclineInvite");
            });
        }

        #endregion

        #region Panel 7 — Moderation

        private void BuildPanelModeration(RectTransform panel)
        {
            MakeTMP(panel, "H", "Moderation & reporting", 16f, FontStyles.Bold, COL_TEXT);

            _moderationTestInput = MakeInputField(panel, "ModText", "Test text (metadata uses fixed sample)");
            _moderationTestInput.text = "Sample chat line for moderation demo.";

            var mod = IVXDiscordModeration.Instance;

            var row = MakeButtonRow(panel);
            AddButton(row, "Process Moderation", () =>
            {
                if (mod == null) { SetFooterStatus("IVXDiscordModeration missing."); return; }
                var meta = new Dictionary<string, string>
                {
                    { "action", "blur" },
                    { "reason", "test_policy" },
                    { "severity", "medium" },
                    { "replacement", "[filtered]" },
                    { "message_id", "424242424242424242" },
                    { "flagged", "true" }
                };
                mod.ProcessModerationMetadata(424242424242424242UL, meta);
                SetFooterStatus("ProcessModerationMetadata");
            });

            var row2 = MakeButtonRow(panel);
            AddButton(row2, "Start Voice Capture", () =>
            {
                if (mod == null) { SetFooterStatus("IVXDiscordModeration missing."); return; }
                ulong lid = IVXDiscordLobby.Instance != null ? IVXDiscordLobby.Instance.CurrentLobbyId : 0UL;
                mod.StartVoiceModerationCapture(lid);
                SetFooterStatus($"StartVoiceModerationCapture({lid})");
            });
            AddButton(row2, "Stop Voice Capture", () =>
            {
                if (mod == null) { SetFooterStatus("IVXDiscordModeration missing."); return; }
                mod.StopVoiceModerationCapture();
                SetFooterStatus("StopVoiceModerationCapture");
            });

            var row3 = MakeButtonRow(panel);
            AddButton(row3, "Report User (test ID)", () =>
            {
                if (mod == null) { SetFooterStatus("IVXDiscordModeration missing."); return; }
                mod.ReportUser(987654321098765432UL, "Demo report from IVXDiscordSocialDemo",
                    ok => SetFooterStatus($"ReportUser completed: {ok}"));
            });

            _moderationResultLabel = MakeTMP(panel, "ModResult", "Moderation decision: (run Process Moderation)", 12f,
                FontStyles.Normal, COL_DIM);
            _moderationResultLabel.gameObject.AddComponent<LayoutElement>().minHeight = 80f;
        }

        #endregion

        #region Tabs

        private void SelectTab(int index)
        {
            for (int i = 0; i < 7; i++)
            {
                _panels[i].gameObject.SetActive(i == index);
                _tabImages[i].color = i == index ? COL_HIGHLIGHT : COL_TAB_OFF;
                SetTabLabelColor(_tabImages[i], i == index);
            }
        }

        private static void SetTabLabelColor(Image tabImg, bool active)
        {
            var lbl = tabImg.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null)
                lbl.color = active ? COL_TEXT : COL_DIM;
        }

        private Image CreateTabButton(RectTransform parent, string goName, string label, Action onClick)
        {
            var go = MakeRect(parent, goName);
            var img = go.gameObject.AddComponent<Image>();
            img.color = COL_TAB_OFF;
            var btn = go.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick());
            var lbl = MakeTMP(go, "Lbl", label, 11f, FontStyles.Bold, COL_DIM, TextAlignmentOptions.Center);
            Stretch(lbl.rectTransform);
            return img;
        }

        #endregion

        #region Helpers — layout rows

        private RectTransform MakeButtonRow(RectTransform parent)
        {
            var row = MakeRect(parent, "BtnRow");
            row.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 8f;
            h.padding = new RectOffset(0, 0, 0, 0);
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = true;
            return row;
        }

        private void AddButton(RectTransform row, string label, Action onClick)
        {
            var btn = MakeButton(row, label.Replace(" ", ""), label, COL_ACCENT, COL_TEXT, -1f, 36f);
            btn.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
            btn.onClick.AddListener(() => onClick());
        }

        #endregion

        #region Helpers — scroll & text

        private RectTransform BuildScrollView(RectTransform parent, string name, out RectTransform content)
        {
            var scrollRt = MakeRect(parent, name);
            scrollRt.gameObject.AddComponent<LayoutElement>();
            scrollRt.gameObject.AddComponent<Image>().color = COL_BG;
            scrollRt.gameObject.AddComponent<RectMask2D>();
            var scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            var vp = MakeRect(scrollRt, "Viewport");
            Stretch(vp);
            var vpImg = vp.gameObject.AddComponent<Image>();
            vpImg.color = COL_BG;
            vp.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            content = MakeRect(vp, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            var vlg = AddVLayout(content.gameObject, 6f, new RectOffset(8, 8, 8, 8));
            vlg.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vp;
            scroll.content = content;
            return scrollRt;
        }

        private static void ClearScroll(RectTransform content)
        {
            if (content == null)
                return;
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);
        }

        private void AppendLine(RectTransform content, string line, Color col)
        {
            if (content == null)
                return;
            var tmp = MakeTMP(content, "Line", line, 12f, FontStyles.Normal, col);
            tmp.gameObject.AddComponent<LayoutElement>().minHeight = 18f;
            Canvas.ForceUpdateCanvases();
            if (content.transform.parent != null &&
                content.transform.parent.parent != null &&
                content.transform.parent.parent.TryGetComponent<ScrollRect>(out var sr))
                sr.verticalNormalizedPosition = 0f;
        }

        #endregion

        #region TMP Dropdown (minimal)

        private TMP_Dropdown CreateTmpDropdown(RectTransform parent, string name)
        {
            var root = MakeRect(parent, name);
            root.gameObject.AddComponent<LayoutElement>().minWidth = 220f;
            root.gameObject.AddComponent<LayoutElement>().minHeight = 36f;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = COL_INPUT;

            var dd = root.gameObject.AddComponent<TMP_Dropdown>();

            var labelRt = MakeRect(root, "Label");
            labelRt.offsetMin = new Vector2(10f, 4f);
            labelRt.offsetMax = new Vector2(-28f, -4f);
            var cap = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            cap.fontSize = 15f;
            cap.color = COL_TEXT;
            cap.alignment = TextAlignmentOptions.Left;
            dd.captionText = cap;

            var arrowRt = MakeRect(root, "Arrow");
            arrowRt.anchorMin = new Vector2(1f, 0.5f);
            arrowRt.anchorMax = new Vector2(1f, 0.5f);
            arrowRt.pivot = new Vector2(1f, 0.5f);
            arrowRt.sizeDelta = new Vector2(22f, 22f);
            arrowRt.anchoredPosition = new Vector2(-8f, 0f);
            var arrowImg = arrowRt.gameObject.AddComponent<Image>();
            arrowImg.color = COL_DIM;

            var template = MakeRect(root, "Template");
            template.gameObject.SetActive(false);
            template.anchorMin = new Vector2(0f, 0f);
            template.anchorMax = new Vector2(1f, 0f);
            template.pivot = new Vector2(0.5f, 1f);
            template.anchoredPosition = new Vector2(0f, 2f);
            template.sizeDelta = new Vector2(0f, 140f);
            var tplImg = template.gameObject.AddComponent<Image>();
            tplImg.color = COL_PANEL;

            var scroll = template.gameObject.AddComponent<ScrollRect>();
            var viewport = MakeRect(template, "Viewport");
            Stretch(viewport);
            viewport.gameObject.AddComponent<Image>().color = COL_PANEL;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var itemContent = MakeRect(viewport, "Content");
            itemContent.anchorMin = new Vector2(0f, 1f);
            itemContent.anchorMax = new Vector2(1f, 1f);
            itemContent.pivot = new Vector2(0.5f, 1f);
            var itemV = AddVLayout(itemContent.gameObject, 0f, new RectOffset(0, 0, 0, 0));
            itemV.childForceExpandHeight = false;
            itemContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = itemContent;
            scroll.horizontal = false;
            scroll.vertical = true;

            var item = MakeRect(itemContent, "Item");
            item.gameObject.AddComponent<LayoutElement>().minHeight = 32f;
            var itemBg = item.gameObject.AddComponent<Image>();
            itemBg.color = COL_INPUT;
            var toggle = item.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = itemBg;
            var itemLabelRt = MakeRect(item, "ItemLabel");
            itemLabelRt.offsetMin = new Vector2(28f, 2f);
            itemLabelRt.offsetMax = new Vector2(-8f, -2f);
            var itemTmp = itemLabelRt.gameObject.AddComponent<TextMeshProUGUI>();
            itemTmp.fontSize = 14f;
            itemTmp.color = COL_TEXT;
            itemTmp.alignment = TextAlignmentOptions.Left;
            dd.itemText = itemTmp;

            dd.template = template;
            dd.captionImage = null;

            return dd;
        }

        #endregion

        #region Primitives

        private void SetFooterStatus(string message)
        {
            if (_footerStatusLabel != null)
                _footerStatusLabel.text = message;
            Debug.Log($"[IVXDiscordSocialDemo] {message}");
        }

        private Button MakeButton(RectTransform parent, string goName, string label, Color bgCol, Color txtCol, float w,
            float h)
        {
            var go = MakeRect(parent, goName);
            var le = go.gameObject.AddComponent<LayoutElement>();
            if (w > 0f)
            {
                le.minWidth = w;
                le.preferredWidth = w;
            }

            if (h > 0f)
            {
                le.minHeight = h;
                le.preferredHeight = h;
            }

            var img = go.gameObject.AddComponent<Image>();
            img.color = bgCol;
            var btn = go.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var lbl = MakeTMP(go, "Lbl", label, 12f, FontStyles.Bold, txtCol, TextAlignmentOptions.Center);
            Stretch(lbl.rectTransform);
            return btn;
        }

        private TMP_InputField MakeInputField(RectTransform parent, string name, string placeholder)
        {
            var row = MakeRect(parent, $"Row_{name}");
            row.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            var inputGo = MakeRect(row, "Input");
            Stretch(inputGo);
            var inputBg = inputGo.gameObject.AddComponent<Image>();
            inputBg.color = COL_INPUT;
            var textArea = MakeRect(inputGo, "TextArea");
            Stretch(textArea, 10f);
            textArea.gameObject.AddComponent<RectMask2D>();
            var textRt = MakeRect(textArea, "Text");
            Stretch(textRt);
            var textTmp = textRt.gameObject.AddComponent<TextMeshProUGUI>();
            textTmp.fontSize = 15f;
            textTmp.color = COL_TEXT;
            var phRt = MakeRect(textArea, "Placeholder");
            Stretch(phRt);
            var phTmp = phRt.gameObject.AddComponent<TextMeshProUGUI>();
            phTmp.text = placeholder;
            phTmp.fontSize = 15f;
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.color = COL_DIM;
            var field = inputGo.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = textArea;
            field.textComponent = textTmp;
            field.placeholder = phTmp;
            field.targetGraphic = inputBg;
            return field;
        }

        private static RectTransform MakeRect(Transform p, string n)
        {
            var go = new GameObject(n, typeof(RectTransform));
            go.transform.SetParent(p, false);
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform MakePanel(Transform p, string n, Color c)
        {
            var r = MakeRect(p, n);
            r.gameObject.AddComponent<Image>().color = c;
            return r;
        }

        private static TextMeshProUGUI MakeTMP(RectTransform p, string n, string t, float s, FontStyles st, Color c,
            TextAlignmentOptions a = TextAlignmentOptions.MidlineLeft)
        {
            var r = MakeRect(p, n);
            var tmp = r.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = t;
            tmp.fontSize = s;
            tmp.fontStyle = st;
            tmp.color = c;
            tmp.alignment = a;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static VerticalLayoutGroup AddVLayout(GameObject go, float sp, RectOffset pad)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = sp;
            v.padding = pad;
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlWidth = true;
            v.childControlHeight = false;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            return v;
        }

        private static void Stretch(RectTransform r, float pad = 0f)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(pad, pad);
            r.offsetMax = new Vector2(-pad, -pad);
        }

        #endregion
    }
}
