using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Loads <c>Editor/RpcIndex/IVXRpcIndex.generated.json</c> for the Control Center APIs tab.
    /// </summary>
    public static class IVXRpcIndex
    {
        [Serializable]
        public class Entry
        {
            public string id;
            public string module;
            public bool authRequired = true;
        }

        [Serializable]
        private class FileDto
        {
            public string generatedAt;
            public string generator;
            public int count;
            public Entry[] rpcs;
        }

        private static Entry[] _cached;
        private static string _loadedFrom;

        public static string LoadedFrom => _loadedFrom;

        public static Entry[] GetEntries()
        {
            if (_cached != null)
                return _cached;

            string path = FindIndexPath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                _loadedFrom = null;
                _cached = Array.Empty<Entry>();
                return _cached;
            }

            try
            {
                string json = File.ReadAllText(path);
                var dto = JsonUtility.FromJson<FileDto>(WrapArrayIfNeeded(json));
                if (dto == null || dto.rpcs == null)
                {
                    // Unity JsonUtility cannot deserialize top-level arrays; file uses object wrapper.
                    _cached = ParseEntriesManual(json);
                }
                else
                {
                    _cached = dto.rpcs;
                }

                _loadedFrom = path;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[IVXRpcIndex] Failed to load: " + ex.Message);
                _cached = Array.Empty<Entry>();
                _loadedFrom = null;
            }

            return _cached;
        }

        public static void Invalidate()
        {
            _cached = null;
            _loadedFrom = null;
        }

        private static string FindIndexPath()
        {
            // Package layout: .../com.intelliversex.sdk/Editor/RpcIndex/IVXRpcIndex.generated.json
            string[] guids = UnityEditor.AssetDatabase.FindAssets("IVXRpcIndex.generated");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                if (assetPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    return Path.GetFullPath(assetPath);
            }

            return null;
        }

        private static string WrapArrayIfNeeded(string json)
        {
            return json;
        }

        private static Entry[] ParseEntriesManual(string json)
        {
            // Fallback if JsonUtility fails on nested array naming
            var list = new List<Entry>();
            int idx = 0;
            while (idx < json.Length)
            {
                int idKey = json.IndexOf("\"id\"", idx, StringComparison.Ordinal);
                if (idKey < 0) break;
                int colon = json.IndexOf(':', idKey);
                int q1 = json.IndexOf('"', colon + 1);
                int q2 = json.IndexOf('"', q1 + 1);
                if (q1 < 0 || q2 < 0) break;
                string id = json.Substring(q1 + 1, q2 - q1 - 1);

                string module = "platform";
                int modKey = json.IndexOf("\"module\"", q2, StringComparison.Ordinal);
                if (modKey > 0 && modKey < q2 + 80)
                {
                    int mc = json.IndexOf(':', modKey);
                    int mq1 = json.IndexOf('"', mc + 1);
                    int mq2 = json.IndexOf('"', mq1 + 1);
                    if (mq1 > 0 && mq2 > mq1)
                        module = json.Substring(mq1 + 1, mq2 - mq1 - 1);
                }

                list.Add(new Entry { id = id, module = module, authRequired = true });
                idx = q2 + 1;
            }

            return list.ToArray();
        }
    }
}
