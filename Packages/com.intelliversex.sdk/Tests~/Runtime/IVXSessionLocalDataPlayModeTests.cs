using System.Collections;
using IntelliVerseX.Storage;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IntelliVerseX.Tests.Runtime
{
    /// <summary>
    /// PlayMode smoke: secure session survives a frame boundary (simulates runtime persistence).
    /// Full matrix lives in EditMode <c>IVXSessionLocalDataTests</c>.
    /// </summary>
    [TestFixture]
    [Category("IVX.Session")]
    public class IVXSessionLocalDataPlayModeTests
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            UserSessionManager.ClearAllLocalData();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            UserSessionManager.ClearAllLocalData();
            yield return null;
        }

        [UnityTest]
        public IEnumerator PersistedSession_SurvivesFrame_AndReloads()
        {
            var resp = new APIManager.LoginResponse
            {
                status = true,
                message = "ok",
                data = new APIManager.LoginData
                {
                    accessToken = "playmode-access",
                    refreshToken = "playmode-refresh",
                    expiresIn = 3600,
                    user = new APIManager.LoginUser
                    {
                        id = "pm-user",
                        email = "playmode@test.local",
                        userName = "pm",
                        idpUsername = "playmode@test.local",
                        role = "user",
                        loginType = "email"
                    }
                }
            };

            UserSessionManager.ApplyLoginResponse(resp, persist: true);
            Assert.IsTrue(UserSessionManager.HasSession);

            yield return null;

            UserSessionManager.Current = null;
            var loaded = UserSessionManager.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual("playmode-access", loaded.accessToken);
            Assert.IsTrue(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));

            string raw = PlayerPrefs.GetString(IVXLocalDataKeys.UserSession, "");
            Assert.AreNotEqual("playmode-access", raw);
        }
    }
}
