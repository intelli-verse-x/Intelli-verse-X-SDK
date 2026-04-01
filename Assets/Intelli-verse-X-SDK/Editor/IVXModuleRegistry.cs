// File: IVXModuleRegistry.cs
// Purpose: Central registry for all SDK modules and their dependencies
// Version: 1.0.0

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Central registry containing metadata for all SDK modules.
    /// Used by the setup wizard and other tools to understand module relationships.
    /// </summary>
    public static class IVXModuleRegistry
    {
        #region Module Definitions

        /// <summary>
        /// All available SDK modules
        /// </summary>
        public enum ModuleType
        {
            // Core Modules
            Core,
            Identity,
            Backend,
            Networking,
            Storage,
            
            // Feature Modules
            Quiz,
            QuizUI,
            Leaderboard,
            Wallet,
            Social,
            Localization,
            
            // Monetization Modules
            IAP,
            Ads,
            Analytics,
            
            // UI Modules
            UI,
            IntroScene,
            
            // Utility Modules
            Diagnostics,
            Resources
        }

        /// <summary>
        /// Module metadata container
        /// </summary>
        public class ModuleInfo
        {
            public ModuleType Type { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Namespace { get; set; }
            public string AssemblyName { get; set; }
            public string FolderPath { get; set; }
            public List<ModuleType> Dependencies { get; set; } = new List<ModuleType>();
            public List<string> RequiredScripts { get; set; } = new List<string>();
            public List<string> RequiredPrefabs { get; set; } = new List<string>();
            public bool IsOptional { get; set; } = false;
            public string SetupPriority { get; set; } = "Normal"; // "Critical", "High", "Normal", "Low"
        }

        #endregion

        #region Module Registry

        private static readonly Dictionary<ModuleType, ModuleInfo> _modules = new Dictionary<ModuleType, ModuleInfo>
        {
            // ========== CORE MODULES ==========
            {
                ModuleType.Core, new ModuleInfo
                {
                    Type = ModuleType.Core,
                    DisplayName = "Core",
                    Description = "Core SDK foundation with singletons, config, and utilities",
                    Namespace = "IntelliVerseX.Core",
                    AssemblyName = "IntelliVerseX.Core",
                    FolderPath = "Assets/_IntelliVerseXSDK/Core",
                    Dependencies = new List<ModuleType>(),
                    RequiredScripts = new List<string>
                    {
                        "IVXSafeSingleton.cs",
                        "IntelliVerseXConfig.cs",
                        "IntelliVerseXManager.cs",
                        "IntelliVerseXIdentity.cs",
                        "IVXLogger.cs"
                    },
                    SetupPriority = "Critical"
                }
            },
            {
                ModuleType.Identity, new ModuleInfo
                {
                    Type = ModuleType.Identity,
                    DisplayName = "Identity",
                    Description = "User identity, authentication, and session management",
                    Namespace = "IntelliVerseX.Identity",
                    AssemblyName = "IntelliVerseX.Identity",
                    FolderPath = "Assets/_IntelliVerseXSDK/Identity",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    RequiredScripts = new List<string>
                    {
                        "UserSessionManager.cs",
                        "APIManager.cs",
                        "IntelliVerseXUserIdentity.cs",
                        "DeviceInfoHelper.cs"
                    },
                    SetupPriority = "Critical"
                }
            },
            {
                ModuleType.Backend, new ModuleInfo
                {
                    Type = ModuleType.Backend,
                    DisplayName = "Backend",
                    Description = "Nakama backend integration and services",
                    Namespace = "IntelliVerseX.Backend",
                    AssemblyName = "IntelliVerseX.Backend",
                    FolderPath = "Assets/_IntelliVerseXSDK/Backend",
                    Dependencies = new List<ModuleType> { ModuleType.Core, ModuleType.Identity },
                    RequiredScripts = new List<string>
                    {
                        "IVXBackendService.cs",
                        "IVXNakamaManager.cs",
                        "IVXWalletManager.cs",
                        "IVXGeolocationService.cs"
                    },
                    RequiredPrefabs = new List<string>
                    {
                        "NakamaManager",
                        "UserData"
                    },
                    SetupPriority = "Critical"
                }
            },
            {
                ModuleType.Networking, new ModuleInfo
                {
                    Type = ModuleType.Networking,
                    DisplayName = "Networking",
                    Description = "HTTP requests with retry logic and offline detection",
                    Namespace = "IntelliVerseX.Networking",
                    AssemblyName = "IntelliVerseX.Networking",
                    FolderPath = "Assets/_IntelliVerseXSDK/Networking",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    RequiredScripts = new List<string>
                    {
                        "IVXNetworkRequest.cs"
                    },
                    SetupPriority = "High"
                }
            },
            {
                ModuleType.Storage, new ModuleInfo
                {
                    Type = ModuleType.Storage,
                    DisplayName = "Storage",
                    Description = "Secure storage with encryption and GDPR compliance",
                    Namespace = "IntelliVerseX.Storage",
                    AssemblyName = "IntelliVerseX.Storage",
                    FolderPath = "Assets/_IntelliVerseXSDK/Storage",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    RequiredScripts = new List<string>
                    {
                        "IVXSecureStorage.cs"
                    },
                    SetupPriority = "High"
                }
            },

            // ========== FEATURE MODULES ==========
            {
                ModuleType.Quiz, new ModuleInfo
                {
                    Type = ModuleType.Quiz,
                    DisplayName = "Quiz",
                    Description = "Quiz system with providers, sessions, and scoring",
                    Namespace = "IntelliVerseX.Quiz",
                    AssemblyName = "IntelliVerseX.Quiz",
                    FolderPath = "Assets/_IntelliVerseXSDK/Quiz",
                    Dependencies = new List<ModuleType> { ModuleType.Core, ModuleType.Networking },
                    RequiredScripts = new List<string>
                    {
                        "IVXQuizSessionManager.cs",
                        "IVXQuizData.cs"
                    },
                    SetupPriority = "Normal"
                }
            },
            {
                ModuleType.Leaderboard, new ModuleInfo
                {
                    Type = ModuleType.Leaderboard,
                    DisplayName = "Leaderboard",
                    Description = "Daily, weekly, monthly, and global leaderboards",
                    Namespace = "IntelliVerseX.Games.Leaderboard",
                    AssemblyName = "IntelliVerseX.Games.Leaderboard",
                    FolderPath = "Assets/_IntelliVerseXSDK/Leaderboard",
                    Dependencies = new List<ModuleType> { ModuleType.Core, ModuleType.Backend },
                    RequiredScripts = new List<string>
                    {
                        "Static/IVXGLeaderboardManager.cs",
                        "Runtime/IVXGLeaderboard.cs",
                        "UI/IVXGLeaderboardUI.cs"
                    },
                    SetupPriority = "Normal"
                }
            },
            {
                ModuleType.Wallet, new ModuleInfo
                {
                    Type = ModuleType.Wallet,
                    DisplayName = "Wallet",
                    Description = "Dual-wallet system (game + global currency)",
                    Namespace = "IntelliVerseX.Core",
                    AssemblyName = "IntelliVerseX.Core",
                    FolderPath = "Assets/_IntelliVerseXSDK/V2/Manager",
                    Dependencies = new List<ModuleType> { ModuleType.Core, ModuleType.Backend },
                    RequiredScripts = new List<string>
                    {
                        "IVXNWalletManager.cs"
                    },
                    SetupPriority = "High"
                }
            },
            {
                ModuleType.Social, new ModuleInfo
                {
                    Type = ModuleType.Social,
                    DisplayName = "Social",
                    Description = "Friends system with search, requests, and chat",
                    Namespace = "IntelliVerseX.Social",
                    AssemblyName = "IntelliVerseX.Social",
                    FolderPath = "Assets/_IntelliVerseXSDK/Social",
                    Dependencies = new List<ModuleType> { ModuleType.Core, ModuleType.Backend, ModuleType.Identity },
                    RequiredScripts = new List<string>
                    {
                        "IVXFriendsService.cs",
                        "IVXFriendsPanel.cs"
                    },
                    RequiredPrefabs = new List<string>
                    {
                        "IVXFriendSlot",
                        "IVXFriendRequestSlot",
                        "IVXFriendSearchSlot"
                    },
                    IsOptional = true,
                    SetupPriority = "Low"
                }
            },
            {
                ModuleType.Localization, new ModuleInfo
                {
                    Type = ModuleType.Localization,
                    DisplayName = "Localization",
                    Description = "Multi-language support with RTL for 13 languages",
                    Namespace = "IntelliVerseX.Localization",
                    AssemblyName = "IntelliVerseX.Localization",
                    FolderPath = "Assets/_IntelliVerseXSDK/Localization",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    SetupPriority = "Normal"
                }
            },

            // ========== MONETIZATION MODULES ==========
            {
                ModuleType.IAP, new ModuleInfo
                {
                    Type = ModuleType.IAP,
                    DisplayName = "In-App Purchases",
                    Description = "Unity IAP integration with subscriptions",
                    Namespace = "IntelliVerseX.IAP",
                    AssemblyName = "IntelliVerseX.IAP",
                    FolderPath = "Assets/_IntelliVerseXSDK/IAP",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    RequiredScripts = new List<string>
                    {
                        "IVXIAPService.cs",
                        "IVXSubscriptionManager.cs",
                        "IVXFreeTrialManager.cs"
                    },
                    IsOptional = true,
                    SetupPriority = "Normal"
                }
            },
            {
                ModuleType.Ads, new ModuleInfo
                {
                    Type = ModuleType.Ads,
                    DisplayName = "Ads",
                    Description = "Ad mediation with LevelPlay and Appodeal",
                    Namespace = "IntelliVerseX.Monetization",
                    AssemblyName = "IntelliVerseX.Monetization",
                    FolderPath = "Assets/_IntelliVerseXSDK/Monetization",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    RequiredPrefabs = new List<string>
                    {
                        "AdsManager"
                    },
                    IsOptional = true,
                    SetupPriority = "Normal"
                }
            },
            {
                ModuleType.Analytics, new ModuleInfo
                {
                    Type = ModuleType.Analytics,
                    DisplayName = "Analytics",
                    Description = "Event tracking and user analytics",
                    Namespace = "IntelliVerseX.Analytics",
                    AssemblyName = "IntelliVerseX.Analytics",
                    FolderPath = "Assets/_IntelliVerseXSDK/Analytics",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    RequiredScripts = new List<string>
                    {
                        "IVXAnalyticsManager.cs",
                        "IVXAnalyticsService.cs"
                    },
                    IsOptional = true,
                    SetupPriority = "Low"
                }
            },

            // ========== UI MODULES ==========
            {
                ModuleType.UI, new ModuleInfo
                {
                    Type = ModuleType.UI,
                    DisplayName = "UI Components",
                    Description = "Reusable UI components and prefabs",
                    Namespace = "IntelliVerseX.UI",
                    AssemblyName = "IntelliVerseX.UI",
                    FolderPath = "Assets/_IntelliVerseXSDK/UI",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    SetupPriority = "Normal"
                }
            },
            {
                ModuleType.IntroScene, new ModuleInfo
                {
                    Type = ModuleType.IntroScene,
                    DisplayName = "Intro Scene",
                    Description = "IntelliVerseX branded intro animation",
                    Namespace = "IntelliVerseX.IntroScene",
                    AssemblyName = "",
                    FolderPath = "Assets/IntroSceneIntelliverseX",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    RequiredScripts = new List<string>
                    {
                        "LogoIntroController.cs"
                    },
                    SetupPriority = "High"
                }
            },

            // ========== UTILITY MODULES ==========
            {
                ModuleType.Diagnostics, new ModuleInfo
                {
                    Type = ModuleType.Diagnostics,
                    DisplayName = "Diagnostics",
                    Description = "Logging, error handling, and performance monitoring",
                    Namespace = "IntelliVerseX.Diagnostics",
                    AssemblyName = "IntelliVerseX.Diagnostics",
                    FolderPath = "Assets/_IntelliVerseXSDK/Diagnostics",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    IsOptional = true,
                    SetupPriority = "Low"
                }
            },
            {
                ModuleType.Resources, new ModuleInfo
                {
                    Type = ModuleType.Resources,
                    DisplayName = "Resources",
                    Description = "Resource pooling and memory management",
                    Namespace = "IntelliVerseX.Resources",
                    AssemblyName = "IntelliVerseX.Resources",
                    FolderPath = "Assets/_IntelliVerseXSDK/Resources",
                    Dependencies = new List<ModuleType> { ModuleType.Core },
                    IsOptional = true,
                    SetupPriority = "Low"
                }
            }
        };

        #endregion

        #region Public API

        /// <summary>
        /// Get module info by type
        /// </summary>
        public static ModuleInfo GetModule(ModuleType type)
        {
            return _modules.TryGetValue(type, out var info) ? info : null;
        }

        /// <summary>
        /// Get all modules
        /// </summary>
        public static IEnumerable<ModuleInfo> GetAllModules()
        {
            return _modules.Values;
        }

        /// <summary>
        /// Get modules by priority
        /// </summary>
        public static IEnumerable<ModuleInfo> GetModulesByPriority(string priority)
        {
            foreach (var module in _modules.Values)
            {
                if (module.SetupPriority == priority)
                    yield return module;
            }
        }

        /// <summary>
        /// Get required modules (non-optional)
        /// </summary>
        public static IEnumerable<ModuleInfo> GetRequiredModules()
        {
            foreach (var module in _modules.Values)
            {
                if (!module.IsOptional)
                    yield return module;
            }
        }

        /// <summary>
        /// Get dependencies for a module (recursive)
        /// </summary>
        public static List<ModuleType> GetAllDependencies(ModuleType type)
        {
            var result = new List<ModuleType>();
            var visited = new HashSet<ModuleType>();
            CollectDependencies(type, result, visited);
            return result;
        }

        private static void CollectDependencies(ModuleType type, List<ModuleType> result, HashSet<ModuleType> visited)
        {
            if (visited.Contains(type)) return;
            visited.Add(type);

            var module = GetModule(type);
            if (module == null) return;

            foreach (var dep in module.Dependencies)
            {
                CollectDependencies(dep, result, visited);
                if (!result.Contains(dep))
                    result.Add(dep);
            }
        }

        /// <summary>
        /// Get setup order (respects dependencies)
        /// </summary>
        public static List<ModuleType> GetSetupOrder()
        {
            var result = new List<ModuleType>();
            var visited = new HashSet<ModuleType>();

            // Add by priority
            foreach (var priority in new[] { "Critical", "High", "Normal", "Low" })
            {
                foreach (var module in GetModulesByPriority(priority))
                {
                    AddWithDependencies(module.Type, result, visited);
                }
            }

            return result;
        }

        private static void AddWithDependencies(ModuleType type, List<ModuleType> result, HashSet<ModuleType> visited)
        {
            if (visited.Contains(type)) return;
            visited.Add(type);

            var module = GetModule(type);
            if (module == null) return;

            // Add dependencies first
            foreach (var dep in module.Dependencies)
            {
                AddWithDependencies(dep, result, visited);
            }

            if (!result.Contains(type))
                result.Add(type);
        }

        #endregion
    }
}
