using System;
using System.Threading.Tasks;
using IntelliVerseX.Backend.Nakama;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Profile Demo")]
    public sealed class IVXUITKProfileDemo : IVXUITKFeatureDemoBase
    {
        private TextField _usernameField;

        protected override void BuildFeatureUI()
        {
            SetTitle("Profile", "Fetch / update via IVXNProfileManager");
            _usernameField = AddField("username", "New username");
            AddAction("Fetch Profile", () => _ = FetchAsync());
            AddAction("Change Username", () => _ = ChangeUsernameAsync());
            AddAction("Fetch Portfolio", () => _ = PortfolioAsync());
            SetStatus("Profile demo ready.");
        }

        private async Task FetchAsync()
        {
            SetBusy(true);
            try
            {
                var result = await IVXNProfileManager.FetchProfileAsync();
                var snap = IVXNProfileManager.Snapshot;
                SetResult(
                    $"User: {snap.FullName}\n" +
                    $"Username: {snap.Username}\n" +
                    $"Email: {snap.Email}\n" +
                    $"Locale: {snap.Locale}\n" +
                    $"Id: {snap.UserId}");
                SetStatus(result != null ? "Profile fetched." : "Fetch returned null.", result == null);
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

        private async Task ChangeUsernameAsync()
        {
            string username = _usernameField?.value?.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                SetStatus("Enter a username.", true);
                return;
            }

            SetBusy(true);
            try
            {
                var result = await IVXNProfileManager.ChangeUsernameAsync(username);
                SetStatus(result != null ? $"Username update: {result}" : "Username change failed.", result == null);
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

        private async Task PortfolioAsync()
        {
            SetBusy(true);
            try
            {
                var result = await IVXNProfileManager.FetchPortfolioAsync();
                SetResult(result != null ? result.ToString() : "No portfolio");
                SetStatus("Portfolio fetched.");
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
