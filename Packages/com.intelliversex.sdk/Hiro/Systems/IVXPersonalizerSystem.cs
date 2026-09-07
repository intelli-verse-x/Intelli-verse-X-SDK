using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXPersonalizerSystem
    {
        private const string RPC_SET_OVERRIDE = "hiro_personalizer_set_override";
        private const string RPC_REMOVE_OVERRIDE = "hiro_personalizer_remove_override";
        private const string RPC_GET_OVERRIDES = "hiro_personalizer_get_overrides";
        private const string RPC_PREVIEW = "hiro_personalizer_preview";

        private readonly IVXHiroRpcClient _rpc;

        public IVXPersonalizerSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<bool> SetOverrideAsync(string userId, string system, string path, object value)
        {
            return await _rpc.CallVoidAsync(RPC_SET_OVERRIDE, new { userId, system, path, value });
        }

        public async Task<bool> RemoveOverrideAsync(string userId, string system, string path)
        {
            return await _rpc.CallVoidAsync(RPC_REMOVE_OVERRIDE, new { userId, system, path });
        }

        public async Task<IVXPersonalizerOverridesResponse> GetOverridesAsync(string userId)
        {
            var r = await _rpc.CallAsync<IVXPersonalizerOverridesResponse>(RPC_GET_OVERRIDES, new { userId });
            return r.success ? r.data : new IVXPersonalizerOverridesResponse();
        }

        public async Task<IVXPersonalizerPreviewResponse> PreviewConfigAsync(string userId, string system)
        {
            var r = await _rpc.CallAsync<IVXPersonalizerPreviewResponse>(RPC_PREVIEW, new { userId, system });
            return r.success ? r.data : null;
        }
    }
}
