using System;
using System.Linq;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace {{game_name}}.UI
{
    /// <summary>
    /// Displays Hiro energy state (current / max), refill countdown from server timestamps,
    /// and uses template defaults for design-time max and regen interval hints.
    /// </summary>
    public sealed class EnergyBar : MonoBehaviour
    {
        #region Constants

        private const int MaxEnergyDefault = {{max_energy}};
        private const int RefillMinutesDefault = {{energy_refill_minutes}};

        #endregion

        #region Serialized Fields

        [SerializeField] private string _energyId = "default";
        [SerializeField] private Image _fill;
        [SerializeField] private TextMeshProUGUI _ratioText;
        [SerializeField] private TextMeshProUGUI _timerText;

        #endregion

        #region Private Fields

        private DateTime _nextRegenUtc = DateTime.MaxValue;
        private int _max = MaxEnergyDefault;

        #endregion

        #region Unity Lifecycle

        private async void OnEnable()
        {
            await RefreshAsync();
        }

        private void Update()
        {
            if (_timerText == null)
                return;
            var span = _nextRegenUtc - DateTime.UtcNow;
            if (span.TotalSeconds <= 0)
            {
                _timerText.text = "Refill soon";
                return;
            }
            _timerText.text =
                $"Next: {span.Minutes:D2}:{span.Seconds:D2} (~{RefillMinutesDefault}m cadence)";
        }

        #endregion

        #region Private Methods

        private async Task RefreshAsync()
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized)
                return;

            var res = await hi.Energy.GetAsync("{{game_id}}");
            var state = res.energies?.FirstOrDefault(e => e.energyId == _energyId)
                        ?? res.energies?.FirstOrDefault();
            if (state == null)
                return;

            _max = state.max > 0 ? state.max : MaxEnergyDefault;
            var cur = state.current;
            if (_fill != null)
                _fill.fillAmount = _max > 0 ? Mathf.Clamp01((float)cur / _max) : 0f;
            if (_ratioText != null)
                _ratioText.text = $"{cur}/{_max}";

            if (state.nextRegenAt > 0)
                _nextRegenUtc = DateTimeOffset.FromUnixTimeSeconds(state.nextRegenAt).UtcDateTime;
            else
                _nextRegenUtc = DateTime.UtcNow.AddMinutes(RefillMinutesDefault);

            {{game_name}}.Analytics.AnalyticsWiring.Instance?.ForwardEnergyChanged(state.current, _max);
        }

        #endregion
    }
}
