using System;
using System.Reflection;
using System.Threading.Tasks;
using IntelliVerseX.Core;
using IntelliVerseX.Editor;
using IntelliVerseX.Games.Leaderboard;
using NUnit.Framework;

namespace IntelliVerseX.Tests.Editor
{
    [TestFixture]
    public class IVXPDoDEditorTests
    {
        [Test]
        public void CanonicalTypes_WalletAndLeaderboard_ArePublic()
        {
            Assert.IsNotNull(typeof(IVXNWalletManager));
            Assert.IsNotNull(typeof(IntelliVerseX.Backend.Nakama.IVXNLeaderbordManager));
        }

        [Test]
        public void LegacyTypes_AreObsolete()
        {
            Assert.IsNotNull(typeof(IVXWalletManager).GetCustomAttribute<ObsoleteAttribute>());
            Assert.IsNotNull(typeof(IVXPhotonConfig).GetCustomAttribute<ObsoleteAttribute>());
            Assert.IsNotNull(typeof(IVXGLeaderboardManager).GetCustomAttribute<ObsoleteAttribute>());
            Assert.IsNotNull(typeof(IVXGLeaderboard).GetCustomAttribute<ObsoleteAttribute>());
        }

        [Test]
        public void PhotonConfig_ShipsNoSharedAppId()
        {
#pragma warning disable CS0618
            Assert.AreEqual(string.Empty, IVXPhotonConfig.SHARED_APP_ID_REALTIME);
            Assert.AreEqual(string.Empty, IVXPhotonConfig.GetAppId());
#pragma warning restore CS0618
        }

        [Test]
        public async Task RequestBus_RecordsTrafficHistory()
        {
            IVXRequestBus.ClearTrafficHistory();
            var result = await IVXRequestBus.ExecuteAsync(
                "dod_traffic_probe",
                token => Task.FromResult("{\"success\":true}"),
                maxAttempts: 1,
                timeoutMs: 2000,
                isSuccessPayload: p => p != null && p.Contains("success"));

            Assert.IsTrue(result.Success);
            var rows = IVXRequestBus.GetRecentTraffic();
            Assert.GreaterOrEqual(rows.Length, 1);
            Assert.AreEqual("dod_traffic_probe", rows[0].RpcId);
            Assert.AreEqual("ok", rows[0].Status);
        }

        [Test]
        public void ControlCenter_TypeExists()
        {
            Assert.IsNotNull(typeof(IVXControlCenter));
        }
    }
}
