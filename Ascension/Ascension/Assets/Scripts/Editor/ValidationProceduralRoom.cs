using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Bloque de Validación 2 — Generación Procedural.
/// Genera N habitaciones consecutivas y verifica invariantes estructurales:
///   1. Todos los tiles de suelo rellenados (sin huecos)
///   2. Anillo de muros completo (sin brechas)
///   3. Exactamente 1 tile de stairs por sala
///   4. Tiles especiales solo en interior (no en borde)
///   5. Distribución estadística de tiles especiales dentro de márgenes razonables
///
/// Resultados exportados a CSV para incluir en la memoria del TFG.
/// Acceso: menú  Ascension ▸ Validación TFG ▸ Validar Generación Procedural
/// </summary>
public class ValidationProceduralRoom : EditorWindow
{
    private int iterations = 100;
    private string csvPath = "Assets/Scripts/Editor/ValidationResults";
    private Vector2 scroll;
    private string lastReport = "";

    [MenuItem("Ascension/Validación TFG/1. Validar Generación Procedural")]
    public static void ShowWindow()
    {
        GetWindow<ValidationProceduralRoom>("Validación Procedural");
    }

    private void OnGUI()
    {
        GUILayout.Label("Validación de Generación Procedural", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Genera N habitaciones consecutivas en Play Mode y verifica:\n" +
            "• Suelo sin huecos (roomWidth × roomHeight tiles)\n" +
            "• Anillo de muros completo\n" +
            "• Exactamente 1 stairs por sala\n" +
            "• Tiles especiales dentro del interior\n" +
            "• Distribución estadística de efectos\n\n" +
            "IMPORTANTE: La escena GameScene debe estar abierta y en Play Mode.",
            MessageType.Info);

        EditorGUILayout.Space();
        iterations = EditorGUILayout.IntSlider("Iteraciones", iterations, 10, 500);
        csvPath = EditorGUILayout.TextField("Carpeta CSV", csvPath);

        EditorGUILayout.Space();

        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("▶  Ejecutar Validación", GUILayout.Height(35)))
        {
            lastReport = RunValidation();
        }
        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Pulsa Play en el Editor antes de ejecutar.", MessageType.Warning);
        }

        if (!string.IsNullOrEmpty(lastReport))
        {
            EditorGUILayout.Space();
            GUILayout.Label("Último resultado", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(300));
            EditorGUILayout.TextArea(lastReport, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    // ───────────────────────────── CORE ─────────────────────────────

    private string RunValidation()
    {
        var roomGen = Object.FindFirstObjectByType<RoomGenerator>();
        if (roomGen == null)
        {
            Debug.LogError("[ValidationProcedural] No se encontró RoomGenerator en la escena.");
            return "ERROR: RoomGenerator no encontrado.";
        }

        Tilemap wallTm = roomGen.wallTilemap;
        Tilemap floorTm = roomGen.floorTilemap;
        if (wallTm == null || floorTm == null)
        {
            return "ERROR: Tilemaps no asignados en RoomGenerator.";
        }

        int width = roomGen.roomWidth;
        int height = roomGen.roomHeight;
        Vector3Int offset = roomGen.roomOffset;

        var sb = new StringBuilder();
        sb.AppendLine($"=== Validación Procedural — {iterations} iteraciones ===");
        sb.AppendLine($"Dimensiones: {width}×{height}  |  Offset: {offset}");
        sb.AppendLine();

        // Contadores globales
        int totalPass = 0;
        int failFloor = 0, failWalls = 0, failStairs = 0, failSpecialBorder = 0;

        // Distribución de tiles especiales
        int totalHeal = 0, totalIce = 0, totalMud = 0, totalPowerUp = 0, totalNormal = 0, totalStairs = 0;

        // CSV rows
        var csvRows = new List<string>();
        csvRows.Add("Iteracion,Suelo_OK,Muros_OK,Stairs_OK,Especiales_OK,Heal,Ice,Mud,PowerUp,Normal,Stairs");

        float startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < iterations; i++)
        {
            // Regenerar suelo (muros se mantienen tras la 1ª generación)
            if (i == 0)
                roomGen.GenerateRoom();
            else
                roomGen.RegenerateFloorKeepWalls();

            // --- 1. Verificar suelo completo ---
            bool floorOk = true;
            int floorCount = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0) + offset;
                    if (floorTm.GetTile(pos) == null)
                    {
                        floorOk = false;
                    }
                    else
                    {
                        floorCount++;
                    }
                }
            }
            if (!floorOk) failFloor++;

            // --- 2. Verificar anillo de muros (grosor 1 mínimo) ---
            bool wallsOk = true;
            // Borde inferior y superior
            for (int x = -1; x <= width; x++)
            {
                if (wallTm.GetTile(new Vector3Int(x, -1, 0) + offset) == null) wallsOk = false;
                if (wallTm.GetTile(new Vector3Int(x, height, 0) + offset) == null) wallsOk = false;
            }
            // Borde izquierdo y derecho
            for (int y = 0; y < height; y++)
            {
                if (wallTm.GetTile(new Vector3Int(-1, y, 0) + offset) == null) wallsOk = false;
                if (wallTm.GetTile(new Vector3Int(width, y, 0) + offset) == null) wallsOk = false;
            }
            if (!wallsOk) failWalls++;

            // --- 3. Contar stairs (debe haber exactamente 1) ---
            int stairsCount = 0;
            // --- 4. Especiales en borde (no debería haber) ---
            bool specialBorderOk = true;

            int heal = 0, ice = 0, mud = 0, powerup = 0, normal = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0) + offset;
                    TileBase tile = floorTm.GetTile(pos);
                    if (tile == null) continue;

                    // Clasificar tile
                    TileEffect effect = GetTileEffect(tile);
                    bool isStairs = IsStairsTile(tile, roomGen);

                    if (isStairs)
                    {
                        stairsCount++;
                    }
                    else if (effect != null && effect.effectType != TileEffectType.None)
                    {
                        switch (effect.effectType)
                        {
                            case TileEffectType.Heal: heal++; break;
                            case TileEffectType.Ice:
                            case TileEffectType.SpeedUp: ice++; break;
                            case TileEffectType.Mud:
                            case TileEffectType.SpeedDown: mud++; break;
                            case TileEffectType.PowerUp: powerup++; break;
                            default: normal++; break;
                        }

                        // Verificar que tiles especiales no estén en el borde exacto
                        bool onBorder = (x == 0 || x == width - 1 || y == 0 || y == height - 1);
                        if (onBorder) specialBorderOk = false;
                    }
                    else
                    {
                        normal++;
                    }
                }
            }

            bool stairsOk = (stairsCount == 1);
            if (!stairsOk) failStairs++;
            if (!specialBorderOk) failSpecialBorder++;

            totalHeal += heal;
            totalIce += ice;
            totalMud += mud;
            totalPowerUp += powerup;
            totalNormal += normal;
            totalStairs += stairsCount;

            bool allOk = floorOk && wallsOk && stairsOk && specialBorderOk;
            if (allOk) totalPass++;

            csvRows.Add($"{i + 1},{B(floorOk)},{B(wallsOk)},{B(stairsOk)},{B(specialBorderOk)},{heal},{ice},{mud},{powerup},{normal},{stairsCount}");
        }

        float elapsed = Time.realtimeSinceStartup - startTime;

        // --- Resumen ---
        sb.AppendLine($"Tiempo total: {elapsed:F3} s  ({elapsed / iterations * 1000:F2} ms/sala)");
        sb.AppendLine();
        sb.AppendLine($"RESULTADO GLOBAL: {totalPass}/{iterations} salas pasan TODAS las invariantes ({100f * totalPass / iterations:F1}%)");
        sb.AppendLine();
        sb.AppendLine("Desglose de fallos:");
        sb.AppendLine($"  Suelo incompleto:      {failFloor}");
        sb.AppendLine($"  Muros con brechas:     {failWalls}");
        sb.AppendLine($"  Stairs ≠ 1:            {failStairs}");
        sb.AppendLine($"  Especial en borde:     {failSpecialBorder}");
        sb.AppendLine();
        sb.AppendLine("Distribución media de tiles por sala:");
        float n = iterations;
        sb.AppendLine($"  Normal:   {totalNormal / n:F1}");
        sb.AppendLine($"  Heal:     {totalHeal / n:F2}");
        sb.AppendLine($"  Ice:      {totalIce / n:F2}");
        sb.AppendLine($"  Mud:      {totalMud / n:F2}");
        sb.AppendLine($"  PowerUp:  {totalPowerUp / n:F2}");
        sb.AppendLine($"  Stairs:   {totalStairs / n:F2}");

        string report = sb.ToString();
        Debug.Log("[ValidationProcedural]\n" + report);

        // Exportar CSV
        ExportCSV(csvRows, "procedural_validation");

        return report;
    }

    // ───────────────────────────── HELPERS ─────────────────────────────

    private static string B(bool v) => v ? "1" : "0";

    private static TileEffect GetTileEffect(TileBase tile)
    {
        if (tile == null) return null;
        if (tile is VariantTile vt) return vt.tileEffect;
        // Fallback reflexión
        var field = tile.GetType().GetField("tileEffect");
        return field?.GetValue(tile) as TileEffect;
    }

    /// <summary>
    /// Compara con el campo privado stairsTile vía reflexión (SerializeField).
    /// </summary>
    private static bool IsStairsTile(TileBase tile, RoomGenerator gen)
    {
        if (tile == null || gen == null) return false;
        var field = typeof(RoomGenerator).GetField("stairsTile",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null) return false;
        var stairs = field.GetValue(gen) as TileBase;
        return stairs != null && tile == stairs;
    }

    private void ExportCSV(List<string> rows, string baseName)
    {
        if (!Directory.Exists(csvPath))
            Directory.CreateDirectory(csvPath);

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fullPath = Path.Combine(csvPath, $"{baseName}_{timestamp}.csv");
        File.WriteAllLines(fullPath, rows, Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"[ValidationProcedural] CSV exportado: {fullPath}");
    }
}
