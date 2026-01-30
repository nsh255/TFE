using UnityEditor;
using UnityEngine;

/// <summary>
/// Limpia componentes Missing Scripts de los prefabs de enemigos
/// </summary>
public class CleanMissingScripts : EditorWindow
{
    [MenuItem("Ascension/Debug/Clean Missing Scripts from Enemies")]
    /// <summary>
    /// Ejecuta la limpieza de scripts faltantes en prefabs de enemigos.
    /// </summary>
    static void ShowWindow()
    {
        CleanAllMissingScripts();
    }

    /// <summary>
    /// Elimina componentes rotos y asegura componentes básicos en prefabs.
    /// </summary>
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
                cleanedCount++;
                totalRemoved += removedFromThis;
                
                // Asegurar que tiene los componentes básicos necesarios
                EnsureBasicComponents(instance);
                
                // Guardar cambios
                PrefabUtility.SaveAsPrefabAsset(instance, path);
            }
            else
            {
            }
            
            PrefabUtility.UnloadPrefabContents(instance);
        }

        AssetDatabase.SaveAssets();

        string message = $"Limpieza completada:\n\n" +
                $"- {cleanedCount} prefabs limpiados\n" +
                $"- {totalRemoved} componentes rotos eliminados.";

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
        }

        // Asegurar CircleCollider2D
        CircleCollider2D col = instance.GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = instance.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            col.isTrigger = false;
        }

        // Asegurar SpriteRenderer
        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = instance.AddComponent<SpriteRenderer>();
        }

        // Verificar si tiene script de comportamiento Enemy
        Enemy enemy = instance.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogWarning($"{instance.name} no tiene componente Enemy base.");
        }
    }
}
