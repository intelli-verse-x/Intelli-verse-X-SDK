using System;
using System.IO;
using System.Linq;
using System.Reflection;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Core;
using IntelliVerseX.Editor;
using IntelliVerseX.Identity;
using IntelliVerseX.Storage;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Tests.Editor
{
    /// <summary>
    /// High-signal quality / security / session evals that keep consumer UX and local-data contracts honest.
    /// Category: IVX.Quality
    /// </summary>
    [TestFixture]
    [Category("IVX.Quality")]
    public class IVXQualityEvalsEditorTests : IVXLocalDataTestFixture
    {
        private static string PackageRoot
        {
            get
            {
                return Path.GetFullPath(Path.Combine(
                    Application.dataPath, "..", "..", "..", "..", "Packages", "com.intelliversex.sdk"));
            }
        }

        [Test]
        public void BootstrapConfig_DefaultsToCloudNakamaNotLocalhost()
        {
            var config = ScriptableObject.CreateInstance<IVXBootstrapConfig>();
            try
            {
                Assert.AreEqual(IVXNakamaConfig.HOST, config.ServerHost);
                Assert.AreEqual(IVXNakamaConfig.PORT, config.ServerPort);
                Assert.AreEqual(IVXNakamaConfig.SERVER_KEY, config.ServerKey);
                Assert.IsTrue(config.UseSSL, "Cloud bootstrap must default UseSSL=true");
                Assert.AreNotEqual("127.0.0.1", config.ServerHost);
                Assert.AreNotEqual(7350, config.ServerPort);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void ControlCenter_SourceOmitsConsumerNakamaServerUi()
        {
            string uitk = Path.Combine(PackageRoot, "Editor", "IVXControlCenter.UIToolkit.cs");
            string imgui = Path.Combine(PackageRoot, "Editor", "IVXControlCenter.ConnectWizard.cs");
            Assert.IsTrue(File.Exists(uitk), uitk);
            Assert.IsTrue(File.Exists(imgui), imgui);

            string uitkSrc = File.ReadAllText(uitk);
            StringAssert.Contains("Intentionally empty", uitkSrc);
            StringAssert.Contains("BuildServerUi omitted from consumer Connect surface", uitkSrc);
            // Empty stub must not recreate TextField("Server key")
            Assert.IsFalse(
                uitkSrc.Contains("new TextField(\"Server key\")"),
                "Control Center must not build a Server key field for consumers");

            string imguiSrc = File.ReadAllText(imgui);
            Assert.IsFalse(
                imguiSrc.Contains("DrawNakamaServerFoldout();"),
                "IMGUI Connect must not call DrawNakamaServerFoldout");
            StringAssert.Contains("Nakama host/port/key intentionally omitted", imguiSrc);
        }

        [Test]
        public void AdvancedSetup_ExposesBackendTabForMaintainers()
        {
            var field = typeof(IVXAdvancedSetup).GetField(
                "TabNames",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "Expected TabNames on IVXAdvancedSetup");
            var names = (string[])field.GetValue(null);
            CollectionAssert.Contains(names, "Backend");
            CollectionAssert.Contains(names, "Dependencies");

            Assert.IsNotNull(
                typeof(IVXAdvancedSetup).GetMethod("DrawBackend", BindingFlags.Instance | BindingFlags.NonPublic),
                "Maintainers Backend drawer must exist");
        }

        [Test]
        public void StaleMenus_ValidateHidden_ConnectToolkitAndHyphenatedSamples()
        {
            Assert.IsFalse(
                Menu.GetEnabled("IntelliVerseX/Connect (UI Toolkit)"),
                "Connect (UI Toolkit) must stay disabled/hidden");

            // Hyphenated legacy sample menu is validate=false (may still appear grayed until domain reload).
            var legacy = typeof(IVXUITKSceneBootstrap).GetMethod(
                "MenuCreateLegacyValidate",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(legacy);
            Assert.IsFalse((bool)legacy.Invoke(null, null));
        }

        [Test]
        public void PackageJson_DeclaresTestables()
        {
            string packageJson = Path.Combine(PackageRoot, "package.json");
            Assert.IsTrue(File.Exists(packageJson));
            string json = File.ReadAllText(packageJson);
            StringAssert.Contains("\"testables\"", json);
            StringAssert.Contains("com.intelliversex.sdk", json);
        }

        [Test]
        public void LocalDataKeys_CatalogCoversSessionRememberMeAndNakama()
        {
            CollectionAssert.Contains(IVXLocalDataKeys.AllSdkKeys, IVXLocalDataKeys.UserSession);
            CollectionAssert.Contains(IVXLocalDataKeys.AllSdkKeys, IVXLocalDataKeys.RememberMe);
            CollectionAssert.Contains(IVXLocalDataKeys.AuthSessionKeys, IVXLocalDataKeys.NakamaAuthTokenV2);
            CollectionAssert.Contains(IVXLocalDataKeys.PlaintextMigrationKeys, IVXLocalDataKeys.BootstrapNakamaSession);
            // Device id is wipe-all only, not logout
            CollectionAssert.DoesNotContain(IVXLocalDataKeys.AuthSessionKeys, IVXLocalDataKeys.DeviceId);
            CollectionAssert.Contains(IVXLocalDataKeys.AllSdkKeys, IVXLocalDataKeys.DeviceId);
        }

        [Test]
        public void Logout_ClearsAuthSession_ButCanKeepRememberedEmail()
        {
            UserSessionManager.ApplyLoginResponse(MakeLoginResponse(), persist: true);
            Assert.IsTrue(UserSessionManager.HasSession);
            Assert.AreEqual("qa@intelliversex.test", UserSessionManager.LastEmail);

            IVXAPIClient.Logout();

            Assert.IsFalse(UserSessionManager.HasSession);
            Assert.IsFalse(IVXSecureStorage.HasKey(IVXLocalDataKeys.UserSession));
            // Logout keeps remember-me email for UX (clearRememberMe: false)
            Assert.AreEqual("qa@intelliversex.test", UserSessionManager.LastEmail);
        }

        [Test]
        public void TryRestoreSession_ReturnsFalseWhenNothingPersisted()
        {
            UserSessionManager.ClearAllLocalData();
            Assert.IsFalse(IVXAPIClient.TryRestoreSession());
            Assert.IsFalse(IVXAPIClient.IsLoggedIn);
        }

        [Test]
        public void TryRestoreSession_TrueAfterPersistedLogin()
        {
            UserSessionManager.ApplyLoginResponse(MakeLoginResponse(expiresIn: 7200), persist: true);
            UserSessionManager.Current = null;

            Assert.IsTrue(IVXAPIClient.TryRestoreSession());
            Assert.IsTrue(UserSessionManager.HasSession);
            Assert.IsTrue(UserSessionManager.IsAccessTokenFresh());
        }

        [Test]
        public void SecureStorage_SetSecureString_DoesNotDeleteOwnKey()
        {
            // Regression: earlier IVXLocalData deleted the key immediately after SetString.
            IVXLocalData.SetSecureString(IVXLocalDataKeys.DeviceId, "device-regression");
            Assert.IsTrue(PlayerPrefs.HasKey(IVXLocalDataKeys.DeviceId));
            Assert.AreEqual("device-regression", IVXLocalData.GetSecureOrMigrate(IVXLocalDataKeys.DeviceId, ""));
            Assert.AreNotEqual("device-regression", PlayerPrefs.GetString(IVXLocalDataKeys.DeviceId, ""));
        }

        [Test]
        public void BootstrapConfigEditor_TypeExistsForMaskedKeyInspector()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types ?? Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t != null && t.Name == "IVXBootstrapConfigEditor");

            Assert.IsNotNull(type, "Custom inspector should mask Nakama server key");
            Assert.IsTrue(typeof(UnityEditor.Editor).IsAssignableFrom(type));
        }
    }
}
