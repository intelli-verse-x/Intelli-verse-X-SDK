using System;
using System.Threading.Tasks;
using IntelliVerseX.Social;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Clan Demo")]
    public sealed class IVXUITKClanDemo : IVXUITKFeatureDemoBase
    {
        private TextField _clanNameField;
        private TextField _clanIdField;
        private IVXClanManager _manager;

        protected override void BuildFeatureUI()
        {
            SetTitle("Clan", "Browse, create, join via IVXClanManager");
            _clanNameField = AddField("clanName", "Clan name (create)");
            _clanIdField = AddField("clanId", "Clan id (join)");
            AddAction("Init / Ensure", () => _ = EnsureAsync());
            AddAction("Refresh Current", () => _ = RefreshCurrentAsync());
            AddAction("Browse", () => _ = BrowseAsync());
            AddAction("Create Clan", () => _ = CreateAsync());
            AddAction("Join Clan", () => _ = JoinAsync());
            AddAction("Leave Clan", () => _ = LeaveAsync(), "ivx-btn--danger");
            EnsureManager();
            SetStatus("Clan demo ready.");
        }

        private void EnsureManager()
        {
            _manager = FindFirstObjectByType<IVXClanManager>();
            if (_manager == null)
            {
                var go = new GameObject("[IVXClanManager]");
                _manager = go.AddComponent<IVXClanManager>();
                DontDestroyOnLoad(go);
            }
        }

        private async Task EnsureAsync()
        {
            EnsureManager();
            SetBusy(true);
            try
            {
                bool ok = await _manager.EnsureInitializedAsync();
                SetStatus(ok ? "Clan manager ready." : "Clan manager init failed.", !ok);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task RefreshCurrentAsync()
        {
            EnsureManager();
            SetBusy(true);
            try
            {
                var result = await _manager.LoadCurrentClanAsync();
                var clan = _manager.CurrentClan;
                if (clan != null && !string.IsNullOrWhiteSpace(clan.ClanId))
                {
                    SetResult($"Clan: {clan.Name}\nId: {clan.ClanId}\nMembers: {clan.MemberCount}");
                    SetStatus("Current clan refreshed.");
                }
                else
                {
                    SetResult(result != null && !result.IsSuccess
                        ? (result.ErrorMessage ?? "Not in a clan.")
                        : "Not in a clan.");
                    SetStatus("No current clan.", result != null && !result.IsSuccess);
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task BrowseAsync()
        {
            EnsureManager();
            SetBusy(true);
            try
            {
                var result = await _manager.BrowseClansAsync(string.Empty);
                int count = result?.Clans?.Count ?? _manager.LastBrowseResults?.Count ?? 0;
                SetResult($"Browse results: {count}");
                SetStatus("Browse complete.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task CreateAsync()
        {
            EnsureManager();
            string name = _clanNameField?.value?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                SetStatus("Enter a clan name.", true);
                return;
            }

            SetBusy(true);
            try
            {
                var result = await _manager.CreateClanAsync(name, string.Empty, true);
                SetStatus(result != null && result.IsSuccess ? "Clan created." : result?.ErrorMessage ?? "Create failed.",
                    result == null || !result.IsSuccess);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task JoinAsync()
        {
            EnsureManager();
            string id = _clanIdField?.value?.Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                SetStatus("Enter a clan id.", true);
                return;
            }

            SetBusy(true);
            try
            {
                var result = await _manager.JoinClanAsync(id);
                SetStatus(result != null && result.IsSuccess ? "Joined clan." : result?.ErrorMessage ?? "Join failed.",
                    result == null || !result.IsSuccess);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task LeaveAsync()
        {
            EnsureManager();
            SetBusy(true);
            try
            {
                var result = await _manager.LeaveClanAsync();
                SetStatus(result != null && result.IsSuccess ? "Left clan." : result?.ErrorMessage ?? "Leave failed.",
                    result == null || !result.IsSuccess);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }
    }
}
