using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Valida configuración para cambiar la resolución del editor a Full HD (1920x1080)
/// sin romper el pixel art ni la UI.
/// - Escanea escenas en Assets/Scenes (sin modificarlas)
/// - Verifica:
///   * Cámara ortográfica + PixelPerfectSetup presente
///   * referenceResolution = 480x270 (base del proyecto)
///   * dynamicResolution = true recomendado
///   * Escala de píxel recomendada para 1080p: 4
///   * CanvasScaler = ScaleWithScreenSize, refRes = 480x270, matchWidthOrHeight (0..1)
/// - Muestra un resumen con recomendaciones en español.
/// </summary>
public static class FullHDSetupValidator
{
    private static readonly Vector2Int TargetResolution = new Vector2Int(1920, 1080);
    private static readonly Vector2 ReferenceUIResolution = new Vector2(480, 270);

    [MenuItem("Ascension/Diagnostics/Validate FullHD Setup")]
    /// <summary>
    /// Valida la configuración de escenas para Full HD.
    /// </summary>
    public static void ValidateFullHD()
    {
        // Buscar escenas en la carpeta estándar del proyecto
        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        if (sceneGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Validate FullHD Setup", "No se encontraron escenas en Assets/Scenes.", "OK");
            return;
        }

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        var results = new List<string>();
        int issues = 0;

        try
        {
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                EditorUtility.DisplayProgressBar("Validando FullHD", $"Escena: {System.IO.Path.GetFileNameWithoutExtension(scenePath)}", (float)(i + 1) / sceneGuids.Length);

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                string sceneHeader = $"\n=== {scene.name} ===";
                results.Add(sceneHeader);

                // 1) Validar cámara y PixelPerfectSetup
                var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
                Camera mainCam = cameras.FirstOrDefault(c => c.CompareTag("MainCamera")) ?? cameras.FirstOrDefault();
                if (mainCam == null)
                {
                    results.Add("[!] No se encontró ninguna Camera en la escena.");
                    issues++;
                }
                else
                {
                    if (!mainCam.orthographic)
                    {
                        results.Add("[!] La cámara principal no es ortográfica.");
                        issues++;
                    }

                    var pps = mainCam.GetComponent("PixelPerfectSetup");
                    if (pps == null)
                    {
                        results.Add("[!] Falta PixelPerfectSetup en la cámara principal.");
                        issues++;
                    }
                    else
                    {
                        // Reflection ligera para no acoplar el compilado si cambian nombres internos
                        var ppsType = pps.GetType();
                        var refResField = ppsType.GetField("referenceResolution");
                        var dynResField = ppsType.GetField("dynamicResolution");
                        var pixelScaleField = ppsType.GetField("pixelScale");

                        Vector2Int refRes = new Vector2Int(0, 0);
                        bool dynRes = false;
                        int pixelScale = 0;

                        if (refResField != null)
                        {
                            refRes = (Vector2Int)refResField.GetValue(pps);
                        }
                        if (dynResField != null)
                        {
                            dynRes = (bool)dynResField.GetValue(pps);
                        }
                        if (pixelScaleField != null)
                        {
                            pixelScale = (int)pixelScaleField.GetValue(pps);
                        }

                        if (refRes != new Vector2Int((int)ReferenceUIResolution.x, (int)ReferenceUIResolution.y))
                        {
                            results.Add($"[!] referenceResolution esperado {ReferenceUIResolution}, actual {refRes}.");
                            issues++;
                        }

                        if (!dynRes)
                        {
                            results.Add("[i] dynamicResolution está desactivado (recomendado activarlo para autoscalar).");
                        }

                        int recommendedScale = Mathf.FloorToInt(Mathf.Min(
                            (float)TargetResolution.x / ReferenceUIResolution.x,
                            (float)TargetResolution.y / ReferenceUIResolution.y));
                        recommendedScale = Mathf.Max(1, recommendedScale); // 1080p -> 4

                        if (pixelScale != recommendedScale)
                        {
                            results.Add($"[i] pixelScale recomendado para 1080p: {recommendedScale}, actual: {pixelScale}.");
                        }
                        else
                        {
                            results.Add($"[ok] PixelPerfectSetup correcto (ref {ReferenceUIResolution}, scale {pixelScale}).");
                        }
                    }
                }

                // 2) Validar CanvasScaler
                var scalers = Object.FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);
                if (scalers.Length == 0)
                {
                    results.Add("[i] No se encontró CanvasScaler (OK si la escena no tiene UI).");
                }
                else
                {
                    foreach (var cs in scalers)
                    {
                        bool ok = true;
                        if (cs.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                        {
                            results.Add($"[!] CanvasScaler '{cs.name}' no usa ScaleWithScreenSize.");
                            ok = false; issues++;
                        }
                        if (cs.referenceResolution != ReferenceUIResolution)
                        {
                            results.Add($"[!] CanvasScaler '{cs.name}' referenceResolution esperado {ReferenceUIResolution}, actual {cs.referenceResolution}.");
                            ok = false; issues++;
                        }
                        if (cs.matchWidthOrHeight < 0f || cs.matchWidthOrHeight > 1f)
                        {
                            results.Add($"[!] CanvasScaler '{cs.name}' matchWidthOrHeight fuera de rango (0..1). Actual: {cs.matchWidthOrHeight}");
                            ok = false; issues++;
                        }

                        if (ok)
                        {
                            results.Add($"[ok] CanvasScaler '{cs.name}' configurado correctamente.");
                        }
                    }
                }

                // Cerrar la escena que abrimos en Additive para no alterar el estado del editor
                EditorSceneManager.CloseScene(scene, true);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                // Volver a la escena original
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }
        }

        string report = string.Join("\n", results);
        Debug.Log("[Validate FullHD Setup] Resultado:\n" + report);

        string dialogMsg = issues == 0
            ? "Validación completada: no se encontraron problemas críticos. Revisa la Consola para el detalle."
            : $"Validación completada: se encontraron {issues} posibles problemas. Revisa la Consola para el detalle.";
        EditorUtility.DisplayDialog("Validate FullHD Setup", dialogMsg, "OK");
    }
}
