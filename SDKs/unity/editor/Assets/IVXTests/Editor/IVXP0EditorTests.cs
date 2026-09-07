using System.Reflection;
using IntelliVerseX.Bootstrap;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Tests.Editor
{
    [TestFixture]
    public class IVXP0EditorTests
    {
        [Test]
        public void Menu_ControlCenterExists()
        {
            var type = typeof(IntelliVerseX.Editor.IVXControlCenter);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            bool hasControlCenter = false;

            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(typeof(MenuItem), false);
                for (int i = 0; i < attrs.Length; i++)
                {
                    var item = (MenuItem)attrs[i];
                    Assert.AreNotEqual(
                        "IntelliVerseX/SDK Setup Wizard",
                        item.menuItem,
                        "SDK Setup Wizard menu alias must not exist; use Control Center.");
                    if (item.menuItem == "IntelliVerseX/Control Center")
                    {
                        hasControlCenter = true;
                    }
                }
            }

            Assert.IsTrue(hasControlCenter, "IVXControlCenter should have IntelliVerseX/Control Center");
        }

        [Test]
        public void BootstrapConfig_EmptyGameId_IsInvalid()
        {
            var config = ScriptableObject.CreateInstance<IVXBootstrapConfig>();
            Assert.IsFalse(config.Validate(), "Empty Game ID must fail validation.");
            Object.DestroyImmediate(config);
        }
    }
}
