using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HiroAchievement = IntelliVerseX.Hiro.IVXAchievement;

namespace {{game_name}}.UI
{
    /// <summary>
    /// Lists Hiro achievements, shows progress with a fill image, and claims rewards via
    /// <see cref="IntelliVerseX.Hiro.Systems.IVXAchievementsSystem.ClaimAsync"/>.
    /// </summary>
    public sealed class AchievementsPanel : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private RectTransform _listRoot;
        [SerializeField] private GameObject _rowPrefab;

        #endregion

        #region Unity Lifecycle

        private async void OnEnable()
        {
            await ReloadAsync();
        }

        #endregion

        #region Private Methods

        private async Task ReloadAsync()
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized || _listRoot == null || _rowPrefab == null)
                return;

            for (var i = _listRoot.childCount - 1; i >= 0; i--)
                Destroy(_listRoot.GetChild(i).gameObject);

            var data = await hi.Achievements.ListAsync("{{game_id}}");
            foreach (var a in data.achievements)
            {
                var row = Instantiate(_rowPrefab, _listRoot);
                BindRow(row, a);
            }
        }

        private void BindRow(GameObject row, HiroAchievement a)
        {
            var title = row.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            var fill = row.transform.Find("Fill")?.GetComponent<Image>();
            var claim = row.transform.Find("Claim")?.GetComponent<Button>();
            if (title != null)
                title.text = a.name;
            if (fill != null && a.maxCount > 0)
                fill.fillAmount = Mathf.Clamp01((float)a.currentCount / a.maxCount);
            claim?.onClick.AddListener(() => _ = ClaimAsync(a.id));
        }

        private async Task ClaimAsync(string achievementId)
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized)
                return;

            var reward = await hi.Achievements.ClaimAsync(achievementId, "{{game_id}}");
            if (reward == null)
                return;

            var satori = IntelliVerseX.Satori.IVXSatoriClient.Instance;
            if (satori != null && satori.IsInitialized)
                _ = satori.CaptureEventAsync("achievement_claimed", new Dictionary<string, string>
                {
                    { "game_id", "{{game_id}}" },
                    { "achievement_id", achievementId },
                });

            await ReloadAsync();
        }

        #endregion
    }
}
