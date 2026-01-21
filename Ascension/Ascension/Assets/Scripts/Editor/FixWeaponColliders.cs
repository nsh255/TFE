using UnityEditor;
using UnityEngine;

/// <summary>
/// Verifica y corrige la configuración de colliders para que las armas puedan dañar enemigos
/// </summary>
public class FixWeaponColliders : EditorWindow
{
    [MenuItem("Ascension/Debug/Fix Weapon and Enemy Colliders")]
    static void ShowWindow()
    {
        FixAllColliders();
    }

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
                    Debug.Log($"✅ CircleCollider2D (trigger) añadido a {prefab.name}");
                }
                else
                {
                    BoxCollider2D boxCol = instance.AddComponent<BoxCollider2D>();
                    boxCol.isTrigger = true;
                    boxCol.size = new Vector2(1f, 0.3f);
                    Debug.Log($"✅ BoxCollider2D (trigger) añadido a {prefab.name}");
                }
                fixedCount++;
            }
            else
            {
                // Asegurar que está como Trigger
                if (!col.isTrigger)
                {
                    col.isTrigger = true;
                    Debug.Log($"✅ {prefab.name}: Collider configurado como Trigger");
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
                Debug.Log($"✅ {prefab.name}: Tag configurado como 'Enemy'");
                fixedCount++;
            }

            // Verificar collider NO es trigger
            Collider2D col = instance.GetComponent<Collider2D>();
            if (col != null && col.isTrigger)
            {
                col.isTrigger = false;
                Debug.Log($"✅ {prefab.name}: Collider configurado como NO-trigger (sólido)");
                fixedCount++;
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }

        AssetDatabase.SaveAssets();

        string message = $"✅ {fixedCount} configuraciones corregidas:\n\n" +
                        "ARMAS:\n" +
                        "- Colliders configurados como TRIGGER\n" +
                        "- Pueden detectar enemigos con OnTriggerEnter2D\n\n" +
                        "ENEMIGOS:\n" +
                        "- Tag 'Enemy' asignado\n" +
                        "- Colliders NO-trigger (sólidos)\n\n" +
                        "Ahora las armas deberían dañar a los enemigos correctamente.";

        Debug.Log(message);

        EditorUtility.DisplayDialog(
            "Colliders Configurados",
            message,
            "OK"
        );
    }
}
