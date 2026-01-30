using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Corrige automáticamente los problemas detectados por FullHDSetupValidator:
/// - Añade PixelPerfectSetup a la cámara principal (si falta) y ajusta parámetros:
///   * referenceResolution = 480x270
///   * dynamicResolution = true
///   * pixelScale = 4 (recomendado para 1080p)
///   * Fuerza cámara ortográfica
/// - Ajusta CanvasScaler a:
///   * ScaleWithScreenSize
///   * referenceResolution = 480x270
///   (No modifica matchWidthOrHeight para respetar el diseño actual)
///
/// Solo guarda escenas que hayan sido modificadas.
/// </summary>
public static class FullHDAutoFixer
{
    private static readonly Vector2Int ReferenceRes = new Vector2Int(480, 270);
    private const int RecommendedPixelScale = 4; // 1080p -> 1920/480 = 4, 1080/270 = 4

    [MenuItem("Ascension/Diagnostics/Auto-Fix FullHD Setup")]
    /// <summary>
    /// Aplica correcciones automáticas de configuración para Full HD.
    /// </summary>
    public static void AutoFixFullHD()
    {
        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        if (sceneGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Auto-Fix FullHD Setup", "No se encontraron escenas en Assets/Scenes.", "OK");
            return;
        }

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        var perSceneLogs = new List<string>();
        int totalFixed = 0;

        try
        {
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                EditorUtility.DisplayProgressBar("Auto-Fix FullHD", $"Escena: {System.IO.Path.GetFileNameWithoutExtension(scenePath)}", (float)(i + 1) / sceneGuids.Length);

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                bool modified = false;
                var log = new List<string>();

                // 1) Cámara principal y PixelPerfectSetup
                var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
                Camera mainCam = cameras.FirstOrDefault(c => c.CompareTag("MainCamera")) ?? cameras.FirstOrDefault();
                if (mainCam == null)
                {
                    log.Add("[!] No se encontró ninguna Camera en la escena.");
                }
                else
                {
                    if (!mainCam.orthographic)
                    {
                        mainCam.orthographic = true;
                        modified = true;
                        log.Add("[fix] Cámara principal puesta en modo ortográfico.");
                    }

                    // Añadir/ajustar PixelPerfectSetup
                    var pps = mainCam.GetComponent<PixelPerfectSetup>();
                    if (pps == null)
                    {
                        pps = mainCam.gameObject.AddComponent<PixelPerfectSetup>();
                        modified = true;
                        log.Add("[fix] Añadido PixelPerfectSetup a la cámara principal.");
                    }

                    // Ajustar campos del PixelPerfectSetup
                    // Usamos reflejo mínimo para ser robustos si los campos fueran privados en el futuro
                    try
                    {
                        pps.GetType().GetField("referenceResolution")?.SetValue(pps, ReferenceRes);
                        pps.GetType().GetField("dynamicResolution")?.SetValue(pps, true);
                        pps.GetType().GetField("pixelScale")?.SetValue(pps, RecommendedPixelScale);
                        modified = true;
                        log.Add($"[fix] PixelPerfectSetup: referenceResolution={ReferenceRes}, dynamicResolution=true, pixelScale={RecommendedPixelScale}.");
                    }
                    catch
                    {
                        // Si los campos cambian, al menos dejamos el componente creado.
                        log.Add("[i] No se pudieron ajustar todos los campos de PixelPerfectSetup (revisa el script).");
                    }
                }

                // 2) CanvasScaler
                var scalers = Object.FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);
                foreach (var cs in scalers)
                {
                    bool changed = false;
                    if (cs.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                    {
                        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        changed = true;
                    }
                    if (cs.referenceResolution != (Vector2)ReferenceRes)
                    {
                        cs.referenceResolution = ReferenceRes;
                        changed = true;
                    }

                    if (changed)
                    {
                        modified = true;
                        log.Add($"[fix] CanvasScaler '{cs.name}': ScaleWithScreenSize + ref {ReferenceRes}.");
                    }
                }

                // Guardar si hubo cambios
                if (modified)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    totalFixed++;
                }

                perSceneLogs.Add("\n=== " + scene.name + " ===\n" + string.Join("\n", log));
                EditorSceneManager.CloseScene(scene, true);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }
        }

        EditorUtility.DisplayDialog(
            "Auto-Fix FullHD Setup",
            totalFixed == 0 ? "No se realizaron cambios (todo estaba correcto o no había cámaras/UI para corregir)." : $"Escenas guardadas con correcciones: {totalFixed}",
            "OK");
    }
}
