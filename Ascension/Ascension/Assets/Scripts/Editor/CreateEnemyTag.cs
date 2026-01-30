using UnityEditor;
using UnityEngine;

/// <summary>
/// Crea el tag 'Enemy' en el proyecto y lo asigna a todos los prefabs de enemigos
/// </summary>
public class CreateEnemyTag : EditorWindow
{
    [MenuItem("Ascension/Debug/Create and Assign Enemy Tag")]
    /// <summary>
    /// Ejecuta la creación del tag Enemy y su asignación a prefabs.
    /// </summary>
    static void ShowWindow()
    {
        CreateTagAndAssign();
    }

    /// <summary>
    /// Crea el tag Enemy si no existe y lo asigna a prefabs de enemigos.
    /// </summary>
    private static void CreateTagAndAssign()
    {
        // 1. Crear el tag 'Enemy' si no existe
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool tagExists = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals("Enemy"))
            {
                tagExists = true;
                break;
            }
        }

        if (!tagExists)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
            newTag.stringValue = "Enemy";
            tagManager.ApplyModifiedProperties();
        }

        // 2. Asignar tag a todos los prefabs de enemigos
        string[] enemyPaths = {
            "Assets/Prefabs/Enemies/SlimeRed.prefab",
            "Assets/Prefabs/Enemies/SlimeBlue.prefab",
            "Assets/Prefabs/Enemies/SlimeGreen.prefab",
            "Assets/Prefabs/Enemies/ChaserEnemy.prefab",
            "Assets/Prefabs/Enemies/JumperEnemy.prefab",
            "Assets/Prefabs/Enemies/ShooterEnemy.prefab",
            "Assets/Prefabs/Enemies/BossEnemy.prefab"
        };

        int fixedCount = 0;

        foreach (var path in enemyPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            
            if (instance.tag != "Enemy")
            {
                instance.tag = "Enemy";
                fixedCount++;
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }

        AssetDatabase.SaveAssets();

        string message = $"Tag 'Enemy' configurado.\n\n" +
                "- Tag creado en TagManager\n" +
                $"- {fixedCount} prefabs actualizados.";

        EditorUtility.DisplayDialog(
            "Tag Enemy Configurado",
            message,
            "OK"
        );
    }
}
