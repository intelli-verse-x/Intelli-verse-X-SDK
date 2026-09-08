using System.IO;
using IntelliVerseX.Core;
using IntelliVerseX.Monetization;
using NUnit.Framework;
using UnityEngine;

namespace IntelliVerseX.Tests.Editor
{
    [TestFixture]
    public class IVXP1EditorTests
    {
        [Test]
        public void WebGLAdsConfig_ApplixirOffByDefault()
        {
            var config = ScriptableObject.CreateInstance<IVXWebGLAdsConfig>();
            Assert.IsFalse(config.enableApplixir, "Applixir must be opt-in (P1).");
            Object.DestroyImmediate(config);
        }

        [Test]
        public void AdsConfig_AppLixirOffByDefault()
        {
            var config = new IVXAdsConfig();
            Assert.IsFalse(config.enableAppLixir, "AppLixir must be opt-in (P1).");
        }

        [Test]
        public void PackageJson_DependenciesExcludePhotonAndAppodeal()
        {
            // Sandbox: Assets/ → editor/ → unity/ → SDKs/ → repo → Packages/com.intelliversex.sdk
            string packageJson = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", "..", "..", "Packages", "com.intelliversex.sdk", "package.json"));
            Assert.IsTrue(File.Exists(packageJson), "Expected Packages/com.intelliversex.sdk/package.json at " + packageJson);

            string json = File.ReadAllText(packageJson);
            int depsStart = json.IndexOf("\"dependencies\"");
            int extStart = json.IndexOf("\"_ivx_externalDependencies\"");
            Assert.Greater(depsStart, 0);
            Assert.Greater(extStart, depsStart);

            string upmDeps = json.Substring(depsStart, extStart - depsStart);
            Assert.IsFalse(upmDeps.Contains("photon"), "Photon must not be a UPM package dependency");
            Assert.IsFalse(upmDeps.Contains("appodeal"), "Appodeal must not be a UPM package dependency");
            Assert.IsTrue(upmDeps.Contains("com.unity.nuget.newtonsoft-json"));
            Assert.IsTrue(json.Contains("\"testables\""), "package.json must declare testables for Tests~ discovery");
        }
    }
}
