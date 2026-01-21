using UnityEditor;
using UnityEngine;

/// <summary>
/// Corrige el PPU (Pixels Per Unit) de los sprites del jugador y enemigos a 16.
/// </summary>
public class FixSpritePPU : EditorWindow
{
    [MenuItem("Ascension/Debug/Fix Sprite PPU to 16")]
    static void ShowWindow()
    {
        FixAllSprites();
    }

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
                
                Debug.Log($"✅ {path}: PPU {oldPPU} → 16");
                fixedCount++;
            }
            else
            {
                Debug.LogWarning($"⚠️ No se encontró: {path}");
            }
        }

        AssetDatabase.Refresh();

        string message = $"✅ {fixedCount} sprites actualizados a PPU=16\n\n" +
                        "Incluye: Jugador, Enemigos (Slimes), Armas, Proyectiles\n\n" +
                        "AHORA todo tendrá el tamaño correcto.\n\n" +
                        "PRUEBA EL JUEGO DE NUEVO.";

        Debug.Log(message);

        EditorUtility.DisplayDialog(
            "Sprites Corregidos",
            message,
            "Probar Juego"
        );
    }
}
