using System.Threading.Tasks;

namespace IntelliVerseX.Console
{
    /// <summary>
    /// Abstraction for console platform services (PlayStation, Xbox, Nintendo).
    /// Implement this interface per-platform to bridge native console SDK calls
    /// into the IntelliVerseX ecosystem.
    /// </summary>
    public interface IIVXConsoleAdapter
    {
        /// <summary>Platform identifier string (e.g. "PSN", "Xbox", "Nintendo").</summary>
        string PlatformId { get; }

        /// <summary>Retrieve the platform-specific user ID asynchronously.</summary>
        Task<string> GetPlatformUserIdAsync();

        /// <summary>Show the native platform overlay (e.g. PS button menu, Xbox guide).</summary>
        Task<bool> ShowPlatformOverlayAsync();

        /// <summary>Check whether a named platform feature is supported.</summary>
        bool SupportsFeature(string feature);

        /// <summary>Sign in using platform credentials.</summary>
        Task SignInWithPlatformAsync();

        /// <summary>Unlock an achievement on the native platform.</summary>
        Task<bool> UnlockAchievementAsync(string achievementId);

        /// <summary>Set the player's rich-presence status string.</summary>
        Task<bool> SetPresenceAsync(string status);
    }
}
