using UnityEditor;
using UnityEngine;

/// <summary>
/// Verifica y corrige la configuración de colliders para que las armas puedan dañar enemigos
/// </summary>
public class FixWeaponColliders : EditorWindow
{
    [MenuItem("Ascension/Debug/Fix Weapon and Enemy Colliders")]
    /// <summary>
    /// Ejecuta la verificación y corrección de colisionadores.
    /// </summary>
    static void ShowWindow()
    {
        FixAllColliders();
    }

    /// <summary>
    /// Corrige colliders en armas y enemigos para la detección de daño.
    /// </summary>
    private static void FixAllColliders()
    {
        int fixedCount = 0;

        // 1. Configurar armas (deben tener colliders como Trigger)
        string[] weaponPaths = {
            "Assets/Prefabs/Weapons/Sword.prefab",
            "Assets/Prefabs/Weapons/Bow.prefab",
            "Assets/Prefabs/Weapons/Staff.prefab",
            "Assets/Prefabs/Weapons/Projectile.prefab"
        };

        foreach (var path in weaponPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            
            // Buscar collider2D
            Collider2D col = instance.GetComponent<Collider2D>();
            if (col == null)
            {
                // Añadir BoxCollider2D o CircleCollider2D según el arma
                if (path.Contains("Projectile"))
                {
                    CircleCollider2D circleCol = instance.AddComponent<CircleCollider2D>();
                    circleCol.isTrigger = true;
                    circleCol.radius = 0.3f;
                }
                else
                {
                    BoxCollider2D boxCol = instance.AddComponent<BoxCollider2D>();
                    boxCol.isTrigger = true;
                    boxCol.size = new Vector2(1f, 0.3f);
                }
                fixedCount++;
            }
            else
            {
                // Asegurar que está como Trigger
                if (!col.isTrigger)
                {
                    col.isTrigger = true;
                    fixedCount++;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }

        // 2. Verificar enemigos (deben tener colliders NO-trigger y tag "Enemy")
        string[] enemyPaths = {
            "Assets/Prefabs/Enemies/SlimeRed.prefab",
            "Assets/Prefabs/Enemies/SlimeBlue.prefab",
            "Assets/Prefabs/Enemies/SlimeGreen.prefab",
            "Assets/Prefabs/Enemies/ChaserEnemy.prefab",
            "Assets/Prefabs/Enemies/JumperEnemy.prefab",
            "Assets/Prefabs/Enemies/ShooterEnemy.prefab"
        };

        foreach (var path in enemyPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            
            // Verificar tag
            if (instance.tag != "Enemy")
            {
                instance.tag = "Enemy";
                fixedCount++;
            }

            // Verificar collider NO es trigger
            Collider2D col = instance.GetComponent<Collider2D>();
            if (col != null && col.isTrigger)
            {
                col.isTrigger = false;
                fixedCount++;
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }

        AssetDatabase.SaveAssets();

        string message = $"{fixedCount} configuraciones corregidas:\n\n" +
                "ARMAS:\n" +
                "- Colliders configurados como trigger\n" +
                "- Detección por OnTriggerEnter2D\n\n" +
                "ENEMIGOS:\n" +
                "- Tag 'Enemy' asignado\n" +
                "- Colliders no trigger (sólidos).";

        EditorUtility.DisplayDialog(
            "Colliders Configurados",
            message,
            "OK"
        );
    }
}
