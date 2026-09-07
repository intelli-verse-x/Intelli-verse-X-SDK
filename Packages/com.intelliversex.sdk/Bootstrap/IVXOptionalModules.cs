using System;
using System.Reflection;
using UnityEngine;

namespace IntelliVerseX.Bootstrap
{
    /// <summary>
    /// Soft-loads optional UPM packages (Discord / AI) without hard asmdef references from Core Bootstrap.
    /// </summary>
    internal static class IVXOptionalModules
    {
        public static bool TryInitDiscord(Transform parent, ScriptableObject discordConfig, Action<string> log, Action<string, Exception> fail)
        {
            try
            {
                Type mgrType = FindType("IntelliVerseX.Discord.IVXDiscordManager, IntelliVerseX.Discord");
                if (mgrType == null)
                {
                    log?.Invoke("Discord package not installed (com.intelliversex.sdk.discord). Skipping.");
                    return false;
                }

                log?.Invoke("Initializing Discord Social SDK (optional package)...");
                var instanceProp = mgrType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                object mgr = instanceProp != null ? instanceProp.GetValue(null) : null;
                if (mgr == null)
                {
                    var go = new GameObject("IVX_Discord");
                    go.transform.SetParent(parent);
                    mgr = go.AddComponent(mgrType);
                    string[] extras =
                    {
                        "IntelliVerseX.Discord.IVXDiscordPresence, IntelliVerseX.Discord",
                        "IntelliVerseX.Discord.IVXDiscordFriends, IntelliVerseX.Discord",
                        "IntelliVerseX.Discord.IVXDiscordMessages, IntelliVerseX.Discord",
                        "IntelliVerseX.Discord.IVXDiscordLobby, IntelliVerseX.Discord",
                        "IntelliVerseX.Discord.IVXDiscordVoice, IntelliVerseX.Discord",
                        "IntelliVerseX.Discord.IVXDiscordInvites, IntelliVerseX.Discord",
                        "IntelliVerseX.Discord.IVXDiscordLinkedChannels, IntelliVerseX.Discord",
                        "IntelliVerseX.Discord.IVXDiscordModeration, IntelliVerseX.Discord",
                        "IntelliVerseX.Discord.IVXDiscordDebug, IntelliVerseX.Discord"
                    };
                    for (int i = 0; i < extras.Length; i++)
                    {
                        Type t = FindType(extras[i]);
                        if (t != null)
                            go.AddComponent(t);
                    }
                }

                MethodInfo init = mgrType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
                if (init != null)
                    init.Invoke(mgr, new object[] { discordConfig });
                return true;
            }
            catch (Exception e)
            {
                fail?.Invoke("Discord", e);
                return false;
            }
        }

        public static void TryShutdownDiscord()
        {
            try
            {
                Type mgrType = FindType("IntelliVerseX.Discord.IVXDiscordManager, IntelliVerseX.Discord");
                if (mgrType == null) return;
                var instanceProp = mgrType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                object mgr = instanceProp != null ? instanceProp.GetValue(null) : null;
                if (mgr == null) return;
                MethodInfo shutdown = mgrType.GetMethod("Shutdown", BindingFlags.Public | BindingFlags.Instance);
                shutdown?.Invoke(mgr, null);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IVXBootstrap] Discord shutdown: " + e.Message);
            }
        }

        public static bool TryInitAI(
            Transform parent,
            ScriptableObject aiConfig,
            string userId,
            string userName,
            string authToken,
            Action<string> log,
            Action<string, Exception> fail)
        {
            try
            {
                Type cfgType = FindType("IntelliVerseX.AI.IVXAIConfig, IntelliVerseX.AI");
                if (cfgType == null)
                {
                    log?.Invoke("AI package not installed (com.intelliversex.sdk.ai). Skipping.");
                    return false;
                }

                if (aiConfig == null || !cfgType.IsInstanceOfType(aiConfig))
                {
                    log?.Invoke("No AI config assigned — AI subsystems will not initialize.");
                    return false;
                }

                log?.Invoke("Initializing AI stack (optional package)...");

                EnsureAndInit(parent, "IntelliVerseX.AI.IVXAISessionManager, IntelliVerseX.AI", "IVX_AISessionManager",
                    true, aiConfig, userId, userName, authToken, sessionOnly: true);
                EnsureAndInit(parent, "IntelliVerseX.AI.IVXAINPCDialogManager, IntelliVerseX.AI", "IVX_AINPCDialog",
                    false, aiConfig, userId, userName, authToken, setAuth: true);
                EnsureAndInit(parent, "IntelliVerseX.AI.IVXAIAssistant, IntelliVerseX.AI", "IVX_AIAssistant",
                    false, aiConfig, userId, userName, authToken, setAuth: true);
                EnsureAndInit(parent, "IntelliVerseX.AI.IVXAIModerator, IntelliVerseX.AI", "IVX_AIModerator",
                    false, aiConfig, userId, userName, authToken);
                EnsureAndInit(parent, "IntelliVerseX.AI.IVXAIContentGenerator, IntelliVerseX.AI", "IVX_AIContentGen",
                    false, aiConfig, userId, userName, authToken);
                EnsureAndInit(parent, "IntelliVerseX.AI.IVXAIProfiler, IntelliVerseX.AI", "IVX_AIProfiler",
                    false, aiConfig, userId, userName, authToken, profiler: true);
                EnsureAndInit(parent, "IntelliVerseX.AI.IVXAIVoiceServices, IntelliVerseX.AI", "IVX_AIVoiceServices",
                    false, aiConfig, userId, userName, authToken);
                return true;
            }
            catch (Exception e)
            {
                fail?.Invoke("AI", e);
                return false;
            }
        }

        private static void EnsureAndInit(
            Transform parent,
            string typeName,
            string goName,
            bool addAudio,
            ScriptableObject aiConfig,
            string userId,
            string userName,
            string authToken,
            bool sessionOnly = false,
            bool setAuth = false,
            bool profiler = false)
        {
            Type t = FindType(typeName);
            if (t == null) return;

            UnityEngine.Object[] found = UnityEngine.Object.FindObjectsByType(
                t, FindObjectsInactive.Include, FindObjectsSortMode.None);
            UnityEngine.Object existing = found != null && found.Length > 0 ? found[0] : null;
            Component comp;
            if (existing != null)
            {
                comp = existing as Component;
            }
            else
            {
                var go = new GameObject(goName);
                go.transform.SetParent(parent);
                if (addAudio)
                    go.AddComponent<AudioSource>();
                comp = go.AddComponent(t);
            }

            if (comp == null) return;

            var instanceProp = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            object target = instanceProp != null ? instanceProp.GetValue(null) : comp;

            if (sessionOnly)
            {
                MethodInfo init = t.GetMethod("Initialize", new[] { typeof(string), typeof(string), typeof(string) });
                init?.Invoke(target, new object[] { userId, userName, authToken });
                return;
            }

            if (profiler)
            {
                MethodInfo init = t.GetMethod("Initialize", new[] { aiConfig.GetType(), typeof(string) });
                if (init == null)
                    init = t.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
                init?.Invoke(target, init != null && init.GetParameters().Length == 2
                    ? new object[] { aiConfig, userId }
                    : new object[] { aiConfig });
                return;
            }

            {
                MethodInfo init = t.GetMethod("Initialize", new[] { aiConfig.GetType() });
                if (init == null)
                    init = t.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
                init?.Invoke(target, new object[] { aiConfig });
            }

            if (setAuth)
            {
                MethodInfo set = t.GetMethod("SetAuthToken", BindingFlags.Public | BindingFlags.Instance);
                set?.Invoke(target, new object[] { authToken });
            }
        }

        private static Type FindType(string assemblyQualified)
        {
            Type t = Type.GetType(assemblyQualified, false);
            if (t != null) return t;

            string typeName = assemblyQualified;
            string asmName = null;
            int comma = assemblyQualified.IndexOf(',');
            if (comma > 0)
            {
                typeName = assemblyQualified.Substring(0, comma).Trim();
                asmName = assemblyQualified.Substring(comma + 1).Trim();
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly a = assemblies[i];
                if (asmName != null && a.GetName().Name != asmName)
                    continue;
                t = a.GetType(typeName, false);
                if (t != null) return t;
            }

            return null;
        }
    }
}
