using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Crea el prefab de proyectil enemigo: esfera pequeña con collider
/// Tamaño: 1/4 del enemigo (enemigo radius ~0.4, proyectil radius ~0.1)
/// </summary>
public class CreateEnemyProjectilePrefab : EditorWindow
{
    [MenuItem("Ascension/Setup/Create Enemy Projectile Prefab")]
    static void CreatePrefab()
    {
        // Crear GameObject
        GameObject projectile = new GameObject("EnemyProjectile");
        
        // SpriteRenderer con círculo
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = Color.white; // El color ya está en el sprite
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 10;
        
        // Escala: proyectil pequeño (0.25, 0.25) = 1/4 del enemigo
        projectile.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
        
        // Rigidbody2D
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        // CircleCollider2D como trigger
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.radius = 0.5f; // Radio relativo al sprite (que ya está escalado)
        col.isTrigger = true;
        
        // Script EnemyProjectile
        EnemyProjectile script = projectile.AddComponent<EnemyProjectile>();
        script.damage = 1;
        script.lifetime = 5f;
        
        // Asegurar que el prefab se guarde en Prefabs/Enemies/
        string folderPath = "Assets/Prefabs/Enemies";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");
        }
        
        string prefabPath = $"{folderPath}/EnemyProjectile.prefab";
        
        // Guardar como prefab
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);
        
        // Limpiar GameObject temporal
        DestroyImmediate(projectile);
        
        // Copiar a Resources para que funcione en runtime
        CopyToResources(savedPrefab);
        
        // Asignar automáticamente a SlimeGreen prefabs
        AssignProjectileToSlimes(savedPrefab);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog(
            "Proyectil Enemigo Creado",
            $"✅ Prefab creado en:\n{prefabPath}\n\n" +
            "Características:\n" +
            "- Radio: 1/4 del enemigo\n" +
            "- Color: Verde brillante\n" +
            "- Velocidad: Configurada en SlimeGreen (0.3125)\n" +
            "- Daño: 1\n" +
            "- Lifetime: 5 segundos\n\n" +
            "Asignado automáticamente a SlimeGreen prefabs.",
            "OK"
        );
        
        Selection.activeObject = savedPrefab;
        EditorGUIUtility.PingObject(savedPrefab);
        
        Debug.Log($"✅ [CreateEnemyProjectilePrefab] Prefab creado: {prefabPath}");
    }
    
    /// <summary>
    /// Crea un sprite circular creando una textura
    /// </summary>
    private static Sprite CreateCircleSprite()
    {
        // Crear textura circular
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
                    // Interior del círculo - verde brillante
                    pixels[y * size + x] = new Color(0.2f, 1f, 0.2f, 1f);
                }
                else if (distance <= radius + 1)
                {
                    // Borde - verde oscuro
                    pixels[y * size + x] = new Color(0.1f, 0.5f, 0.1f, 1f);
                }
                else
                {
                    // Exterior - transparente
                    pixels[y * size + x] = new Color(0, 0, 0, 0);
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point; // Pixel perfect
        
        // Guardar textura como asset
        string texturePath = "Assets/Sprites/EnemyProjectile_Circle.png";
        System.IO.Directory.CreateDirectory("Assets/Sprites");
        
        byte[] pngData = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(texturePath, pngData);
        
        AssetDatabase.Refresh();
        
        // Configurar import settings
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16; // PPU=16
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        
        // Cargar sprite
        return AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
    }
    
    /// <summary>
    /// Copia el prefab a Resources para que funcione en runtime
    /// </summary>
    private static void CopyToResources(GameObject prefab)
    {
        // Crear carpeta Resources si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        string resourcePath = "Assets/Resources/EnemyProjectile.prefab";
        
        // Copiar el prefab
        if (AssetDatabase.CopyAsset("Assets/Prefabs/Enemies/EnemyProjectile.prefab", resourcePath))
        {
            Debug.Log($"✓ Proyectil copiado a Resources para runtime");
        }
    }
    
    /// <summary>
    /// Asigna el proyectil a todos los prefabs SlimeGreen
    /// </summary>
    private static void AssignProjectileToSlimes(GameObject projectilePrefab)
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
