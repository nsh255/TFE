using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ajusta automáticamente la importación de sprites para carpetas VFX y Tiles.
/// - PPU = 16
/// - Filtro = Point (sin blur)
/// - Compresión = None
/// - Tipo = Sprite (no modifica el modo de Sprite: Single/Multiple)
///
/// Busca en rutas comunes:
///   - Assets/VFX
///   - Assets/Sprites/VFX
///   - Assets/Tiles
/// </summary>
public static class FixVFXAndTilesSprites
{
    // Cambia este valor si tu proyecto usa otro PPU
    private const float TargetPPU = 16f;

    // Menú rápido
    [MenuItem("Ascension/Pixel Art/Fix Sprites in VFX & Tiles")] 
    /// <summary>
    /// Normaliza sprites en carpetas de VFX y tiles predefinidas.
    /// </summary>
    public static void FixNow()
    {
        var candidateFolders = new List<string>
        {
            "Assets/VFX",
            "Assets/Sprites/VFX",
            "Assets/Tiles",
            // Variantes típicas donde pueden estar los tiles
            "Assets/Sprites/Tiles",
            "Assets/Tilesets",
            "Assets/Tilemaps",
            "Assets/Art/Tiles",
            "Assets/Graphics/Tiles"
        };

        // Filtrar solo las carpetas que existen en el proyecto
        var existingFolders = new List<string>();
        foreach (var f in candidateFolders)
        {
            if (AssetDatabase.IsValidFolder(f)) existingFolders.Add(f);
        }

        if (existingFolders.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Fix Sprites",
                "No se encontraron carpetas VFX o Tiles (Assets/VFX, Assets/Sprites/VFX, Assets/Tiles).",
                "OK");
            return;
        }

        FixSpritesInFolders(existingFolders);
    }

    // Opción adicional: permitir elegir una carpeta manualmente
    [MenuItem("Ascension/Pixel Art/Fix Sprites (Pick Folder…)")]
    /// <summary>
    /// Normaliza sprites en una carpeta seleccionada manualmente.
    /// </summary>
    public static void FixFromSelectedFolder()
    {
        string abs = EditorUtility.OpenFolderPanel(
            "Selecciona una carpeta dentro de Assets",
            Application.dataPath,
            string.Empty);

        if (string.IsNullOrEmpty(abs)) return;

        string normAbs = abs.Replace("\\", "/");
        string normAssets = Application.dataPath.Replace("\\", "/");

        if (!normAbs.StartsWith(normAssets))
        {
            EditorUtility.DisplayDialog(
                "Carpeta inválida",
                "Debes seleccionar una carpeta que esté dentro de 'Assets/'.",
                "OK");
            return;
        }

        string rel = "Assets" + normAbs.Substring(normAssets.Length);
        if (!AssetDatabase.IsValidFolder(rel))
        {
            EditorUtility.DisplayDialog(
                "No es una carpeta de proyecto",
                "La ruta seleccionada no es una carpeta válida dentro del proyecto de Unity.",
                "OK");
            return;
        }

        FixSpritesInFolders(new List<string> { rel });
    }

    /// <summary>
    /// Aplica la normalización de importación a las carpetas indicadas.
    /// </summary>
    private static void FixSpritesInFolders(List<string> folders)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", folders.ToArray());
        int processed = 0;
        int changed = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar(
                    "Arreglando Sprites (VFX & Tiles)",
                    $"Procesando: {System.IO.Path.GetFileName(path)}",
                    (float)(i + 1) / guids.Length);

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                bool didChange = false;

                // Tipo Sprite
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    didChange = true;
                }

                // Modo de Sprite: SINGLE (solicitado)
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    didChange = true;
                }

                // PPU
                if (!Mathf.Approximately(importer.spritePixelsPerUnit, TargetPPU))
                {
                    importer.spritePixelsPerUnit = TargetPPU;
                    didChange = true;
                }

                // Filtro Point
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    didChange = true;
                }

                // Compresión None
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    didChange = true;
                }

                // Transparencia alfa
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    didChange = true;
                }

                // Evitar artefactos por bleeding
                if (importer.wrapMode != TextureWrapMode.Clamp)
                {
                    importer.wrapMode = TextureWrapMode.Clamp;
                    didChange = true;
                }

                if (didChange)
                {
                    changed++;
                    importer.SaveAndReimport();
                }

                processed++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        EditorUtility.DisplayDialog(
            "Fix Sprites — VFX & Tiles",
            $"Texturas procesadas: {processed}\nModificadas: {changed}",
            "OK");
    }
}
