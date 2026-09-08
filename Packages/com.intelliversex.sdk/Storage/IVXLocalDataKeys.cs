namespace IntelliVerseX.Storage
{
    /// <summary>
    /// Canonical PlayerPrefs / secure-storage keys used by the SDK.
    /// Keep this list complete so wipe, GDPR delete, and migrations stay consistent.
    /// </summary>
    public static class IVXLocalDataKeys
    {
        // ── Auth user session (JSON blob via UserSessionManager) ─────────────
        public const string UserSession = "ivx_user_session";

        // ── Remember-me / login UX (non-secret preference + last email) ───────
        public const string RememberMe = "IVX_auth.remember";
        public const string RememberMeLegacy = "auth.remember";
        public const string LastEmail = "IVX_auth.last_email";
        public const string PersistFlag = "IVX_auth.persisted";
        public const string AuthUserId = "IVX_auth.user_id";
        public const string AuthLoginType = "IVX_auth.login_type";
        public const string SavedEmailLegacy = "IVX_SavedEmail";

        // ── Core identity mirrors (IntelliVerseXIdentity) ─────────────────────
        public const string Username = "IVX_Username";
        public const string DeviceId = "IVX_DeviceId";
        public const string GameId = "IVX_GameId";
        public const string GameWalletId = "IVX_GameWalletId";
        public const string GlobalWalletId = "IVX_GlobalWalletId";
        public const string GameWalletBalance = "IVX_GameWalletBalance";
        public const string GlobalWalletBalance = "IVX_GlobalWalletBalance";
        public const string GameWalletCurrency = "IVX_GameWalletCurrency";
        public const string GlobalWalletCurrency = "IVX_GlobalWalletCurrency";
        public const string CognitoUserId = "IVX_CognitoUserId";
        public const string Email = "IVX_Email";
        public const string IdpUsername = "IVX_IdpUsername";
        public const string FirstName = "IVX_FirstName";
        public const string LastName = "IVX_LastName";
        public const string WalletAddress = "IVX_WalletAddress";
        public const string Role = "IVX_Role";
        public const string IsAdult = "IVX_IsAdult";
        public const string LoginType = "IVX_LoginType";
        public const string AccountStatus = "IVX_AccountStatus";
        public const string KycStatus = "IVX_KycStatus";
        public const string IsGuest = "IVX_IsGuestUser";
        public const string GuestCreatedEpoch = "IVX_GuestCreatedEpoch";
        public const string AccessToken = "IVX_AccessToken";
        public const string IdToken = "IVX_IdToken";
        public const string RefreshToken = "IVX_RefreshToken";
        public const string AccessTokenExpiry = "IVX_AccessTokenExpiry";
        public const string IdentityMigrated = "IVX_IdentityMigratedV1";

        // ── Nakama sessions ──────────────────────────────────────────────────
        public const string BootstrapNakamaSession = "IVX_SessionToken";
        public const string NakamaAuthTokenLegacy = "nakama_auth_token";
        public const string NakamaRefreshTokenLegacy = "nakama_refresh_token";
        public const string NakamaAuthTokenV2 = "ivxn.nakama.auth_token";
        public const string NakamaRefreshTokenV2 = "ivxn.nakama.refresh_token";

        // ── Storage meta ─────────────────────────────────────────────────────
        public const string StorageVersion = "IVX_STORAGE_VERSION";

        /// <summary>Keys cleared on logout / ClearAuthSession (tokens + session, keep device/game id).</summary>
        public static readonly string[] AuthSessionKeys =
        {
            UserSession,
            PersistFlag,
            AuthUserId,
            AuthLoginType,
            AccessToken,
            IdToken,
            RefreshToken,
            AccessTokenExpiry,
            BootstrapNakamaSession,
            NakamaAuthTokenLegacy,
            NakamaRefreshTokenLegacy,
            NakamaAuthTokenV2,
            NakamaRefreshTokenV2,
            CognitoUserId,
            Email,
            IdpUsername,
            FirstName,
            LastName,
            Username,
            WalletAddress,
            Role,
            IsAdult,
            LoginType,
            AccountStatus,
            KycStatus,
            IsGuest,
            GuestCreatedEpoch,
            GameWalletId,
            GlobalWalletId,
            GameWalletBalance,
            GlobalWalletBalance,
            GameWalletCurrency,
            GlobalWalletCurrency,
        };

        /// <summary>Remember-me UX keys (optional clear on full wipe or when remember is off).</summary>
        public static readonly string[] RememberMeKeys =
        {
            RememberMe,
            RememberMeLegacy,
            LastEmail,
            SavedEmailLegacy,
        };

        /// <summary>All SDK local keys including device/game id and storage version (full wipe).</summary>
        public static readonly string[] AllSdkKeys =
        {
            UserSession,
            RememberMe,
            RememberMeLegacy,
            LastEmail,
            PersistFlag,
            AuthUserId,
            AuthLoginType,
            SavedEmailLegacy,
            Username,
            DeviceId,
            GameId,
            GameWalletId,
            GlobalWalletId,
            GameWalletBalance,
            GlobalWalletBalance,
            GameWalletCurrency,
            GlobalWalletCurrency,
            CognitoUserId,
            Email,
            IdpUsername,
            FirstName,
            LastName,
            WalletAddress,
            Role,
            IsAdult,
            LoginType,
            AccountStatus,
            KycStatus,
            IsGuest,
            GuestCreatedEpoch,
            AccessToken,
            IdToken,
            RefreshToken,
            AccessTokenExpiry,
            IdentityMigrated,
            BootstrapNakamaSession,
            NakamaAuthTokenLegacy,
            NakamaRefreshTokenLegacy,
            NakamaAuthTokenV2,
            NakamaRefreshTokenV2,
            StorageVersion,
        };

        /// <summary>Plaintext keys that must be migrated into IVXSecureStorage.</summary>
        public static readonly string[] PlaintextMigrationKeys =
        {
            BootstrapNakamaSession,
            DeviceId,
            LastEmail,
            SavedEmailLegacy,
            NakamaAuthTokenLegacy,
            NakamaRefreshTokenLegacy,
            NakamaAuthTokenV2,
            NakamaRefreshTokenV2,
        };
    }
}
