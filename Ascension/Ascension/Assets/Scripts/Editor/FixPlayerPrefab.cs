using UnityEditor;
using UnityEngine;

/// <summary>
/// Fuerza la actualización del prefab Player con el weaponOffset correcto.
/// </summary>
public class FixPlayerPrefab : EditorWindow
{
    [MenuItem("Ascension/Debug/Fix Player Prefab Weapon Offset")]
    static void ShowWindow()
    {
        FixPrefab();
    }

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
        
        Debug.Log("✅ Player prefab actualizado. weaponOffset = (0, 0, 0)");
        Debug.Log("   El código en Awake() lo inicializará a (0.5, 0.25, 0)");
        
        EditorUtility.DisplayDialog(
            "Prefab Actualizado",
            "✅ Player prefab configurado correctamente.\n\n" +
            "IMPORTANTE: Cierra Unity y vuelve a abrir para que los cambios surtan efecto.\n\n" +
            "O simplemente prueba el juego desde MainMenu (nueva instancia).",
            "OK"
        );
    }
}
