using System.Reflection;
using IntelliVerseX.Backend.Nakama;
using IntelliVerseX.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Tests.Editor
{
    [TestFixture]
    public class IVXP3EditorTests
    {
        [Test]
        public void Menu_TopLevelOnly_ControlCenter_Advanced_Maintainers()
        {
            var forbiddenPrefixes = new[]
            {
                "IntelliVerseX/Documentation/",
                "IntelliVerseX/Tools/",
                "IntelliVerseX/SDK Tools/",
                "IntelliVerseX/SDK Status/",
                "IntelliVerseX/Quick Actions/",
                "IntelliVerseX/SDK Setup Wizard",
                "IntelliVerseX/GitHub Repository",
                "IntelliVerseX/Report Issue",
                "IntelliVerseX/About IntelliVerseX SDK"
            };

            bool hasControlCenter = false;
            bool hasAdvanced = false;
            bool hasMaintainerExport = false;

            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                if (types == null)
                    continue;

                for (int t = 0; t < types.Length; t++)
                {
                    var type = types[t];
                    if (type == null)
                        continue;

                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    for (int m = 0; m < methods.Length; m++)
                    {
                        var attrs = methods[m].GetCustomAttributes(typeof(MenuItem), false);
                        for (int a = 0; a < attrs.Length; a++)
                        {
                            string path = ((MenuItem)attrs[a]).menuItem;
                            if (string.IsNullOrEmpty(path) || !path.StartsWith("IntelliVerseX/"))
                                continue;

                            for (int f = 0; f < forbiddenPrefixes.Length; f++)
                            {
                                Assert.IsFalse(
                                    path.StartsWith(forbiddenPrefixes[f]) || path == forbiddenPrefixes[f],
                                    "Unexpected top-level IVX menu: " + path);
                            }

                            if (path == "IntelliVerseX/Control Center")
                                hasControlCenter = true;
                            if (path == "IntelliVerseX/Advanced Setup")
                                hasAdvanced = true;
                            if (path.StartsWith("IntelliVerseX/Maintainers/"))
                                hasMaintainerExport = true;
                        }
                    }
                }
            }

            Assert.IsTrue(hasControlCenter);
            Assert.IsTrue(hasAdvanced);
            Assert.IsTrue(hasMaintainerExport, "Expected at least one Maintainers menu item");
        }

        [Test]
        public void AdvancedSetup_AndDependencies_Exist_NoFatWizardType()
        {
            Assert.IsNotNull(typeof(IVXAdvancedSetup));
            Assert.IsNotNull(typeof(IVXDependencies));
            Assert.IsNull(System.Type.GetType("IntelliVerseX.Editor.IVXSDKSetupWizard, IntelliVerseX.Editor"));
        }

        [Test]
        public void Dependencies_GetStatus_DoesNotThrow()
        {
            var status = IVXDependencies.GetStatus();
            Assert.IsNotNull(status);
        }

        [Test]
        public void IVXNManager_HasNoLegacyIntelliVerseXConfigField()
        {
            var field = typeof(IVXNManager).GetField(
                "sdkConfig",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNull(field, "IVXNManager must not keep IntelliVerseXConfig fallback field");
        }
    }
}
