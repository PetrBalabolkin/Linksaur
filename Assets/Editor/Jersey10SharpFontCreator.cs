using UnityEngine;
using UnityEditor;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine.TextCore.LowLevel;

public class Jersey10SharpFontCreator
{
    [MenuItem("Tools/Linksaurus/Create Jersey10 Sharp Font Asset")]
    public static void CreateJersey10SharpFontAsset()
    {
        string fontPath = "Assets/Jersey10-Regular.ttf";
        string outputPath = "Assets/Jersey10-Regular SDF.asset";

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (sourceFont == null)
        {
            Debug.LogError("Jersey10-Regular.ttf not found at: " + fontPath);
            return;
        }

        // Delete old blurry asset if it exists so we can recreate it crisp
        TMP_FontAsset existingAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath);
        if (existingAsset != null)
        {
            AssetDatabase.DeleteAsset(outputPath);
            AssetDatabase.SaveAssets();
        }

        // Create font asset with RASTER mode for crisp pixel-perfect rendering
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            samplingPointSize: 32,
            atlasPadding: 2,
            renderMode: (GlyphRenderMode)4118, // RASTER
            atlasWidth: 1024,
            atlasHeight: 1024,
            atlasPopulationMode: AtlasPopulationMode.Dynamic
        );

        fontAsset.name = "Jersey10-Regular SDF";

        AssetDatabase.CreateAsset(fontAsset, outputPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Jersey10 SHARP RASTER font asset created at: " + outputPath);
    }
}
