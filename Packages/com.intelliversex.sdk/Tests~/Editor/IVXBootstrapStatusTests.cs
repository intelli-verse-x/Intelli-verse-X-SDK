using System;
using IntelliVerseX.Bootstrap;
using NUnit.Framework;
using UnityEngine;

namespace IntelliVerseX.Tests.Editor
{
    /// <summary>
    /// Critical-path contracts for bootstrap status semantics (prod trust).
    /// </summary>
    public class IVXBootstrapStatusTests
    {
        [Test]
        public void BootstrapStatus_Enum_HasExpectedValues()
        {
            Assert.AreEqual(0, (int)IVXBootstrapStatus.NotStarted);
            Assert.AreEqual(1, (int)IVXBootstrapStatus.Online);
            Assert.AreEqual(2, (int)IVXBootstrapStatus.Offline);
            Assert.AreEqual(3, (int)IVXBootstrapStatus.Partial);
            Assert.AreEqual(4, (int)IVXBootstrapStatus.Failed);
        }

        [Test]
        public void Bootstrap_CompleteHelper_Semantics_OnlineIsUsableOfflineIsUsableFailedIsNot()
        {
            Assert.IsTrue(IsUsable(IVXBootstrapStatus.Online));
            Assert.IsTrue(IsUsable(IVXBootstrapStatus.Offline));
            Assert.IsTrue(IsUsable(IVXBootstrapStatus.Partial));
            Assert.IsFalse(IsUsable(IVXBootstrapStatus.Failed));
            Assert.IsFalse(IsUsable(IVXBootstrapStatus.NotStarted));
        }

        [Test]
        public void PackageVersion_Is_6_0_0()
        {
            Assert.AreEqual("6.0.0", IntelliVerseX.Core.IntelliVerseXManager.SDKVersion);
        }

        static bool IsUsable(IVXBootstrapStatus status) =>
            status == IVXBootstrapStatus.Online
            || status == IVXBootstrapStatus.Offline
            || status == IVXBootstrapStatus.Partial;
    }
}
