using System.IO;
using ApplixirSDK.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ApplixirSDK.Editor
{
    public class WebGLPostBuild : MonoBehaviour
    {
        private const string Tag = "ApplixirWebGL";

        [PostProcessBuild]
        public static void OnPostBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.WebGL)
            {
                Log("Post-build step for WebGL");
                CopyAppAdsFile(pathToBuiltProject);
                EditIndexHtml(pathToBuiltProject);
                Log("Post-build step for WebGL Done");
            }
        }

        private static void EditIndexHtml(string pathToBuiltProject)
        {
            var indexPath = Path.Combine(pathToBuiltProject, "index.html");
            if (File.Exists(indexPath))
            {
                string htmlContent = File.ReadAllText(indexPath);
                htmlContent = AddDivBlock(htmlContent);
                htmlContent = AddScriptBlock(htmlContent);

                File.WriteAllText(indexPath, htmlContent);
            }
        }

        private static string AddDivBlock(string htmlContent)
        {
            var divBlock = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ApplixirSDK/Editor/res/divBlock.txt")
                .text;
            // Add div block as the topmost div inside the body
            htmlContent = htmlContent.Replace("<body>", "<body>\n" + divBlock);
            return htmlContent;
        }

        private static string AddScriptBlock(string htmlContent)
        {
            var script = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ApplixirSDK/Editor/res/scriptBlock.txt")
                .text;
            var cfg = Resources.Load<AppLixirAdsConfig>("AppLixirAdsConfig");
            script = script.Replace("{{APPLIXIR_API_KEY}}", cfg.apiKey);
            // Add script as the last block inside the body
            htmlContent = htmlContent.Replace("</body>", script + "\n</body>");
            return htmlContent;
        }

        private static void CopyAppAdsFile(string pathToBuiltProject)
        {
            string copyfrom = Path.Combine(Application.dataPath, "ApplixirSDK/Editor/res/app-ads.txt");
            if (!File.Exists(copyfrom))
            {
                Debug.LogError("ApplixirSDK could not find app-ads.txt file. " +
                               "If you moved SDK files - please set new path to app-ads.txt file here.");
                return;
            }

            string filePath = Path.Combine(pathToBuiltProject, "app-ads.txt");
            File.Copy(copyfrom, filePath, true);
        }

        private static void Log(string message)
        {
            Debug.Log($"[{Tag}] {message}");
        }
    }
}