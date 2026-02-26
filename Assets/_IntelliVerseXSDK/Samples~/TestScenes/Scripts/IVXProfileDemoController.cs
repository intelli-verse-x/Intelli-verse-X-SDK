using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.Backend.Nakama;
using UnityEngine;

namespace IntelliVerseX.Samples.TestScenes
{
    /// <summary>
    /// Controller for profile sample workflow:
    /// auth -> profile fetch -> edit/save -> profile refresh -> portfolio fetch.
    /// </summary>
    public sealed class IVXProfileDemoController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private IVXProfileDemoView view;

        [Header("Behavior")]
        [SerializeField] private bool initializeNakamaIfNeeded = true;
        [SerializeField] private bool autoFetchOnStart = true;
        [SerializeField] private bool useMockDataWhenUnavailable = true;
        [SerializeField] private bool enableVerboseLogs = true;

        private CancellationTokenSource _cts;
        private bool _busy;

        private void OnEnable()
        {
            if (view == null)
            {
                view = GetComponent<IVXProfileDemoView>();
            }

            if (view != null)
            {
                view.OnRefreshClicked += HandleRefreshClicked;
                view.OnSaveClicked += HandleSaveClicked;
                view.OnPortfolioClicked += HandlePortfolioClicked;
            }

            IVXNProfileManager.OnProfileLoaded += HandleProfileLoaded;
            IVXNProfileManager.OnProfileUpdated += HandleProfileUpdated;
            IVXNProfileManager.OnProfileError += HandleProfileError;
        }

        private void OnDisable()
        {
            if (view != null)
            {
                view.OnRefreshClicked -= HandleRefreshClicked;
                view.OnSaveClicked -= HandleSaveClicked;
                view.OnPortfolioClicked -= HandlePortfolioClicked;
            }

            IVXNProfileManager.OnProfileLoaded -= HandleProfileLoaded;
            IVXNProfileManager.OnProfileUpdated -= HandleProfileUpdated;
            IVXNProfileManager.OnProfileError -= HandleProfileError;
            CancelCurrentTask();
        }

        private async void Start()
        {
            _cts = new CancellationTokenSource();
            if (!autoFetchOnStart)
            {
                return;
            }

            await BootstrapAndRefreshAsync(_cts.Token);
        }

        private async Task BootstrapAndRefreshAsync(CancellationToken cancellationToken)
        {
            if (!await EnsureManagerReadyAsync(cancellationToken))
            {
                return;
            }

            await RefreshProfileAsync(cancellationToken);
        }

        private async Task<bool> EnsureManagerReadyAsync(CancellationToken cancellationToken)
        {
            var manager = IVXNManager.Instance;
            if (manager == null)
            {
                ApplyMockIfNeeded("IVXNManager is missing in scene.");
                return false;
            }

            if (!initializeNakamaIfNeeded)
            {
                return manager.IsInitialized;
            }

            if (manager.IsInitialized)
            {
                return true;
            }

            SetStatus("Initializing Nakama session...");
            var success = await manager.InitializeForCurrentUserAsync();
            if (!success)
            {
                ApplyMockIfNeeded("Nakama initialization failed.");
                return false;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            return true;
        }

        private async void HandleRefreshClicked()
        {
            await RunBusyGuardedAsync(async token => await RefreshProfileAsync(token));
        }

        private async void HandleSaveClicked()
        {
            await RunBusyGuardedAsync(async token => await SaveProfileAsync(token));
        }

        private async void HandlePortfolioClicked()
        {
            await RunBusyGuardedAsync(async token => await RefreshPortfolioAsync(token));
        }

        private async Task RefreshProfileAsync(CancellationToken cancellationToken)
        {
            SetStatus("Fetching profile...");
            var result = await IVXNProfileManager.FetchProfileAsync(cancellationToken);
            if (!result.Success)
            {
                SetStatus(MapError(result.ErrorCode, result.ErrorMessage), true);
                return;
            }

            if (result.Profile != null)
            {
                view?.ApplyProfile(result.Profile);
            }

            SetStatus("Profile loaded.");
            SetDebug($"traceId={result.TraceId}, requestId={result.RequestId}");
        }

        private async Task SaveProfileAsync(CancellationToken cancellationToken)
        {
            if (view == null)
            {
                SetStatus("View is missing.", true);
                return;
            }

            SetStatus("Saving profile...");
            var request = view.BuildUpdateRequest();
            var result = await IVXNProfileManager.UpdateProfileAsync(request, cancellationToken);
            if (!result.Success)
            {
                SetStatus(MapError(result.ErrorCode, result.ErrorMessage), true);
                return;
            }

            if (result.Profile != null)
            {
                view.ApplyProfile(result.Profile);
            }

            SetStatus("Profile saved.");
            SetDebug($"traceId={result.TraceId}, requestId={result.RequestId}");
        }

        private async Task RefreshPortfolioAsync(CancellationToken cancellationToken)
        {
            SetStatus("Fetching portfolio...");
            var result = await IVXNProfileManager.FetchPortfolioAsync(cancellationToken);
            if (!result.Success)
            {
                SetStatus(MapError(result.ErrorCode, result.ErrorMessage), true);
                return;
            }

            view?.ApplyPortfolio(result.Portfolio);
            SetStatus("Portfolio loaded.");
            SetDebug($"traceId={result.TraceId}, requestId={result.RequestId}");
        }

        private async Task RunBusyGuardedAsync(Func<CancellationToken, Task> action)
        {
            if (_busy)
            {
                SetStatus("Another operation is already running.");
                return;
            }

            _busy = true;
            try
            {
                if (_cts == null || _cts.IsCancellationRequested)
                {
                    _cts?.Dispose();
                    _cts = new CancellationTokenSource();
                }
                await action(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                SetStatus("Operation canceled.");
            }
            catch (Exception ex)
            {
                SetStatus("Unexpected error: " + ex.Message, true);
            }
            finally
            {
                _busy = false;
            }
        }

        private void HandleProfileLoaded(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            view?.ApplyProfile(snapshot);
            if (enableVerboseLogs)
            {
                Debug.Log("[IVX Profile Demo] Profile loaded event received.");
            }
        }

        private void HandleProfileUpdated(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            view?.ApplyProfile(snapshot);
            if (enableVerboseLogs)
            {
                Debug.Log("[IVX Profile Demo] Profile updated event received.");
            }
        }

        private void HandleProfileError(string error)
        {
            SetStatus(MapError("PROFILE_ERROR", error), true);
        }

        private void ApplyMockIfNeeded(string reason)
        {
            if (!useMockDataWhenUnavailable)
            {
                SetStatus(reason, true);
                return;
            }

            var profile = IVXProfileDemoMocks.CreateMockProfile();
            var portfolio = IVXProfileDemoMocks.CreateMockPortfolio();
            view?.ApplyProfile(profile);
            view?.ApplyPortfolio(portfolio);
            SetStatus("Using mock profile data. " + reason, true);
        }

        private static string MapError(string errorCode, string errorMessage)
        {
            var code = errorCode ?? "UNKNOWN_ERROR";
            switch (code.ToUpperInvariant())
            {
                case "AUTH_REQUIRED":
                    return "Session expired or missing. Please login again.";
                case "RATE_LIMITED":
                case "HTTP_429":
                    return "Rate limited. Please wait and retry.";
                case "VERSION_CONFLICT":
                    return "Profile was changed elsewhere. Refresh and retry.";
                case "FORBIDDEN":
                    return "Operation is not allowed for current user.";
                case "UPSTREAM_ERROR":
                    return "Geo/upstream service unavailable. Try again.";
                default:
                    return string.IsNullOrWhiteSpace(errorMessage)
                        ? $"Request failed ({code})."
                        : $"{errorMessage} ({code})";
            }
        }

        private void SetStatus(string message, bool isError = false)
        {
            view?.SetStatus(message, isError);
            if (enableVerboseLogs)
            {
                Debug.Log($"[IVX Profile Demo] {message}");
            }
        }

        private void SetDebug(string message)
        {
            view?.SetDebug(message);
        }

        private void CancelCurrentTask()
        {
            if (_cts == null)
            {
                return;
            }

            try
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            catch
            {
                // Ignore cancellation disposal errors during teardown.
            }
            finally
            {
                _cts = null;
            }
        }
    }
}
