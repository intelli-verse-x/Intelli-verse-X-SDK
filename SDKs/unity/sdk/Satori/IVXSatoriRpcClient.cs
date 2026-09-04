using System;
using System.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace IntelliVerseX.Satori
{
    /// <summary>
    /// Low-level RPC client for Satori operations routed through Nakama.
    /// </summary>
    public sealed class IVXSatoriRpcClient
    {
        private const string LOG_TAG = "[IVX-SATORI-RPC]";

        private readonly IClient _client;
        private ISession _session;
        private readonly JsonSerializerSettings _jsonSettings;

        public static bool EnableDebugLogs { get; set; } = true;

        public IVXSatoriRpcClient(IClient client, ISession session)
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

        public void UpdateSession(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<T> CallAsync<T>(string rpcId, object payload = null)
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
                    return default;

                var wrapper = JsonConvert.DeserializeObject<SatoriRpcResponse<T>>(result.Payload, _jsonSettings);
                if (wrapper != null && wrapper.success)
                    return wrapper.data;

                if (wrapper != null && !string.IsNullOrEmpty(wrapper.error))
                    Debug.LogWarning($"{LOG_TAG} {rpcId} server error: {wrapper.error}");

                return default;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} {rpcId} failed: {ex.Message}");
                return default;
            }
        }

        public async Task<bool> CallVoidAsync(string rpcId, object payload = null)
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
                    return true;

                var wrapper = JsonConvert.DeserializeObject<SatoriRpcResponse<object>>(result.Payload, _jsonSettings);
                return wrapper == null || wrapper.success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} {rpcId} failed: {ex.Message}");
                return false;
            }
        }
    }
}
