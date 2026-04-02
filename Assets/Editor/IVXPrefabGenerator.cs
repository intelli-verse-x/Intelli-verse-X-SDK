#if UNITY_EDITOR
using System.IO;
using IntelliVerseX.AI;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Demos;
using IntelliVerseX.Discord;
using IntelliVerseX.GameModes;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Editor utility that generates manager and demo prefabs under <c>Assets/_IntelliVerseXSDK/Prefabs/</c>.
    /// Run via IntelliVerseX > Generate All Prefabs.
    /// </summary>
    public static class IVXPrefabGenerator
    {
        private const string PrefabFolder = "Assets/_IntelliVerseXSDK/Prefabs";

        [MenuItem("IntelliVerseX/Generate All Prefabs")]
        public static void GenerateAllPrefabs()
        {
            EnsurePrefabFolderExists();
            var count = 0;

            count += SavePrefab("IVX_DiscordManager.prefab", go =>
            {
                go.AddComponent<IVXDiscordManager>();
                go.AddComponent<IVXDiscordPresence>();
                go.AddComponent<IVXDiscordFriends>();
                go.AddComponent<IVXDiscordMessages>();
                go.AddComponent<IVXDiscordLobby>();
                go.AddComponent<IVXDiscordVoice>();
                go.AddComponent<IVXDiscordInvites>();
                go.AddComponent<IVXDiscordLinkedChannels>();
                go.AddComponent<IVXDiscordModeration>();
                go.AddComponent<IVXDiscordDebug>();
            });

            count += SavePrefab("IVX_MultiplayerManager.prefab", go =>
            {
                go.AddComponent<IVXGameModeManager>();
                go.AddComponent<IVXLobbyManager>();
                go.AddComponent<IVXMatchmakingManager>();
                go.AddComponent<IVXLocalMultiplayerManager>();
            });

            count += SavePrefab("IVX_AIManager.prefab", go =>
            {
                go.AddComponent<AudioSource>();
                go.AddComponent<IVXAISessionManager>();
                go.AddComponent<IVXAINPCDialogManager>();
                go.AddComponent<IVXAIAssistant>();
                go.AddComponent<IVXAIModerator>();
                go.AddComponent<IVXAIContentGenerator>();
                go.AddComponent<IVXAIProfiler>();
                go.AddComponent<IVXAIVoiceServices>();
            });

            count += SavePrefab("IVX_Bootstrap.prefab", go => go.AddComponent<IVXBootstrap>());

            count += SavePrefab("IVX_DemoHub.prefab", go => go.AddComponent<IVXDemoHub>());
            count += SavePrefab("IVX_DiscordSocialDemo.prefab", go => go.AddComponent<IVXDiscordSocialDemo>());
            count += SavePrefab("IVX_SpinWheelDemo.prefab", go => go.AddComponent<IVXSpinWheelDemo>());
            count += SavePrefab("IVX_StreakDemo.prefab", go => go.AddComponent<IVXStreakDemo>());
            count += SavePrefab("IVX_OfferwallDemo.prefab", go => go.AddComponent<IVXOfferwallDemo>());
            count += SavePrefab("IVX_AIVoiceChatDemo.prefab", go => go.AddComponent<IVXAIVoiceChatDemo>());
            count += SavePrefab("IVX_AIHostDemo.prefab", go => go.AddComponent<IVXAIHostDemo>());
            count += SavePrefab("IVX_GameModeSelectorDemo.prefab", go => go.AddComponent<IVXGameModeSelectorDemo>());
            count += SavePrefab("IVX_LobbyDemo.prefab", go => go.AddComponent<IVXLobbyDemo>());
            count += SavePrefab("IVX_AINPCDemo.prefab", go => go.AddComponent<IVXAINPCDemo>());
            count += SavePrefab("IVX_AIAssistantDemo.prefab", go => go.AddComponent<IVXAIAssistantDemo>());
            count += SavePrefab("IVX_AIModerationDemo.prefab", go => go.AddComponent<IVXAIModerationDemo>());
            count += SavePrefab("IVX_AIContentGenDemo.prefab", go => go.AddComponent<IVXAIContentGenDemo>());
            count += SavePrefab("IVX_IdentityDemo.prefab", go => go.AddComponent<IVXIdentityDemo>());
            count += SavePrefab("IVX_LeaderboardDemo.prefab", go => go.AddComponent<IVXLeaderboardDemo>());
            count += SavePrefab("IVX_AIProfilerDemo.prefab", go => go.AddComponent<IVXAIProfilerDemo>());
            count += SavePrefab("IVX_AIVoiceServicesDemo.prefab", go => go.AddComponent<IVXAIVoiceServicesDemo>());

            count += SavePrefab("IVX_AllManagers.prefab", go =>
            {
                go.AddComponent<IVXDiscordManager>();
                go.AddComponent<IVXDiscordPresence>();
                go.AddComponent<IVXDiscordFriends>();
                go.AddComponent<IVXDiscordMessages>();
                go.AddComponent<IVXDiscordLobby>();
                go.AddComponent<IVXDiscordVoice>();
                go.AddComponent<IVXDiscordInvites>();
                go.AddComponent<IVXDiscordLinkedChannels>();
                go.AddComponent<IVXDiscordModeration>();
                go.AddComponent<IVXDiscordDebug>();
                go.AddComponent<IVXGameModeManager>();
                go.AddComponent<IVXLobbyManager>();
                go.AddComponent<IVXMatchmakingManager>();
                go.AddComponent<IVXLocalMultiplayerManager>();
                go.AddComponent<AudioSource>();
                go.AddComponent<IVXAISessionManager>();
                go.AddComponent<IVXAINPCDialogManager>();
                go.AddComponent<IVXAIAssistant>();
                go.AddComponent<IVXAIModerator>();
                go.AddComponent<IVXAIContentGenerator>();
                go.AddComponent<IVXAIProfiler>();
                go.AddComponent<IVXAIVoiceServices>();
                go.AddComponent<IVXBootstrap>();
            });

            Debug.Log($"[IVXPrefabGenerator] Created {count} prefabs in {PrefabFolder}/");
            AssetDatabase.Refresh();
        }

        private static void EnsurePrefabFolderExists()
        {
            if (AssetDatabase.IsValidFolder(PrefabFolder)) return;
            if (!AssetDatabase.IsValidFolder("Assets/_IntelliVerseXSDK"))
                AssetDatabase.CreateFolder("Assets", "_IntelliVerseXSDK");
            AssetDatabase.CreateFolder("Assets/_IntelliVerseXSDK", "Prefabs");
        }

        private static int SavePrefab(string fileName, System.Action<GameObject> configure)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var go = new GameObject(baseName);
            configure(go);
            var assetPath = $"{PrefabFolder}/{fileName}";
            PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            Object.DestroyImmediate(go);
            return 1;
        }
    }
}
#endif
