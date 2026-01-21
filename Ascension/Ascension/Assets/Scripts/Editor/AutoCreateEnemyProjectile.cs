using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Auto-ejecuta la creación del proyectil enemigo si no existe
/// </summary>
[InitializeOnLoad]
public class AutoCreateEnemyProjectile
{
    static AutoCreateEnemyProjectile()
    {
        EditorApplication.delayCall += CheckAndCreateProjectile;
    }

    private static void CheckAndCreateProjectile()
    {
        string prefabPath = "Assets/Prefabs/Enemies/EnemyProjectile.prefab";
        
        // Si ya existe, no hacer nada
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            return;
        }

        Debug.Log("[AutoCreateEnemyProjectile] Proyectil no encontrado, creándolo automáticamente...");
        
        // Asegurar que existe la carpeta
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Enemies"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
        {
            AssetDatabase.CreateFolder("Assets", "Sprites");
        }

        // Crear textura circular
        CreateCircleTexture();
        
        // Crear prefab
        GameObject projectile = new GameObject("EnemyProjectile");
        
        // SpriteRenderer
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        string spritePath = "Assets/Sprites/EnemyProjectile_Circle.png";
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        sr.color = Color.white;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 10;
        
        // Escala pequeña
        projectile.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
        
        // Rigidbody2D
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        // CircleCollider2D
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;
        
        // Script EnemyProjectile
        EnemyProjectile script = projectile.AddComponent<EnemyProjectile>();
        script.damage = 1;
        script.lifetime = 5f;
        
        // Guardar prefab
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);
        Object.DestroyImmediate(projectile);
        
        // Copiar a Resources para runtime
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        AssetDatabase.CopyAsset(prefabPath, "Assets/Resources/EnemyProjectile.prefab");
        
        // Asignar a SlimeGreen prefabs
        AssignToSlimes(savedPrefab);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ [AutoCreateEnemyProjectile] Proyectil creado en: {prefabPath}");
    }

    private static void CreateCircleTexture()
    {
        string texturePath = "Assets/Sprites/EnemyProjectile_Circle.png";
        
        // Si ya existe, no recrear
        if (File.Exists(texturePath))
        {
            return;
        }

        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance <= radius)
                {
                    pixels[y * size + x] = new Color(0.2f, 1f, 0.2f, 1f);
                }
                else if (distance <= radius + 1)
                {
                    pixels[y * size + x] = new Color(0.1f, 0.5f, 0.1f, 1f);
                }
                else
                {
                    pixels[y * size + x] = new Color(0, 0, 0, 0);
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(texturePath, pngData);
        
        AssetDatabase.Refresh();
        
        // Configurar import settings
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static void AssignToSlimes(GameObject projectilePrefab)
    {
        string[] slimePaths = new string[]
        {
            "Assets/Prefabs/Enemies/SlimeGreen.prefab",
            "Assets/Prefabs/Enemies/ShooterEnemy.prefab"
        };
        
        foreach (string path in slimePaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            
            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            
            SlimeGreen slime = instance.GetComponent<SlimeGreen>();
            if (slime != null)
            {
                slime.projectilePrefab = projectilePrefab;
                Debug.Log($"✓ {prefab.name}: Proyectil asignado");
            }
            
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }
    }
}
