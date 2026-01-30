using UnityEditor;
using UnityEngine;

/// <summary>
/// Normaliza PPU de los sprites usados por los proyectiles y resetea la escala de los prefabs.
/// - Pone PPU=16, Filter=Point, Compression=None.
/// - Prefabs: BulletPrefab, EnemyProjectile → localScale = (1,1,1)
/// Uso: Menu Ascension/Setup/Fix Projectile PPU & Scale
/// </summary>
public static class FixProjectilePPUAndScale
{
    [MenuItem("Ascension/Setup/Fix Projectile PPU & Scale")] 
    /// <summary>
    /// Ejecuta la normalización de sprites y la corrección de prefabs.
    /// </summary>
    public static void Run()
    {
        int changedImporters = 0;
        int changedPrefabs = 0;

        // Asegurar que existe un sprite circular para proyectiles
        string playerBulletSpritePath = "Assets/Sprites/Weapons/PlayerBullet_Circle.png";
        string enemyBulletSpritePath = "Assets/Sprites/EnemyProjectile_Circle.png";
        
        if (!System.IO.File.Exists(playerBulletSpritePath))
        {
            CreateCircleSprite(playerBulletSpritePath, new Color(1f, 0.92f, 0.016f, 1f)); // Amarillo
        }
        
        // Normalizar los sprites de proyectil
        NormalizeSpriteImporter(playerBulletSpritePath, ref changedImporters);
        NormalizeSpriteImporter(enemyBulletSpritePath, ref changedImporters);

        // Cargar prefabs
        string[] prefabPaths = {
            "Assets/Prefabs/Weapons/BulletPrefab.prefab",
            "Assets/Prefabs/Enemies/EnemyProjectile.prefab"
        };
        
        string[] spritePaths = {
            playerBulletSpritePath,
            enemyBulletSpritePath
        };

        for (int i = 0; i < prefabPaths.Length; i++)
        {
            string path = prefabPaths[i];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[FixProjectilePPUAndScale] Prefab no encontrado: {path}");
                continue;
            }

            // Ajustar escala si es distinta de (1,1,1)
            if (prefab.transform.localScale != Vector3.one)
            {
                prefab.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(prefab);
                changedPrefabs++;
            }

            // Asignar el sprite correcto
            var sr = prefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
                if (newSprite != null && sr.sprite != newSprite)
                {
                    sr.sprite = newSprite;
                    EditorUtility.SetDirty(prefab);
                    changedPrefabs++;
                }
            }
            else
            {
                Debug.LogWarning($"[FixProjectilePPUAndScale] SpriteRenderer no encontrado en prefab {prefab.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Fix Projectile PPU & Scale",
            $"Importers modificados: {changedImporters}\nPrefabs ajustados: {changedPrefabs}.", "OK");
    }

    /// <summary>
    /// Normaliza la configuración de importación de un sprite.
    /// </summary>
    private static void NormalizeSpriteImporter(string path, ref int changedCount)
    {
        if (!System.IO.File.Exists(path)) return;
        
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        bool modified = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            modified = true;
        }
        if (importer.spritePixelsPerUnit != 16)
        {
            importer.spritePixelsPerUnit = 16;
            modified = true;
        }
        if (importer.filterMode != FilterMode.Point)
        {
            importer.filterMode = FilterMode.Point;
            modified = true;
        }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            modified = true;
        }
        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            modified = true;
        }
        if (importer.wrapMode != TextureWrapMode.Clamp)
        {
            importer.wrapMode = TextureWrapMode.Clamp;
            modified = true;
        }
        if (modified)
        {
            importer.SaveAndReimport();
            changedCount++;
        }
    }

    /// <summary>
    /// Crea un sprite circular simple en disco.
    /// </summary>
    private static void CreateCircleSprite(string path, Color color)
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        
        Color transparent = new Color(0, 0, 0, 0);
        int center = size / 2;
        float radius = size / 2f - 1;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                {
                    tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, transparent);
                }
            }
        }
        
        tex.Apply();
        
        string dir = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }
        
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.Refresh();
    }
}
