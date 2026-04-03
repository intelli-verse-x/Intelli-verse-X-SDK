using System.Collections.Generic;
using IntelliVerseX.Satori;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace {{game_name}}.UI
{
    /// <summary>
    /// Three-step overlay FTUE: wallet, play, then daily rewards. Persists completion in PlayerPrefs.
    /// </summary>
    public sealed class FTUEController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject _root;
        [SerializeField] private GameObject _walletHighlight;
        [SerializeField] private GameObject _playHighlight;
        [SerializeField] private GameObject _dailyHighlight;
        [SerializeField] private TextMeshProUGUI _body;
        [SerializeField] private Button _nextButton;

        #endregion

        #region Private Fields

        private int _step;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (PlayerPrefs.GetInt("ftue_completed", 0) == 1)
            {
                if (_root != null)
                    _root.SetActive(false);
                return;
            }

            if (_root != null)
                _root.SetActive(true);

            _nextButton?.onClick.AddListener(OnNext);
            _step = 0;
            ApplyStep();
        }

        private void OnDestroy()
        {
            _nextButton?.onClick.RemoveListener(OnNext);
        }

        #endregion

        #region Private Methods

        private void OnNext()
        {
            _step++;
            if (_step > 2)
                CompleteFtue();
            else
                ApplyStep();
        }

        private void ApplyStep()
        {
            SetActiveSafe(_walletHighlight, _step == 0);
            SetActiveSafe(_playHighlight, _step == 1);
            SetActiveSafe(_dailyHighlight, _step == 2);

            if (_body != null)
            {
                _body.text = _step switch
                {
                    0 => "This is your wallet — coins and gems live here.",
                    1 => "Tap Play to jump into a match.",
                    _ => "Come back daily for streak rewards.",
                };
            }
        }

        private static void SetActiveSafe(GameObject go, bool on)
        {
            if (go != null)
                go.SetActive(on);
        }

        private void CompleteFtue()
        {
            PlayerPrefs.SetInt("ftue_completed", 1);
            PlayerPrefs.Save();
            if (_root != null)
                _root.SetActive(false);

            var satori = IVXSatoriClient.Instance;
            if (satori != null && satori.IsInitialized)
                _ = satori.CaptureEventAsync("ftue_completed", new Dictionary<string, string>
                {
                    { "game_id", "{{game_id}}" },
                });
        }

        #endregion
    }
}
