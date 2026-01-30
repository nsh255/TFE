using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Herramienta para pintar tiles con efectos (slow, daño, buff).
/// Asigna automáticamente TileEffect ScriptableObjects a VariantTiles.
/// </summary>
public class TileEffectPainter : EditorWindow
{
    private Tilemap targetTilemap;
    private TileEffect selectedEffect;
    private VariantTile selectedTile;
    private bool showEffectList = true;

    // Lista de efectos disponibles
    private TileEffect[] availableEffects;

    [MenuItem("Tools/Ascension/Tile Effect Painter")]
    /// <summary>
    /// Abre la herramienta de asignación de efectos a tiles.
    /// </summary>
    public static void ShowWindow()
    {
        GetWindow<TileEffectPainter>("Tile Effect Painter");
    }

    /// <summary>
    /// Carga los TileEffect disponibles al habilitar la ventana.
    /// </summary>
    void OnEnable()
    {
        LoadAvailableEffects();
    }

    /// <summary>
    /// Dibuja la interfaz de edición en el editor.
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Tile Effect Painter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Selección de Tilemap
        targetTilemap = (Tilemap)EditorGUILayout.ObjectField("Target Tilemap", targetTilemap, typeof(Tilemap), true);

        EditorGUILayout.Space();

        // Botón de refresh
        if (GUILayout.Button("Refresh Effects List", GUILayout.Height(25)))
        {
            LoadAvailableEffects();
        }

        EditorGUILayout.Space();

        // Lista de efectos disponibles
        showEffectList = EditorGUILayout.Foldout(showEffectList, $"Efectos Disponibles ({(availableEffects != null ? availableEffects.Length : 0)})");
        
        if (showEffectList && availableEffects != null)
        {
            EditorGUI.indentLevel++;
            foreach (TileEffect effect in availableEffects)
            {
                if (effect == null) continue;

                EditorGUILayout.BeginHorizontal("box");
                
                // Color indicator
                EditorGUILayout.ColorField(GUIContent.none, GetEffectColor(effect.effectType), false, false, false, GUILayout.Width(20));
                
                // Info
                EditorGUILayout.LabelField(effect.effectType.ToString(), GUILayout.Width(100));
                EditorGUILayout.LabelField($"Speed: {effect.speedMultiplier}x", GUILayout.Width(100));

                // Botón de selección
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    selectedEffect = effect;
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Efecto seleccionado
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Efecto Seleccionado", EditorStyles.boldLabel);
        selectedEffect = (TileEffect)EditorGUILayout.ObjectField("TileEffect", selectedEffect, typeof(TileEffect), false);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Tile seleccionado
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Tile a Modificar", EditorStyles.boldLabel);
        selectedTile = (VariantTile)EditorGUILayout.ObjectField("VariantTile", selectedTile, typeof(VariantTile), false);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Botón de aplicar
        GUI.enabled = selectedEffect != null && selectedTile != null;
        if (GUILayout.Button("Aplicar Efecto al Tile", GUILayout.Height(40)))
        {
            ApplyEffectToTile();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        // Información
        EditorGUILayout.HelpBox(
            "Pasos:\n" +
            "uno. Selecciona un TileEffect de la lista o asígnalo manualmente\n" +
            "dos. Selecciona un VariantTile desde el proyecto\n" +
            "tres. Haz clic en 'Aplicar Efecto al Tile'\n\n" +
            "El tile automáticamente tendrá el efecto asignado",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // Botón de crear nuevo efecto
        if (GUILayout.Button("Abrir TileEffect Creator", GUILayout.Height(25)))
        {
            // Recargar efectos disponibles
            LoadAvailableEffects();
            EditorUtility.DisplayDialog("Info", "Para crear nuevos TileEffects, usa el menú: Tools > Create TileEffects", "OK");
        }
    }

    /// <summary>
    /// Carga todos los TileEffect disponibles en el proyecto.
    /// </summary>
    private void LoadAvailableEffects()
    {
        string[] guids = AssetDatabase.FindAssets("t:TileEffect");
        availableEffects = new TileEffect[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            availableEffects[i] = AssetDatabase.LoadAssetAtPath<TileEffect>(path);
        }

    }

    /// <summary>
    /// Aplica el TileEffect seleccionado al VariantTile seleccionado.
    /// </summary>
    private void ApplyEffectToTile()
    {
        if (selectedEffect == null || selectedTile == null)
        {
            EditorUtility.DisplayDialog("Error", "Selecciona un efecto y un tile", "OK");
            return;
        }

        // Usar SerializedObject para modificar el tile
        SerializedObject serializedTile = new SerializedObject(selectedTile);
        SerializedProperty effectProp = serializedTile.FindProperty("tileEffect");
        
        if (effectProp != null)
        {
            effectProp.objectReferenceValue = selectedEffect;
            serializedTile.ApplyModifiedProperties();

            EditorUtility.SetDirty(selectedTile);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Proceso completado", $"Efecto aplicado correctamente a {selectedTile.name}.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "No se encontró el campo 'tileEffect' en el VariantTile", "OK");
        }
    }

    /// <summary>
    /// Devuelve un color representativo por tipo de efecto.
    /// </summary>
    private Color GetEffectColor(TileEffectType type)
    {
        return type switch
        {
            TileEffectType.None => Color.white,
            TileEffectType.Heal => Color.green,
            TileEffectType.Damage => Color.red,
            TileEffectType.SpeedUp => Color.cyan,
            TileEffectType.SpeedDown => Color.yellow,
            TileEffectType.Ice => Color.blue,
            TileEffectType.Mud => new Color(0.6f, 0.4f, 0.2f),
            TileEffectType.PowerUp => new Color(1f, 0.5f, 0f),
            _ => Color.gray
        };
    }
}
