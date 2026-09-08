using System;
using System.Threading.Tasks;
using IntelliVerseX.V2;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Leaderboard Demo")]
    public sealed class IVXUITKLeaderboardDemo : IVXUITKFeatureDemoBase
    {
        private TextField _scoreField;

        protected override void BuildFeatureUI()
        {
            SetTitle("Leaderboard", "Submit / fetch via IVXNLeaderbordManager");
            _scoreField = AddField("score", "Score to submit", "100");
            AddAction("Fetch Boards", () => _ = FetchAsync());
            AddAction("Submit Score", () => _ = SubmitAsync());
            AddAction("Get My Rank", () => _ = RankAsync());
            SetStatus("Leaderboard demo ready.");
        }

        private async Task FetchAsync()
        {
            SetBusy(true);
            try
            {
                var resp = await IVXNLeaderbordManager.GetAllLeaderboardsAsync(25);
                SetResult(resp != null ? resp.ToString() : "No data");
                SetStatus("Leaderboards fetched.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SubmitAsync()
        {
            if (!int.TryParse(_scoreField?.value, out int score))
            {
                SetStatus("Enter a numeric score.", true);
                return;
            }

            SetBusy(true);
            try
            {
                var resp = await IVXNLeaderbordManager.SubmitScoreAsync(score);
                SetResult(resp != null ? resp.ToString() : "No response");
                SetStatus("Score submitted.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task RankAsync()
        {
            SetBusy(true);
            try
            {
                int rank = await IVXNLeaderbordManager.GetPlayerRankAsync(global: true);
                long best = await IVXNLeaderbordManager.GetPlayerBestScoreAsync(global: true);
                SetResult($"Rank={rank}, Best={best}");
                SetStatus("Rank loaded.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }
    }
}
