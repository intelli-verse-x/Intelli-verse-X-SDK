using System;
using System.IO;
using System.Text;
using IntelliVerseX.Core;
using IntelliVerseX.Storage;
using UnityEngine;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Preferred facade for auth session storage. Backed by <see cref="UserSessionManager"/>
    /// + <see cref="IVXSecureStorage"/>.
    /// </summary>
    public static class IVXUserSession
    {
        public static UserSessionManager.UserSession Current
        {
            get => UserSessionManager.Current;
            set => UserSessionManager.Current = value;
        }

        public static string AccessToken => UserSessionManager.AccessToken;
        public static bool HasSession => UserSessionManager.HasSession;
        public static string SessionPath => UserSessionManager.SessionPath;
        public static bool RememberMe
        {
            get => UserSessionManager.RememberMe;
            set => UserSessionManager.RememberMe = value;
        }

        public static void Save(UserSessionManager.UserSession session) => UserSessionManager.Save(session);
        public static UserSessionManager.UserSession Load() => UserSessionManager.Load();
        public static void Clear() => UserSessionManager.Clear();
        public static void ClearAuthSession(bool clearRememberMe = false) =>
            UserSessionManager.ClearAuthSession(clearRememberMe);
        public static void ClearAllLocalData() => UserSessionManager.ClearAllLocalData();
        public static bool IsAccessTokenFresh(int skewSeconds = 60) =>
            UserSessionManager.IsAccessTokenFresh(skewSeconds);
        public static void ApplyLoginResponse(APIManager.LoginResponse resp, bool persist) =>
            UserSessionManager.ApplyLoginResponse(resp, persist);
    }
}

/// <summary>
/// Canonical auth session store. Persists encrypted JSON via <see cref="IVXSecureStorage"/>
/// under <see cref="IVXLocalDataKeys.UserSession"/>. Prefer <see cref="IntelliVerseX.Identity.IVXUserSession"/> in new code.
/// </summary>
public static class UserSessionManager
{
    private const string FileName = "user_session.json";
    private static readonly object _lock = new object();
    private static UserSession _cached;
    private static bool _temporaryOnly;

    [Serializable]
    public class UserSession
    {
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public long accessTokenExpiryEpoch;
        public int expiresIn;

        public string idpUsername;
        public string userId;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string role;
        public bool isAdult;
        public string loginType;
        public bool isGuest;

        public string walletAddress;
        public string fcmToken;
        public string kycStatus;
        public string accountStatus;
        public string createdAt;
        public string updatedAt;

        public DateTime SavedAtUtc;
    }

    /// <summary>Legacy on-disk path (pre–secure storage). Migration only.</summary>
    public static string SessionPath =>
        Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>True when session exists only in memory (remember-me off).</summary>
    public static bool IsTemporaryOnly
    {
        get { lock (_lock) return _temporaryOnly; }
    }

    public static bool RememberMe
    {
        get => IVXLocalData.GetRememberMe(true);
        set => IVXLocalData.SetRememberMe(value);
    }

    public static string LastEmail
    {
        get => IVXLocalData.GetLastEmail();
        set => IVXLocalData.SetLastEmail(value);
    }

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

    public static string AccessToken => Current?.accessToken;
    public static bool HasSession => Current != null && !string.IsNullOrWhiteSpace(Current.accessToken);

    public static void SaveFromLoginResponse(APIManager.LoginResponse resp) =>
        ApplyLoginResponse(resp, persist: true);

    public static void SetTemporaryFromLoginResponse(APIManager.LoginResponse resp) =>
        ApplyLoginResponse(resp, persist: false);

    /// <summary>
    /// Single login handoff: always sets runtime session; persists only when <paramref name="persist"/> is true.
    /// Also updates remember-me prefs and mirrors profile into <see cref="IntelliVerseXIdentity"/> when available.
    /// </summary>
    public static void ApplyLoginResponse(APIManager.LoginResponse resp, bool persist)
    {
        var session = CreateSessionFromLoginResponse(resp);
        if (persist)
        {
            RememberMe = true;
            if (!string.IsNullOrWhiteSpace(session.email))
                LastEmail = session.email;
            IVXLocalData.SetPersistFlag(true);
            IVXLocalData.SetAuthUserHint(session.userId, session.loginType);
            Save(session);
        }
        else
        {
            IVXLocalData.SetPersistFlag(false);
            SetTemporary(session);
        }

        TryMirrorIdentity(session);
    }

    public static void SaveFromGuestResponse(APIManager.GuestSignupResponse resp)
    {
        if (resp == null || resp.data == null || resp.data.user == null)
            throw new ArgumentException("Invalid guest-signup response to persist.");

        var asLogin = new APIManager.LoginResponse
        {
            status = resp.status,
            message = resp.message,
            data = resp.data
        };
        ApplyLoginResponse(asLogin, persist: true);
    }

    public static void Save(UserSession session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));

        IVXLocalData.EnsureInitialized();
        var json = JsonUtility.ToJson(session, prettyPrint: false);

        lock (_lock)
        {
            IVXSecureStorage.SetString(IVXLocalDataKeys.UserSession, json);
            IVXLocalData.DeleteLegacySessionFile(SessionPath);
            _cached = session;
            _temporaryOnly = false;
        }

        TryMirrorIdentity(session);
#if UNITY_EDITOR
        Debug.Log($"[UserSession] Saved to secure storage ({IVXLocalDataKeys.UserSession})");
#endif
    }

    public static UserSession Load()
    {
        lock (_lock)
        {
            _cached = LoadInternal();
            return _cached;
        }
    }

    public static void ClearPersisted() => Clear();

    /// <summary>Clears in-memory + persisted user session blob only.</summary>
    public static void Clear()
    {
        lock (_lock)
        {
            _cached = null;
            _temporaryOnly = false;
            IVXSecureStorage.DeleteKey(IVXLocalDataKeys.UserSession);
            IVXLocalData.DeleteLegacySessionFile(SessionPath);
        }
#if UNITY_EDITOR
        Debug.Log("[UserSession] Cleared.");
#endif
    }

    /// <summary>
    /// Logout-safe clear: user session, mirrored identity tokens/profile, Nakama token keys.
    /// Keeps device/game id. Optionally clears remember-me email.
    /// </summary>
    public static void ClearAuthSession(bool clearRememberMe = false)
    {
        Clear();
        IVXLocalData.ClearAuthRelatedKeys(clearRememberMe);
        try
        {
            IntelliVerseXIdentity.ClearUserData();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UserSession] Identity clear failed: {ex.Message}");
        }
#if UNITY_EDITOR
        Debug.Log("[UserSession] Auth session + related local tokens cleared.");
#endif
    }

    /// <summary>
    /// Full SDK local wipe (editor tool / GDPR). Clears registered keys + legacy session file + memory.
    /// </summary>
    public static void ClearAllLocalData()
    {
        lock (_lock)
        {
            _cached = null;
            _temporaryOnly = false;
        }

        IVXLocalData.DeleteLegacySessionFile(SessionPath);
        IVXLocalData.ClearAllRegisteredKeys();

        try
        {
            IntelliVerseXIdentity.ClearUserData();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UserSession] Identity clear during full wipe failed: {ex.Message}");
        }

        Debug.Log("[UserSession] All registered SDK local data cleared.");
    }

    public static void SetTemporary(UserSession session)
    {
        lock (_lock)
        {
            _cached = session;
            _temporaryOnly = session != null;
        }

        // Ensure a previous persisted session cannot outlive "remember me" off.
        IVXSecureStorage.DeleteKey(IVXLocalDataKeys.UserSession);
        IVXLocalData.DeleteLegacySessionFile(SessionPath);
        IVXLocalData.SetPersistFlag(false);

        TryMirrorIdentity(session);
#if UNITY_EDITOR
        Debug.Log($"[UserSession] Temporary session set (not persisted): {session?.userId ?? "null"}");
#endif
    }

    public static bool IsAccessTokenFresh(int skewSeconds = 60)
    {
        var c = Current;
        if (c == null || c.accessTokenExpiryEpoch <= 0) return false;
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return now + skewSeconds < c.accessTokenExpiryEpoch;
    }

    /// <summary>
    /// Startup helper: load persisted session only when remember-me / persist flag allows it.
    /// </summary>
    public static UserSession TryRestorePersistedSession()
    {
        IVXLocalData.EnsureInitialized();
        if (!RememberMe && !IVXLocalData.GetPersistFlag())
        {
            // Stale disk session with remember-me off — drop it.
            if (IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession))
                Clear();
            return null;
        }

        return Load();
    }

    private static void TryMirrorIdentity(UserSession session)
    {
        if (session == null)
            return;

        try
        {
            var existing = IntelliVerseXIdentity.GetUser();
            var mapped = new IntelliVerseXUser
            {
                Username = session.userName ?? existing?.Username ?? session.firstName ?? string.Empty,
                DeviceId = existing?.DeviceId ?? string.Empty,
                GameId = existing?.GameId ?? string.Empty,
                CognitoUserId = !string.IsNullOrWhiteSpace(session.idpUsername)
                    ? session.idpUsername
                    : (session.userId ?? string.Empty),
                Email = session.email ?? string.Empty,
                IdpUsername = session.idpUsername ?? string.Empty,
                FirstName = session.firstName ?? string.Empty,
                LastName = session.lastName ?? string.Empty,
                AccessToken = session.accessToken ?? string.Empty,
                IdToken = session.idToken ?? string.Empty,
                RefreshToken = session.refreshToken ?? string.Empty,
                AccessTokenExpiryEpoch = session.accessTokenExpiryEpoch,
                GameWalletId = existing?.GameWalletId ?? string.Empty,
                GlobalWalletId = existing?.GlobalWalletId ?? string.Empty,
                GameWalletBalance = existing?.GameWalletBalance ?? 0,
                GlobalWalletBalance = existing?.GlobalWalletBalance ?? 0,
                GameWalletCurrency = string.IsNullOrWhiteSpace(existing?.GameWalletCurrency)
                    ? "coins"
                    : existing.GameWalletCurrency,
                GlobalWalletCurrency = string.IsNullOrWhiteSpace(existing?.GlobalWalletCurrency)
                    ? "gems"
                    : existing.GlobalWalletCurrency,
                WalletAddress = session.walletAddress ?? string.Empty,
                Role = session.role ?? "user",
                IsAdult = session.isAdult ? "True" : "False",
                LoginType = session.loginType ?? "email",
                AccountStatus = session.accountStatus ?? string.Empty,
                KycStatus = session.kycStatus ?? string.Empty,
                IsGuestUser = session.isGuest,
                GuestCreatedEpoch = existing?.GuestCreatedEpoch ?? 0
            };

            // Only write when identity system has been initialized (avoids null-instance spam).
            if (IntelliVerseXIdentity.Instance != null || existing != null)
                IntelliVerseXIdentity.SetCurrentUser(mapped);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UserSession] Identity mirror skipped: {ex.Message}");
        }
    }

    private static UserSession CreateSessionFromLoginResponse(APIManager.LoginResponse resp)
    {
        if (resp == null || resp.data == null || resp.data.user == null)
            throw new ArgumentException("Invalid login response to persist.");

        var d = resp.data;
        var u = d.user;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return new UserSession
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
            isGuest = false,

            walletAddress = u.walletAddress,
            fcmToken = u.fcmToken,
            kycStatus = u.kycStatus,
            accountStatus = u.accountStatus,
            createdAt = u.createdAt,
            updatedAt = u.updatedAt,

            SavedAtUtc = DateTime.UtcNow
        };
    }

    private static UserSession LoadInternal()
    {
        try
        {
            IVXLocalData.EnsureInitialized();

            string json = IVXSecureStorage.GetString(IVXLocalDataKeys.UserSession, "");
            if (string.IsNullOrWhiteSpace(json) && File.Exists(SessionPath))
            {
                json = File.ReadAllText(SessionPath, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    IVXSecureStorage.SetString(IVXLocalDataKeys.UserSession, json);
                    IVXLocalData.DeleteLegacySessionFile(SessionPath);
                }
            }

            if (string.IsNullOrWhiteSpace(json))
                return null;

            UserSession session = JsonUtility.FromJson<UserSession>(json);
            if (!IsPlausiblePersistedSession(session))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("[UserSession] Persisted session rejected (empty or invalid shape); clearing.");
#else
                Debug.LogWarning("[UserSession] Saved login state was invalid and was cleared.");
#endif
                Clear();
                return null;
            }

            _temporaryOnly = false;
            return session;
        }
        catch (Exception e)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[UserSession] Load failed (corrupt JSON); clearing session. " + e.Message);
#else
            Debug.LogWarning("[UserSession] Load failed; saved session cleared.");
#endif
            Clear();
            return null;
        }
    }

    private static bool IsPlausiblePersistedSession(UserSession s)
    {
        if (s == null)
            return false;

        if (!string.IsNullOrWhiteSpace(s.accessToken))
            return true;
        if (!string.IsNullOrWhiteSpace(s.idToken))
            return true;
        if (!string.IsNullOrWhiteSpace(s.refreshToken))
            return true;
        if (!string.IsNullOrWhiteSpace(s.userId))
            return true;
        if (!string.IsNullOrWhiteSpace(s.email))
            return true;
        if (!string.IsNullOrWhiteSpace(s.userName))
            return true;
        if (!string.IsNullOrWhiteSpace(s.idpUsername))
            return true;

        return false;
    }
}
