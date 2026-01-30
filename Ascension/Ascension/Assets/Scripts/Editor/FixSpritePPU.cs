using UnityEditor;
using UnityEngine;

/// <summary>
/// Corrige el PPU (Pixels Per Unit) de los sprites del jugador y enemigos a 16.
/// </summary>
public class FixSpritePPU : EditorWindow
{
    [MenuItem("Ascension/Debug/Fix Sprite PPU to 16")]
    /// <summary>
    /// Ejecuta la normalización de PPU de los sprites.
    /// </summary>
    static void ShowWindow()
    {
        FixAllSprites();
    }

    /// <summary>
    /// Normaliza el PPU y la importación de los sprites del proyecto.
    /// </summary>
    private static void FixAllSprites()
    {
        // Buscar TODOS los sprites en la carpeta Sprites
        string[] allSprites = System.IO.Directory.GetFiles("Assets/Sprites", "*.png", System.IO.SearchOption.AllDirectories);
        
        // Convertir paths de Windows a Unity paths
        for (int i = 0; i < allSprites.Length; i++)
        {
            allSprites[i] = allSprites[i].Replace("\\", "/");
        }

        int fixedCount = 0;
        foreach (var path in allSprites)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                float oldPPU = importer.spritePixelsPerUnit;
                importer.spritePixelsPerUnit = 16f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();

                fixedCount++;
            }
            else
            {
            }
        }

        AssetDatabase.Refresh();

        string message = $"Se han actualizado {fixedCount} sprites a PPU = 16.\n\n" +
                "Incluye: jugador, enemigos, armas y proyectiles.\n\n" +
                "Se recomienda validar el resultado en ejecución.";

        EditorUtility.DisplayDialog(
            "Sprites Corregidos",
            message,
            "Probar Juego"
        );
    }
}
