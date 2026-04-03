using System.Threading.Tasks;
using IntelliVerseX.Identity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace {{game_name}}.UI
{
    /// <summary>
    /// Local settings: music/SFX levels, notification opt-in, and logout back to the Login scene.
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        #region Constants

        private const string PrefMusic = "ivx_music_volume";
        private const string PrefSfx = "ivx_sfx_volume";
        private const string PrefNotify = "ivx_notifications_enabled";

        #endregion

        #region Serialized Fields

        [Header("Audio")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        [Header("Notifications")]
        [SerializeField] private Toggle _notificationsToggle;

        [Header("Account")]
        [SerializeField] private Button _logoutButton;
        [SerializeField] private string _loginSceneName = "Login";

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (_musicSlider != null)
            {
                _musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(PrefMusic, 1f));
                _musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(PrefSfx, 1f));
                _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            }

            if (_notificationsToggle != null)
            {
                _notificationsToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(PrefNotify, 1) == 1);
                _notificationsToggle.onValueChanged.AddListener(OnNotifyChanged);
            }

            _logoutButton?.onClick.AddListener(OnLogoutClicked);
            ApplyAudioFromPrefs();
        }

        private void OnDisable()
        {
            _musicSlider?.onValueChanged.RemoveListener(OnMusicChanged);
            _sfxSlider?.onValueChanged.RemoveListener(OnSfxChanged);
            _notificationsToggle?.onValueChanged.RemoveListener(OnNotifyChanged);
            _logoutButton?.onClick.RemoveListener(OnLogoutClicked);
        }

        #endregion

        #region Private Methods

        private void OnMusicChanged(float v)
        {
            PlayerPrefs.SetFloat(PrefMusic, v);
            ApplyAudioFromPrefs();
        }

        private void OnSfxChanged(float v)
        {
            PlayerPrefs.SetFloat(PrefSfx, v);
        }

        private void OnNotifyChanged(bool on)
        {
            PlayerPrefs.SetInt(PrefNotify, on ? 1 : 0);
        }

        private void ApplyAudioFromPrefs()
        {
            var m = PlayerPrefs.GetFloat(PrefMusic, 1f);
            AudioListener.volume = Mathf.Clamp01(m);
        }

        private void OnLogoutClicked()
        {
            _ = LogoutAsync();
        }

        private async Task LogoutAsync()
        {
            if (IVXAuthManager.Instance != null)
                await IVXAuthManager.Instance.SignOutAsync();

            PlayerPrefs.Save();
            SceneManager.LoadScene(_loginSceneName);
        }

        #endregion
    }
}
