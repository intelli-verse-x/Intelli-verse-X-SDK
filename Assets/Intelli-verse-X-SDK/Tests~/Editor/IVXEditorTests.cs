// IVXEditorTests.cs
// EditMode unit tests for IntelliVerseX SDK
// Tests core functionality, utilities, and editor tools

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace IntelliVerseX.Tests.Editor
{
    /// <summary>
    /// EditMode tests for IntelliVerseX SDK core functionality.
    /// </summary>
    [TestFixture]
    public class IVXEditorTests
    {
        #region Assembly Definition Tests
        
        [Test]
        public void Core_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Core");
            Assert.IsNotNull(assembly, "IntelliVerseX.Core assembly should exist");
        }
        
        [Test]
        public void Editor_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Editor");
            Assert.IsNotNull(assembly, "IntelliVerseX.Editor assembly should exist");
        }
        
        [Test]
        public void Tests_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Editor.Tests");
            Assert.IsNotNull(assembly, "IntelliVerseX.Editor.Tests assembly should exist");
        }
        
        #endregion
        
        #region Package.json Tests
        
        [Test]
        public void PackageJson_Exists()
        {
            string[] guids = AssetDatabase.FindAssets("package t:TextAsset", new[] { "Packages/com.intelliversex.sdk" });
            
            // If installed as package, check there
            if (guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets("package t:TextAsset", new[] { "Assets/_IntelliVerseXSDK" });
            }
            
            Assert.Greater(guids.Length, 0, "package.json should exist in the SDK folder");
        }
        
        [Test]
        public void PackageJson_HasRequiredFields()
        {
            string packagePath = GetPackageJsonPath();
            if (string.IsNullOrEmpty(packagePath))
            {
                Assert.Inconclusive("Could not locate package.json");
                return;
            }
            
            string json = System.IO.File.ReadAllText(packagePath);
            Assert.IsFalse(string.IsNullOrEmpty(json), "package.json should not be empty");
            
            // Check for required fields
            Assert.IsTrue(json.Contains("\"name\""), "package.json should have 'name' field");
            Assert.IsTrue(json.Contains("\"version\""), "package.json should have 'version' field");
            Assert.IsTrue(json.Contains("\"displayName\""), "package.json should have 'displayName' field");
            Assert.IsTrue(json.Contains("\"unity\""), "package.json should have 'unity' field");
        }
        
        [Test]
        public void PackageJson_VersionIsSemVer()
        {
            string packagePath = GetPackageJsonPath();
            if (string.IsNullOrEmpty(packagePath))
            {
                Assert.Inconclusive("Could not locate package.json");
                return;
            }
            
            string json = System.IO.File.ReadAllText(packagePath);
            
            // Extract version using simple parsing
            int versionStart = json.IndexOf("\"version\"");
            if (versionStart > 0)
            {
                int colonPos = json.IndexOf(':', versionStart);
                int quoteStart = json.IndexOf('"', colonPos + 1);
                int quoteEnd = json.IndexOf('"', quoteStart + 1);
                
                if (quoteStart > 0 && quoteEnd > quoteStart)
                {
                    string version = json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                    
                    // Validate semantic versioning
                    var semVerPattern = new System.Text.RegularExpressions.Regex(@"^\d+\.\d+\.\d+(-[\w.]+)?$");
                    Assert.IsTrue(semVerPattern.IsMatch(version), $"Version '{version}' should be valid semantic versioning");
                }
            }
        }
        
        #endregion
        
        #region Core Type Tests
        
        [Test]
        public void IVXLogger_TypeExists()
        {
            var type = GetType("IntelliVerseX.Core.IVXLogger");
            Assert.IsNotNull(type, "IVXLogger type should exist in IntelliVerseX.Core");
        }
        
        [Test]
        public void IVXSafeSingleton_TypeExists()
        {
            var type = GetType("IntelliVerseX.Core.IVXSafeSingleton`1");
            Assert.IsNotNull(type, "IVXSafeSingleton<T> type should exist in IntelliVerseX.Core");
        }
        
        [Test]
        public void IVXUtilities_TypeExists()
        {
            var type = GetType("IntelliVerseX.Core.IVXUtilities");
            Assert.IsNotNull(type, "IVXUtilities type should exist in IntelliVerseX.Core");
        }
        
        #endregion
        
        #region Editor Menu Tests
        
        [Test]
        public void Menu_SetupWizardExists()
        {
            // Check that the menu item exists by looking for the type
            var type = GetType("IntelliVerseX.Editor.IVXSetupWizard");
            Assert.IsNotNull(type, "IVXSetupWizard type should exist");
            
            // Check for MenuItem attribute
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            bool hasMenuItem = false;
            
            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(typeof(MenuItem), false);
                if (attrs.Length > 0)
                {
                    hasMenuItem = true;
                    break;
                }
            }
            
            Assert.IsTrue(hasMenuItem, "IVXSetupWizard should have a MenuItem");
        }
        
        [Test]
        public void Menu_DependencyCheckerExists()
        {
            var type = GetType("IntelliVerseX.Editor.IVXDependencyChecker");
            Assert.IsNotNull(type, "IVXDependencyChecker type should exist");
        }
        
        [Test]
        public void Menu_ProjectSetupExists()
        {
            var type = GetType("IntelliVerseX.Editor.IVXProjectSetup");
            Assert.IsNotNull(type, "IVXProjectSetup type should exist");
        }
        
        #endregion
        
        #region Helper Methods
        
        private Assembly GetAssembly(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == name)
                {
                    return assembly;
                }
            }
            return null;
        }
        
        private Type GetType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
        
        private string GetPackageJsonPath()
        {
            // Try package location first
            string packagePath = "Packages/com.intelliversex.sdk/package.json";
            if (System.IO.File.Exists(packagePath))
            {
                return packagePath;
            }
            
            // Try Assets location
            packagePath = "Assets/_IntelliVerseXSDK/package.json";
            if (System.IO.File.Exists(packagePath))
            {
                return packagePath;
            }
            
            return null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Tests for SDK configuration and settings.
    /// </summary>
    [TestFixture]
    public class IVXConfigurationTests
    {
        [Test]
        public void ScriptingDefines_INTELLIVERSEX_SDK_Exists()
        {
            #if INTELLIVERSEX_SDK
            Assert.Pass("INTELLIVERSEX_SDK define symbol is present");
            #else
            Assert.Inconclusive("INTELLIVERSEX_SDK define symbol is not present. Run Project Setup to add it.");
            #endif
        }
        
        [Test]
        public void Namespaces_FollowConvention()
        {
            // Check that core types follow the IntelliVerseX namespace convention
            var coreAssembly = GetAssembly("IntelliVerseX.Core");
            if (coreAssembly == null)
            {
                Assert.Inconclusive("IntelliVerseX.Core assembly not found");
                return;
            }
            
            var types = coreAssembly.GetTypes();
            var invalidTypes = new List<string>();
            
            foreach (var type in types)
            {
                if (type.IsPublic && !type.Namespace?.StartsWith("IntelliVerseX") == true)
                {
                    invalidTypes.Add($"{type.FullName}");
                }
            }
            
            Assert.IsEmpty(invalidTypes, $"All public types should be in IntelliVerseX namespace. Found: {string.Join(", ", invalidTypes)}");
        }
        
        private Assembly GetAssembly(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == name)
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}
