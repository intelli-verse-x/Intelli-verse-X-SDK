using System;
using IntelliVerseX.Backend.Nakama;

namespace IntelliVerseX.Samples.TestScenes
{
    /// <summary>
    /// Optional mock data for sample scene fallback.
    /// </summary>
    public static class IVXProfileDemoMocks
    {
        public static IVXNProfileManager.IVXNProfileSnapshot CreateMockProfile()
        {
            return new IVXNProfileManager.IVXNProfileSnapshot
            {
                UserId = "mock-user-001",
                Email = "demo@intelliversex.ai",
                FirstName = "Demo",
                LastName = "Player",
                City = "Bengaluru",
                Region = "Karnataka",
                Country = "India",
                CountryCode = "IN",
                Locale = "en-in",
                Platform = "editor",
                DeviceId = "mock-device",
                ProfileVersion = 1,
                SchemaVersion = 2,
                TraceId = "mock-trace",
                RequestId = "mock-request",
                RawMetadataJson = "{\"mock\":true}"
            };
        }

        public static IVXNProfileManager.IVXNProfilePortfolioSnapshot CreateMockPortfolio()
        {
            var portfolio = new IVXNProfileManager.IVXNProfilePortfolioSnapshot
            {
                UserId = "mock-user-001",
                TotalGames = 2,
                GlobalWalletBalance = 1500,
                RawJson = "{\"mock\":true}"
            };

            portfolio.Games.Add(new IVXNProfileManager.IVXNProfileGameEntry
            {
                GameId = "quiz-verse",
                PlayCount = 12,
                SessionCount = 18,
                LastPlayedAt = DateTime.UtcNow.ToString("O"),
                WalletBalance = 840
            });

            portfolio.Games.Add(new IVXNProfileManager.IVXNProfileGameEntry
            {
                GameId = "weekly-quiz",
                PlayCount = 4,
                SessionCount = 5,
                LastPlayedAt = DateTime.UtcNow.AddDays(-1).ToString("O"),
                WalletBalance = 230
            });

            return portfolio;
        }
    }
}
