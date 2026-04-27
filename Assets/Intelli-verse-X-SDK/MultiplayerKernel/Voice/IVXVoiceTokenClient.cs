// IVXVoiceTokenClient — typed Unity helper around the kernel's
// `mp_voice_token` Nakama RPC.
//
// Without this helper a Unity dev had to:
//   1. Recall the exact RPC name (`mp_voice_token`).
//   2. Hand-shape a JSON payload that matches the TS request type.
//   3. Hand-shape a response DTO that matches `IVXVoiceSessionToken`'s
//      JSON property names.
//   4. Pull `_client` and `_session` off the (protected) abstract
//      `IVXNakamaManager` via reflection or a custom subclass.
//
// Now they call:
//
//   var token = await IVXVoiceTokenClient.MintAsync(nakamaProvider, matchId);
//   await voiceProvider.ConnectAsync(token);
//
// Wire contract source of truth:
//   nakama/data/modules/src/multiplayer-kernel/voice-providers/index.ts
//   (rpcVoiceToken, lines 165-211)
//
// On any failure (kernel unconfigured, auth expired, network blip) we
// log and rethrow — the caller can fall back to text-only or schedule
// a retry, exactly the same surface as the rest of IIVXVoice.

using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.MultiplayerKernel.API;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

namespace IntelliVerseX.MultiplayerKernel.Voice
{
    /// <summary>
    /// Typed client around the <c>mp_voice_token</c> Nakama RPC.
    /// </summary>
    public static class IVXVoiceTokenClient
    {
        #region Constants
        private const string LOG_PREFIX = "[IVXVoiceTokenClient]";
        private const string RPC_VOICE_TOKEN = "mp_voice_token";
        #endregion

        #region Request / Response DTOs

        /// <summary>
        /// Wire shape for the <c>mp_voice_token</c> request.
        /// Mirrors the TS handler in <c>voice-providers/index.ts</c>.
        /// </summary>
        [Serializable]
        private sealed class VoiceTokenRequest
        {
            [JsonProperty("match_id")]
            public string MatchId { get; set; } = string.Empty;

            [JsonProperty("can_publish")]
            public bool CanPublish { get; set; }

            [JsonProperty("can_subscribe")]
            public bool CanSubscribe { get; set; }

            [JsonProperty("spatial")]
            public bool Spatial { get; set; }

            [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
            public string Region { get; set; }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Mint a per-match voice session token from the kernel.
        /// </summary>
        /// <param name="nakama">Nakama realtime provider (e.g. your <see cref="IVXNakamaManager"/>).</param>
        /// <param name="matchId">Match id returned by <c>mp_create_match</c> / OP_MATCH_JOINED.</param>
        /// <param name="canPublish">Mic publish capability requested by the client.</param>
        /// <param name="canSubscribe">Audio subscribe capability requested by the client.</param>
        /// <param name="spatial">Whether the client wants spatial-audio routing on the SFU.</param>
        /// <param name="region">Optional region hint (e.g. <c>"us"</c>, <c>"eu"</c>); empty selects the default.</param>
        /// <param name="cancellationToken">Cancellation propagated to the underlying RPC.</param>
        /// <returns>Fully-populated <see cref="IVXVoiceSessionToken"/> ready to feed to <c>IIVXVoice.ConnectAsync</c>.</returns>
        public static async Task<IVXVoiceSessionToken> MintAsync(
            IIVXNakamaRealtimeProvider nakama,
            string matchId,
            bool canPublish    = true,
            bool canSubscribe  = true,
            bool spatial       = false,
            string region      = null,
            CancellationToken cancellationToken = default)
        {
            if (nakama == null) throw new ArgumentNullException(nameof(nakama));
            if (string.IsNullOrEmpty(matchId)) throw new ArgumentException("matchId is required", nameof(matchId));
            if (nakama.Client == null) throw new InvalidOperationException(LOG_PREFIX + " Nakama client is null; initialize the manager first.");
            if (nakama.Session == null || nakama.Session.IsExpired)
            {
                throw new InvalidOperationException(LOG_PREFIX + " Nakama session is null or expired; refresh auth before minting voice tokens.");
            }

            var request = new VoiceTokenRequest
            {
                MatchId      = matchId,
                CanPublish   = canPublish,
                CanSubscribe = canSubscribe,
                Spatial      = spatial,
                Region       = string.IsNullOrEmpty(region) ? null : region,
            };

            string jsonPayload = JsonConvert.SerializeObject(request);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                Debug.Log($"{LOG_PREFIX} POST {RPC_VOICE_TOKEN} match={matchId} pub={canPublish} sub={canSubscribe} spatial={spatial} region={region ?? "<default>"}");
                var rpc = await nakama.Client.RpcAsync(nakama.Session, RPC_VOICE_TOKEN, jsonPayload).ConfigureAwait(false);

                if (string.IsNullOrEmpty(rpc.Payload))
                {
                    throw new InvalidOperationException(LOG_PREFIX + " kernel returned empty payload (voice unconfigured?).");
                }

                var token = JsonConvert.DeserializeObject<IVXVoiceSessionToken>(rpc.Payload);
                if (token == null)
                {
                    throw new InvalidOperationException(LOG_PREFIX + " could not deserialize voice token payload: " + rpc.Payload);
                }

                if (token.Provider == IVXVoiceProvider.None || token.Provider == IVXVoiceProvider.Unspecified)
                {
                    Debug.LogWarning($"{LOG_PREFIX} kernel returned NONE provider — voice degraded for this match (expected when LiveKit is unconfigured).");
                }

                if (string.IsNullOrEmpty(token.Token) || string.IsNullOrEmpty(token.Url))
                {
                    Debug.LogWarning($"{LOG_PREFIX} kernel returned a token with empty token/url; provider will fall back to text-only.");
                }

                return token;
            }
            catch (ApiResponseException apiEx)
            {
                Debug.LogError($"{LOG_PREFIX} RPC failed: {apiEx.Message} (status={apiEx.StatusCode})");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX} mint failed: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}
