#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AutoSetupTileVFX
{
    private const string VfxSpritesFolder = "Assets/Sprites/Vfx";
    private const string VfxPrefabsFolder = "Assets/Prefabs/VFX";
    private const string TileEffectsFolder = "Assets/Data/TileEffects";

    [MenuItem("Ascension/Tiles/Auto-Setup Tile VFX (Create Prefabs + Assign)")]
    public static void CreateAndAssign()
    {
        EnsureFolder(VfxPrefabsFolder);

        var powerUpSprite = LoadSpriteByFileName("powerUpVfx");
        var healSprite = LoadSpriteByFileName("healVfx");
        var speedUpSprite = LoadSpriteByFileName("speedUpVfx");
        var speedDownSprite = LoadSpriteByFileName("speedDownVfx");

        var powerUpPrefab = powerUpSprite != null ? CreateOrUpdateVfxPrefab("PowerUpVfx", powerUpSprite) : null;
        var healPrefab = healSprite != null ? CreateOrUpdateVfxPrefab("HealVfx", healSprite) : null;
        var speedUpPrefab = speedUpSprite != null ? CreateOrUpdateVfxPrefab("SpeedUpVfx", speedUpSprite) : null;
        var speedDownPrefab = speedDownSprite != null ? CreateOrUpdateVfxPrefab("SpeedDownVfx", speedDownSprite) : null;

        int assigned = 0;
        var guids = AssetDatabase.FindAssets("t:TileEffect", new[] { TileEffectsFolder });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var effect = AssetDatabase.LoadAssetAtPath<TileEffect>(path);
            if (effect == null) continue;

            GameObject prefab = null;
            switch (effect.effectType)
            {
                case TileEffectType.PowerUp:
                    prefab = powerUpPrefab;
                    break;
                case TileEffectType.Heal:
                    prefab = healPrefab;
                    break;
                case TileEffectType.SpeedUp:
                case TileEffectType.Ice:
                    prefab = speedUpPrefab;
                    break;
                case TileEffectType.SpeedDown:
                case TileEffectType.Mud:
                    prefab = speedDownPrefab;
                    break;
                case TileEffectType.Damage:
                    prefab = speedDownPrefab;
                    break;
            }

            if (prefab != null && effect.vfxPrefab != prefab)
            {
                effect.vfxPrefab = prefab;
                EditorUtility.SetDirty(effect);
                assigned++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Auto-Setup Tile VFX",
            $"Prefabs VFX generados en: {VfxPrefabsFolder}\nTileEffects actualizados: {assigned}",
            "OK"
        );
    }

    private static Sprite LoadSpriteByFileName(string baseName)
    {
        string[] candidates =
        {
            $"{VfxSpritesFolder}/{baseName}.png",
            $"{VfxSpritesFolder}/{baseName}.PNG"
        };

        foreach (var p in candidates)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (s != null) return s;
        }

        Debug.LogWarning($"[AutoSetupTileVFX] No se encontró sprite {baseName} en {VfxSpritesFolder}");
        return null;
    }

    private static GameObject CreateOrUpdateVfxPrefab(string prefabName, Sprite sprite)
    {
        string prefabPath = $"{VfxPrefabsFolder}/{prefabName}.prefab";

        var temp = new GameObject(prefabName);
        try
        {
            var sr = temp.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 0;

            temp.AddComponent<PulseVFX>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            return prefab;
        }
        finally
        {
            Object.DestroyImmediate(temp);
        }
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
        var name = Path.GetFileName(folderPath);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)) return;

        if (!AssetDatabase.IsValidFolder(parent))
        {
            // Create recursively (one level)
            var grandParent = Path.GetDirectoryName(parent)?.Replace('\\', '/');
            var parentName = Path.GetFileName(parent);
            if (!string.IsNullOrEmpty(grandParent) && !string.IsNullOrEmpty(parentName) && !AssetDatabase.IsValidFolder(parent))
            {
                if (!AssetDatabase.IsValidFolder(grandParent))
                {
                    Debug.LogWarning($"[AutoSetupTileVFX] Carpeta padre no existe: {grandParent}");
                    return;
                }
                AssetDatabase.CreateFolder(grandParent, parentName);
            }
        }

        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
