#if UNITY_EDITOR
using System.IO;
using IntelliVerseX.AI;
using IntelliVerseX.Demos;
using IntelliVerseX.Discord;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Editor utility that generates manager and demo prefabs under <c>Assets/_IntelliVerseXSDK/Prefabs/</c>.
    /// </summary>
    public static class IVXPrefabGenerator
    {
        private const string PrefabFolder = "Assets/_IntelliVerseXSDK/Prefabs";

        [MenuItem("IntelliVerseX/Generate All Prefabs")]
        public static void GenerateAllPrefabs()
        {
            EnsurePrefabFolderExists();

            var count = 0;

            count += SavePrefab("IVX_DiscordManager.prefab", go => go.AddComponent<IVXDiscordManager>());
            count += SavePrefab("IVX_DiscordPresence.prefab", go => go.AddComponent<IVXDiscordPresence>());
            count += SavePrefab("IVX_DiscordFriends.prefab", go => go.AddComponent<IVXDiscordFriends>());
            count += SavePrefab("IVX_DiscordMessages.prefab", go => go.AddComponent<IVXDiscordMessages>());
            count += SavePrefab("IVX_DiscordLobby.prefab", go => go.AddComponent<IVXDiscordLobby>());
            count += SavePrefab("IVX_DiscordVoice.prefab", go => go.AddComponent<IVXDiscordVoice>());
            count += SavePrefab("IVX_DiscordInvites.prefab", go => go.AddComponent<IVXDiscordInvites>());
            count += SavePrefab("IVX_DiscordLinkedChannels.prefab", go => go.AddComponent<IVXDiscordLinkedChannels>());
            count += SavePrefab("IVX_DiscordModeration.prefab", go => go.AddComponent<IVXDiscordModeration>());
            count += SavePrefab("IVX_DiscordDebug.prefab", go => go.AddComponent<IVXDiscordDebug>());

            count += SavePrefab("IVX_AISessionManager.prefab", go =>
            {
                go.AddComponent<AudioSource>();
                go.AddComponent<IVXAISessionManager>();
            });

            count += SavePrefab("IVX_AINPCDialogManager.prefab", go => go.AddComponent<IVXAINPCDialogManager>());
            count += SavePrefab("IVX_AIAssistant.prefab", go => go.AddComponent<IVXAIAssistant>());
            count += SavePrefab("IVX_AIModerator.prefab", go => go.AddComponent<IVXAIModerator>());
            count += SavePrefab("IVX_AIContentGenerator.prefab", go => go.AddComponent<IVXAIContentGenerator>());
            count += SavePrefab("IVX_AIProfiler.prefab", go => go.AddComponent<IVXAIProfiler>());
            count += SavePrefab("IVX_AIVoiceServices.prefab", go => go.AddComponent<IVXAIVoiceServices>());

            count += SavePrefab("IVX_DemoHub.prefab", go => go.AddComponent<IVXDemoHub>());
            count += SavePrefab("IVX_DiscordSocialDemo.prefab", go => go.AddComponent<IVXDiscordSocialDemo>());

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
                go.AddComponent<AudioSource>();
                go.AddComponent<IVXAISessionManager>();
                go.AddComponent<IVXAINPCDialogManager>();
                go.AddComponent<IVXAIAssistant>();
                go.AddComponent<IVXAIModerator>();
                go.AddComponent<IVXAIContentGenerator>();
                go.AddComponent<IVXAIProfiler>();
                go.AddComponent<IVXAIVoiceServices>();
            });

            Debug.Log($"Created {count} prefabs in Assets/_IntelliVerseXSDK/Prefabs/");
            AssetDatabase.Refresh();
        }

        private static void EnsurePrefabFolderExists()
        {
            if (AssetDatabase.IsValidFolder(PrefabFolder))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/_IntelliVerseXSDK"))
            {
                AssetDatabase.CreateFolder("Assets", "_IntelliVerseXSDK");
            }

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
