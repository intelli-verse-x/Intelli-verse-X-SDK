using IntelliVerseX.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static partial class APIManager
{
    #region Friends API

    /// <summary>
    /// Gets the current user session. Returns null if not logged in.
    /// </summary>
    public static UserSessionManager.UserSession GetCurrentSession() => UserSessionManager.Current;

    /// <summary>
    /// Log warning (if debug enabled).
    /// </summary>
    private static void LogWarning(string msg) { if (DebugLogs) Debug.LogWarning(msg); OnLog?.Invoke("[WARN] " + msg); }

    /// <summary>
    /// Generic authenticated GET request.
    /// </summary>
    private static async Task GetAuthenticatedRequestAsync(
        string url,
        Action<string> onSuccess,
        Action<string> onError,
        CancellationToken ct = default)
    {
        try
        {
            string token = await ResolveBearerTokenAsync(null, ct);
            string authHeader = string.IsNullOrWhiteSpace(token) ? null : $"Bearer {token}";

            using (var req = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(authHeader))
                    req.SetRequestHeader("Authorization", authHeader);
                req.SetRequestHeader("Accept", "application/json");
                req.timeout = RequestTimeoutSeconds;

                PrintCurl("GET", url, new Dictionary<string, string> {
                    { "Accept", "application/json" },
                    { "Authorization", string.IsNullOrWhiteSpace(token) ? "" : "Bearer ***" }
                }, null);

                var op = req.SendWebRequest();
                float start = Time.realtimeSinceStartup;

                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                    {
                        req.Abort();
                        throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                    }
                    await Task.Yield();
                }

#if UNITY_2020_1_OR_NEWER
                bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                string text = req.downloadHandler?.text ?? "";
                long code = req.responseCode;

                Log($"[HTTP] GET ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                {
                    onSuccess?.Invoke(text);
                }
                else
                {
                    onError?.Invoke($"HTTP {code}: {req.error} - {text}");
                }
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Generic authenticated POST request.
    /// </summary>
    private static async Task PostAuthenticatedRequestAsync<T>(
        string url,
        T payload,
        Action<string> onSuccess,
        Action<string> onError,
        CancellationToken ct = default)
    {
        try
        {
            string token = await ResolveBearerTokenAsync(null, ct);
            string authHeader = string.IsNullOrWhiteSpace(token) ? null : $"Bearer {token}";
            string json = JsonUtility.ToJson(payload);

            using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(authHeader))
                    req.SetRequestHeader("Authorization", authHeader);
                req.timeout = RequestTimeoutSeconds;

                PrintCurl("POST", url, new Dictionary<string, string> {
                    { "Content-Type", "application/json" },
                    { "Authorization", string.IsNullOrWhiteSpace(token) ? "" : "Bearer ***" }
                }, json);

                var op = req.SendWebRequest();
                float start = Time.realtimeSinceStartup;

                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                    {
                        req.Abort();
                        throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                    }
                    await Task.Yield();
                }

#if UNITY_2020_1_OR_NEWER
                bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                string text = req.downloadHandler?.text ?? "";
                long code = req.responseCode;

                Log($"[HTTP] POST ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                {
                    onSuccess?.Invoke(text);
                }
                else
                {
                    onError?.Invoke($"HTTP {code}: {req.error} - {text}");
                }
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Generic authenticated PATCH request.
    /// </summary>
    private static async Task PatchAuthenticatedRequestAsync<T>(
        string url,
        T payload,
        Action<string> onSuccess,
        Action<string> onError,
        CancellationToken ct = default)
    {
        try
        {
            string token = await ResolveBearerTokenAsync(null, ct);
            string authHeader = string.IsNullOrWhiteSpace(token) ? null : $"Bearer {token}";
            string json = JsonUtility.ToJson(payload);

            using (var req = new UnityWebRequest(url, "PATCH"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(authHeader))
                    req.SetRequestHeader("Authorization", authHeader);
                req.timeout = RequestTimeoutSeconds;

                PrintCurl("PATCH", url, new Dictionary<string, string> {
                    { "Content-Type", "application/json" },
                    { "Authorization", string.IsNullOrWhiteSpace(token) ? "" : "Bearer ***" }
                }, json);

                var op = req.SendWebRequest();
                float start = Time.realtimeSinceStartup;

                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                    {
                        req.Abort();
                        throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                    }
                    await Task.Yield();
                }

#if UNITY_2020_1_OR_NEWER
                bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                string text = req.downloadHandler?.text ?? "";
                long code = req.responseCode;

                Log($"[HTTP] PATCH ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                {
                    onSuccess?.Invoke(text);
                }
                else
                {
                    onError?.Invoke($"HTTP {code}: {req.error} - {text}");
                }
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Fetches the friends list for a user.
    /// </summary>
    /// <param name="userId">User ID to fetch friends for</param>
    /// <param name="status">Filter by status: accepted | pending | rejected</param>
    /// <param name="query">Optional search query</param>
    /// <param name="onSuccess">Callback with list of friends</param>
    /// <param name="onError">Error callback</param>
    public static async void FetchFriendsList(
        string userId,
        string status,
        string query,
        Action<List<IVXModels.FriendData>> onSuccess,
        Action<string> onError)
    {
        string reqId = "FRI-LIST-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        try
        {
            var session = GetCurrentSession();
            if (session == null)
            {
                LogError($"[{reqId}] [Friends] No session found. User must sign in first.");
                onError?.Invoke("Not signed in. Please login first.");
                return;
            }

            // Validate session has required fields
            if (string.IsNullOrWhiteSpace(session.accessToken))
            {
                LogError($"[{reqId}] [Friends] Session exists but accessToken is empty. User must re-login.");
                onError?.Invoke("Session invalid. Please login again.");
                return;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = session.userId;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    LogError($"[{reqId}] [Friends] No userId available in session. Session is incomplete.");
                    onError?.Invoke("User ID not found in session. Please login again.");
                    return;
                }
            }

            string url = IVXURLs.GetFriendsUrl(userId, status, query);
            Log($"[{reqId}] [Friends] GET {url} (userId={userId}, status={status})");

            await GetAuthenticatedRequestAsync(
                url,
                response =>
                {
                    Log($"[{reqId}] [Friends] 200 OK; payload length={response?.Length ?? 0}");

                    try
                    {
                        var result = JsonUtility.FromJson<IVXModels.FriendsResponse>(response);
                        if (result?.status == true && result.data != null)
                        {
                            Log($"[{reqId}] [Friends] Parsed OK; count={result.data.Count}");
                            onSuccess?.Invoke(result.data);
                        }
                        else
                        {
                            LogWarning($"[{reqId}] [Friends] API returned no data or status=false. message='{result?.message}'");
                            onSuccess?.Invoke(new List<IVXModels.FriendData>());
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"[{reqId}] [Friends] Parse error: {ex.Message}");
                        onError?.Invoke("Invalid friends response format.");
                    }
                },
                error =>
                {
                    LogError($"[{reqId}] [Friends] GET failed: {error}");
                    onError?.Invoke(error);
                }
            );
        }
        catch (Exception ex)
        {
            LogError($"[{reqId}] [Friends] Unexpected exception: {ex}");
            onError?.Invoke("Unexpected error while fetching friends.");
        }
    }

    /// <summary>
    /// Async version of FetchFriendsList for modern async/await usage.
    /// </summary>
    public static Task<List<IVXModels.FriendData>> GetFriendsAsync(
        string userId = null,
        string status = "accepted",
        string query = null,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<List<IVXModels.FriendData>>();
        
        FetchFriendsList(
            userId,
            status,
            query,
            friends => tcs.TrySetResult(friends ?? new List<IVXModels.FriendData>()),
            error => tcs.TrySetException(new Exception(error ?? "Unknown error fetching friends"))
        );
        
        ct.Register(() => tcs.TrySetCanceled());
        return tcs.Task;
    }

    /// <summary>
    /// Searches for users to add as friends.
    /// </summary>
    /// <param name="query">Search query (min 2 characters)</param>
    /// <param name="onSuccess">Callback with list of matching users</param>
    /// <param name="onError">Error callback</param>
    public static void SearchFriends(
        string query,
        Action<List<IVXModels.SearchUser>> onSuccess,
        Action<string> onError)
    {
        string reqId = "FRI-SEARCH-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            Log($"[{reqId}] [Friends] Query too short → returning empty list.");
            onSuccess?.Invoke(new List<IVXModels.SearchUser>());
            return;
        }

        var session = GetCurrentSession();
        if (session == null)
        {
            LogError($"[{reqId}] [Friends] No session found. User must sign in first.");
            onError?.Invoke("Not signed in. Please login first.");
            return;
        }

        // Validate session has required fields
        if (string.IsNullOrWhiteSpace(session.accessToken))
        {
            LogError($"[{reqId}] [Friends] Session exists but accessToken is empty. User must re-login.");
            onError?.Invoke("Session invalid. Please login again.");
            return;
        }

        if (string.IsNullOrWhiteSpace(session.userId))
        {
            LogError($"[{reqId}] [Friends] Session exists but userId is empty. Session is incomplete.");
            onError?.Invoke("Session incomplete. Please login again.");
            return;
        }

        string url = IVXURLs.GetSearchFriendsUrl(query, session.userId);
        Log($"[{reqId}] [Friends] Search GET {url} (userId={session.userId})");

        _ = GetAuthenticatedRequestAsync(
            url,
            response =>
            {
                Log($"[{reqId}] [Friends] Search 200 OK; payload length={response?.Length ?? 0}");
                try
                {
                    var result = JsonUtility.FromJson<IVXModels.FriendSearchResponse>(response);
                    if (result?.status == true && result.data != null)
                    {
                        Log($"[{reqId}] [Friends] Search parsed OK; count={result.data.Count}");
                        onSuccess?.Invoke(result.data);
                    }
                    else
                    {
                        LogWarning($"[{reqId}] [Friends] Search empty/invalid. message='{result?.message}'");
                        onSuccess?.Invoke(new List<IVXModels.SearchUser>());
                    }
                }
                catch (Exception ex)
                {
                    LogError($"[{reqId}] [Friends] Search parse failed: {ex.Message}");
                    onError?.Invoke("Failed to parse search response.");
                }
            },
            error =>
            {
                LogError($"[{reqId}] [Friends] Search failed: {error}");
                onError?.Invoke(error);
            }
        );
    }

    /// <summary>
    /// Async version of SearchFriends.
    /// </summary>
    public static Task<List<IVXModels.SearchUser>> SearchFriendsAsync(
        string query,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<List<IVXModels.SearchUser>>();
        
        SearchFriends(
            query,
            users => tcs.TrySetResult(users ?? new List<IVXModels.SearchUser>()),
            error => tcs.TrySetException(new Exception(error ?? "Unknown error searching users"))
        );
        
        ct.Register(() => tcs.TrySetCanceled());
        return tcs.Task;
    }

    /// <summary>
    /// Sends a friend request/invite.
    /// </summary>
    /// <param name="receiverId">User ID to send invite to</param>
    /// <param name="onSuccess">Success callback with response</param>
    /// <param name="onError">Error callback</param>
    public static void SendFriendInvite(
        string receiverId,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string reqId = "FRI-INVITE-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        var session = GetCurrentSession();
        if (session == null)
        {
            LogError($"[{reqId}] [Friends] No session; user must sign in.");
            onError?.Invoke("Not signed in.");
            return;
        }

        if (string.IsNullOrWhiteSpace(receiverId))
        {
            LogError($"[{reqId}] [Friends] receiverId is required.");
            onError?.Invoke("Receiver ID is required.");
            return;
        }

        if (receiverId == session.userId)
        {
            LogError($"[{reqId}] [Friends] Cannot send invite to yourself.");
            onError?.Invoke("Cannot send friend request to yourself.");
            return;
        }

        var payload = new IVXModels.FriendInvitePayload(session.userId, receiverId);
        Log($"[{reqId}] [Friends] Invite POST → receiverId={receiverId}");

        _ = PostAuthenticatedRequestAsync(
            IVXURLs.SendFriendRequest,
            payload,
            response =>
            {
                Log($"[{reqId}] [Friends] Invite 200 OK; payload length={response?.Length ?? 0}");
                onSuccess?.Invoke(response);
            },
            error =>
            {
                LogError($"[{reqId}] [Friends] Invite failed: {error}");
                onError?.Invoke(error);
            }
        );
    }

    /// <summary>
    /// Async version of SendFriendInvite.
    /// </summary>
    public static Task<bool> SendFriendInviteAsync(
        string receiverId,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        SendFriendInvite(
            receiverId,
            _ => tcs.TrySetResult(true),
            error => tcs.TrySetException(new Exception(error))
        );
        
        ct.Register(() => tcs.TrySetCanceled());
        return tcs.Task;
    }

    /// <summary>
    /// Updates friend relationship status.
    /// </summary>
    /// <param name="relationId">The relation/request ID</param>
    /// <param name="newStatus">New status: accepted | rejected | blocked | cancelled</param>
    /// <param name="onSuccess">Success callback</param>
    /// <param name="onError">Error callback</param>
    public static void UpdateFriendStatus(
        string relationId,
        string newStatus,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string reqId = "FRI-STATUS-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        var session = GetCurrentSession();
        if (session == null)
        {
            LogError($"[{reqId}] [Friends] No session; user must sign in.");
            onError?.Invoke("Not signed in.");
            return;
        }

        if (string.IsNullOrWhiteSpace(relationId))
        {
            LogError($"[{reqId}] [Friends] relationId is required.");
            onError?.Invoke("Relation ID is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(newStatus))
        {
            LogError($"[{reqId}] [Friends] newStatus is required.");
            onError?.Invoke("Status is required.");
            return;
        }

        var payload = new IVXModels.FriendStatusUpdatePayload(session.userId, relationId, newStatus);
        Log($"[{reqId}] [Friends] Update PATCH → relationId={relationId}, status={newStatus}");

        _ = PatchAuthenticatedRequestAsync(
            IVXURLs.UpdateFriendStatus,
            payload,
            response =>
            {
                Log($"[{reqId}] [Friends] Update 200 OK; payload length={response?.Length ?? 0}");
                onSuccess?.Invoke(response);
            },
            error =>
            {
                LogError($"[{reqId}] [Friends] Update failed: {error}");
                onError?.Invoke(error);
            }
        );
    }

    /// <summary>
    /// Async version of UpdateFriendStatus.
    /// </summary>
    public static Task<bool> UpdateFriendStatusAsync(
        string relationId,
        string newStatus,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        UpdateFriendStatus(
            relationId,
            newStatus,
            _ => tcs.TrySetResult(true),
            error => tcs.TrySetException(new Exception(error))
        );
        
        ct.Register(() => tcs.TrySetCanceled());
        return tcs.Task;
    }

    /// <summary>
    /// Accept a friend request. Obsolete: Use IVXFriendsService.AcceptRequestAsync (Nakama native).
    /// </summary>
    [Obsolete("Use IVXFriendsService.AcceptRequestAsync (100% Nakama). HTTP friends API deprecated.")]
    public static Task<bool> AcceptFriendRequestAsync(string relationId, CancellationToken ct = default)
        => UpdateFriendStatusAsync(relationId, "accepted", ct);

    /// <summary>
    /// Reject a friend request. Obsolete: Use IVXFriendsService.RejectRequestAsync (Nakama native).
    /// </summary>
    [Obsolete("Use IVXFriendsService.RejectRequestAsync (100% Nakama). HTTP friends API deprecated.")]
    public static Task<bool> RejectFriendRequestAsync(string relationId, CancellationToken ct = default)
        => UpdateFriendStatusAsync(relationId, "rejected", ct);

    /// <summary>
    /// Remove a friend. Obsolete: Use IVXFriendsService.RemoveFriendAsync (Nakama native).
    /// </summary>
    [Obsolete("Use IVXFriendsService.RemoveFriendAsync (100% Nakama). HTTP friends API deprecated.")]
    public static Task<bool> RemoveFriendAsync(string relationId, CancellationToken ct = default)
        => UpdateFriendStatusAsync(relationId, "cancelled", ct);

    /// <summary>
    /// Block a user. Obsolete: Use IVXFriendsService.BlockUserAsync (Nakama native).
    /// </summary>
    [Obsolete("Use IVXFriendsService.BlockUserAsync (100% Nakama). HTTP friends API deprecated.")]
    public static Task<bool> BlockUserAsync(string relationId, CancellationToken ct = default)
        => UpdateFriendStatusAsync(relationId, "blocked", ct);

    /// <summary>
    /// Get pending incoming friend requests. Obsolete: Use IVXFriendsService.GetIncomingRequestsAsync (Nakama).
    /// </summary>
    [Obsolete("Use IVXFriendsService.GetIncomingRequestsAsync (100% Nakama). HTTP friends API deprecated.")]
    public static Task<List<IVXModels.FriendData>> GetIncomingRequestsAsync(CancellationToken ct = default)
        => GetFriendsAsync(null, "pending", null, ct);

    /// <summary>
    /// Get accepted friends list. Obsolete: Use IVXFriendsService.GetFriendsAsync (Nakama native).
    /// </summary>
    [Obsolete("Use IVXFriendsService.GetFriendsAsync (100% Nakama). HTTP friends API deprecated.")]
    public static Task<List<IVXModels.FriendData>> GetAcceptedFriendsAsync(CancellationToken ct = default)
        => GetFriendsAsync(null, "accepted", null, ct);

    #endregion

    // ========================================================================
    // GUEST CONVERSION API (WITH OTP)
    // ========================================================================

    #region Guest Conversion

    /// <summary>
    /// Initiate guest conversion with OTP verification.
    /// </summary>
    public static async Task<IVXModels.GuestConvertInitiateResponse> InitiateGuestConversionAsync(
        string guestUserId,
        string email,
        string password,
        string userName,
        CancellationToken ct = default)
    {
        string reqId = "GUEST-INIT-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        if (string.IsNullOrWhiteSpace(guestUserId))
            throw new ArgumentException("Guest user ID is required.", nameof(guestUserId));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username is required.", nameof(userName));

        var request = new IVXModels.GuestConvertInitiateRequest
        {
            guestUserId = guestUserId,
            email = email.Trim(),
            password = password,
            userName = userName.Trim()
        };

        Log($"[{reqId}] [Guest] Convert initiate POST → {IVXURLs.ConvertGuest_V2_Init}");

        var tcs = new TaskCompletionSource<IVXModels.GuestConvertInitiateResponse>();

        await PostAuthenticatedRequestAsync(
            IVXURLs.ConvertGuest_V2_Init,
            request,
            response =>
            {
                try
                {
                    var result = JsonUtility.FromJson<IVXModels.GuestConvertInitiateResponse>(response);
                    Log($"[{reqId}] [Guest] Convert initiate OK: status={result?.status}");
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    LogError($"[{reqId}] [Guest] Convert initiate parse error: {ex.Message}");
                    tcs.TrySetException(ex);
                }
            },
            error =>
            {
                LogError($"[{reqId}] [Guest] Convert initiate failed: {error}");
                tcs.TrySetException(new Exception(error));
            }
        );

        ct.Register(() => tcs.TrySetCanceled());
        return await tcs.Task;
    }

    /// <summary>
    /// Confirm guest conversion with OTP code.
    /// </summary>
    public static async Task<IVXModels.GuestConvertConfirmResponse> ConfirmGuestConversionAsync(
        string guestUserId,
        string email,
        string password,
        string userName,
        string otp,
        CancellationToken ct = default)
    {
        string reqId = "GUEST-CONFIRM-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        if (string.IsNullOrWhiteSpace(otp))
            throw new ArgumentException("OTP code is required.", nameof(otp));

        var request = new IVXModels.GuestConvertConfirmRequest
        {
            guestUserId = guestUserId,
            email = email.Trim(),
            password = password,
            userName = userName.Trim(),
            otp = otp.Trim()
        };

        Log($"[{reqId}] [Guest] Convert confirm POST → {IVXURLs.ConvertGuest_V2_Confirm}");

        var tcs = new TaskCompletionSource<IVXModels.GuestConvertConfirmResponse>();

        await PostAuthenticatedRequestAsync(
            IVXURLs.ConvertGuest_V2_Confirm,
            request,
            response =>
            {
                try
                {
                    var result = JsonUtility.FromJson<IVXModels.GuestConvertConfirmResponse>(response);
                    Log($"[{reqId}] [Guest] Convert confirm OK: status={result?.status}");
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    LogError($"[{reqId}] [Guest] Convert confirm parse error: {ex.Message}");
                    tcs.TrySetException(ex);
                }
            },
            error =>
            {
                LogError($"[{reqId}] [Guest] Convert confirm failed: {error}");
                tcs.TrySetException(new Exception(error));
            }
        );

        ct.Register(() => tcs.TrySetCanceled());
        return await tcs.Task;
    }

    #endregion

    // ========================================================================
    // REFERRAL STATS & CLAIM
    // ========================================================================

    #region Referral Stats & Claim

    // === Endpoints ===
    public static string ReferralStatsEndpoint = "https://api.intelli-verse-x.ai/api/user/referral/stats";
    public static string ClaimReferralRewardsEndpoint = "https://api.intelli-verse-x.ai/api/user/referral/claim";

    // === DTOs ===
    [Serializable]
    public class ReferralStatsData
    {
        public int totalReferrals;
        public int completedReferrals;
        public int pendingReferrals;
        public int expiredReferrals;
        public ReferralItem[] referrals;
    }

    [Serializable]
    public class ReferralItem
    {
        public string id;
        public string referredUserId;
        public string referredUserName;
        public string referredUserEmail;
        public string status;
        public string createdAt;
        public string completedAt;
        public int rewardAmount;
        public string rewardCurrency;
    }

    [Serializable]
    public class ReferralStatsResponse
    {
        public bool status;
        public string message;
        public ReferralStatsData data;
        public object other;
    }

    [Serializable]
    public class ClaimReferralRewardsRequest
    {
        public string[] referralIds;
    }

    [Serializable]
    public class ClaimReferralRewardsData
    {
        public int totalClaimed;
        public int totalRewardAmount;
        public string rewardCurrency;
        public string[] claimedReferralIds;
    }

    [Serializable]
    public class ClaimReferralRewardsResponse
    {
        public bool status;
        public string message;
        public ClaimReferralRewardsData data;
        public object other;
    }

    /// <summary>
    /// Get referral statistics for the current user.
    /// Returns total, completed, pending, and expired referral counts.
    /// </summary>
    public static async Task<ReferralStatsResponse> GetReferralStatsAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        PrintCurl("GET", ReferralStatsEndpoint,
            new Dictionary<string, string> {
                { "accept", "application/json" },
                { "Authorization", "Bearer ****" }
            },
            null);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (referral-stats) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = UnityWebRequest.Get(ReferralStatsEndpoint))
            {
                try
                {
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login.");

                    req.SetRequestHeader("accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] GET {ReferralStatsEndpoint} (referral/stats)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";

                    Log($"[HTTP] ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        ReferralStatsResponse resp = null;
                        try { resp = JsonUtility.FromJson<ReferralStatsResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }

                        if (resp == null)
                            resp = new ReferralStatsResponse { status = true, message = "OK", data = new ReferralStatsData() };

                        return resp;
                    }

                    // Handle 401 with token refresh
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on referral-stats – attempting token refresh...");
                        try
                        {
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null; // Use refreshed token
                            continue;
                        }
                        catch (Exception ex) { Debug.LogWarning($"[APIManager] Token refresh failed: {ex.Message}"); }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception($"Referral stats failed: HTTP {code} - {text}");
                }
                catch (OperationCanceledException) { throw; }
                catch (TimeoutException tex) { lastErr = tex; if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown referral stats error");
    }

    /// <summary>
    /// Claim referral rewards.
    /// Pass null or empty array to claim all completed referrals.
    /// </summary>
    public static async Task<ClaimReferralRewardsResponse> ClaimReferralRewardsAsync(
        string[] referralIds = null,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        var requestBody = new ClaimReferralRewardsRequest { referralIds = referralIds ?? new string[0] };
        string jsonBody = JsonUtility.ToJson(requestBody);

        PrintCurl("POST", ClaimReferralRewardsEndpoint,
            new Dictionary<string, string> {
                { "Content-Type", "application/json" },
                { "Authorization", "Bearer ****" }
            },
            jsonBody);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (referral-claim) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(ClaimReferralRewardsEndpoint, "POST"))
            {
                try
                {
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login.");

                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                    req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {ClaimReferralRewardsEndpoint} (referral/claim)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";

                    Log($"[HTTP] ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        ClaimReferralRewardsResponse resp = null;
                        try { resp = JsonUtility.FromJson<ClaimReferralRewardsResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }

                        if (resp == null)
                            resp = new ClaimReferralRewardsResponse { status = true, message = "OK", data = new ClaimReferralRewardsData() };

                        return resp;
                    }

                    // Handle 401 with token refresh
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on referral-claim – attempting token refresh...");
                        try
                        {
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null;
                            continue;
                        }
                        catch (Exception ex) { Debug.LogWarning($"[APIManager] Token refresh failed: {ex.Message}"); }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception($"Claim rewards failed: HTTP {code} - {text}");
                }
                catch (OperationCanceledException) { throw; }
                catch (TimeoutException tex) { lastErr = tex; if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown claim rewards error");
    }

    #endregion

    // ========================================================================
    // AI AVATAR GENERATION
    // ========================================================================

    #region AI Avatar Generation

    // === Endpoint ===
    public static string EnhanceImageEndpoint = "https://ai.intelli-verse-x.ai/api/ai/ai-enhancement/images/enhance";

    // === DTOs (using existing IVXModels.EnhanceImageRequest/Response) ===

    /// <summary>
    /// Generate AI avatar from text prompt.
    /// Uses AI service to create avatar images.
    /// </summary>
    public static async Task<IVXModels.EnhanceImageResponse> GenerateAvatarFromPromptAsync(
        string prompt,
        string[] tags = null,
        string model = "gpt-image-1",
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt is required", nameof(prompt));

        var request = new IVXModels.EnhanceImageRequest
        {
            imageUrls = new string[0],
            type = "user",
            model = model,
            prompt = prompt,
            isAiPrompt = true,
            tags = tags ?? new string[0],
            id = "866"
        };

        return await EnhanceImageInternalAsync(request, bearerToken, ct);
    }

    /// <summary>
    /// Enhance existing images with AI.
    /// </summary>
    public static async Task<IVXModels.EnhanceImageResponse> EnhanceImagesAsync(
        string[] imageUrls,
        string prompt = null,
        string[] tags = null,
        string model = "gpt-image-1",
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (imageUrls == null || imageUrls.Length == 0)
            throw new ArgumentException("At least one image URL is required", nameof(imageUrls));

        var request = new IVXModels.EnhanceImageRequest
        {
            imageUrls = imageUrls,
            type = "user",
            model = model,
            prompt = prompt ?? string.Empty,
            isAiPrompt = !string.IsNullOrEmpty(prompt),
            tags = tags ?? new string[0],
            id = "866"
        };

        return await EnhanceImageInternalAsync(request, bearerToken, ct);
    }

    /// <summary>
    /// Internal implementation for image enhancement/avatar generation.
    /// </summary>
    private static async Task<IVXModels.EnhanceImageResponse> EnhanceImageInternalAsync(
        IVXModels.EnhanceImageRequest request,
        string bearerToken,
        CancellationToken ct)
    {
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        string jsonBody = JsonUtility.ToJson(request);

        PrintCurl("POST", EnhanceImageEndpoint,
            new Dictionary<string, string> {
                { "Content-Type", "application/json" },
                { "Authorization", "Bearer ****" }
            },
            jsonBody);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (enhance-image) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(EnhanceImageEndpoint, "POST"))
            {
                try
                {
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login.");

                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                    req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = 60; // Longer timeout for AI generation

                    Log($"[HTTP] POST {EnhanceImageEndpoint} (enhance-image)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > 60)
                        {
                            req.Abort();
                            throw new TimeoutException("AI generation timed out after 60s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";

                    Log($"[HTTP] ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        IVXModels.EnhanceImageResponse resp = null;
                        try { resp = JsonUtility.FromJson<IVXModels.EnhanceImageResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }

                        if (resp == null)
                            resp = new IVXModels.EnhanceImageResponse { status = true, message = "OK" };

                        return resp;
                    }

                    // Handle 401 with token refresh
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on enhance-image – attempting token refresh...");
                        try
                        {
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null;
                            continue;
                        }
                        catch (Exception ex) { Debug.LogWarning($"[APIManager] Token refresh failed: {ex.Message}"); }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception($"Image enhancement failed: HTTP {code} - {text}");
                }
                catch (OperationCanceledException) { throw; }
                catch (TimeoutException tex) { lastErr = tex; if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown image enhancement error");
    }

    #endregion

}
