using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Configuración global para pixel art.
/// Asegura que todos los sprites usen Point filtering (sin blur).
/// </summary>
public static class PixelArtSettings
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializePixelArtSettings()
    {
        // Configurar calidad de texturas para pixel art
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.antiAliasing = 0; // Sin antialiasing para pixel art
        
        Debug.Log("[PixelArt] Configuración aplicada: Filtros desactivados para pixel art nítido");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Tool de Unity Editor para configurar todos los sprites del proyecto
    /// </summary>
    [MenuItem("Tools/Pixel Art/Configure All Sprites (Point Filter + Compress None)")]
    static void ConfigureAllSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
        int configuredCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                bool changed = false;

                // Configurar para pixel art
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }

                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    changed = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (changed)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    configuredCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[PixelArt] {configuredCount} sprites configurados correctamente");
        EditorUtility.DisplayDialog("Pixel Art Setup", 
            $"✅ {configuredCount} sprites configurados.\n\n" +
            "Configuración aplicada:\n" +
            "- Filter Mode: Point (sin blur)\n" +
            "- Compression: None\n" +
            "- Mipmaps: Disabled", 
            "OK");
    }

    [MenuItem("Tools/Pixel Art/Setup Camera for Pixel Art")]
    static void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("No se encontró Main Camera");
            return;
        }

        // Asegurar que sea ortográfica
        mainCam.orthographic = true;

        // Añadir componente PixelPerfectSetup si no existe
        if (mainCam.GetComponent<PixelPerfectSetup>() == null)
        {
            mainCam.gameObject.AddComponent<PixelPerfectSetup>();
            Debug.Log("✅ Componente PixelPerfectSetup añadido a la cámara");
        }

        EditorUtility.DisplayDialog("Camera Setup", 
            "✅ Cámara configurada para pixel art.\n\n" +
            "Se añadió el componente PixelPerfectSetup.\n" +
            "Configura la Reference Resolution en el Inspector.", 
            "OK");
    }

    [MenuItem("Tools/Pixel Art/Setup All Canvas (UI)")]
    static void SetupAllCanvas()
    {
        Canvas[] allCanvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        int configuredCount = 0;

        foreach (Canvas canvas in allCanvas)
        {
            // Solo configurar Canvas de Screen Space
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                // Añadir UIScalerSetup si no existe
                if (canvas.GetComponent<UIScalerSetup>() == null)
                {
                    UIScalerSetup scaler = canvas.gameObject.AddComponent<UIScalerSetup>();
                    scaler.referenceResolution = new Vector2(480, 270);
                    scaler.matchWidthOrHeight = 0f; // Match width para 16:9
                    scaler.pixelPerfect = false;
                    configuredCount++;
                    Debug.Log($"✅ Canvas '{canvas.gameObject.name}' configurado");
                }
            }
        }

        EditorUtility.DisplayDialog("Canvas Setup", 
            $"✅ {configuredCount} Canvas configurados para pixel art.\n\n" +
            "Configuración aplicada:\n" +
            "- Reference Resolution: 480x270\n" +
            "- Scale Mode: Scale With Screen Size\n" +
            "- Match: Width (0.0)\n\n" +
            "Ahora tu UI se verá correcta en fullscreen.", 
            "OK");
    }

    [MenuItem("Tools/Pixel Art/Setup Complete Project (Camera + Canvas + Sprites)")]
    static void SetupCompleteProject()
    {
        // Configurar sprites
        ConfigureAllSprites();
        
        // Configurar cámara
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            if (mainCam.GetComponent<PixelPerfectSetup>() == null)
            {
                mainCam.gameObject.AddComponent<PixelPerfectSetup>();
            }
        }

        // Configurar todos los Canvas
        Canvas[] allCanvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvas)
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                if (canvas.GetComponent<UIScalerSetup>() == null)
                {
                    UIScalerSetup scaler = canvas.gameObject.AddComponent<UIScalerSetup>();
                    scaler.referenceResolution = new Vector2(480, 270);
                    scaler.matchWidthOrHeight = 0f;
                    scaler.pixelPerfect = false;
                }
            }
        }

        EditorUtility.DisplayDialog("Complete Setup", 
            "✅ Proyecto completamente configurado para Pixel Art.\n\n" +
            "✓ Sprites: Point filter\n" +
            "✓ Cámara: PixelPerfectSetup\n" +
            "✓ Canvas: UIScalerSetup (480x270)\n\n" +
            "¡Listo para fullscreen!", 
            "OK");
    }
#endif
}
