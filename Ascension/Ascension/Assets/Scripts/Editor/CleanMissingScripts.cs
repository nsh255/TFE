using UnityEditor;
using UnityEngine;

/// <summary>
/// Limpia componentes Missing Scripts de los prefabs de enemigos
/// </summary>
public class CleanMissingScripts : EditorWindow
{
    [MenuItem("Ascension/Debug/Clean Missing Scripts from Enemies")]
    static void ShowWindow()
    {
        CleanAllMissingScripts();
    }

    private static void CleanAllMissingScripts()
    {
        string[] enemyPaths = {
            "Assets/Prefabs/Enemies/ChaserEnemy.prefab",
            "Assets/Prefabs/Enemies/JumperEnemy.prefab",
            "Assets/Prefabs/Enemies/ShooterEnemy.prefab",
            "Assets/Prefabs/Enemies/BossEnemy.prefab"
        };

        int cleanedCount = 0;
        int totalRemoved = 0;

        foreach (var path in enemyPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"No se encontró prefab: {path}");
                continue;
            }

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            
            // Contar y eliminar componentes missing
            int removedFromThis = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(instance);
            
            if (removedFromThis > 0)
            {
                Debug.Log($"✅ {prefab.name}: {removedFromThis} componente(s) rotos eliminados");
                cleanedCount++;
                totalRemoved += removedFromThis;
                
                // Asegurar que tiene los componentes básicos necesarios
                EnsureBasicComponents(instance);
                
                // Guardar cambios
                PrefabUtility.SaveAsPrefabAsset(instance, path);
            }
            else
            {
                Debug.Log($"✓ {prefab.name}: Sin componentes rotos");
            }
            
            PrefabUtility.UnloadPrefabContents(instance);
        }

        AssetDatabase.SaveAssets();

        string message = $"✅ Limpieza completada:\n\n" +
                        $"- {cleanedCount} prefabs limpiados\n" +
                        $"- {totalRemoved} componentes rotos eliminados\n\n" +
                        "Los prefabs ahora pueden guardarse correctamente.";

        Debug.Log(message);

        EditorUtility.DisplayDialog(
            "Limpieza Completada",
            message,
            "OK"
        );
    }

    private static void EnsureBasicComponents(GameObject instance)
    {
        // Asegurar Rigidbody2D
        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = instance.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log($"  → Rigidbody2D añadido a {instance.name}");
        }

        // Asegurar CircleCollider2D
        CircleCollider2D col = instance.GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = instance.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            col.isTrigger = false;
            Debug.Log($"  → CircleCollider2D añadido a {instance.name}");
        }

        // Asegurar SpriteRenderer
        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = instance.AddComponent<SpriteRenderer>();
            Debug.Log($"  → SpriteRenderer añadido a {instance.name}");
        }

        // Verificar si tiene script de comportamiento Enemy
        Enemy enemy = instance.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogWarning($"  ⚠️ {instance.name} no tiene componente Enemy base!");
        }
    }
}
