using UnityEditor;
using UnityEngine;

/// <summary>
/// Configura los prefabs de armas cuerpo a cuerpo para que funcionen correctamente
/// </summary>
public class SetupMeleeWeapons : EditorWindow
{
    [MenuItem("Ascension/Debug/Setup Melee Weapons")]
    /// <summary>
    /// Ejecuta la configuración de prefabs de armas cuerpo a cuerpo.
    /// </summary>
    static void ShowWindow()
    {
        SetupAllMeleeWeapons();
    }

    /// <summary>
    /// Recorre los prefabs configurados y asegura hitbox y colisionadores.
    /// </summary>
    private static void SetupAllMeleeWeapons()
    {
        string[] weaponPaths = {
            "Assets/Prefabs/Weapons/MeleeWeaponPrefab.prefab",
            "Assets/Prefabs/Weapons/WeaponPrefab.prefab"
        };

        int fixedCount = 0;

        foreach (var path in weaponPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            MeleeWeapon meleeWeapon = instance.GetComponent<MeleeWeapon>();

            if (meleeWeapon == null)
            {
                Debug.LogWarning($"{prefab.name} no tiene componente MeleeWeapon, se omite su configuración.");
                PrefabUtility.UnloadPrefabContents(instance);
                continue;
            }

            // Buscar o crear hijo "Hitbox"
            Transform hitboxTransform = instance.transform.Find("Hitbox");
            GameObject hitboxObj;

            if (hitboxTransform == null)
            {
                // Crear hijo para la hitbox
                hitboxObj = new GameObject("Hitbox");
                hitboxObj.transform.SetParent(instance.transform);
                hitboxObj.transform.localPosition = new Vector3(0.6f, 0f, 0f); // Adelante del arma
                hitboxObj.transform.localRotation = Quaternion.identity;
                hitboxObj.transform.localScale = Vector3.one;
            }
            else
            {
                hitboxObj = hitboxTransform.gameObject;
            }

            // Añadir/configurar BoxCollider2D
            BoxCollider2D hitboxCollider = hitboxObj.GetComponent<BoxCollider2D>();
            if (hitboxCollider == null)
            {
                hitboxCollider = hitboxObj.AddComponent<BoxCollider2D>();
            }

            hitboxCollider.isTrigger = true;
            hitboxCollider.size = new Vector2(1.2f, 0.8f); // Tamaño razonable para espada
            hitboxCollider.enabled = false; // Desactivado por defecto

            // Añadir WeaponHitbox script
            WeaponHitbox weaponHitbox = hitboxObj.GetComponent<WeaponHitbox>();
            if (weaponHitbox == null)
            {
                weaponHitbox = hitboxObj.AddComponent<WeaponHitbox>();
            }

            // Asignar referencia en MeleeWeapon
            meleeWeapon.hitbox = hitboxCollider;

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
            fixedCount++;
        }

        AssetDatabase.SaveAssets();

        string message = $"{fixedCount} armas configuradas.\n\n" +
                "- Hitbox creada como hijo\n" +
                "- BoxCollider2D (trigger) configurado\n" +
                "- WeaponHitbox añadido\n" +
                "- Referencia asignada en MeleeWeapon.";

        EditorUtility.DisplayDialog(
            "Armas Configuradas",
            message,
            "OK"
        );
    }
}
