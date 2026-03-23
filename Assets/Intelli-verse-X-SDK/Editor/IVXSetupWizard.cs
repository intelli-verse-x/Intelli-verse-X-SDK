// IVXSetupWizard.cs
// Setup wizard for IntelliVerseX SDK
// Guides users through dependency installation and SDK configuration

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Setup wizard for IntelliVerseX SDK.
    /// Guides users through dependency installation and configuration.
    /// </summary>
    public class IVXSetupWizard : EditorWindow
    {
        private enum SetupStep
        {
            Welcome,
            CheckDependencies,
            InstallNewtonsoft,
            InstallNakama,
            Verification,
            Complete
        }
        
        private SetupStep currentStep = SetupStep.Welcome;
        private AddRequest newtonsoftRequest;
        private bool isInstalling = false;
        private bool newtonsoftInstalled = false;
        private bool nakamaInstalled = false;
        private Vector2 scrollPosition;
        
        // REMOVED: Duplicate menu item - use "SDK Setup Wizard" instead
        // [MenuItem("IntelliVerse-X SDK/Setup Wizard", priority = 0)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard
        public static void ShowWizard()
        {
            var window = GetWindow<IVXSetupWizard>("SDK Setup Wizard");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }
        
        private void OnGUI()
        {
            // Header
            DrawHeader();
            
            GUILayout.Space(20);
            
            // Content area with scroll
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (currentStep)
            {
                case SetupStep.Welcome:
                    DrawWelcomeStep();
                    break;
                case SetupStep.CheckDependencies:
                    DrawCheckDependenciesStep();
                    break;
                case SetupStep.InstallNewtonsoft:
                    DrawInstallNewtonsoftStep();
                    break;
                case SetupStep.InstallNakama:
                    DrawInstallNakamaStep();
                    break;
                case SetupStep.Verification:
                    DrawVerificationStep();
                    break;
                case SetupStep.Complete:
                    DrawCompleteStep();
                    break;
            }
            
            EditorGUILayout.EndScrollView();
            
            GUILayout.FlexibleSpace();
            
            // Footer with navigation buttons
            DrawFooter();
        }
        
        private void DrawHeader()
        {
            GUILayout.Space(10);
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("IntelliVerseX SDK Setup Wizard", headerStyle);
            
            GUILayout.Space(5);
            
            // Progress bar
            Rect progressRect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));
            progressRect.x += 50;
            progressRect.width -= 100;
            
            float progress = ((int)currentStep) / 5.0f;
            EditorGUI.ProgressBar(progressRect, progress, $"Step {(int)currentStep + 1} of 6");
        }
        
        private void DrawWelcomeStep()
        {
            EditorGUILayout.HelpBox(
                "Welcome to the IntelliVerseX SDK Setup Wizard!\n\n" +
                "This wizard will guide you through installing the required dependencies for the SDK.\n\n" +
                "The SDK requires:\n" +
                "• Newtonsoft.Json (JSON serialization)\n" +
                "• Nakama Unity SDK (Backend services)",
                MessageType.Info
            );
            
            GUILayout.Space(20);
            
            EditorGUILayout.LabelField("What you'll need:", EditorStyles.boldLabel);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("• Internet connection for package installation");
            EditorGUILayout.LabelField("• Unity 2021.3 or later");
            EditorGUILayout.LabelField("• ~5 minutes of your time");
            
            GUILayout.Space(20);
            
            EditorGUILayout.HelpBox(
                "This wizard will:\n" +
                "1. Check existing dependencies\n" +
                "2. Install Newtonsoft.Json automatically\n" +
                "3. Guide you through Nakama installation\n" +
                "4. Verify everything is working",
                MessageType.None
            );
        }
        
        private void DrawCheckDependenciesStep()
        {
            EditorGUILayout.LabelField("Checking Dependencies", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Check Newtonsoft
            bool hasNewtonsoft = CheckType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
            DrawDependencyStatus("Newtonsoft.Json", hasNewtonsoft);
            
            GUILayout.Space(10);
            
            // Check Nakama
            bool hasNakama = CheckType("Nakama.IClient, Nakama");
            DrawDependencyStatus("Nakama SDK", hasNakama);
            
            GUILayout.Space(20);
            
            newtonsoftInstalled = hasNewtonsoft;
            nakamaInstalled = hasNakama;
            
            if (hasNewtonsoft && hasNakama)
            {
                EditorGUILayout.HelpBox(
                    "✅ All dependencies are already installed!\n\nYou can skip to the verification step.",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Some dependencies are missing. Click 'Next' to install them.",
                    MessageType.Warning
                );
            }
        }
        
        private void DrawInstallNewtonsoftStep()
        {
            EditorGUILayout.LabelField("Install Newtonsoft.Json", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (newtonsoftInstalled)
            {
                EditorGUILayout.HelpBox(
                    "✅ Newtonsoft.Json is already installed!",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Newtonsoft.Json is a JSON serialization library required by the SDK.\n\n" +
                    "This will be installed automatically via Unity Package Manager.",
                    MessageType.Info
                );
                
                GUILayout.Space(10);
                
                if (!isInstalling)
                {
                    if (GUILayout.Button("Install Newtonsoft.Json", GUILayout.Height(40)))
                    {
                        InstallNewtonsoft();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Installing... Please wait.", MessageType.None);
                    
                    if (newtonsoftRequest != null && newtonsoftRequest.IsCompleted)
                    {
                        isInstalling = false;
                        
                        if (newtonsoftRequest.Status == StatusCode.Success)
                        {
                            newtonsoftInstalled = true;
                            Debug.Log("✅ Newtonsoft.Json installed successfully!");
                            EditorUtility.DisplayDialog("Success", "Newtonsoft.Json installed successfully!", "OK");
                        }
                        else
                        {
                            Debug.LogError($"❌ Failed to install Newtonsoft.Json: {newtonsoftRequest.Error.message}");
                            EditorUtility.DisplayDialog("Error", $"Failed to install Newtonsoft.Json:\n{newtonsoftRequest.Error.message}", "OK");
                        }
                        
                        newtonsoftRequest = null;
                    }
                }
            }
        }
        
        private void DrawInstallNakamaStep()
        {
            EditorGUILayout.LabelField("Install Nakama Unity SDK", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (nakamaInstalled)
            {
                EditorGUILayout.HelpBox(
                    "✅ Nakama SDK is already installed!",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Nakama SDK provides backend services (authentication, leaderboards, storage).\n\n" +
                    "Nakama must be installed manually from GitHub.",
                    MessageType.Info
                );
                
                GUILayout.Space(10);
                
                EditorGUILayout.LabelField("Installation Steps:", EditorStyles.boldLabel);
                GUILayout.Space(5);
                
                EditorGUILayout.LabelField("1. Click the button below to open GitHub releases");
                EditorGUILayout.LabelField("2. Download the latest .unitypackage file");
                EditorGUILayout.LabelField("3. In Unity: Assets > Import Package > Custom Package");
                EditorGUILayout.LabelField("4. Select the downloaded .unitypackage");
                EditorGUILayout.LabelField("5. Click 'Import' (import all files)");
                EditorGUILayout.LabelField("6. Return to this wizard and click 'Next'");
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("Open Nakama GitHub Releases", GUILayout.Height(40)))
                {
                    Application.OpenURL("https://github.com/heroiclabs/nakama-unity/releases");
                }
                
                GUILayout.Space(10);
                
                EditorGUILayout.HelpBox(
                    "After installing Nakama, click 'Next' to verify the installation.",
                    MessageType.None
                );
            }
        }
        
        private void DrawVerificationStep()
        {
            EditorGUILayout.LabelField("Verification", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Re-check Dependencies", GUILayout.Height(30)))
            {
                newtonsoftInstalled = CheckType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
                nakamaInstalled = CheckType("Nakama.IClient, Nakama");
            }
            
            GUILayout.Space(10);
            
            DrawDependencyStatus("Newtonsoft.Json", newtonsoftInstalled);
            GUILayout.Space(10);
            DrawDependencyStatus("Nakama SDK", nakamaInstalled);
            
            GUILayout.Space(20);
            
            if (newtonsoftInstalled && nakamaInstalled)
            {
                EditorGUILayout.HelpBox(
                    "✅ All dependencies verified!\n\nClick 'Next' to complete the setup.",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "❌ Some dependencies are still missing.\n\n" +
                    "Please complete the installation steps and click 'Re-check Dependencies'.",
                    MessageType.Error
                );
            }
        }
        
        private void DrawCompleteStep()
        {
            EditorGUILayout.LabelField("Setup Complete!", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "✅ IntelliVerseX SDK is now ready to use!\n\n" +
                "All required dependencies have been installed and verified.",
                MessageType.Info
            );
            
            GUILayout.Space(20);
            
            EditorGUILayout.LabelField("Next Steps:", EditorStyles.boldLabel);
            GUILayout.Space(5);
            
            EditorGUILayout.LabelField("• Read the SDK documentation");
            EditorGUILayout.LabelField("• Check out the example scenes");
            EditorGUILayout.LabelField("• Start integrating the SDK into your project");
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Open Documentation", GUILayout.Height(40)))
            {
                string docPath = Path.Combine(Application.dataPath, "_IntelliVerseXSDK/README.md");
                if (File.Exists(docPath))
                {
                    System.Diagnostics.Process.Start(docPath);
                }
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Open DEPENDENCIES.md", GUILayout.Height(30)))
            {
                string docPath = Path.Combine(Application.dataPath, "_IntelliVerseXSDK/DEPENDENCIES.md");
                if (File.Exists(docPath))
                {
                    System.Diagnostics.Process.Start(docPath);
                }
            }
        }
        
        private void DrawFooter()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            
            // Back button
            GUI.enabled = currentStep > SetupStep.Welcome && !isInstalling;
            if (GUILayout.Button("Back", GUILayout.Height(30), GUILayout.Width(100)))
            {
                currentStep--;
            }
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            
            // Next/Finish button
            bool canProceed = CanProceedToNextStep();
            GUI.enabled = canProceed && !isInstalling;
            
            string buttonText = currentStep == SetupStep.Complete ? "Finish" : "Next";
            if (GUILayout.Button(buttonText, GUILayout.Height(30), GUILayout.Width(100)))
            {
                if (currentStep == SetupStep.Complete)
                {
                    Close();
                }
                else
                {
                    currentStep++;
                    
                    // Auto-check dependencies when entering check step
                    if (currentStep == SetupStep.CheckDependencies)
                    {
                        newtonsoftInstalled = CheckType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
                        nakamaInstalled = CheckType("Nakama.IClient, Nakama");
                    }
                }
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
        
        private bool CanProceedToNextStep()
        {
            switch (currentStep)
            {
                case SetupStep.Welcome:
                    return true;
                case SetupStep.CheckDependencies:
                    return true;
                case SetupStep.InstallNewtonsoft:
                    return newtonsoftInstalled;
                case SetupStep.InstallNakama:
                    return true; // Can proceed to verification even if not installed
                case SetupStep.Verification:
                    return newtonsoftInstalled && nakamaInstalled;
                case SetupStep.Complete:
                    return true;
                default:
                    return false;
            }
        }
        
        private void DrawDependencyStatus(string name, bool installed)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            Color originalColor = GUI.color;
            GUI.color = installed ? Color.green : Color.red;
            GUILayout.Label(installed ? "✓" : "✗", EditorStyles.boldLabel, GUILayout.Width(20));
            GUI.color = originalColor;
            
            EditorGUILayout.LabelField(name, installed ? "Installed" : "Not Installed");
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void InstallNewtonsoft()
        {
            isInstalling = true;
            newtonsoftRequest = Client.Add("com.unity.nuget.newtonsoft-json");
            EditorApplication.update += CheckInstallProgress;
        }
        
        private void CheckInstallProgress()
        {
            if (newtonsoftRequest != null && newtonsoftRequest.IsCompleted)
            {
                EditorApplication.update -= CheckInstallProgress;
                Repaint();
            }
        }
        
        private bool CheckType(string fullTypeName)
        {
            try
            {
                var type = System.Type.GetType(fullTypeName);
                return type != null;
            }
            catch
            {
                return false;
            }
        }
        
        private void OnDestroy()
        {
            EditorApplication.update -= CheckInstallProgress;
        }
    }
}
#endif
