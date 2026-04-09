using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXRewardBucketSystem
    {
        private const string RPC_GET = "hiro_reward_bucket_get";
        private const string RPC_PROGRESS = "hiro_reward_bucket_progress";
        private const string RPC_UNLOCK = "hiro_reward_bucket_unlock";

        private readonly IVXHiroRpcClient _rpc;

        public IVXRewardBucketSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXRewardBucketGetResponse> GetBucketsAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXRewardBucketGetResponse>(RPC_GET, new { gameId });
            return r.success ? r.data : new IVXRewardBucketGetResponse();
        }

        public async Task<IVXRewardBucketProgressResponse> AddProgressAsync(string bucketId, int amount, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXRewardBucketProgressResponse>(RPC_PROGRESS, new { bucketId, amount, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXRewardBucketUnlockResponse> UnlockTierAsync(string bucketId, int tierIndex, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXRewardBucketUnlockResponse>(RPC_UNLOCK, new { bucketId, tierIndex, gameId });
            return r.success ? r.data : null;
        }
    }
}
