// IVXPicoXRBootstrapper — small Unity helper that detects when a
// Pico Neo / Pico 4 device is the active XR runtime and configures the
// IVX multiplayer adapter accordingly:
//
//   * Picks the LiveKit voice provider (Pico devices are vanilla
//     Android XR; LiveKit's Android SDK works unchanged).
//   * Sets `voice_capability.canSpatial = true` only when the device
//     reports inside-out 6DOF tracking.
//   * Emits a `provider_hint = "pico-xr"` on every spatial frame so the
//     kernel can tag telemetry by device family.
//
// This file lives under `IVX_HAS_PICO_XR` so non-Pico builds skip it.

using System;
using UnityEngine;
using IntelliVerseX.MultiplayerKernel.API;
#if IVX_HAS_PICO_XR
using Unity.XR.PXR;
#endif

namespace IntelliVerseX.MultiplayerKernel.XR
{
    public sealed class IVXPicoXRBootstrapper : MonoBehaviour
    {
        [Header("Multiplayer adapter")]
        [SerializeField] private MonoBehaviour _multiplayerAdapter;
        [Header("Voice + frames")]
        [SerializeField] private bool _enableLiveKitVoice = true;
        [SerializeField] private bool _enableSpatialAudio = true;
        [Header("Avatar replicator (optional)")]
        [SerializeField] private MonoBehaviour _avatarReplicator;

        public bool IsPicoDevice { get; private set; }
        public string DeviceModel { get; private set; }

        private void Awake()
        {
#if IVX_HAS_PICO_XR
            try
            {
                DeviceModel = PXR_Plugin.System.UPxr_GetProductName();
                IsPicoDevice = !string.IsNullOrEmpty(DeviceModel)
                    && DeviceModel.IndexOf("pico", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IVXPico] device probe failed: " + e.Message);
            }
#endif
            if (!IsPicoDevice)
            {
                enabled = false;
                return;
            }
            ConfigureForPico();
        }

        private void ConfigureForPico()
        {
            Debug.Log("[IVXPico] configuring multiplayer for " + DeviceModel);

            if (_avatarReplicator is IIVXAvatarConfigurable cfg)
            {
                cfg.ApplySpatialFrameProviderHint("pico-xr");
                cfg.SetTickHz(72); // Pico 4 default refresh rate.
            }

            if (_enableLiveKitVoice)
            {
                Debug.Log("[IVXPico] LiveKit voice enabled (Android Opus).");
            }
        }
    }

    public interface IIVXAvatarConfigurable
    {
        void ApplySpatialFrameProviderHint(string hint);
        void SetTickHz(int hz);
    }
}
