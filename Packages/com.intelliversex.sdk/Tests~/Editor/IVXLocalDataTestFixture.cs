using IntelliVerseX.Storage;
using NUnit.Framework;

namespace IntelliVerseX.Tests.Editor
{
    /// <summary>
    /// Isolates PlayerPrefs / secure-storage keys so session tests do not leak.
    /// Clears registered IVX keys before/after each test (does not call PlayerPrefs.DeleteAll).
    /// </summary>
    public abstract class IVXLocalDataTestFixture
    {
        [SetUp]
        public virtual void SetUp()
        {
            ResetLocalData();
        }

        [TearDown]
        public virtual void TearDown()
        {
            ResetLocalData();
        }

        protected static void ResetLocalData()
        {
            UserSessionManager.Clear();
            IVXLocalData.DeleteLegacySessionFile(UserSessionManager.SessionPath);
            IVXLocalData.ClearAllRegisteredKeys();
            IVXLocalData.EnsureInitialized();
        }

        protected static APIManager.LoginResponse MakeLoginResponse(
            string email = "qa@intelliversex.test",
            string userId = "user-test-001",
            string access = "access-token-test",
            string refresh = "refresh-token-test",
            int expiresIn = 3600)
        {
            return new APIManager.LoginResponse
            {
                status = true,
                message = "ok",
                data = new APIManager.LoginData
                {
                    accessToken = access,
                    refreshToken = refresh,
                    idToken = "id-token-test",
                    expiresIn = expiresIn,
                    user = new APIManager.LoginUser
                    {
                        id = userId,
                        email = email,
                        userName = "qa_user",
                        idpUsername = email,
                        firstName = "QA",
                        lastName = "Tester",
                        role = "user",
                        loginType = "email",
                        isAdult = true
                    }
                }
            };
        }
    }
}
