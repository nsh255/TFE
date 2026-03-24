using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// Bloque de Validación 1 — Rendimiento / Benchmark.
/// Mide tiempos de generación de sala, regeneración de suelo y spawn de enemigos.
/// Calcula media, desviación estándar, mínimo y máximo.
/// Opcionalmente mide asignaciones de memoria GC por iteración.
///
/// Resultados exportados a CSV para incluir en la memoria del TFG.
/// Acceso: menú  Ascension ▸ Validación TFG ▸ Benchmark de Rendimiento
/// </summary>
public class ValidationPerformanceBenchmark : EditorWindow
{
    private int iterations = 100;
    private bool measureGC = true;
    private string csvPath = "Assets/Scripts/Editor/ValidationResults";
    private Vector2 scroll;
    private string lastReport = "";

    [MenuItem("Ascension/Validación TFG/2. Benchmark de Rendimiento")]
    public static void ShowWindow()
    {
        GetWindow<ValidationPerformanceBenchmark>("Benchmark Rendimiento");
    }

    private void OnGUI()
    {
        GUILayout.Label("Benchmark de Rendimiento", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Mide el coste temporal de las operaciones críticas:\n" +
            "• GenerateRoom()  (suelo + muros + colliders)\n" +
            "• RegenerateFloorKeepWalls()  (solo suelo)\n" +
            "• SpawnByCost()  (instanciación de enemigos)\n\n" +
            "Genera CSV con media, σ, min, max y percentiles.\n" +
            "IMPORTANTE: Ejecutar en Play Mode con GameScene abierta.",
            MessageType.Info);

        EditorGUILayout.Space();
        iterations = EditorGUILayout.IntSlider("Iteraciones", iterations, 10, 500);
        measureGC = EditorGUILayout.Toggle("Medir GC Alloc (aprox.)", measureGC);
        csvPath = EditorGUILayout.TextField("Carpeta CSV", csvPath);

        EditorGUILayout.Space();

        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("▶  Ejecutar Benchmark", GUILayout.Height(35)))
        {
            lastReport = RunBenchmark();
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
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(350));
            EditorGUILayout.TextArea(lastReport, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    // ───────────────────────── BENCHMARK CORE ─────────────────────────

    private string RunBenchmark()
    {
        var roomGen = Object.FindFirstObjectByType<RoomGenerator>();
        var enemyMgr = Object.FindFirstObjectByType<EnemyManager>();
        if (roomGen == null)
        {
            Debug.LogError("[Benchmark] RoomGenerator no encontrado.");
            return "ERROR: RoomGenerator no encontrado.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"=== Benchmark de Rendimiento — {iterations} iteraciones ===");
        sb.AppendLine($"Sala: {roomGen.roomWidth}×{roomGen.roomHeight}");
        sb.AppendLine();

        // --- CSV ---
        var csvRows = new List<string>();
        csvRows.Add("Iteracion,GenerateRoom_ms,RegenerateFloor_ms,SpawnByCost_ms,GC_Generate_KB,GC_Regen_KB,GC_Spawn_KB");

        double[] genTimes = new double[iterations];
        double[] regenTimes = new double[iterations];
        double[] spawnTimes = new double[iterations];
        double[] genGC = new double[iterations];
        double[] regenGC = new double[iterations];
        double[] spawnGC = new double[iterations];

        Stopwatch sw = new Stopwatch();

        for (int i = 0; i < iterations; i++)
        {
            // 1) GenerateRoom (full: suelo + muros)
            long gcBefore = measureGC ? System.GC.GetTotalMemory(false) : 0;
            sw.Restart();
            roomGen.GenerateRoom();
            sw.Stop();
            long gcAfter = measureGC ? System.GC.GetTotalMemory(false) : 0;
            genTimes[i] = sw.Elapsed.TotalMilliseconds;
            genGC[i] = measureGC ? (gcAfter - gcBefore) / 1024.0 : 0;

            // 2) RegenerateFloorKeepWalls (solo suelo)
            gcBefore = measureGC ? System.GC.GetTotalMemory(false) : 0;
            sw.Restart();
            roomGen.RegenerateFloorKeepWalls();
            sw.Stop();
            gcAfter = measureGC ? System.GC.GetTotalMemory(false) : 0;
            regenTimes[i] = sw.Elapsed.TotalMilliseconds;
            regenGC[i] = measureGC ? (gcAfter - gcBefore) / 1024.0 : 0;

            // 3) SpawnByCost (si hay EnemyManager)
            if (enemyMgr != null)
            {
                Rect area = roomGen.GetInnerWorldRect(2);
                int cost = 5 + i % 20; // variar carga
                enemyMgr.ClearAll();

                gcBefore = measureGC ? System.GC.GetTotalMemory(false) : 0;
                sw.Restart();
                enemyMgr.SpawnByCost(area, cost, i + 1);
                sw.Stop();
                gcAfter = measureGC ? System.GC.GetTotalMemory(false) : 0;
                spawnTimes[i] = sw.Elapsed.TotalMilliseconds;
                spawnGC[i] = measureGC ? (gcAfter - gcBefore) / 1024.0 : 0;

                enemyMgr.ClearAll();
            }

            csvRows.Add($"{i + 1},{genTimes[i]:F4},{regenTimes[i]:F4},{spawnTimes[i]:F4},{genGC[i]:F2},{regenGC[i]:F2},{spawnGC[i]:F2}");
        }

        // --- Estadísticas ---
        AppendStats(sb, "GenerateRoom()", genTimes);
        AppendStats(sb, "RegenerateFloorKeepWalls()", regenTimes);
        if (enemyMgr != null) AppendStats(sb, "SpawnByCost()", spawnTimes);

        if (measureGC)
        {
            sb.AppendLine("─── Memoria GC (KB por llamada, aproximado) ───");
            AppendStats(sb, "GC GenerateRoom", genGC);
            AppendStats(sb, "GC RegenFloor", regenGC);
            if (enemyMgr != null) AppendStats(sb, "GC SpawnByCost", spawnGC);
        }

        // Veredicto simple
        double meanGen = Mean(genTimes);
        sb.AppendLine();
        sb.AppendLine($"VEREDICTO: GenerateRoom media = {meanGen:F2} ms");
        if (meanGen < 5.0)
            sb.AppendLine("  ✔ Rendimiento excelente (< 5 ms). Sin impacto perceptible a 60 FPS.");
        else if (meanGen < 16.6)
            sb.AppendLine("  ⚠ Rendimiento aceptable (< 16.6 ms / 1 frame a 60 FPS).");
        else
            sb.AppendLine("  ✖ Rendimiento insuficiente (> 16.6 ms). Considerar optimización.");

        string report = sb.ToString();
        Debug.Log("[Benchmark]\n" + report);

        ExportCSV(csvRows, "performance_benchmark");
        return report;
    }

    // ───────────────────────── ESTADÍSTICAS ─────────────────────────

    private static void AppendStats(StringBuilder sb, string label, double[] data)
    {
        double mean = Mean(data);
        double stdDev = StdDev(data, mean);
        double min = Min(data);
        double max = Max(data);
        double p50 = Percentile(data, 50);
        double p95 = Percentile(data, 95);
        double p99 = Percentile(data, 99);

        sb.AppendLine($"─── {label} ───");
        sb.AppendLine($"  Media:  {mean:F4} ms   σ: {stdDev:F4} ms");
        sb.AppendLine($"  Min:    {min:F4} ms   Max: {max:F4} ms");
        sb.AppendLine($"  P50:    {p50:F4} ms   P95: {p95:F4} ms   P99: {p99:F4} ms");
        sb.AppendLine();
    }

    private static double Mean(double[] d)
    {
        double sum = 0;
        foreach (var v in d) sum += v;
        return sum / d.Length;
    }

    private static double StdDev(double[] d, double mean)
    {
        double sumSq = 0;
        foreach (var v in d) sumSq += (v - mean) * (v - mean);
        return System.Math.Sqrt(sumSq / d.Length);
    }

    private static double Min(double[] d)
    {
        double m = double.MaxValue;
        foreach (var v in d) if (v < m) m = v;
        return m;
    }

    private static double Max(double[] d)
    {
        double m = double.MinValue;
        foreach (var v in d) if (v > m) m = v;
        return m;
    }

    private static double Percentile(double[] data, int p)
    {
        var sorted = (double[])data.Clone();
        System.Array.Sort(sorted);
        double idx = (p / 100.0) * (sorted.Length - 1);
        int lo = (int)System.Math.Floor(idx);
        int hi = (int)System.Math.Ceiling(idx);
        if (lo == hi) return sorted[lo];
        double frac = idx - lo;
        return sorted[lo] * (1 - frac) + sorted[hi] * frac;
    }

    // ───────────────────────── EXPORT ─────────────────────────

    private void ExportCSV(List<string> rows, string baseName)
    {
        if (!Directory.Exists(csvPath))
            Directory.CreateDirectory(csvPath);

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fullPath = Path.Combine(csvPath, $"{baseName}_{timestamp}.csv");
        File.WriteAllLines(fullPath, rows, Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"[Benchmark] CSV exportado: {fullPath}");
    }
}
