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
    public static void ConfigureProjectSettings()
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║        CONFIGURANDO TAGS, LAYERS Y FÍSICA AUTOMÁTICAMENTE      ║");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝\n");

        AddTags();
        AddLayers();
        SetGravity();

        Debug.Log("\n╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║              CONFIGURACIÓN COMPLETADA                          ║");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
        Debug.Log("\n✅ Tags, Layers y Gravedad configurados correctamente");
        Debug.Log("✅ Ahora ejecuta el juego y todo debería funcionar");
    }

    static void AddTags()
    {
        Debug.Log("\n【 CONFIGURANDO TAGS 】");
        
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
                    Debug.Log($"  ℹ️  Tag '{tag}' ya existe");
                    break;
                }
            }

            // Añadir si no existe
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                newTag.stringValue = tag;
                Debug.Log($"  ✅ Tag '{tag}' añadido");
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    static void AddLayers()
    {
        Debug.Log("\n【 CONFIGURANDO LAYERS 】");

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
                Debug.Log($"  ✅ Layer '{layerName}' asignado a Layer {layerIndex}");
            }
            else if (currentLayerName == layerName)
            {
                Debug.Log($"  ℹ️  Layer '{layerName}' ya existe en Layer {layerIndex}");
            }
            else
            {
                Debug.LogWarning($"  ⚠️  Layer {layerIndex} ya está ocupado por '{currentLayerName}', no se pudo asignar '{layerName}'");
                Debug.LogWarning($"     Asígnalo manualmente a otro User Layer disponible");
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    static void SetGravity()
    {
        Debug.Log("\n【 CONFIGURANDO GRAVEDAD 】");

        // Acceder a Physics2D settings
        var physics2DSettings = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/Physics2DSettings.asset");
        if (physics2DSettings == null)
        {
            Debug.LogWarning("  ⚠️  No se pudo acceder a Physics2DSettings");
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
                Debug.Log($"  ✅ Gravedad cambiada de {currentGravity} a (0, 0)");
            }
            else
            {
                Debug.Log($"  ℹ️  Gravedad ya está en (0, 0)");
            }
        }
        else
        {
            Debug.LogWarning("  ⚠️  No se encontró la propiedad m_Gravity");
        }
    }

    // Método adicional para configurar la matriz de colisiones (opcional)
    [MenuItem("Tools/Ascension/Configurar Matriz de Colisiones")]
    public static void ConfigureCollisionMatrix()
    {
        Debug.Log("\n【 CONFIGURANDO MATRIZ DE COLISIONES 】");

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
            Debug.Log("  ✅ Floor: No colisiona con nada");
        }

        // Player colisiona con Wall y Enemy
        if (playerLayer != -1 && wallLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, wallLayer, false);
            Debug.Log("  ✅ Player ↔ Wall: Colisiona");
        }
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            Debug.Log("  ✅ Player ↔ Enemy: Colisiona");
        }

        // Enemy colisiona con Wall
        if (enemyLayer != -1 && wallLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(enemyLayer, wallLayer, false);
            Debug.Log("  ✅ Enemy ↔ Wall: Colisiona");
        }

        // Entities colisiona con Wall
        if (entitiesLayer != -1 && wallLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(entitiesLayer, wallLayer, false);
            Debug.Log("  ✅ Entities ↔ Wall: Colisiona");
        }

        Debug.Log("\n  ⚠️  NOTA: Esta configuración solo afecta al Play Mode actual");
        Debug.Log("  Para hacer cambios permanentes, ve a Edit → Project Settings → Physics 2D");
    }
#endif
}
