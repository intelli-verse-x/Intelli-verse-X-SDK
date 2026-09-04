// IVXAudioManager.cs
// IntelliVerseX SDK Audio Manager Base Class
// Reusable audio system for all games in the platform
// Handles music, sound effects, mixer snapshots, and volume control

using System;
using UnityEngine;
using UnityEngine.Audio;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Base class for game audio management
    /// Provides music/SFX control, volume persistence, and mixer snapshot management
    /// 
    /// Usage:
    /// 1. Create game-specific class: public class QuizVerseAudioManager : IVXAudioManager
    /// 2. Assign AudioMixer snapshots in Inspector
    /// 3. Use ToggleMusic(bool) and ToggleSound(bool) for on/off
    /// 4. Use SetMusicVolume(float) and SetSoundVolume(float) for 0-1 control
    /// 5. Subscribe to OnMusicToggled / OnSoundToggled for UI updates
    /// </summary>
    public abstract class IVXAudioManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        private static IVXAudioManager _instance;
        public static IVXAudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    _instance = FindFirstObjectByType<IVXAudioManager>();
#else
                    _instance = FindObjectOfType<IVXAudioManager>();
#endif
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Configuration
        
        [Header("Audio Sources")]
        [SerializeField] protected AudioSource musicSource;
        [SerializeField] protected AudioSource sfxSource;
        
        [Header("Audio Mixer Snapshots")]
        [SerializeField] protected AudioMixerSnapshot musicEnabledSnapshot;
        [SerializeField] protected AudioMixerSnapshot musicDisabledSnapshot;
        [SerializeField] protected AudioMixerSnapshot sfxEnabledSnapshot;
        [SerializeField] protected AudioMixerSnapshot sfxDisabledSnapshot;
        
        [Header("Settings")]
        [SerializeField] protected float snapshotTransitionTime = 0f;
        [SerializeField] protected bool enableMusicByDefault = true;
        [SerializeField] protected bool enableSoundByDefault = true;
        
        // PlayerPrefs keys
        private const string MUSIC_ENABLED_KEY = "IVX_MusicEnabled";
        private const string SOUND_ENABLED_KEY = "IVX_SoundEnabled";
        private const string MUSIC_VOLUME_KEY = "IVX_MusicVolume";
        private const string SOUND_VOLUME_KEY = "IVX_SoundVolume";
        
        /// <summary>
        /// Override for custom log prefix
        /// </summary>
        protected virtual string GetLogPrefix() => "[IVX-AUDIO]";
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Fired when music is toggled on/off
        /// </summary>
        public event Action<bool> OnMusicToggled;
        
        /// <summary>
        /// Fired when sound effects are toggled on/off
        /// </summary>
        public event Action<bool> OnSoundToggled;
        
        /// <summary>
        /// Fired when music volume changes (0-1)
        /// </summary>
        public event Action<float> OnMusicVolumeChanged;
        
        /// <summary>
        /// Fired when sound volume changes (0-1)
        /// </summary>
        public event Action<float> OnSoundVolumeChanged;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Is music currently enabled?
        /// </summary>
        public bool IsMusicEnabled { get; protected set; }
        
        /// <summary>
        /// Are sound effects currently enabled?
        /// </summary>
        public bool IsSoundEnabled { get; protected set; }
        
        /// <summary>
        /// Current music volume (0-1)
        /// </summary>
        public float MusicVolume { get; protected set; }
        
        /// <summary>
        /// Current sound effects volume (0-1)
        /// </summary>
        public float SoundVolume { get; protected set; }
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        protected virtual void Start()
        {
            LoadSettings();
            ApplySettings();
        }
        
        #endregion
        
        #region Settings Management
        
        /// <summary>
        /// Load audio settings from PlayerPrefs
        /// </summary>
        protected virtual void LoadSettings()
        {
            // Load enabled states (1 = true, 0 = false)
            int musicEnabledInt = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, enableMusicByDefault ? 1 : 0);
            int soundEnabledInt = PlayerPrefs.GetInt(SOUND_ENABLED_KEY, enableSoundByDefault ? 1 : 0);
            
            IsMusicEnabled = musicEnabledInt == 1;
            IsSoundEnabled = soundEnabledInt == 1;
            
            // Load volume levels
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
            SoundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY, 1f);
            
            Debug.Log($"{GetLogPrefix()} Settings loaded - Music: {IsMusicEnabled} ({MusicVolume:F2}), Sound: {IsSoundEnabled} ({SoundVolume:F2})");
        }
        
        /// <summary>
        /// Apply current settings to audio system
        /// </summary>
        protected virtual void ApplySettings()
        {
            ApplyMusicSettings();
            ApplySoundSettings();
        }
        
        /// <summary>
        /// Save current settings to PlayerPrefs
        /// </summary>
        protected virtual void SaveSettings()
        {
            PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, IsMusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt(SOUND_ENABLED_KEY, IsSoundEnabled ? 1 : 0);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, MusicVolume);
            PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, SoundVolume);
            PlayerPrefs.Save();
        }
        
        #endregion
        
        #region Music Control
        
        /// <summary>
        /// Toggle music on/off
        /// </summary>
        public virtual void ToggleMusic(bool enabled)
        {
            IsMusicEnabled = enabled;
            ApplyMusicSettings();
            SaveSettings();
            OnMusicToggled?.Invoke(enabled);
            
            Debug.Log($"{GetLogPrefix()} Music {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set music volume (0-1)
        /// </summary>
        public virtual void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            
            if (musicSource != null)
            {
                musicSource.volume = MusicVolume;
            }
            
            SaveSettings();
            OnMusicVolumeChanged?.Invoke(MusicVolume);
            
            Debug.Log($"{GetLogPrefix()} Music volume: {MusicVolume:F2}");
        }
        
        /// <summary>
        /// Play music clip
        /// </summary>
        public virtual void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null)
            {
                Debug.LogWarning($"{GetLogPrefix()} No music AudioSource assigned!");
                return;
            }
            
            StopMusic();
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = MusicVolume;
            musicSource.Play();
            
            Debug.Log($"{GetLogPrefix()} Playing music: {clip?.name}");
        }
        
        /// <summary>
        /// Stop current music
        /// </summary>
        public virtual void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }
        
        /// <summary>
        /// Apply music settings (snapshot + volume)
        /// </summary>
        protected virtual void ApplyMusicSettings()
        {
            // Apply mixer snapshot
            if (IsMusicEnabled && musicEnabledSnapshot != null)
            {
                musicEnabledSnapshot.TransitionTo(snapshotTransitionTime);
            }
            else if (!IsMusicEnabled && musicDisabledSnapshot != null)
            {
                musicDisabledSnapshot.TransitionTo(snapshotTransitionTime);
            }
            
            // Apply volume
            if (musicSource != null)
            {
                musicSource.volume = IsMusicEnabled ? MusicVolume : 0f;
            }
        }
        
        #endregion
        
        #region Sound Effects Control
        
        /// <summary>
        /// Toggle sound effects on/off
        /// </summary>
        public virtual void ToggleSound(bool enabled)
        {
            IsSoundEnabled = enabled;
            ApplySoundSettings();
            SaveSettings();
            OnSoundToggled?.Invoke(enabled);
            
            Debug.Log($"{GetLogPrefix()} Sound {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set sound effects volume (0-1)
        /// </summary>
        public virtual void SetSoundVolume(float volume)
        {
            SoundVolume = Mathf.Clamp01(volume);
            
            if (sfxSource != null)
            {
                sfxSource.volume = SoundVolume;
            }
            
            SaveSettings();
            OnSoundVolumeChanged?.Invoke(SoundVolume);
            
            Debug.Log($"{GetLogPrefix()} Sound volume: {SoundVolume:F2}");
        }
        
        /// <summary>
        /// Play sound effect clip
        /// </summary>
        public virtual void PlaySound(AudioClip clip, float volumeScale = 1f)
        {
            if (sfxSource == null)
            {
                Debug.LogWarning($"{GetLogPrefix()} No SFX AudioSource assigned!");
                return;
            }
            
            if (!IsSoundEnabled) return;
            
            sfxSource.PlayOneShot(clip, SoundVolume * volumeScale);
        }
        
        /// <summary>
        /// Play sound at specific position (3D audio)
        /// </summary>
        public virtual void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (!IsSoundEnabled) return;
            
            AudioSource.PlayClipAtPoint(clip, position, SoundVolume * volumeScale);
        }
        
        /// <summary>
        /// Apply sound settings (snapshot + volume)
        /// </summary>
        protected virtual void ApplySoundSettings()
        {
            // Apply mixer snapshot
            if (IsSoundEnabled && sfxEnabledSnapshot != null)
            {
                sfxEnabledSnapshot.TransitionTo(snapshotTransitionTime);
            }
            else if (!IsSoundEnabled && sfxDisabledSnapshot != null)
            {
                sfxDisabledSnapshot.TransitionTo(snapshotTransitionTime);
            }
            
            // Apply volume
            if (sfxSource != null)
            {
                sfxSource.volume = IsSoundEnabled ? SoundVolume : 0f;
            }
        }
        
        #endregion
        
        #region Debug Utilities
        
        /// <summary>
        /// Test music toggle
        /// </summary>
        [ContextMenu("Toggle Music")]
        public void TestToggleMusic()
        {
            ToggleMusic(!IsMusicEnabled);
        }
        
        /// <summary>
        /// Test sound toggle
        /// </summary>
        [ContextMenu("Toggle Sound")]
        public void TestToggleSound()
        {
            ToggleSound(!IsSoundEnabled);
        }
        
        /// <summary>
        /// Reset all audio settings to defaults
        /// </summary>
        [ContextMenu("Reset Audio Settings")]
        public void ResetAudioSettings()
        {
            IsMusicEnabled = enableMusicByDefault;
            IsSoundEnabled = enableSoundByDefault;
            MusicVolume = 1f;
            SoundVolume = 1f;
            
            ApplySettings();
            SaveSettings();
            
            Debug.Log($"{GetLogPrefix()} Audio settings reset to defaults");
        }
        
        #endregion
    }
}
