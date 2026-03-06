using System;
using System.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace IntelliVerseX.Hiro
{
    [Serializable]
    public class HiroRpcResponse<T>
    {
        [JsonProperty("success")] public bool success;
        [JsonProperty("data")] public T data;
        [JsonProperty("error")] public string error;
    }

    /// <summary>
    /// Wraps Nakama RPC calls for Hiro systems.
    /// Handles JSON serialization, deserialization, session refresh, and error handling.
    /// </summary>
    public class IVXHiroRpcClient
    {
        private const string LOG_TAG = "[IVX-HIRO-RPC]";

        private readonly IClient _client;
        private ISession _session;
        private readonly JsonSerializerSettings _jsonSettings;

        public static bool EnableDebugLogs { get; set; } = true;

        public IVXHiroRpcClient(IClient client, ISession session)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
        }

        /// <summary>
        /// Update the session after a token refresh.
        /// </summary>
        public void UpdateSession(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <summary>
        /// Call a Hiro RPC and return deserialized typed response.
        /// </summary>
        public async Task<HiroRpcResponse<T>> CallAsync<T>(string rpcId, object payload = null)
        {
            if (string.IsNullOrEmpty(rpcId))
                throw new ArgumentNullException(nameof(rpcId));
            if (_session == null || _session.IsExpired)
                throw new InvalidOperationException("Nakama session is null or expired.");

            try
            {
                var json = payload != null
                    ? JsonConvert.SerializeObject(payload, _jsonSettings)
                    : "{}";

                if (EnableDebugLogs)
                    Debug.Log($"{LOG_TAG} >> {rpcId}");

                var result = await _client.RpcAsync(_session, rpcId, json);

                if (result == null || string.IsNullOrEmpty(result.Payload))
                {
                    return new HiroRpcResponse<T> { success = false, error = $"RPC {rpcId}: empty response" };
                }

                var response = JsonConvert.DeserializeObject<HiroRpcResponse<T>>(result.Payload, _jsonSettings);
                return response ?? new HiroRpcResponse<T> { success = false, error = "Deserialization failed" };
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} {rpcId} failed: {ex.Message}");
                return new HiroRpcResponse<T> { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Call a Hiro RPC that does not return data (void).
        /// </summary>
        public async Task<bool> CallVoidAsync(string rpcId, object payload = null)
        {
            var response = await CallAsync<object>(rpcId, payload);
            return response.success;
        }
    }
}
