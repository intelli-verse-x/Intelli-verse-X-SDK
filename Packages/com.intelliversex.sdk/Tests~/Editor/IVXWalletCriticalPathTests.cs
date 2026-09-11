using System.Threading.Tasks;
using IntelliVerseX.Core;
using NUnit.Framework;

namespace IntelliVerseX.Tests.Editor
{
    /// <summary>
    /// Critical-path wallet consolidation: IVXNWalletManager is the canonical API.
    /// </summary>
    public class IVXWalletCriticalPathTests
    {
        [TearDown]
        public void TearDown()
        {
            IVXNWalletManager.Initialize(0, 0);
        }

        [Test]
        public void WalletManager_Initialize_ExposesBalances()
        {
            IVXNWalletManager.Initialize(100, 250);
            Assert.IsTrue(IVXNWalletManager.IsInitialized);
            Assert.AreEqual(100, IVXNWalletManager.GameBalance);
            Assert.AreEqual(250, IVXNWalletManager.GlobalBalance);
        }

        [Test]
        public void WalletManager_Snapshot_RawJson_IsStable()
        {
            IVXNWalletManager.Initialize(42, 7);
            string raw = $"{{\"game\":{IVXNWalletManager.GameBalance},\"global\":{IVXNWalletManager.GlobalBalance}}}";
            StringAssert.Contains("\"game\":42", raw);
            StringAssert.Contains("\"global\":7", raw);
        }

#pragma warning disable CS0618
        [Test]
        public async Task ApiClient_GetWalletBalance_IsRetiredStub()
        {
            var response = await IntelliVerseX.Identity.IVXAPIClient.GetWalletBalanceAsync();
            Assert.IsFalse(response.success);
            StringAssert.Contains("IVXNWalletManager", response.message);
        }
#pragma warning restore CS0618
    }
}
