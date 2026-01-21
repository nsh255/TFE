using UnityEditor;
using UnityEngine;

/// <summary>
/// Configura correctamente los Rigidbody2D de los enemigos para evitar rebotes/repeler
/// </summary>
public class FixEnemyPhysics : EditorWindow
{
    [MenuItem("Ascension/Debug/Fix Enemy Physics")]
    static void ShowWindow()
    {
        FixAllEnemyPrefabs();
    }

    private static void FixAllEnemyPrefabs()
    {
        string[] prefabPaths = {
            "Assets/Prefabs/Enemies/SlimeRed.prefab",
            "Assets/Prefabs/Enemies/SlimeBlue.prefab",
            "Assets/Prefabs/Enemies/SlimeGreen.prefab",
            "Assets/Prefabs/Enemies/ChaserEnemy.prefab",
            "Assets/Prefabs/Enemies/JumperEnemy.prefab",
            "Assets/Prefabs/Enemies/ShooterEnemy.prefab"
        };

        int fixedCount = 0;

        foreach (var path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"No se encontró prefab: {path}");
                continue;
            }

            // Cargar el prefab para edición
            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                rb = instance.AddComponent<Rigidbody2D>();
                Debug.Log($"✅ Rigidbody2D añadido a {prefab.name}");
            }

            // Configurar propiedades óptimas para enemigos top-down
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 1f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.gravityScale = 0f; // Sin gravedad (top-down)
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // No rotar
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Verificar Collider2D
            CircleCollider2D col = instance.GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = instance.AddComponent<CircleCollider2D>();
                col.radius = 0.4f; // Radio razonable para enemigo
                Debug.Log($"✅ CircleCollider2D añadido a {prefab.name}");
            }

            // Guardar cambios
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);

            Debug.Log($"✅ {prefab.name}: Rigidbody2D configurado (FreezeRotation, gravityScale=0, Interpolate)");
            fixedCount++;
        }

        AssetDatabase.SaveAssets();

        string message = $"✅ {fixedCount} prefabs de enemigos configurados:\n\n" +
                        "- Rigidbody2D: Dynamic\n" +
                        "- Gravity: 0 (top-down)\n" +
                        "- Freeze Rotation: activado\n" +
                        "- Interpolation: Interpolate\n" +
                        "- Collision Detection: Continuous\n\n" +
                        "Esto evita rebotes y rotaciones no deseadas.";

        Debug.Log(message);

        EditorUtility.DisplayDialog(
            "Física de Enemigos Corregida",
            message,
            "OK"
        );
    }
}
