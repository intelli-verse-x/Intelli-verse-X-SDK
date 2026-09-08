using System;
using System.IO;
using IntelliVerseX.Storage;
using NUnit.Framework;
using UnityEngine;

namespace IntelliVerseX.Tests.Editor
{
    /// <summary>
    /// World-class EditMode coverage for session + local data:
    /// IVXSecureStorage, UserSessionManager, remember-me, wipe catalog, migrations.
    /// Run: <c>unity test SDKs/unity/editor --mode EditMode</c>
    /// Filter: <c>--filter IVX.Session</c> (when supported by runner) or Test Runner category IVX.Session.
    /// </summary>
    [TestFixture]
    [Category("IVX.Session")]
    public class IVXSessionLocalDataTests : IVXLocalDataTestFixture
    {
        [Test]
        public void SecureStorage_RoundTripsStringEncrypted()
        {
            const string key = IVXLocalDataKeys.LastEmail;
            const string value = "secret-email@example.com";

            IVXSecureStorage.SetString(key, value);

            Assert.IsTrue(PlayerPrefs.HasKey(key), "Value should be stored under PlayerPrefs key");
            string raw = PlayerPrefs.GetString(key, "");
            Assert.AreNotEqual(value, raw, "Raw PlayerPrefs must not store plaintext secrets");
            Assert.AreEqual(value, IVXSecureStorage.GetString(key, ""), "Decrypt round-trip failed");
        }

        [Test]
        public void SecureStorage_DeleteKey_RemovesValue()
        {
            IVXSecureStorage.SetString(IVXLocalDataKeys.LastEmail, "x@y.z");
            IVXSecureStorage.DeleteKey(IVXLocalDataKeys.LastEmail);
            Assert.IsFalse(IVXSecureStorage.HasKey(IVXLocalDataKeys.LastEmail));
            Assert.AreEqual("", IVXSecureStorage.GetString(IVXLocalDataKeys.LastEmail, ""));
        }

        [Test]
        public void RememberMe_DefaultsTrue_AndPersistsToggle()
        {
            Assert.IsTrue(IVXLocalData.GetRememberMe(true));

            IVXLocalData.SetRememberMe(false);
            Assert.IsFalse(IVXLocalData.GetRememberMe(true));
            Assert.IsFalse(UserSessionManager.RememberMe);

            IVXLocalData.SetRememberMe(true);
            Assert.IsTrue(UserSessionManager.RememberMe);
        }

        [Test]
        public void RememberMe_Off_ClearsLastEmail()
        {
            IVXLocalData.SetLastEmail("keep@example.com");
            Assert.AreEqual("keep@example.com", IVXLocalData.GetLastEmail());

            IVXLocalData.SetRememberMe(false);
            Assert.AreEqual("", IVXLocalData.GetLastEmail());
        }

        [Test]
        public void LastEmail_MigratesFromLegacyPlaintextPrefs()
        {
            PlayerPrefs.SetString(IVXLocalDataKeys.SavedEmailLegacy, "legacy@example.com");
            PlayerPrefs.Save();

            Assert.AreEqual("legacy@example.com", IVXLocalData.GetLastEmail());
            Assert.AreEqual("legacy@example.com", IVXSecureStorage.GetString(IVXLocalDataKeys.LastEmail, ""));
            Assert.IsFalse(PlayerPrefs.HasKey(IVXLocalDataKeys.SavedEmailLegacy));
        }

        [Test]
        public void ApplyLogin_PersistTrue_WritesSecureSessionAndRememberMe()
        {
            var resp = MakeLoginResponse();
            UserSessionManager.ApplyLoginResponse(resp, persist: true);

            Assert.IsTrue(UserSessionManager.HasSession);
            Assert.IsFalse(UserSessionManager.IsTemporaryOnly);
            Assert.AreEqual("access-token-test", UserSessionManager.AccessToken);
            Assert.IsTrue(UserSessionManager.RememberMe);
            Assert.AreEqual("qa@intelliversex.test", UserSessionManager.LastEmail);
            Assert.IsTrue(IVXLocalData.GetPersistFlag());
            Assert.IsTrue(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));

            string raw = PlayerPrefs.GetString(IVXLocalDataKeys.UserSession, "");
            StringAssert.DoesNotContain("access-token-test", raw);
        }

        [Test]
        public void ApplyLogin_PersistFalse_KeepsMemoryOnly_AndClearsDiskSession()
        {
            UserSessionManager.ApplyLoginResponse(MakeLoginResponse(), persist: true);
            Assert.IsTrue(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));

            UserSessionManager.ApplyLoginResponse(
                MakeLoginResponse(email: "temp@example.com", access: "temp-access"),
                persist: false);

            Assert.IsTrue(UserSessionManager.HasSession);
            Assert.IsTrue(UserSessionManager.IsTemporaryOnly);
            Assert.AreEqual("temp-access", UserSessionManager.AccessToken);
            Assert.IsFalse(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));
            Assert.IsFalse(IVXLocalData.GetPersistFlag());
        }

        [Test]
        public void Load_AfterSave_RestoresSession()
        {
            UserSessionManager.ApplyLoginResponse(
                MakeLoginResponse(access: "persisted-access"),
                persist: true);

            UserSessionManager.Current = null;
            var loaded = UserSessionManager.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual("persisted-access", loaded.accessToken);
            Assert.AreEqual("qa@intelliversex.test", loaded.email);
        }

        [Test]
        public void TryRestorePersistedSession_RespectsRememberMeOff()
        {
            UserSessionManager.ApplyLoginResponse(MakeLoginResponse(), persist: true);
            Assert.IsTrue(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));

            UserSessionManager.RememberMe = false;
            IVXLocalData.SetPersistFlag(false);

            var restored = UserSessionManager.TryRestorePersistedSession();
            Assert.IsNull(restored);
            Assert.IsFalse(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession),
                "Stale disk session must be dropped when remember-me is off");
        }

        [Test]
        public void TryRestorePersistedSession_ReturnsSessionWhenRememberMeOn()
        {
            UserSessionManager.ApplyLoginResponse(MakeLoginResponse(access: "restore-me"), persist: true);
            UserSessionManager.Current = null;

            var restored = UserSessionManager.TryRestorePersistedSession();
            Assert.IsNotNull(restored);
            Assert.AreEqual("restore-me", restored.accessToken);
        }

        [Test]
        public void IsAccessTokenFresh_UsesExpirySkew()
        {
            var resp = MakeLoginResponse(expiresIn: 3600);
            UserSessionManager.ApplyLoginResponse(resp, persist: true);
            Assert.IsTrue(UserSessionManager.IsAccessTokenFresh(60));

            var expired = UserSessionManager.Current;
            expired.accessTokenExpiryEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 10;
            UserSessionManager.Current = expired;
            Assert.IsFalse(UserSessionManager.IsAccessTokenFresh(60));
        }

        [Test]
        public void ClearAuthSession_RemovesSessionAndTokenKeys_KeepsDeviceId()
        {
            IVXLocalData.SetSecureString(IVXLocalDataKeys.DeviceId, "device-keep-me");
            IVXLocalData.SetSecureString(IVXLocalDataKeys.BootstrapNakamaSession, "nakama-token");
            IVXSecureStorage.SetString(IVXLocalDataKeys.NakamaAuthTokenV2, "v2-auth");
            UserSessionManager.ApplyLoginResponse(MakeLoginResponse(), persist: true);

            UserSessionManager.ClearAuthSession(clearRememberMe: false);

            Assert.IsFalse(UserSessionManager.HasSession);
            Assert.IsFalse(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));
            Assert.IsFalse(IVXSecureStorage.HasKey(IVXLocalDataKeys.BootstrapNakamaSession));
            Assert.IsFalse(IVXSecureStorage.HasKey(IVXLocalDataKeys.NakamaAuthTokenV2));
            Assert.AreEqual("device-keep-me", IVXLocalData.GetSecureOrMigrate(IVXLocalDataKeys.DeviceId, ""));
        }

        [Test]
        public void ClearAllLocalData_WipesRegisteredCatalogIncludingRememberMe()
        {
            UserSessionManager.ApplyLoginResponse(MakeLoginResponse(), persist: true);
            IVXLocalData.SetLastEmail("wipe-me@example.com");
            IVXLocalData.SetSecureString(IVXLocalDataKeys.DeviceId, "device-wipe");

            UserSessionManager.ClearAllLocalData();

            Assert.IsFalse(UserSessionManager.HasSession);
            Assert.IsFalse(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));
            Assert.AreEqual("", IVXLocalData.GetLastEmail());
            Assert.AreEqual("", IVXLocalData.GetSecureOrMigrate(IVXLocalDataKeys.DeviceId, ""));
        }

        [Test]
        public void LegacySessionFile_MigratesIntoSecureStorage()
        {
            var session = new UserSessionManager.UserSession
            {
                accessToken = "legacy-file-token",
                email = "legacy-file@example.com",
                userId = "legacy-user",
                accessTokenExpiryEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600,
                SavedAtUtc = DateTime.UtcNow
            };
            string json = JsonUtility.ToJson(session);
            File.WriteAllText(UserSessionManager.SessionPath, json);

            UserSessionManager.Current = null;
            var loaded = UserSessionManager.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual("legacy-file-token", loaded.accessToken);
            Assert.IsTrue(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));
            Assert.IsFalse(File.Exists(UserSessionManager.SessionPath), "Legacy file should be deleted after migration");
        }

        [Test]
        public void RejectsImplausiblePersistedBlob()
        {
            IVXSecureStorage.SetString(IVXLocalDataKeys.UserSession, "{}");
            UserSessionManager.Current = null;

            var loaded = UserSessionManager.Load();
            Assert.IsNull(loaded);
            Assert.IsFalse(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));
        }

        [Test]
        public void LocalDataKeys_AuthSessionCatalog_IncludesUserSessionAndNakama()
        {
            CollectionAssert.Contains(IVXLocalDataKeys.AuthSessionKeys, IVXLocalDataKeys.UserSession);
            CollectionAssert.Contains(IVXLocalDataKeys.AuthSessionKeys, IVXLocalDataKeys.NakamaAuthTokenV2);
            CollectionAssert.Contains(IVXLocalDataKeys.AllSdkKeys, IVXLocalDataKeys.DeviceId);
            CollectionAssert.DoesNotContain(IVXLocalDataKeys.AuthSessionKeys, IVXLocalDataKeys.DeviceId);
        }

        [Test]
        public void BootstrapNakamaToken_UsesSecurePathNotPlaintext()
        {
            const string token = "bootstrap-nakama-auth";
            IVXLocalData.SetSecureString(IVXLocalDataKeys.BootstrapNakamaSession, token);

            string raw = PlayerPrefs.GetString(IVXLocalDataKeys.BootstrapNakamaSession, "");
            Assert.AreNotEqual(token, raw);
            Assert.AreEqual(token, IVXLocalData.GetSecureOrMigrate(IVXLocalDataKeys.BootstrapNakamaSession, ""));
        }
    }
}
