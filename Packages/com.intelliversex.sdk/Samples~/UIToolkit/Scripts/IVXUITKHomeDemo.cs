using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    /// <summary>
    /// UITK home hub with animated scene navigation cards.
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Home Demo")]
    public sealed class IVXUITKHomeDemo : IVXUITKDemoShell
    {
        private static readonly (string button, string scene, string label)[] Links =
        {
            ("navAuth", "IVX_AuthTest_UITK", "Auth"),
            ("navFriends", "IVX_Friends_UITK", "Friends"),
            ("navClan", "IVX_Clan_UITK", "Clan"),
            ("navProfile", "IVX_Profile_UITK", "Profile"),
            ("navLeaderboard", "IVX_LeaderboardTest_UITK", "Leaderboard"),
            ("navWallet", "IVX_WalletTest_UITK", "Wallet"),
            ("navWeeklyQuiz", "IVX_WeeklyQuizTest_UITK", "Weekly Quiz"),
            ("navDailyQuiz", "IVX_DailyQuiz_UITK", "Daily Quiz"),
            ("navAds", "IVX_AdsTest_UITK", "Ads"),
            ("navMoreOfUs", "IVX_MoreOfUs_UITK", "More Of Us"),
            ("navShareRate", "IVX_Share&RateUs_UITK", "Share & Rate"),
        };

        protected override void OnUIReady()
        {
            for (int i = 0; i < Links.Length; i++)
            {
                var link = Links[i];
                var button = Q<Button>(link.button);
                if (button == null)
                    continue;

                string scene = link.scene;
                string label = link.label;
                BindClick(button, () =>
                {
                    SetStatus($"Opening {label}...");
                    LoadScene(scene);
                });
            }

            SetStatus("Select a demo scene.");
            StartCoroutine(PlayEnterMotion());
        }

        private IEnumerator PlayEnterMotion()
        {
            var grid = Q<VisualElement>("navGrid");
            yield return StaggerEnter(grid, "ivx-nav-card", 0.04f);
        }
    }
}
