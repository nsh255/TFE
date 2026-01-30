using UnityEditor;
using UnityEngine;

/// <summary>
/// Fuerza la actualización del prefab Player con el weaponOffset correcto.
/// </summary>
public class FixPlayerPrefab : EditorWindow
{
    [MenuItem("Ascension/Debug/Fix Player Prefab Weapon Offset")]
    /// <summary>
    /// Ejecuta la corrección del weaponOffset en el prefab de jugador.
    /// </summary>
    static void ShowWindow()
    {
        FixPrefab();
    }

    /// <summary>
    /// Fuerza weaponOffset a cero para que se inicialice en Awake.
    /// </summary>
    private static void FixPrefab()
    {
        string prefabPath = "Assets/Prefabs/Player.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontró Player.prefab", "OK");
            return;
        }

        PlayerController controller = prefab.GetComponent<PlayerController>();
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Error", "PlayerController no encontrado en el prefab", "OK");
            return;
        }

        // Forzar weaponOffset a (0,0,0) para que el código en Awake() lo inicialice
        controller.weaponOffset = Vector3.zero;
        
        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog(
            "Prefab Actualizado",
            "El prefab Player ha sido configurado correctamente.\n\n" +
            "Se recomienda reiniciar Unity o validar el cambio en una nueva ejecución.",
            "OK"
        );
    }
}
