#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using IntelliVerseX.Games.Leaderboard;

namespace IntelliVerseX.Games.Leaderboard.Editor
{
    /// <summary>
    /// Custom inspector for IVXGLeaderboard.
    /// Provides debug tools for testing leaderboard functionality during Play Mode.
    /// </summary>
    [CustomEditor(typeof(IVXGLeaderboard))]
    public class IVXGLeaderboardEditor : UnityEditor.Editor
    {
        private bool _showDebugTools = true;
        private bool _showScoreTests = true;
        private bool _showLeaderboardTests = true;
        private bool _showSettings = true;

        public override void OnInspectorGUI()
        {
            var leaderboard = (IVXGLeaderboard)target;

            // =============== Debug Tools Header ===============
            EditorGUILayout.Space();
            
            _showDebugTools = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebugTools, "🎮 Debug Tools");
            
            if (_showDebugTools)
            {
                EditorGUILayout.HelpBox(
                    "Use these buttons to test Nakama / Leaderboard integration during Play Mode.\n" +
                    "Make sure to authenticate and initialize Nakama first.",
                    MessageType.Info);

                // Only allow buttons while in Play Mode
                bool prevEnabled = GUI.enabled;
                GUI.enabled = Application.isPlaying;

                EditorGUILayout.Space(5);

                // =============== Initialize Nakama ===============
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🛠 Initialize Nakama", GUILayout.Height(30)))
                {
                    _ = leaderboard.InitializeNakamaAsync();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);

                // =============== Score Submission Tests ===============
                _showScoreTests = EditorGUILayout.Foldout(_showScoreTests, "📊 Score Submission Tests", true);
                
                if (_showScoreTests)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    if (GUILayout.Button("🎯 Submit Random Score (100–10000)", GUILayout.Height(25)))
                    {
                        int randomScore = Random.Range(100, 10001);
                        _ = leaderboard.SubmitScoreAsync(randomScore);
                    }

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("🎯 Submit High Score (9999)", GUILayout.Height(25)))
                    {
                        leaderboard.TestSubmitHighScore();
                    }

                    if (GUILayout.Button("🎯 Submit Low Score (10)", GUILayout.Height(25)))
                    {
                        leaderboard.TestSubmitLowScore();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space(5);

                // =============== Leaderboard Tests ===============
                _showLeaderboardTests = EditorGUILayout.Foldout(_showLeaderboardTests, "🏆 Leaderboard Tests", true);
                
                if (_showLeaderboardTests)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    if (GUILayout.Button("📊 Fetch All Leaderboards (Top 10)", GUILayout.Height(25)))
                    {
                        leaderboard.TestFetchLeaderboardsTop10();
                    }

                    if (GUILayout.Button("📊 Fetch All Leaderboards (Top 50)", GUILayout.Height(25)))
                    {
                        leaderboard.TestFetchLeaderboardsTop50();
                    }

                    EditorGUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("🏅 Get Player Best Score", GUILayout.Height(25)))
                    {
                        leaderboard.TestGetPlayerBestScore();
                    }

                    if (GUILayout.Button("♻ Reset Win Streak", GUILayout.Height(25)))
                    {
                        leaderboard.TestResetWinStreak();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }

                GUI.enabled = prevEnabled;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            // =============== Status Block ===============
            EditorGUILayout.LabelField("📈 Status", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string initLabel = leaderboard.NakamaInitialized ? "✅ Yes" : "❌ No";
            EditorGUILayout.LabelField("Initialized:", initLabel);

            string guestLabel = leaderboard.IsGuestUser ? "✅ Yes (Leaderboard Disabled)" : "❌ No";
            EditorGUILayout.LabelField("Guest User:", guestLabel);

            EditorGUILayout.LabelField("User ID:",
                string.IsNullOrEmpty(leaderboard.NakamaUserId) || leaderboard.NakamaUserId == "<none>"
                    ? "<none>"
                    : leaderboard.NakamaUserId);

            EditorGUILayout.LabelField("Username:",
                string.IsNullOrEmpty(leaderboard.NakamaUsername) || leaderboard.NakamaUsername == "<none>"
                    ? "<none>"
                    : leaderboard.NakamaUsername);

            EditorGUILayout.LabelField("Current Rank:",
                leaderboard.CurrentPlayerRank > 0
                    ? $"#{leaderboard.CurrentPlayerRank}"
                    : "-");

            EditorGUILayout.LabelField("Best Score:",
                leaderboard.CurrentPlayerBestScore > 0
                    ? leaderboard.CurrentPlayerBestScore.ToString("N0")
                    : "-");

            EditorGUILayout.LabelField("Is Refreshing:", leaderboard.IsRefreshing ? "✅ Yes" : "❌ No");

            EditorGUILayout.LabelField("Last Error:",
                string.IsNullOrEmpty(leaderboard.LastErrorMessage) ? "-" : leaderboard.LastErrorMessage);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // =============== Settings (serialized fields) ===============
            _showSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showSettings, "⚙️ Settings");
            
            if (_showSettings)
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoInitializeNakama"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoTestOnStart"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("enableDebugLogs"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Test Configuration", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("testMinScore"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("testMaxScore"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("testLeaderboardLimit"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useGlobalForRankAndBest"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Timeout Configuration", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("operationTimeoutSeconds"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rankCheckCooldownSeconds"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Notification Integration", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("enableRankDropNotifications"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Platform Settings", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("forceMainThreadCallbacks"));

                serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
#endif
