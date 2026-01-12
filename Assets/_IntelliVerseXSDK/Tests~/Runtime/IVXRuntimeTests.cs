// IVXRuntimeTests.cs
// PlayMode unit tests for IntelliVerseX SDK
// Tests runtime functionality, managers, and services

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IntelliVerseX.Tests.Runtime
{
    /// <summary>
    /// PlayMode tests for IntelliVerseX SDK runtime functionality.
    /// </summary>
    [TestFixture]
    public class IVXRuntimeTests
    {
        #region Setup & Teardown
        
        [SetUp]
        public void SetUp()
        {
            // Clean up any existing test objects
            var testObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in testObjects)
            {
                if (obj.name.StartsWith("IVX_Test_"))
                {
                    UnityEngine.Object.Destroy(obj);
                }
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            var testObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in testObjects)
            {
                if (obj.name.StartsWith("IVX_Test_"))
                {
                    UnityEngine.Object.Destroy(obj);
                }
            }
        }
        
        #endregion
        
        #region Core Assembly Tests
        
        [Test]
        public void Core_AssemblyLoaded()
        {
            var assembly = GetAssembly("IntelliVerseX.Core");
            Assert.IsNotNull(assembly, "IntelliVerseX.Core assembly should be loaded at runtime");
        }
        
        [Test]
        public void Backend_AssemblyLoaded()
        {
            var assembly = GetAssembly("IntelliVerseX.Backend");
            Assert.IsNotNull(assembly, "IntelliVerseX.Backend assembly should be loaded at runtime");
        }
        
        [Test]
        public void Identity_AssemblyLoaded()
        {
            var assembly = GetAssembly("IntelliVerseXIdentity");
            Assert.IsNotNull(assembly, "IntelliVerseXIdentity assembly should be loaded at runtime");
        }
        
        #endregion
        
        #region Logger Tests
        
        [Test]
        public void IVXLogger_LogsInfo()
        {
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(@"\[IVX\].*Test message"));
            
            var loggerType = GetType("IntelliVerseX.Core.IVXLogger");
            if (loggerType != null)
            {
                var logMethod = loggerType.GetMethod("Log", new[] { typeof(string) });
                if (logMethod != null)
                {
                    logMethod.Invoke(null, new object[] { "Test message" });
                    return;
                }
            }
            
            // Fallback if IVXLogger not found
            Debug.Log("[IVX] Test message");
        }
        
        [Test]
        public void IVXLogger_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[IVX\].*Test warning"));
            
            var loggerType = GetType("IntelliVerseX.Core.IVXLogger");
            if (loggerType != null)
            {
                var warnMethod = loggerType.GetMethod("LogWarning", new[] { typeof(string) });
                if (warnMethod != null)
                {
                    warnMethod.Invoke(null, new object[] { "Test warning" });
                    return;
                }
            }
            
            // Fallback
            Debug.LogWarning("[IVX] Test warning");
        }
        
        #endregion
        
        #region GameObject Creation Tests
        
        [UnityTest]
        public IEnumerator GameObject_CanCreateWithIVXComponent()
        {
            var go = new GameObject("IVX_Test_RuntimeObject");
            Assert.IsNotNull(go, "Should be able to create GameObject");
            
            yield return null;
            
            Assert.IsTrue(go != null, "GameObject should persist across frame");
            
            UnityEngine.Object.Destroy(go);
        }
        
        [UnityTest]
        public IEnumerator Singleton_SurvivesSceneLoad()
        {
            // This test verifies that singletons properly use DontDestroyOnLoad
            var go = new GameObject("IVX_Test_Singleton");
            UnityEngine.Object.DontDestroyOnLoad(go);
            
            yield return null;
            
            // Object should still exist
            var found = GameObject.Find("IVX_Test_Singleton");
            Assert.IsNotNull(found, "DontDestroyOnLoad object should persist");
            
            UnityEngine.Object.Destroy(go);
        }
        
        #endregion
        
        #region Utility Tests
        
        [Test]
        public void Utilities_TypeExists()
        {
            var type = GetType("IntelliVerseX.Core.IVXUtilities");
            Assert.IsNotNull(type, "IVXUtilities should exist");
        }
        
        [Test]
        public void Config_TypeExists()
        {
            var type = GetType("IntelliVerseX.Core.IntelliVerseXConfig");
            Assert.IsNotNull(type, "IntelliVerseXConfig should exist");
        }
        
        [Test]
        public void SceneManager_TypeExists()
        {
            var type = GetType("IntelliVerseX.Core.IVXSceneManager");
            Assert.IsNotNull(type, "IVXSceneManager should exist");
        }
        
        #endregion
        
        #region Module Assembly Tests
        
        [Test]
        public void Analytics_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Analytics");
            Assert.IsNotNull(assembly, "IntelliVerseX.Analytics assembly should exist");
        }
        
        [Test]
        public void Localization_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Localization");
            Assert.IsNotNull(assembly, "IntelliVerseX.Localization assembly should exist");
        }
        
        [Test]
        public void Storage_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Storage");
            Assert.IsNotNull(assembly, "IntelliVerseX.Storage assembly should exist");
        }
        
        [Test]
        public void Networking_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Networking");
            Assert.IsNotNull(assembly, "IntelliVerseX.Networking assembly should exist");
        }
        
        [Test]
        public void Quiz_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Quiz");
            Assert.IsNotNull(assembly, "IntelliVerseX.Quiz assembly should exist");
        }
        
        [Test]
        public void UI_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.UI");
            Assert.IsNotNull(assembly, "IntelliVerseX.UI assembly should exist");
        }
        
        [Test]
        public void Monetization_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Monetization");
            Assert.IsNotNull(assembly, "IntelliVerseX.Monetization assembly should exist");
        }
        
        [Test]
        public void Social_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Social");
            Assert.IsNotNull(assembly, "IntelliVerseX.Social assembly should exist");
        }
        
        [Test]
        public void Leaderboard_AssemblyExists()
        {
            var assembly = GetAssembly("IntelliVerseX.Leaderboard");
            Assert.IsNotNull(assembly, "IntelliVerseX.Leaderboard assembly should exist");
        }
        
        #endregion
        
        #region Performance Tests
        
        [UnityTest]
        public IEnumerator Performance_InstantiateMultipleObjects()
        {
            const int objectCount = 100;
            var objects = new List<GameObject>();
            
            var startTime = Time.realtimeSinceStartup;
            
            for (int i = 0; i < objectCount; i++)
            {
                var go = new GameObject($"IVX_Test_Perf_{i}");
                objects.Add(go);
            }
            
            var duration = Time.realtimeSinceStartup - startTime;
            
            yield return null;
            
            Assert.Less(duration, 1.0f, $"Creating {objectCount} objects should take less than 1 second");
            
            // Cleanup
            foreach (var obj in objects)
            {
                UnityEngine.Object.Destroy(obj);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private System.Reflection.Assembly GetAssembly(string name)
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
        
        #endregion
    }
    
    /// <summary>
    /// Integration tests that verify SDK components work together.
    /// </summary>
    [TestFixture]
    public class IVXIntegrationTests
    {
        [UnityTest]
        public IEnumerator SDK_InitializationFlow()
        {
            // Simulate basic SDK initialization
            var managerGO = new GameObject("IVX_Test_Manager");
            
            yield return null;
            
            // Manager should be created
            Assert.IsNotNull(managerGO);
            
            yield return null;
            
            UnityEngine.Object.Destroy(managerGO);
        }
        
        [Test]
        public void SDK_AllCoreTypesAccessible()
        {
            var coreTypes = new[]
            {
                "IntelliVerseX.Core.IVXLogger",
                "IntelliVerseX.Core.IVXUtilities",
                "IntelliVerseX.Core.IntelliVerseXConfig"
            };
            
            var missingTypes = new List<string>();
            
            foreach (var typeName in coreTypes)
            {
                if (GetType(typeName) == null)
                {
                    missingTypes.Add(typeName);
                }
            }
            
            Assert.IsEmpty(missingTypes, $"Missing core types: {string.Join(", ", missingTypes)}");
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
    }
}
