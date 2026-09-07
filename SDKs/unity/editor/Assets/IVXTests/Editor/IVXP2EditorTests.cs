using System;
using System.Reflection;
using System.Threading.Tasks;
using IntelliVerseX.Backend.Nakama;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Core;
using NUnit.Framework;
using UnityEngine;

namespace IntelliVerseX.Tests.Editor
{
    [TestFixture]
    public class IVXP2EditorTests
    {
        [Test]
        public void RequestBus_ParsesRetryAfterMs()
        {
            Assert.AreEqual(2500, IVXRequestBus.TryParseRetryAfterMs("{\"ok\":false,\"retry_after_ms\":2500}"));
            Assert.IsNull(IVXRequestBus.TryParseRetryAfterMs("{\"ok\":true}"));
            Assert.IsNull(IVXRequestBus.TryParseRetryAfterMs(null));
            Assert.IsNull(IVXRequestBus.TryParseRetryAfterMs("{\"retry_after_ms\":0}"));
        }

        [Test]
        public async Task RequestBus_HonorsRetryAfterOnSoftFailure()
        {
            int calls = 0;
            var result = await IVXRequestBus.ExecuteAsync(
                "test_rpc",
                token =>
                {
                    calls++;
                    if (calls == 1)
                        return Task.FromResult("{\"success\":false,\"retry_after_ms\":50}");
                    return Task.FromResult("{\"success\":true}");
                },
                maxAttempts: 3,
                timeoutMs: 2000,
                baseDelaySeconds: 0.05f,
                isSuccessPayload: p => p != null && p.Contains("\"success\":true"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Attempts);
            Assert.AreEqual(2, calls);
        }

        [Test]
        public void BootstrapConfig_EmptyGameId_FailsFast()
        {
            var config = ScriptableObject.CreateInstance<IVXBootstrapConfig>();
            Assert.IsFalse(config.Validate(), "Empty Game ID must fail validation (P2).");
            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void IdentityGate_WalletThrowsWhenNotSynced()
        {
            var go = new GameObject("IVXP2_IdentityGate");
            try
            {
                var mgr = go.AddComponent<IVXNManager>();
                var field = typeof(IVXNManager).GetField(
                    "_identitySyncSucceeded",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(field, "Expected _identitySyncSucceeded field");
                field.SetValue(mgr, false);

                var ex = Assert.Throws<InvalidOperationException>(() => mgr.EnsureIdentitySyncedOrThrow());
                StringAssert.Contains("Identity sync required", ex.Message);
                StringAssert.Contains("create_or_sync_user", ex.Message);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
