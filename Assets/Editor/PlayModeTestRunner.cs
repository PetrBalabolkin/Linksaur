using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Linksaurus.Core;
using Linksaurus.Spawning;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";

        private static readonly int WaitFrames = 60;
        private static readonly float TestTimeout = 20.0f;

        private static List<string> _capturedLogs = new List<string>();

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            if (state == "WaitingForCompile")
            {
                EditorApplication.delayCall += () =>
                {
                    SessionState.SetString(StateKey, "EnteringPlayMode");
                    EditorApplication.isPlaying = true;
                };
            }
            else if (state == "EnteringPlayMode" && EditorApplication.isPlaying)
            {
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
            else if (state == "InPlayMode" && EditorApplication.isPlaying)
            {
                EditorApplication.update += WaitFramesThenRun;
            }
            else if (state == "Done")
            {
                EditorApplication.delayCall += SelfDestruct;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;
            if (_testDone) return;

            if (!_setupDone)
            {
                _setupDone = true;
                _testStartTime = EditorApplication.timeSinceStartup;
                Setup();
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            if (elapsed >= TestTimeout) { FinishTest(true, "Test timed out"); return; }

            if (Tick(elapsed)) FinishTest(false, null);
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            string resultJson = JsonUtility.ToJson(new { success = !isError, error = errorMessage, maxGap = _maxGapFound });
            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath)) AssetDatabase.DeleteAsset(scriptPath);
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        private static float _maxGapFound = 0;
        private static ScrollingObject _skyTile1;
        private static ScrollingObject _skyTile2;

        private static void Setup()
        {
            if (SceneManager.GetActiveScene().name != "GameScene") { SceneManager.LoadScene("GameScene"); return; }
            if (GameManager.Instance == null) return;
            GameManager.Instance.InitialScrollSpeed = GameManager.Instance.MaxScrollSpeed;
            GameManager.Instance.StartGame();
            
            _skyTile1 = GameObject.Find("Layer_0_0")?.GetComponent<ScrollingObject>();
            _skyTile2 = GameObject.Find("Layer_0_1")?.GetComponent<ScrollingObject>();
            Debug.Log("[Test] Setup complete. Sky speed multiplier is 0.1. Global speed: " + GameManager.Instance.ScrollSpeed);
        }

        private static bool Tick(float elapsed)
        {
            if (_skyTile1 == null || _skyTile2 == null) return false;

            float dist = Mathf.Abs(_skyTile1.transform.position.x - _skyTile2.transform.position.x);
            // Expected width is 22
            float remainder = dist % 22f;
            float gap = Mathf.Min(remainder, 22f - remainder);
            if (gap > _maxGapFound) _maxGapFound = gap;

            return elapsed >= 5.0f;
        }
    }
}
