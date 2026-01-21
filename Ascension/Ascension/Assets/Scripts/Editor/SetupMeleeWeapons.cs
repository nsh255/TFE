using UnityEditor;
using UnityEngine;

/// <summary>
/// Configura los prefabs de armas cuerpo a cuerpo para que funcionen correctamente
/// </summary>
public class SetupMeleeWeapons : EditorWindow
{
    [MenuItem("Ascension/Debug/Setup Melee Weapons")]
    static void ShowWindow()
    {
        SetupAllMeleeWeapons();
    }

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
                Debug.LogWarning($"{prefab.name} no tiene componente MeleeWeapon, saltando...");
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
                Debug.Log($"✅ {prefab.name}: Hitbox creada");
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
                Debug.Log($"✅ {prefab.name}: BoxCollider2D añadido a Hitbox");
            }

            hitboxCollider.isTrigger = true;
            hitboxCollider.size = new Vector2(1.2f, 0.8f); // Tamaño razonable para espada
            hitboxCollider.enabled = false; // Desactivado por defecto

            // Añadir WeaponHitbox script
            WeaponHitbox weaponHitbox = hitboxObj.GetComponent<WeaponHitbox>();
            if (weaponHitbox == null)
            {
                weaponHitbox = hitboxObj.AddComponent<WeaponHitbox>();
                Debug.Log($"✅ {prefab.name}: WeaponHitbox script añadido");
            }

            // Asignar referencia en MeleeWeapon
            meleeWeapon.hitbox = hitboxCollider;

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);

            Debug.Log($"✅ {prefab.name} configurado correctamente");
            fixedCount++;
        }

        AssetDatabase.SaveAssets();

        string message = $"✅ {fixedCount} armas configuradas:\n\n" +
                        "- Hitbox creada como hijo\n" +
                        "- BoxCollider2D (trigger) configurado\n" +
                        "- WeaponHitbox script añadido\n" +
                        "- Referencia asignada en MeleeWeapon\n\n" +
                        "Ahora las armas cuerpo a cuerpo deberían funcionar.";

        Debug.Log(message);

        EditorUtility.DisplayDialog(
            "Armas Configuradas",
            message,
            "OK"
        );
    }
}
