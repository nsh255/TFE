using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script que configura automáticamente Tags, Layers y Physics2D
/// IMPORTANTE: Debe estar en una carpeta "Editor" para funcionar
/// </summary>
public class ProjectSettingsAutoFix
{
#if UNITY_EDITOR
    [MenuItem("Tools/Ascension/Configurar Tags, Layers y Física")]
    /// <summary>
    /// Configura tags, layers y gravedad para el proyecto.
    /// </summary>
    public static void ConfigureProjectSettings()
    {
        AddTags();
        AddLayers();
        SetGravity();

        EditorUtility.DisplayDialog(
            "Configuración completada",
            "Tags, layers y gravedad configurados.\n\n" +
            "Se recomienda validar la configuración en Project Settings.",
            "OK");
    }

    /// <summary>
    /// Añade tags necesarios al proyecto.
    /// </summary>
    static void AddTags()
    {
        // Tags necesarios para el proyecto
        string[] tagsToAdd = { "Wall" };

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        foreach (string tag in tagsToAdd)
        {
            // Verificar si el tag ya existe
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tag))
                {
                    found = true;
                    break;
                }
            }

            // Añadir si no existe
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                newTag.stringValue = tag;
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    static void AddLayers()
    {
        // Layers necesarios: nombre → número de layer deseado
        // Layers 0-7 están reservados por Unity, usamos 8-15
        var layersToAdd = new System.Collections.Generic.Dictionary<string, int>
        {
            { "Player", 8 },
            { "Enemy", 9 },
            { "Wall", 10 },
            { "Floor", 11 },
            { "Entities", 12 }
        };

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        foreach (var layer in layersToAdd)
        {
            string layerName = layer.Key;
            int layerIndex = layer.Value;

            // Verificar si el layer ya existe en ese índice
            SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(layerIndex);
            string currentLayerName = layerSP.stringValue;

            if (string.IsNullOrEmpty(currentLayerName))
            {
                layerSP.stringValue = layerName;
            }
            else
            {
                Debug.LogWarning($"Layer {layerIndex} ya está ocupado por '{currentLayerName}', no se pudo asignar '{layerName}'.");
                Debug.LogWarning("Se recomienda asignar el layer manualmente en Project Settings.");
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    static void SetGravity()
    {
        // Acceder a Physics2D settings
        var physics2DSettings = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/Physics2DSettings.asset");
        if (physics2DSettings == null)
        {
            Debug.LogWarning("No se pudo acceder a Physics2DSettings.");
            return;
        }

        SerializedObject serializedSettings = new SerializedObject(physics2DSettings);
        SerializedProperty gravityProp = serializedSettings.FindProperty("m_Gravity");

        if (gravityProp != null)
        {
            Vector2 currentGravity = gravityProp.vector2Value;
            
            if (currentGravity.y != 0)
            {
                gravityProp.vector2Value = new Vector2(0, 0);
                serializedSettings.ApplyModifiedProperties();
            }
        }
        else
        {
            Debug.LogWarning("No se encontró la propiedad m_Gravity.");
        }
    }

    // Método adicional para configurar la matriz de colisiones (opcional)
    [MenuItem("Tools/Ascension/Configurar Matriz de Colisiones")]
    /// <summary>
    /// Configura la matriz de colisiones para el modo Play actual.
    /// </summary>
    public static void ConfigureCollisionMatrix()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int wallLayer = LayerMask.NameToLayer("Wall");
        int floorLayer = LayerMask.NameToLayer("Floor");
        int entitiesLayer = LayerMask.NameToLayer("Entities");

        // Configurar colisiones (true = NO colisiona, false = SÍ colisiona)
        
        // Floor no colisiona con nada (solo visual)
        if (floorLayer != -1)
        {
            for (int i = 0; i < 32; i++)
            {
                Physics2D.IgnoreLayerCollision(floorLayer, i, true);
            }
        }

        // Player colisiona con Wall y Enemy
        if (playerLayer != -1 && wallLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, wallLayer, false);
        }
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }

        // Enemy colisiona con Wall
        if (enemyLayer != -1 && wallLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(enemyLayer, wallLayer, false);
        }

        // Entities colisiona con Wall
        if (entitiesLayer != -1 && wallLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(entitiesLayer, wallLayer, false);
        }

        EditorUtility.DisplayDialog(
            "Matriz de colisiones",
            "La configuración se ha aplicado para el modo Play actual.\n\n" +
            "Para cambios permanentes, revisar Project Settings > Physics 2D.",
            "OK");
    }
#endif
}
