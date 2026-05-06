using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Linq;

public class Jersey10FontReplacer
{
    [MenuItem("Tools/Linksaurus/Apply Jersey10 to All UI Text")]
    public static void ApplyJersey10ToAllUI()
    {
        string fontAssetPath = "Assets/Jersey10-Regular SDF.asset";
        TMP_FontAsset jersey10 = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);

        if (jersey10 == null)
        {
            Debug.LogError("Jersey10 SDF font asset not found! Run 'Create Jersey10 Font Asset' first.");
            return;
        }

        int changedCount = 0;

        // 1. Update all UI prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/UI" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabRoot == null) continue;

            // Need to open prefab for editing to modify it properly
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);
            if (prefabInstance == null) continue;

            TMP_Text[] texts = prefabInstance.GetComponentsInChildren<TMP_Text>(true);
            bool modified = false;
            foreach (TMP_Text text in texts)
            {
                if (text.font != jersey10)
                {
                    Undo.RecordObject(text, "Change Font to Jersey10");
                    text.font = jersey10;
                    EditorUtility.SetDirty(text);
                    modified = true;
                    changedCount++;
                }
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            }
            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }

        // 2. Update all open scenes
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            var rootObjects = scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text text in texts)
                {
                    if (text.font != jersey10)
                    {
                        Undo.RecordObject(text, "Change Font to Jersey10");
                        text.font = jersey10;
                        EditorUtility.SetDirty(text);
                        changedCount++;
                    }
                }
            }

            if (scene.isDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Applied Jersey10 font to {changedCount} TMP_Text components.");
    }
}
