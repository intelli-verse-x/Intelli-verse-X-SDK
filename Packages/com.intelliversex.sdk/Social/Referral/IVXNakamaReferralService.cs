using System;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Prod Nakama referral / invite incentives.
    /// Canonical RPCs: <c>hiro_incentives_referral_code</c>, <c>hiro_incentives_apply_referral</c>.
    /// Prefer this over HTTP <c>APIManager</c> referral endpoints when a Nakama session exists.
    /// </summary>
    public static class IVXNakamaReferralService
    {
        private const string LOG_TAG = "[IVXReferral]";
        private const string RPC_GET_CODE = "hiro_incentives_referral_code";
        private const string RPC_APPLY = "hiro_incentives_apply_referral";

        [Serializable]
        public class ReferralCodeResult
        {
            [JsonProperty("code")] public string code;
            [JsonProperty("referralCode")] public string referralCode;
            [JsonProperty("referral_code")] public string referral_code;
            [JsonProperty("url")] public string url;
            [JsonProperty("referralUrl")] public string referralUrl;
            [JsonProperty("success")] public bool success;

            public string ResolvedCode =>
                !string.IsNullOrEmpty(code) ? code :
                !string.IsNullOrEmpty(referralCode) ? referralCode : referral_code;

            public string ResolvedUrl => !string.IsNullOrEmpty(url) ? url : referralUrl;
        }

        [Serializable]
        public class ApplyReferralResult
        {
            [JsonProperty("success")] public bool success;
            [JsonProperty("applied")] public bool applied;
            [JsonProperty("reward")] public object reward;
            [JsonProperty("error")] public string error;
        }

        public static async Task<ReferralCodeResult> GetReferralCodeAsync(IClient client, ISession session)
        {
            if (client == null || session == null || session.IsExpired)
            {
                Debug.LogWarning($"{LOG_TAG} Nakama session required for {RPC_GET_CODE}");
                return null;
            }

            var rpc = new IVXHiroRpcClient(client, session);
            var response = await rpc.CallAsync<ReferralCodeResult>(RPC_GET_CODE);
            if (!HiroRpcResponseUtility.TryGetData(response, out var data, RPC_GET_CODE))
                return null;
            return data;
        }

        public static async Task<ApplyReferralResult> ApplyReferralCodeAsync(
            IClient client,
            ISession session,
            string code)
        {
            if (client == null || session == null || session.IsExpired)
            {
                Debug.LogWarning($"{LOG_TAG} Nakama session required for {RPC_APPLY}");
                return null;
            }

            if (string.IsNullOrWhiteSpace(code))
                return new ApplyReferralResult { success = false, error = "code required" };

            var rpc = new IVXHiroRpcClient(client, session);
            var payload = new
            {
                code = code.Trim(),
                referralCode = code.Trim(),
                referral_code = code.Trim()
            };
            var response = await rpc.CallAsync<ApplyReferralResult>(RPC_APPLY, payload);
            if (!HiroRpcResponseUtility.TryGetData(response, out var data, RPC_APPLY))
            {
                return new ApplyReferralResult
                {
                    success = false,
                    error = response?.error ?? "apply failed"
                };
            }

            return data ?? new ApplyReferralResult { success = true, applied = true };
        }
    }
}
