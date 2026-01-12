using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Centralized storage for the entire auth payload so you can reference
/// refresh/access/id tokens and user details anywhere.
/// Swap the storage impl with your BinaryDataManager if you prefer.
/// </summary>
public static class UserSessionManager
{
    private const string FileName = "user_session.json";
    private static readonly object _lock = new object();
    private static UserSession _cached;

    [Serializable]
    public class UserSession
    {
        // Tokens
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public long accessTokenExpiryEpoch;   // when access token expires (epoch seconds)
        public int expiresIn;                 // server-provided seconds

        // Identity
        public string idpUsername;
        public string userId;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string role;
        public bool isAdult;
        public string loginType;

        public bool isGuest { get; set; } = false;

        // Misc (keep as raw strings)
        public string walletAddress;
        public string fcmToken;
        public string kycStatus;
        public string accountStatus;
        public string createdAt;
        public string updatedAt;

        // For quick checks
        public DateTime SavedAtUtc;
    }

    public static string SessionPath =>
        Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>
    /// Gets or sets the current user session.
    /// Setting this will update the in-memory cache but NOT persist to disk.
    /// Use Save() to persist the session to disk.
    /// </summary>
    public static UserSession Current
    {
        get
        {
            lock (_lock)
            {
                if (_cached != null) return _cached;
                _cached = LoadInternal();
                return _cached;
            }
        }
        set
        {
            lock (_lock)
            {
                _cached = value;
            }
        }
    }

    // Convenience accessors
    public static string AccessToken => Current?.accessToken;
    public static bool HasSession => Current != null && !string.IsNullOrWhiteSpace(Current.accessToken);

    /// <summary>
    /// Creates a session from a LoginResponse and persists it to disk.
    /// </summary>
    public static void SaveFromLoginResponse(APIManager.LoginResponse resp)
    {
        if (resp == null || resp.data == null || resp.data.user == null)
            throw new ArgumentException("Invalid login response to persist.");

        var d = resp.data;
        var u = d.user;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var session = new UserSession
        {
            accessToken = !string.IsNullOrWhiteSpace(d.accessToken) ? d.accessToken : d.token,
            idToken = d.idToken,
            refreshToken = d.refreshToken,
            expiresIn = d.expiresIn,
            accessTokenExpiryEpoch = now + Math.Max(0, d.expiresIn <= 0 ? 1800 : d.expiresIn),

            idpUsername = u.idpUsername,
            userId = u.id,
            firstName = u.firstName,
            lastName = u.lastName,
            userName = u.userName,
            email = u.email,
            role = u.role,
            isAdult = u.isAdult,
            loginType = u.loginType,

            walletAddress = u.walletAddress,
            fcmToken = u.fcmToken,
            kycStatus = u.kycStatus,
            accountStatus = u.accountStatus,
            createdAt = u.createdAt,
            updatedAt = u.updatedAt,

            SavedAtUtc = DateTime.UtcNow
        };

        Save(session);
    }

    /// <summary>
    /// Saves the given session to disk and updates the in-memory cache.
    /// </summary>
    public static void Save(UserSession session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));

        var json = JsonUtility.ToJson(session, prettyPrint: false);

        lock (_lock)
        {
            // Atomic write
            var tmp = SessionPath + ".tmp";
            File.WriteAllText(tmp, json, Encoding.UTF8);
            if (File.Exists(SessionPath)) File.Delete(SessionPath);
            File.Move(tmp, SessionPath);
            _cached = session;
        }
#if UNITY_EDITOR
        Debug.Log($"[UserSession] Saved to {SessionPath}");
#endif
    }

    /// <summary>
    /// Loads the session from disk and returns it (also updates the cache).
    /// </summary>
    public static UserSession Load()
    {
        lock (_lock)
        {
            _cached = LoadInternal();
            return _cached;
        }
    }

    /// <summary>
    /// Creates a session from a GuestSignupResponse and persists it to disk.
    /// </summary>
    public static void SaveFromGuestResponse(APIManager.GuestSignupResponse resp)
    {
        if (resp == null || resp.data == null || resp.data.user == null)
            throw new ArgumentException("Invalid guest-signup response to persist.");

        // Reuse existing logic by adapting to LoginResponse shape
        var asLogin = new APIManager.LoginResponse
        {
            status = resp.status,
            message = resp.message,
            data = resp.data
        };
        SaveFromLoginResponse(asLogin);
    }

    /// <summary>
    /// Clears the persisted session file and the in-memory cache.
    /// </summary>
    public static void ClearPersisted()
    {
        // Alias for clarity—clears the cached session and the persisted JSON file.
        Clear();
    }

    /// <summary>
    /// Clears the in-memory session cache and deletes the persisted file.
    /// </summary>
    public static void Clear()
    {
        lock (_lock)
        {
            _cached = null;
            if (File.Exists(SessionPath)) File.Delete(SessionPath);
        }
#if UNITY_EDITOR
        Debug.Log("[UserSession] Cleared.");
#endif
    }

    /// <summary>
    /// Sets the current session in memory without persisting to disk.
    /// Useful for temporary sessions (e.g., guest before full signup).
    /// Call Save() afterward if you want to persist it.
    /// </summary>
    public static void SetTemporary(UserSession session)
    {
        lock (_lock)
        {
            _cached = session;
        }
#if UNITY_EDITOR
        Debug.Log($"[UserSession] Temporary session set (not persisted): {session?.userId ?? "null"}");
#endif
    }

    /// <summary>
    /// Checks if the access token is still fresh (not expired).
    /// </summary>
    public static bool IsAccessTokenFresh(int skewSeconds = 60)
    {
        var c = Current;
        if (c == null || c.accessTokenExpiryEpoch <= 0) return false;
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return now + skewSeconds < c.accessTokenExpiryEpoch;
    }

    private static UserSession LoadInternal()
    {
        try
        {
            if (!File.Exists(SessionPath)) return null;
            var json = File.ReadAllText(SessionPath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonUtility.FromJson<UserSession>(json);
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError("[UserSession] Load failed: " + e.Message);
#endif
            return null;
        }
    }
}