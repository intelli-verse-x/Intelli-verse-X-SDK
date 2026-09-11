using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Preferred auth surface for new code. Thin facade over Auth V2 APIs.
    /// Prefer this (or <see cref="IVXAPIClient"/>) over calling <c>APIManager</c> directly.
    /// Legacy <c>IntelliVerseXManager</c> init is retired — use <c>IVXBootstrap</c>.
    /// </summary>
    public static class IVXAuthClient
    {
        public static Task<APIManager.LoginResponse> LoginAsync(
            string email,
            string password,
            bool persistSession = true,
            CancellationToken ct = default)
            => IVXAPIClient.LoginAsync(email, password, persistSession, ct);

        public static Task<APIManager.SocialLoginResponse> SocialLoginAsync(
            string loginType,
            string email,
            string firstName = null,
            string lastName = null,
            string userName = null,
            string appleKey = null,
            bool persistSession = true,
            CancellationToken ct = default)
            => IVXAPIClient.SocialLoginAsync(
                loginType, email, firstName, lastName, userName, appleKey, persistSession, ct);

        public static void Logout() => IVXAPIClient.Logout();

        public static bool IsLoggedIn => IVXAPIClient.IsLoggedIn;

        public static bool TryRestoreSession() => IVXAPIClient.TryRestoreSession();
    }
}
