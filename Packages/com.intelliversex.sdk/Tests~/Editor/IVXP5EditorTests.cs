using System.IO;
using IntelliVerseX.Editor;
using NUnit.Framework;
using UnityEngine;

namespace IntelliVerseX.Tests.Editor
{
    [TestFixture]
    public class IVXP5EditorTests
    {
        [Test]
        public void RpcIndex_LoadsGeneratedCatalog()
        {
            IVXRpcIndex.Invalidate();
            var entries = IVXRpcIndex.GetEntries();
            Assert.GreaterOrEqual(entries.Length, 50, "Expected generated RPC index with many facade ids");
            bool hasIdentity = false;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null && entries[i].id == "create_or_sync_user")
                    hasIdentity = true;
            }
            Assert.IsTrue(hasIdentity, "create_or_sync_user must be in the index");
        }

        [Test]
        public void OptionalPackages_ExistBesideCore()
        {
            string data = Application.dataPath;
            string repo = Path.GetFullPath(Path.Combine(data, "..", "..", "..", ".."));
            Assert.IsTrue(Directory.Exists(Path.Combine(repo, "Packages", "com.intelliversex.sdk.ai")));
            Assert.IsTrue(Directory.Exists(Path.Combine(repo, "Packages", "com.intelliversex.sdk.discord")));
            Assert.IsTrue(Directory.Exists(Path.Combine(repo, "Packages", "com.intelliversex.sdk.photon")));
            Assert.IsTrue(File.Exists(Path.Combine(repo, "Packages", "com.intelliversex.sdk.ai", "package.json")));
            Assert.IsTrue(File.Exists(Path.Combine(repo, "Packages", "com.intelliversex.sdk.discord", "package.json")));
            Assert.IsTrue(File.Exists(Path.Combine(repo, "Packages", "com.intelliversex.sdk.photon", "package.json")));
        }

        [Test]
        public void DualTree_FlattenedModulesAtPackageRoot()
        {
            string data = Application.dataPath;
            string pkg = Path.GetFullPath(Path.Combine(data, "..", "..", "..", "..", "Packages", "com.intelliversex.sdk"));
            Assert.IsTrue(Directory.Exists(Path.Combine(pkg, "Quest")));
            Assert.IsTrue(Directory.Exists(Path.Combine(pkg, "Platform")));
            Assert.IsTrue(Directory.Exists(Path.Combine(pkg, "Multiplayer")));
            Assert.IsFalse(Directory.Exists(Path.Combine(pkg, "_IntelliVerseXSDK", "Quest")));
            Assert.IsFalse(Directory.Exists(Path.Combine(pkg, "_IntelliVerseXSDK", "AI")));
            Assert.IsFalse(Directory.Exists(Path.Combine(pkg, "_IntelliVerseXSDK", "Discord")));
        }
    }
}
