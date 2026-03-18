using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IntelliVerseX.Backend.Nakama
{
    /// <summary>
    /// Runtime profile manager for SDK consumers.
    /// Handles profile fetch/update/portfolio calls after auth and keeps a local snapshot.
    /// </summary>
    public static class IVXNProfileManager
    {
        private const string LOG_PREFIX = "[IVX-PROFILE]";
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int BASE_RETRY_DELAY_MS = 500;
        private static readonly Regex SafeNamePattern = new Regex(@"^[\p{L}\p{N}\s\.\-_']{1,100}$", RegexOptions.Compiled);
        private static readonly Regex UsernamePattern = new Regex(@"^[a-zA-Z0-9_]{3,20}$", RegexOptions.Compiled);

        private static readonly string[] ReservedUsernames =
        {
            "admin", "system", "nakama", "root", "moderator", "support", "null", "undefined",
            "guest", "anonymous", "intelliversex", "intelliverse"
        };

        private static readonly object SyncRoot = new object();
        private static readonly System.Threading.SemaphoreSlim ChangeUsernameLock = new System.Threading.SemaphoreSlim(1, 1);
        private static readonly System.Threading.SemaphoreSlim UpdateProfileLock = new System.Threading.SemaphoreSlim(1, 1);
        private static IVXNProfileSnapshot _snapshot = new IVXNProfileSnapshot();
        private static bool _isInitialized;
        private static bool _isDirty;

        public static bool EnableDebugLogs { get; set; } = true;
        public static bool IsInitialized => _isInitialized;
        public static bool IsDirty => _isDirty;
        public static DateTime LastSyncedAtUtc { get; private set; } = DateTime.MinValue;
        public static IVXNProfileSnapshot Snapshot
        {
            get
            {
                lock (SyncRoot)
                {
                    return _snapshot.Clone();
                }
            }
        }

        public static event Action<IVXNProfileSnapshot> OnProfileLoaded;
        public static event Action<IVXNProfileSnapshot> OnProfileUpdated;
        public static event Action<IVXNUsernameChangeResult> OnUsernameChanged;
        public static event Action<string> OnProfileError;

        [Serializable]
        public sealed class IVXNProfileSnapshot
        {
            public string UserId;
            public string Username;
            public string DisplayName;
            public string Email;
            public string FirstName;
            public string LastName;
            public string City;
            public string Region;
            public string Country;
            public string CountryCode;
            public string Locale;
            public string AvatarUrl;
            public int AvatarPresetId;
            public string DeviceId;
            public string Platform;
            public int ProfileVersion;
            public int SchemaVersion;
            public string TraceId;
            public string RequestId;
            public string RawMetadataJson;

            public string FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
                ? DisplayName ?? Username ?? "Player"
                : $"{FirstName ?? ""} {LastName ?? ""}".Trim();

            public IVXNProfileSnapshot Clone()
            {
                return (IVXNProfileSnapshot)MemberwiseClone();
            }
        }

        [Serializable]
        public sealed class IVXNProfileUpdateRequest
        {
            public string DisplayName;
            public string FirstName;
            public string LastName;
            public string Locale;
            public string City;
            public string Region;
            public string Country;
            public string CountryCode;
            public string AvatarUrl;
            public int? AvatarPresetId;
            public int? ExpectedProfileVersion;
        }

        [Serializable]
        public sealed class IVXNUsernameChangeRequest
        {
            public string NewUsername;
        }

        [Serializable]
        public sealed class IVXNUsernameChangeResult
        {
            public bool Success;
            public string ErrorCode;
            public string ErrorMessage;
            public bool Retryable;
            public string TraceId;
            public string RequestId;
            public string OldUsername;
            public string NewUsername;
        }

        [Serializable]
        public sealed class IVXNProfileGameEntry
        {
            public string GameId;
            public int PlayCount;
            public int SessionCount;
            public string LastPlayedAt;
            public long WalletBalance;
        }

        [Serializable]
        public sealed class IVXNProfilePortfolioSnapshot
        {
            public string UserId;
            public int TotalGames;
            public long GlobalWalletBalance;
            public List<IVXNProfileGameEntry> Games = new List<IVXNProfileGameEntry>();
            public string RawJson;
        }

        [Serializable]
        public class IVXNProfileFetchResult
        {
            public bool Success;
            public string ErrorCode;
            public string ErrorMessage;
            public bool Retryable;
            public string TraceId;
            public string RequestId;
            public IVXNProfileSnapshot Profile;
        }

        [Serializable]
        public sealed class IVXNProfileUpdateResult : IVXNProfileFetchResult
        {
        }

        [Serializable]
        public sealed class IVXNProfilePortfolioResult
        {
            public bool Success;
            public string ErrorCode;
            public string ErrorMessage;
            public bool Retryable;
            public string TraceId;
            public string RequestId;
            public IVXNProfilePortfolioSnapshot Portfolio;
        }

        private sealed class RpcCallResult
        {
            public bool Success;
            public string Payload;
            public string ErrorCode;
            public string ErrorMessage;
            public bool Retryable;
        }

        public static async Task<bool> RefreshProfileAfterAuthAsync(CancellationToken cancellationToken = default)
        {
            var result = await FetchProfileAsync(cancellationToken);
            return result.Success;
        }

        public static async Task<IVXNProfileFetchResult> FetchProfileAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            for (var attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                var rpc = await CallRpcAsync("get_player_metadata", "{}", cancellationToken);
                if (!rpc.Success)
                {
                    if (attempt < MAX_RETRY_ATTEMPTS && rpc.Retryable)
                    {
                        await Task.Delay(GetRetryDelayMs(attempt), cancellationToken);
                        continue;
                    }

                    return BuildFetchFailure(rpc.ErrorCode, rpc.ErrorMessage, rpc.Retryable);
                }

                var parsed = ParseProfileResponse(rpc.Payload);
                if (!parsed.Success && attempt < MAX_RETRY_ATTEMPTS && parsed.Retryable)
                {
                    await Task.Delay(GetRetryDelayMs(attempt), cancellationToken);
                    continue;
                }

                if (!parsed.Success)
                {
                    RaiseProfileError(parsed.ErrorMessage);
                    return parsed;
                }

                SetSnapshot(parsed.Profile, isDirty: false);
                RaiseProfileLoaded(parsed.Profile);
                return parsed;
            }

            return BuildFetchFailure("MAX_RETRIES_EXCEEDED", "Profile fetch failed after retries.", true);
        }

        public static async Task<IVXNProfileUpdateResult> UpdateProfileAsync(IVXNProfileUpdateRequest request, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var validationError = ValidateUpdateRequest(request);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new IVXNProfileUpdateResult
                {
                    Success = false,
                    ErrorCode = "VALIDATION_ERROR",
                    ErrorMessage = validationError,
                    Retryable = false
                };
            }

            await UpdateProfileLock.WaitAsync(cancellationToken);
            try
            {
            var payload = BuildUpdatePayload(request);
            var payloadJson = payload.ToString(Formatting.None);

            for (var attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                var rpc = await CallRpcAsync("rpc_update_player_metadata", payloadJson, cancellationToken);
                if (!rpc.Success)
                {
                    if (attempt < MAX_RETRY_ATTEMPTS && rpc.Retryable)
                    {
                        await Task.Delay(GetRetryDelayMs(attempt), cancellationToken);
                        continue;
                    }

                    var transportFailure = new IVXNProfileUpdateResult
                    {
                        Success = false,
                        ErrorCode = rpc.ErrorCode,
                        ErrorMessage = rpc.ErrorMessage,
                        Retryable = rpc.Retryable
                    };
                    RaiseProfileError(transportFailure.ErrorMessage);
                    return transportFailure;
                }

                var parsed = ParseProfileResponse(rpc.Payload);
                if (!parsed.Success && attempt < MAX_RETRY_ATTEMPTS && parsed.Retryable)
                {
                    await Task.Delay(GetRetryDelayMs(attempt), cancellationToken);
                    continue;
                }

                var result = new IVXNProfileUpdateResult
                {
                    Success = parsed.Success,
                    ErrorCode = parsed.ErrorCode,
                    ErrorMessage = parsed.ErrorMessage,
                    Retryable = parsed.Retryable,
                    TraceId = parsed.TraceId,
                    RequestId = parsed.RequestId,
                    Profile = parsed.Profile
                };

                if (!result.Success)
                {
                    lock (SyncRoot)
                    {
                        _isDirty = true;
                    }
                    RaiseProfileError(result.ErrorMessage);
                    return result;
                }

                SetSnapshot(result.Profile, isDirty: false);
                RaiseProfileUpdated(result.Profile);
                return result;
            }

            return new IVXNProfileUpdateResult
            {
                Success = false,
                ErrorCode = "MAX_RETRIES_EXCEEDED",
                ErrorMessage = "Profile update failed after retries.",
                Retryable = true
            };
            }
            finally
            {
                UpdateProfileLock.Release();
            }
        }

        /// <summary>Alias for ChangeUsernameAsync. Updates username via rpc_change_username.</summary>
        public static Task<IVXNUsernameChangeResult> UpdateUsernameAsync(string newUsername, CancellationToken cancellationToken = default) => ChangeUsernameAsync(newUsername, cancellationToken);

        public static async Task<IVXNUsernameChangeResult> ChangeUsernameAsync(string newUsername, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                return new IVXNUsernameChangeResult
                {
                    Success = false,
                    ErrorCode = "VALIDATION_ERROR",
                    ErrorMessage = "Username is required.",
                    Retryable = false
                };
            }

            newUsername = newUsername.Trim();

            if (newUsername.Length < 3 || newUsername.Length > 20)
            {
                return new IVXNUsernameChangeResult
                {
                    Success = false,
                    ErrorCode = "VALIDATION_ERROR",
                    ErrorMessage = "Username must be 3-20 characters.",
                    Retryable = false
                };
            }

            if (!UsernamePattern.IsMatch(newUsername))
            {
                return new IVXNUsernameChangeResult
                {
                    Success = false,
                    ErrorCode = "VALIDATION_ERROR",
                    ErrorMessage = "Username can only contain letters, numbers, and underscores.",
                    Retryable = false
                };
            }

            var normalized = newUsername.ToLowerInvariant();
            for (var i = 0; i < ReservedUsernames.Length; i++)
            {
                if (string.Equals(ReservedUsernames[i], normalized, StringComparison.Ordinal))
                {
                    return new IVXNUsernameChangeResult
                    {
                        Success = false,
                        ErrorCode = "VALIDATION_ERROR",
                        ErrorMessage = "Username is reserved.",
                        Retryable = false
                    };
                }
            }

            await ChangeUsernameLock.WaitAsync(cancellationToken);
            try
            {
            var oldUsername = Snapshot.Username;
            var payload = new JObject { ["new_username"] = newUsername };
            var payloadJson = payload.ToString(Formatting.None);

            for (var attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                var rpc = await CallRpcAsync("rpc_change_username", payloadJson, cancellationToken);
                if (!rpc.Success)
                {
                    if (attempt < MAX_RETRY_ATTEMPTS && rpc.Retryable)
                    {
                        await Task.Delay(GetRetryDelayMs(attempt), cancellationToken);
                        continue;
                    }

                    var transportFailure = new IVXNUsernameChangeResult
                    {
                        Success = false,
                        ErrorCode = rpc.ErrorCode,
                        ErrorMessage = rpc.ErrorMessage,
                        Retryable = rpc.Retryable,
                        OldUsername = oldUsername
                    };
                    RaiseProfileError(transportFailure.ErrorMessage);
                    return transportFailure;
                }

                var parsed = ParseUsernameChangeResponse(rpc.Payload, oldUsername);
                if (!parsed.Success && attempt < MAX_RETRY_ATTEMPTS && parsed.Retryable)
                {
                    await Task.Delay(GetRetryDelayMs(attempt), cancellationToken);
                    continue;
                }

                if (parsed.Success)
                {
                    lock (SyncRoot)
                    {
                        _snapshot.Username = parsed.NewUsername;
                    }
                    RaiseUsernameChanged(parsed);
                }
                else
                {
                    RaiseProfileError(parsed.ErrorMessage);
                }

                return parsed;
            }

            return new IVXNUsernameChangeResult
            {
                Success = false,
                ErrorCode = "MAX_RETRIES_EXCEEDED",
                ErrorMessage = "Username change failed after retries.",
                Retryable = true,
                OldUsername = oldUsername
            };
            }
            finally
            {
                ChangeUsernameLock.Release();
            }
        }

        public static async Task<IVXNProfilePortfolioResult> FetchPortfolioAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            for (var attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                var rpc = await CallRpcAsync("get_player_portfolio", "{}", cancellationToken);
                if (!rpc.Success)
                {
                    if (attempt < MAX_RETRY_ATTEMPTS && rpc.Retryable)
                    {
                        await Task.Delay(GetRetryDelayMs(attempt), cancellationToken);
                        continue;
                    }

                    return new IVXNProfilePortfolioResult
                    {
                        Success = false,
                        ErrorCode = rpc.ErrorCode,
                        ErrorMessage = rpc.ErrorMessage,
                        Retryable = rpc.Retryable
                    };
                }

                var parsed = ParsePortfolioResponse(rpc.Payload);
                if (!parsed.Success && attempt < MAX_RETRY_ATTEMPTS && parsed.Retryable)
                {
                    await Task.Delay(GetRetryDelayMs(attempt), cancellationToken);
                    continue;
                }

                if (!parsed.Success)
                {
                    RaiseProfileError(parsed.ErrorMessage);
                }

                return parsed;
            }

            return new IVXNProfilePortfolioResult
            {
                Success = false,
                ErrorCode = "MAX_RETRIES_EXCEEDED",
                ErrorMessage = "Portfolio fetch failed after retries.",
                Retryable = true
            };
        }

        private static void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_isInitialized)
                {
                    return;
                }

                _snapshot = new IVXNProfileSnapshot();
                _isDirty = false;
                LastSyncedAtUtc = DateTime.MinValue;
                _isInitialized = true;
            }
        }

        private static JObject BuildUpdatePayload(IVXNProfileUpdateRequest request)
        {
            var payload = new JObject();

            if (!string.IsNullOrWhiteSpace(request.DisplayName)) payload["display_name"] = request.DisplayName.Trim();
            if (!string.IsNullOrWhiteSpace(request.FirstName)) payload["first_name"] = request.FirstName.Trim();
            if (!string.IsNullOrWhiteSpace(request.LastName)) payload["last_name"] = request.LastName.Trim();
            if (!string.IsNullOrWhiteSpace(request.Locale)) payload["locale"] = request.Locale.Trim().Replace("_", "-").ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(request.City)) payload["city"] = request.City.Trim();
            if (!string.IsNullOrWhiteSpace(request.Region)) payload["region"] = request.Region.Trim();
            if (!string.IsNullOrWhiteSpace(request.Country)) payload["country"] = request.Country.Trim();
            if (!string.IsNullOrWhiteSpace(request.CountryCode)) payload["country_code"] = request.CountryCode.Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(request.AvatarUrl)) payload["avatar_url"] = request.AvatarUrl.Trim();
            if (request.AvatarPresetId.HasValue) payload["avatar_preset_id"] = request.AvatarPresetId.Value;

            var expectedVersion = request.ExpectedProfileVersion;
            if (!expectedVersion.HasValue)
            {
                var snapshot = Snapshot;
                if (snapshot.ProfileVersion > 0)
                {
                    expectedVersion = snapshot.ProfileVersion;
                }
            }

            if (expectedVersion.HasValue)
            {
                payload["expected_profile_version"] = expectedVersion.Value;
            }

            return payload;
        }

        private static string ValidateUpdateRequest(IVXNProfileUpdateRequest request)
        {
            if (request == null)
            {
                return "Profile update request is required.";
            }

            if (!string.IsNullOrWhiteSpace(request.DisplayName) &&
                (request.DisplayName.Length < 2 || request.DisplayName.Length > 50 || !SafeNamePattern.IsMatch(request.DisplayName)))
            {
                return "Display name must be 2-50 safe characters.";
            }

            if (!string.IsNullOrWhiteSpace(request.FirstName) &&
                (request.FirstName.Length > 100 || !SafeNamePattern.IsMatch(request.FirstName)))
            {
                return "First name must be 1-100 safe characters.";
            }

            if (!string.IsNullOrWhiteSpace(request.LastName) &&
                (request.LastName.Length > 100 || !SafeNamePattern.IsMatch(request.LastName)))
            {
                return "Last name must be 1-100 safe characters.";
            }

            if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            {
                if (!Uri.TryCreate(request.AvatarUrl, UriKind.Absolute, out var avatarUri) ||
                    (avatarUri.Scheme != Uri.UriSchemeHttp && avatarUri.Scheme != Uri.UriSchemeHttps))
                {
                    return "Avatar URL must be an absolute HTTP/HTTPS URL.";
                }
            }

            if (request.AvatarPresetId.HasValue && (request.AvatarPresetId.Value < 0 || request.AvatarPresetId.Value > 999))
            {
                return "Avatar preset ID must be between 0 and 999.";
            }

            return null;
        }

        private static async Task<RpcCallResult> CallRpcAsync(string rpcName, string payloadJson, CancellationToken cancellationToken)
        {
            var manager = IVXNManager.Instance;
            if (manager == null)
            {
                return new RpcCallResult { Success = false, ErrorCode = "MANAGER_UNAVAILABLE", ErrorMessage = "IVXNManager instance is not available.", Retryable = true };
            }

            var hasValidSession = await manager.EnsureValidSessionAsync();
            if (!hasValidSession || manager.Client == null || manager.Session == null)
            {
                return new RpcCallResult { Success = false, ErrorCode = "AUTH_REQUIRED", ErrorMessage = "Nakama session is not ready.", Retryable = true };
            }

            try
            {
                var rpcResponse = await manager.Client.RpcAsync(
                    manager.Session,
                    rpcName,
                    payloadJson,
                    retryConfiguration: null,
                    canceller: cancellationToken);

                return new RpcCallResult { Success = true, Payload = rpcResponse.Payload };
            }
            catch (ApiResponseException apiEx)
            {
                return new RpcCallResult
                {
                    Success = false,
                    ErrorCode = $"HTTP_{apiEx.StatusCode}",
                    ErrorMessage = apiEx.Message,
                    Retryable = apiEx.StatusCode == 429 || apiEx.StatusCode >= 500
                };
            }
            catch (OperationCanceledException)
            {
                return new RpcCallResult { Success = false, ErrorCode = "CANCELLED", ErrorMessage = "Operation was cancelled.", Retryable = false };
            }
            catch (Exception ex)
            {
                return new RpcCallResult { Success = false, ErrorCode = "NETWORK_ERROR", ErrorMessage = ex.Message, Retryable = true };
            }
        }

        private static IVXNProfileFetchResult ParseProfileResponse(string payload)
        {
            try
            {
                var root = JObject.Parse(payload ?? "{}");
                var success = root.Value<bool?>("success") != false;
                var errorCode = GetErrorCode(root);
                var errorMessage = root.Value<string>("error");
                var traceId = root.Value<string>("traceId");
                var requestId = root.Value<string>("requestId") ?? root.Value<string>("request_id");

                if (!success)
                {
                    return new IVXNProfileFetchResult
                    {
                        Success = false,
                        ErrorCode = errorCode,
                        ErrorMessage = string.IsNullOrEmpty(errorMessage) ? "Profile RPC failed." : errorMessage,
                        Retryable = IsRetryable(errorCode),
                        TraceId = traceId,
                        RequestId = requestId
                    };
                }

                var metadataToken = root.SelectToken("data.metadata") ?? root["metadata"];
                if (metadataToken == null || metadataToken.Type != JTokenType.Object)
                {
                    return new IVXNProfileFetchResult
                    {
                        Success = false,
                        ErrorCode = "INVALID_RESPONSE",
                        ErrorMessage = "Profile metadata was not returned by server.",
                        Retryable = false,
                        TraceId = traceId,
                        RequestId = requestId
                    };
                }

                var snapshot = BuildSnapshotFromMetadata((JObject)metadataToken);
                snapshot.TraceId = traceId;
                snapshot.RequestId = requestId;

                return new IVXNProfileFetchResult
                {
                    Success = true,
                    Profile = snapshot,
                    TraceId = traceId,
                    RequestId = requestId
                };
            }
            catch (Exception ex)
            {
                return BuildFetchFailure("DESERIALIZATION_ERROR", ex.Message, false);
            }
        }

        private static IVXNProfilePortfolioResult ParsePortfolioResponse(string payload)
        {
            try
            {
                var root = JObject.Parse(payload ?? "{}");
                var success = root.Value<bool?>("success") != false;
                var errorCode = GetErrorCode(root);
                var errorMessage = root.Value<string>("error");
                var traceId = root.Value<string>("traceId");
                var requestId = root.Value<string>("requestId") ?? root.Value<string>("request_id");

                if (!success)
                {
                    return new IVXNProfilePortfolioResult
                    {
                        Success = false,
                        ErrorCode = errorCode,
                        ErrorMessage = string.IsNullOrEmpty(errorMessage) ? "Portfolio RPC failed." : errorMessage,
                        Retryable = IsRetryable(errorCode),
                        TraceId = traceId,
                        RequestId = requestId
                    };
                }

                var data = root.SelectToken("data") as JObject ?? root;
                var portfolio = new IVXNProfilePortfolioSnapshot
                {
                    UserId = data.Value<string>("userId") ?? data.Value<string>("user_id"),
                    TotalGames = data.Value<int?>("totalGames") ?? data.Value<int?>("total_games") ?? 0,
                    RawJson = payload
                };

                var globalWalletToken = data["globalWallet"] ?? data["global_wallet"];
                if (globalWalletToken != null && globalWalletToken.Type == JTokenType.Object)
                {
                    portfolio.GlobalWalletBalance = globalWalletToken.Value<long?>("balance") ?? 0;
                }

                var gamesToken = data["games"];
                if (gamesToken is JArray gamesArray)
                {
                    foreach (var gameToken in gamesArray)
                    {
                        if (!(gameToken is JObject gameObj))
                        {
                            continue;
                        }

                        var entry = new IVXNProfileGameEntry
                        {
                            GameId = gameObj.Value<string>("game_id") ?? gameObj.Value<string>("gameId"),
                            PlayCount = gameObj.Value<int?>("play_count") ?? gameObj.Value<int?>("playCount") ?? 0,
                            SessionCount = gameObj.Value<int?>("session_count") ?? gameObj.Value<int?>("sessionCount") ?? 0,
                            LastPlayedAt = gameObj.Value<string>("last_played") ?? gameObj.Value<string>("lastPlayedAt")
                        };

                        var walletObj = gameObj["wallet"] as JObject;
                        if (walletObj != null)
                        {
                            entry.WalletBalance = walletObj.Value<long?>("balance") ?? 0;
                        }

                        portfolio.Games.Add(entry);
                    }
                }

                return new IVXNProfilePortfolioResult
                {
                    Success = true,
                    Portfolio = portfolio,
                    TraceId = traceId,
                    RequestId = requestId
                };
            }
            catch (Exception ex)
            {
                return new IVXNProfilePortfolioResult
                {
                    Success = false,
                    ErrorCode = "DESERIALIZATION_ERROR",
                    ErrorMessage = ex.Message,
                    Retryable = false
                };
            }
        }

        private static IVXNProfileSnapshot BuildSnapshotFromMetadata(JObject metadata)
        {
            return new IVXNProfileSnapshot
            {
                UserId = metadata.Value<string>("user_id") ?? metadata.Value<string>("userId"),
                Username = metadata.Value<string>("username") ?? metadata.Value<string>("userName"),
                DisplayName = metadata.Value<string>("display_name") ?? metadata.Value<string>("displayName"),
                Email = metadata.Value<string>("email"),
                FirstName = metadata.Value<string>("first_name") ?? metadata.Value<string>("firstName"),
                LastName = metadata.Value<string>("last_name") ?? metadata.Value<string>("lastName"),
                City = metadata.Value<string>("city") ?? metadata.Value<string>("cityName"),
                Region = metadata.Value<string>("region") ?? metadata.Value<string>("state"),
                Country = metadata.Value<string>("country") ?? metadata.Value<string>("countryName"),
                CountryCode = metadata.Value<string>("country_code") ??
                              metadata.Value<string>("countryCode") ??
                              metadata.Value<string>("geo_location") ??
                              metadata.Value<string>("geoLocation"),
                Locale = metadata.Value<string>("locale"),
                AvatarUrl = metadata.Value<string>("avatar_url") ?? metadata.Value<string>("avatarUrl") ?? metadata.Value<string>("avatar"),
                AvatarPresetId = metadata.Value<int?>("avatar_preset_id") ?? metadata.Value<int?>("avatarPresetId") ?? 0,
                DeviceId = metadata.Value<string>("device_id") ?? metadata.Value<string>("deviceId"),
                Platform = metadata.Value<string>("platform"),
                ProfileVersion = metadata.Value<int?>("profileVersion") ?? metadata.Value<int?>("profile_version") ?? metadata.Value<int?>("version") ?? 0,
                SchemaVersion = metadata.Value<int?>("schemaVersion") ?? metadata.Value<int?>("schema_version") ?? 0,
                RawMetadataJson = metadata.ToString(Formatting.None)
            };
        }

        private static IVXNUsernameChangeResult ParseUsernameChangeResponse(string payload, string oldUsername)
        {
            try
            {
                var root = JObject.Parse(payload ?? "{}");
                var success = root.Value<bool?>("success") != false;
                var errorCode = GetErrorCode(root);
                var errorMessage = root.Value<string>("error") ?? root.Value<string>("message");
                var traceId = root.Value<string>("traceId");
                var requestId = root.Value<string>("requestId") ?? root.Value<string>("request_id");
                var newUsername = root.Value<string>("username") ?? root.Value<string>("new_username");

                if (!success)
                {
                    return new IVXNUsernameChangeResult
                    {
                        Success = false,
                        ErrorCode = errorCode,
                        ErrorMessage = string.IsNullOrEmpty(errorMessage) ? "Username change failed." : errorMessage,
                        Retryable = IsRetryable(errorCode),
                        TraceId = traceId,
                        RequestId = requestId,
                        OldUsername = oldUsername
                    };
                }

                return new IVXNUsernameChangeResult
                {
                    Success = true,
                    TraceId = traceId,
                    RequestId = requestId,
                    OldUsername = oldUsername,
                    NewUsername = newUsername ?? oldUsername
                };
            }
            catch (Exception ex)
            {
                return new IVXNUsernameChangeResult
                {
                    Success = false,
                    ErrorCode = "DESERIALIZATION_ERROR",
                    ErrorMessage = ex.Message,
                    Retryable = false,
                    OldUsername = oldUsername
                };
            }
        }

        private static IVXNProfileFetchResult BuildFetchFailure(string errorCode, string errorMessage, bool retryable)
        {
            return new IVXNProfileFetchResult
            {
                Success = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                Retryable = retryable
            };
        }

        private static string GetErrorCode(JObject root)
        {
            return root.Value<string>("errorCode") ?? root.Value<string>("error_code") ?? "UNKNOWN_ERROR";
        }

        private static bool IsRetryable(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                return false;
            }

            var normalized = errorCode.ToUpperInvariant();
            return normalized == "RATE_LIMITED" ||
                   normalized == "HTTP_429" ||
                   normalized == "HTTP_500" ||
                   normalized == "HTTP_502" ||
                   normalized == "HTTP_503" ||
                   normalized == "HTTP_504" ||
                   normalized == "UPSTREAM_ERROR" ||
                   normalized == "INTERNAL_ERROR" ||
                   normalized == "NETWORK_ERROR" ||
                   normalized == "AUTH_REQUIRED";
        }

        private static int GetRetryDelayMs(int attempt)
        {
            var multiplier = Math.Pow(2, Math.Max(0, attempt - 1));
            return (int)(BASE_RETRY_DELAY_MS * multiplier);
        }

        private static void SetSnapshot(IVXNProfileSnapshot snapshot, bool isDirty)
        {
            lock (SyncRoot)
            {
                _snapshot = snapshot?.Clone() ?? new IVXNProfileSnapshot();
                _isDirty = isDirty;
                LastSyncedAtUtc = DateTime.UtcNow;
            }
        }

        private static void RaiseProfileLoaded(IVXNProfileSnapshot snapshot)
        {
            SafeLog($"Loaded profile for user '{snapshot?.UserId ?? "unknown"}'.");
            try
            {
                OnProfileLoaded?.Invoke(snapshot?.Clone());
            }
            catch (Exception ex)
            {
                SafeLog("Profile loaded event handler failed: " + ex.Message, true);
            }
        }

        private static void RaiseProfileUpdated(IVXNProfileSnapshot snapshot)
        {
            SafeLog($"Updated profile for user '{snapshot?.UserId ?? "unknown"}'.");
            try
            {
                OnProfileUpdated?.Invoke(snapshot?.Clone());
            }
            catch (Exception ex)
            {
                SafeLog("Profile updated event handler failed: " + ex.Message, true);
            }
        }

        private static void RaiseProfileError(string message)
        {
            SafeLog("Profile error: " + message, true);
            try
            {
                OnProfileError?.Invoke(message);
            }
            catch (Exception ex)
            {
                SafeLog("Profile error event handler failed: " + ex.Message, true);
            }
        }

        private static void RaiseUsernameChanged(IVXNUsernameChangeResult result)
        {
            SafeLog($"Username changed from '{result.OldUsername ?? "unknown"}' to '{result.NewUsername ?? "unknown"}'.");
            try
            {
                OnUsernameChanged?.Invoke(result);
            }
            catch (Exception ex)
            {
                SafeLog("Username changed event handler failed: " + ex.Message, true);
            }
        }

        private static void SafeLog(string message, bool warning = false)
        {
            if (!EnableDebugLogs && !warning)
            {
                return;
            }

            if (warning)
            {
                Debug.LogWarning($"{LOG_PREFIX} {message}");
            }
            else
            {
                Debug.Log($"{LOG_PREFIX} {message}");
            }
        }
    }
}
