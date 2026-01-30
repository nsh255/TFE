using TMPro;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AscensionTmpFontAssetRepair
{
    private const string FontAssetPath = "Assets/Font/VCR_OSD_MONO.asset";

    static AscensionTmpFontAssetRepair()
    {
        // Dejar que Unity termine de compilar/importar antes de tocar assets.
        EditorApplication.delayCall += TryRepair;
    }

    /// <summary>
    /// Repara el TMP Font Asset si está incompleto.
    /// </summary>
    private static void TryRepair()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset == null) return;

        if (!IsBroken(fontAsset)) return;

        Repair(fontAsset);
    }

    /// <summary>
    /// Comprueba si faltan atlas o material en el TMP Font Asset.
    /// </summary>
    private static bool IsBroken(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null) return false;

        var atlas = fontAsset.atlasTextures;
        if (atlas == null || atlas.Length == 0 || atlas[0] == null) return true;

        if (fontAsset.material == null) return true;

        return false;
    }

    /// <summary>
    /// Reconstruye atlas y material y los asigna al TMP Font Asset.
    /// </summary>
    public static void Repair(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null) return;

        var path = AssetDatabase.GetAssetPath(fontAsset);
        if (string.IsNullOrEmpty(path)) return;

        int w = fontAsset.atlasWidth > 0 ? fontAsset.atlasWidth : 1024;
        int h = fontAsset.atlasHeight > 0 ? fontAsset.atlasHeight : 1024;

        // Atlas texture (sub-asset)
        var atlasTex = new Texture2D(w, h, TextureFormat.Alpha8, false, true)
        {
            name = fontAsset.name + " Atlas",
            hideFlags = HideFlags.HideInHierarchy
        };

        // Material (sub-asset)
        var shader = Shader.Find("TextMeshPro/Distance Field");
        if (shader == null)
            shader = Shader.Find("TextMeshPro/Mobile/Distance Field");

        var mat = new Material(shader)
        {
            name = fontAsset.name + " Material",
            hideFlags = HideFlags.HideInHierarchy
        };
        if (shader != null)
        {
            mat.SetTexture(ShaderUtilities.ID_MainTex, atlasTex);
        }

        AssetDatabase.AddObjectToAsset(atlasTex, path);
        AssetDatabase.AddObjectToAsset(mat, path);

        // Asignar referencias por SerializedObject (robusto a cambios internos de TMP).
        var so = new SerializedObject(fontAsset);

        var atlasProp = so.FindProperty("m_AtlasTextures");
        if (atlasProp != null)
        {
            atlasProp.arraySize = 1;
            atlasProp.GetArrayElementAtIndex(0).objectReferenceValue = atlasTex;
        }

        var matProp = so.FindProperty("m_Material");
        if (matProp != null)
        {
            matProp.objectReferenceValue = mat;
        }

        // Algunos assets TMP antiguos/mixtos también guardan un campo 'atlas'.
        var legacyAtlasProp = so.FindProperty("atlas");
        if (legacyAtlasProp != null)
        {
            legacyAtlasProp.objectReferenceValue = atlasTex;
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }
}
