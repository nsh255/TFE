using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine.Tilemaps;

/// <summary>
/// Editor script que genera automáticamente VariantTiles y TileEffects
/// leyendo los sprites desde Assets/Sprites/Tiles/.
/// 
/// Agrupación por prefijo:
/// - floor1, floor2, floor3 → FloorVariantTile (sin efecto)
/// - ice1, ice2, ice3 → IceVariantTile (velocidad +50%)
/// - mud1, mud2, mud3 → MudVariantTile (velocidad -50%)
/// - heal → HealVariantTile (cura 1 HP cada 0.5s)
/// - powerUp → PowerUpVariantTile (daño +1)
/// - wall → WallVariantTile (sin efecto, para muros)
/// - stairs → StairsVariantTile (sin efecto, interacción manual)
/// 
/// Uso: Unity Editor → Tools → Generate Variant Tiles from Sprites
/// </summary>
public class GenerateVariantTilesFromSprites : EditorWindow
{
    private string spritesPath = "Assets/Sprites/Tiles";
    private string tilesOutputPath = "Assets/Tiles";
    private string effectsOutputPath = "Assets/Data/TileEffects";
    
    [MenuItem("Tools/Generate Variant Tiles from Sprites")]
    /// <summary>
    /// Abre la ventana de generación de tiles.
    /// </summary>
    public static void ShowWindow()
    {
        GetWindow<GenerateVariantTilesFromSprites>("Generate Variant Tiles");
    }

    /// <summary>
    /// Dibuja la interfaz de configuración del generador.
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Generador de VariantTiles", EditorStyles.boldLabel);
        
        spritesPath = EditorGUILayout.TextField("Sprites Path", spritesPath);
        tilesOutputPath = EditorGUILayout.TextField("Tiles Output", tilesOutputPath);
        effectsOutputPath = EditorGUILayout.TextField("Effects Output", effectsOutputPath);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Tiles & Effects", GUILayout.Height(40)))
        {
            GenerateTiles();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Este script:\n" +
            "uno. Lee todos los PNG en Assets/Sprites/Tiles/\n" +
            "dos. Agrupa sprites por prefijo (floor1,floor2,floor3 → floor)\n" +
            "tres. Crea VariantTile por cada grupo\n" +
            "cuatro. Crea TileEffect según el nombre del grupo\n" +
            "cinco. Asigna el TileEffect al VariantTile\n\n" +
            "Efectos:\n" +
            "- floor: Sin efecto\n" +
            "- ice: Velocidad +50%\n" +
            "- mud: Velocidad -50%\n" +
            "- heal: Cura un HP cada 0.5 s\n" +
            "- powerUp: más uno de daño permanente\n" +
            "- wall: Sin efecto (collider)\n" +
            "- stairs: Sin efecto (interacción)",
            MessageType.Info
        );
    }

    /// <summary>
    /// Genera VariantTiles y TileEffects a partir de sprites.
    /// </summary>
    void GenerateTiles()
    {
        if (!Directory.Exists(spritesPath))
        {
            Debug.LogError($"[GenerateVariantTiles] No existe la carpeta {spritesPath}");
            return;
        }

        // Crear carpetas de salida si no existen
        if (!Directory.Exists(tilesOutputPath))
            Directory.CreateDirectory(tilesOutputPath);
        if (!Directory.Exists(effectsOutputPath))
            Directory.CreateDirectory(effectsOutputPath);

        // Cargar todos los sprites
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { spritesPath });
        List<Sprite> allSprites = new List<Sprite>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                allSprites.Add(sprite);
        }

        if (allSprites.Count == 0)
        {
            Debug.LogWarning("[GenerateVariantTiles] No se encontraron sprites en " + spritesPath);
            return;
        }

        // Agrupar sprites por prefijo (floor1, floor2 → floor)
        Dictionary<string, List<Sprite>> groupedSprites = GroupSpritesByPrefix(allSprites);

        // Crear VariantTile + TileEffect para cada grupo
        foreach (var group in groupedSprites)
        {
            string baseName = group.Key;
            List<Sprite> sprites = group.Value;

            CreateVariantTileAndEffect(baseName, sprites);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Agrupa sprites por prefijo, eliminando sufijos numéricos.
    /// </summary>
    Dictionary<string, List<Sprite>> GroupSpritesByPrefix(List<Sprite> sprites)
    {
        Dictionary<string, List<Sprite>> groups = new Dictionary<string, List<Sprite>>();

        foreach (Sprite sprite in sprites)
        {
            string spriteName = sprite.name.ToLower();
            
            // Extraer prefijo (quitar números al final)
            string prefix = System.Text.RegularExpressions.Regex.Replace(spriteName, @"\d+$", "");
            
            // Limpiar guiones bajos o espacios
            prefix = prefix.Trim('_', ' ', '-');

            if (string.IsNullOrEmpty(prefix))
                prefix = spriteName; // Si no hay prefijo, usar nombre completo

            if (!groups.ContainsKey(prefix))
                groups[prefix] = new List<Sprite>();

            groups[prefix].Add(sprite);
        }

        return groups;
    }

    /// <summary>
    /// Crea un VariantTile y su TileEffect asociado.
    /// </summary>
    void CreateVariantTileAndEffect(string baseName, List<Sprite> sprites)
    {
        string capitalizedName = char.ToUpper(baseName[0]) + baseName.Substring(1);
        string variantTileName = $"{capitalizedName}VariantTile";
        string tileEffectName = $"{capitalizedName}TileEffect";

        // Crear TileEffect
        TileEffect effect = CreateTileEffect(baseName, tileEffectName);

        // Crear VariantTile
        VariantTile variantTile = ScriptableObject.CreateInstance<VariantTile>();
        variantTile.variants = sprites.ToArray();
        variantTile.deterministic = true;
        variantTile.seed = baseName.GetHashCode();
        variantTile.tileEffect = effect;

        // Guardar VariantTile
        string tilePath = $"{tilesOutputPath}/{variantTileName}.asset";
        AssetDatabase.CreateAsset(variantTile, tilePath);
    }

    /// <summary>
    /// Crea un TileEffect según el nombre base del grupo.
    /// </summary>
    TileEffect CreateTileEffect(string baseName, string effectName)
    {
        TileEffect effect = ScriptableObject.CreateInstance<TileEffect>();
        effect.tileName = baseName;

        // Configurar efectos según el nombre
        switch (baseName.ToLower())
        {
            case "floor":
                effect.effectType = TileEffectType.None;
                break;

            case "heal":
                effect.effectType = TileEffectType.Heal;
                effect.healthChange = 1;
                effect.tickRate = 0.5f;
                effect.continuous = true;
                effect.tintColor = new Color(0.5f, 1f, 0.5f); // Verde claro
                break;

            case "ice":
                effect.effectType = TileEffectType.SpeedUp;
                effect.speedMultiplier = 1.5f;
                effect.continuous = true;
                effect.tintColor = new Color(0.7f, 0.9f, 1f); // Azul claro
                break;

            case "mud":
                effect.effectType = TileEffectType.SpeedDown;
                effect.speedMultiplier = 0.5f;
                effect.effectDuration = 0.5f; // Efecto persiste 0.5s después de salir
                effect.continuous = true;
                effect.tintColor = new Color(0.6f, 0.5f, 0.3f); // Marrón
                break;

            case "powerup":
                effect.effectType = TileEffectType.None; // Efecto custom (daño +1)
                effect.tileName = "powerup"; // Identificador especial
                effect.tintColor = new Color(1f, 0.8f, 0.2f); // Dorado
                break;

            case "wall":
                effect.effectType = TileEffectType.None;
                break;

            case "stairs":
                effect.effectType = TileEffectType.None;
                effect.tileName = "stairs"; // Identificador especial para interacción
                effect.tintColor = new Color(0.8f, 0.8f, 1f); // Azul grisáceo
                break;

            default:
                effect.effectType = TileEffectType.None;
                break;
        }

        // Guardar TileEffect
        string effectPath = $"{effectsOutputPath}/{effectName}.asset";
        AssetDatabase.CreateAsset(effect, effectPath);

        return effect;
    }
}
