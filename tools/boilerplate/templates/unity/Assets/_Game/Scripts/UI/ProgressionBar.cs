using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace {{game_name}}.UI
{
    /// <summary>
    /// XP bar and level from Hiro progression. Detects level-ups between refreshes and shows a celebration popup.
    /// </summary>
    public sealed class ProgressionBar : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Image _xpFill;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _levelUpPopup;

        #endregion

        #region Private Fields

        private int _lastLevel = -1;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            _ = RefreshAsync();
            InvokeRepeating(nameof(RunRefresh), 2f, 4f);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(RunRefresh));
        }

        private void RunRefresh()
        {
            _ = RefreshAsync();
        }

        #endregion

        #region Private Methods

        private async Task RefreshAsync()
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized)
                return;

            var p = await hi.Progression.GetAsync("{{game_id}}");
            if (p == null)
                return;

            if (_levelText != null)
                _levelText.text = $"Lv {p.level}";

            if (_xpFill != null && p.xpRequired > 0)
            {
                var slice = 1f - (float)p.xpRemaining / p.xpRequired;
                _xpFill.fillAmount = Mathf.Clamp01(slice);
            }

            if (_lastLevel >= 0 && p.level > _lastLevel)
            {
                if (_levelUpPopup != null)
                    _levelUpPopup.SetActive(true);

                {{game_name}}.Analytics.AnalyticsWiring.Instance?.ForwardPlayerLevelUp(p.level, p.xp);
            }

            _lastLevel = p.level;
        }

        #endregion
    }
}
