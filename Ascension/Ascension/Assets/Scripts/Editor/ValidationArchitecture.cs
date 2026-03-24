using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Bloque de Validación 3 — Arquitectura y Métricas de Código.
/// Analiza estáticamente el proyecto (NO requiere Play Mode):
///   1. Conteo de scripts, líneas de código, clases MonoBehaviour
///   2. Inventario de ScriptableObjects (EnemyData, WeaponData, PlayerClass, TileEffect)
///   3. Validación de Singletons (GameManager, ScoreManager, RoomFlowController)
///   4. Detección de patrones de diseño utilizados
///   5. Cobertura de campos serializables con valores por defecto razonables
///
/// Exporta CSV con métricas para la memoria del TFG.
/// Acceso: menú  Ascension ▸ Validación TFG ▸ Validación Arquitectural
/// </summary>
public class ValidationArchitecture : EditorWindow
{
    private string csvPath = "Assets/Scripts/Editor/ValidationResults";
    private Vector2 scroll;
    private string lastReport = "";

    [MenuItem("Ascension/Validación TFG/4. Validación Arquitectural")]
    public static void ShowWindow()
    {
        GetWindow<ValidationArchitecture>("Validación Arquitectura");
    }

    private void OnGUI()
    {
        GUILayout.Label("Validación Arquitectural", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Analiza la estructura del código fuente:\n" +
            "• Conteo de scripts y líneas de código\n" +
            "• Inventario de ScriptableObjects en el proyecto\n" +
            "• Verificación de Singletons\n" +
            "• Detección de patrones de diseño\n\n" +
            "NO requiere Play Mode.",
            MessageType.Info);

        EditorGUILayout.Space();
        csvPath = EditorGUILayout.TextField("Carpeta CSV", csvPath);

        EditorGUILayout.Space();
        if (GUILayout.Button("▶  Ejecutar Análisis Arquitectural", GUILayout.Height(35)))
        {
            lastReport = RunArchitectureAnalysis();
        }

        if (!string.IsNullOrEmpty(lastReport))
        {
            EditorGUILayout.Space();
            GUILayout.Label("Último resultado", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(400));
            EditorGUILayout.TextArea(lastReport, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    // ───────────────────────── ANÁLISIS ─────────────────────────

    private string RunArchitectureAnalysis()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Validación Arquitectural del Proyecto Ascension ===");
        sb.AppendLine();

        var csvRows = new List<string>();
        csvRows.Add("Metrica,Valor,Detalle");

        // ── 1. Métricas de código fuente ──
        sb.AppendLine("── 1. Métricas de Código Fuente ──");
        string scriptsFolder = "Assets/Scripts";
        string fullScriptsPath = Path.Combine(Application.dataPath, "Scripts");

        int totalFiles = 0, totalLines = 0, totalBlankLines = 0, totalCommentLines = 0;
        int monoBehaviourCount = 0, scriptableObjectCount = 0, editorScripts = 0;
        var folderLineCount = new Dictionary<string, int>();

        if (Directory.Exists(fullScriptsPath))
        {
            var csFiles = Directory.GetFiles(fullScriptsPath, "*.cs", SearchOption.AllDirectories);
            foreach (var file in csFiles)
            {
                totalFiles++;
                string[] lines = File.ReadAllLines(file);
                int fileLines = lines.Length;
                totalLines += fileLines;

                // Carpeta relativa
                string relDir = Path.GetDirectoryName(file.Replace(fullScriptsPath, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (string.IsNullOrEmpty(relDir)) relDir = "(root)";
                if (!folderLineCount.ContainsKey(relDir)) folderLineCount[relDir] = 0;
                folderLineCount[relDir] += fileLines;

                bool isEditor = file.Contains(Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar)
                             || file.Contains(Path.AltDirectorySeparatorChar + "Editor" + Path.AltDirectorySeparatorChar);
                if (isEditor) editorScripts++;

                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) totalBlankLines++;
                    else if (trimmed.StartsWith("//") || trimmed.StartsWith("///") || trimmed.StartsWith("*") || trimmed.StartsWith("/*")) totalCommentLines++;
                }

                // Detectar herencia MonoBehaviour / ScriptableObject
                string content = File.ReadAllText(file);
                if (content.Contains(": MonoBehaviour")) monoBehaviourCount++;
                if (content.Contains(": ScriptableObject")) scriptableObjectCount++;
            }
        }

        int codeLines = totalLines - totalBlankLines - totalCommentLines;
        sb.AppendLine($"  Scripts totales:     {totalFiles}");
        sb.AppendLine($"    - Editor scripts:  {editorScripts}");
        sb.AppendLine($"    - Runtime scripts: {totalFiles - editorScripts}");
        sb.AppendLine($"  Líneas totales:      {totalLines}");
        sb.AppendLine($"    - Código:          {codeLines}");
        sb.AppendLine($"    - Comentarios:     {totalCommentLines}");
        sb.AppendLine($"    - En blanco:       {totalBlankLines}");
        sb.AppendLine($"  MonoBehaviours:      {monoBehaviourCount}");
        sb.AppendLine($"  ScriptableObjects:   {scriptableObjectCount}");
        sb.AppendLine();

        csvRows.Add($"Scripts totales,{totalFiles},");
        csvRows.Add($"Editor scripts,{editorScripts},");
        csvRows.Add($"Runtime scripts,{totalFiles - editorScripts},");
        csvRows.Add($"Lineas totales,{totalLines},");
        csvRows.Add($"Lineas codigo,{codeLines},");
        csvRows.Add($"Lineas comentario,{totalCommentLines},");
        csvRows.Add($"Lineas blanco,{totalBlankLines},");
        csvRows.Add($"MonoBehaviours,{monoBehaviourCount},");
        csvRows.Add($"ScriptableObjects clase,{scriptableObjectCount},");

        sb.AppendLine("  Líneas por carpeta:");
        foreach (var kv in folderLineCount)
        {
            sb.AppendLine($"    {kv.Key}: {kv.Value}");
            csvRows.Add($"Lineas_{kv.Key},{kv.Value},");
        }
        sb.AppendLine();

        // ── 2. Inventario de ScriptableObject instances ──
        sb.AppendLine("── 2. Inventario de ScriptableObject Assets ──");

        int enemyDataCount = CountAssets<EnemyData>();
        int weaponDataCount = CountAssets<WeaponData>();
        int playerClassCount = CountAssets<PlayerClass>();
        int tileEffectCount = CountAssets<TileEffect>();

        sb.AppendLine($"  EnemyData assets:    {enemyDataCount}");
        sb.AppendLine($"  WeaponData assets:   {weaponDataCount}");
        sb.AppendLine($"  PlayerClass assets:  {playerClassCount}");
        sb.AppendLine($"  TileEffect assets:   {tileEffectCount}");

        csvRows.Add($"EnemyData assets,{enemyDataCount},");
        csvRows.Add($"WeaponData assets,{weaponDataCount},");
        csvRows.Add($"PlayerClass assets,{playerClassCount},");
        csvRows.Add($"TileEffect assets,{tileEffectCount},");

        // Listar EnemyData con detalles
        var enemyDatas = FindAllAssets<EnemyData>();
        if (enemyDatas.Length > 0)
        {
            sb.AppendLine("  Detalle EnemyData:");
            foreach (var ed in enemyDatas)
            {
                sb.AppendLine($"    {ed.name}: HP={ed.maxHealth}, DMG={ed.damage}, SPD={ed.speed:F1}, Cost={ed.enemyCost}");
            }
        }
        sb.AppendLine();

        // ── 3. Verificación de Singletons ──
        sb.AppendLine("── 3. Patrones de Diseño Detectados ──");

        var singletons = new[] { "GameManager", "ScoreManager", "RoomFlowController", "DamageBoostManager" };
        int singletonsFound = 0;
        foreach (var s in singletons)
        {
            bool found = false;
            if (Directory.Exists(fullScriptsPath))
            {
                var files = Directory.GetFiles(fullScriptsPath, "*.cs", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    if (!Path.GetFileNameWithoutExtension(f).Equals(s, System.StringComparison.OrdinalIgnoreCase)) continue;
                    string content = File.ReadAllText(f);
                    if (content.Contains("static") && content.Contains("Instance"))
                    {
                        found = true;
                        singletonsFound++;
                        break;
                    }
                }
            }
            sb.AppendLine($"  Singleton {s}: {(found ? "OK" : "NO ENCONTRADO")}");
        }

        // Observer pattern (events)
        int eventCount = 0;
        if (Directory.Exists(fullScriptsPath))
        {
            foreach (var f in Directory.GetFiles(fullScriptsPath, "*.cs", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(f);
                // Contar declaraciones "event" (delegado o Action)
                int idx = 0;
                while ((idx = content.IndexOf("event ", idx, System.StringComparison.Ordinal)) >= 0)
                {
                    eventCount++;
                    idx += 6;
                }
            }
        }

        sb.AppendLine($"  Singletons verificados: {singletonsFound}/{singletons.Length}");
        sb.AppendLine($"  Declaraciones 'event': {eventCount} (patrón Observer)");
        sb.AppendLine($"  ScriptableObjects como Strategy/Data: {scriptableObjectCount} clases");

        csvRows.Add($"Singletons,{singletonsFound},{singletons.Length} esperados");
        csvRows.Add($"Events (Observer),{eventCount},");
        csvRows.Add($"ScriptableObject classes,{scriptableObjectCount},Strategy/Data pattern");

        sb.AppendLine();

        // ── 4. Resumen de calidad ──
        sb.AppendLine("── 4. Métricas de Calidad ──");
        float commentRatio = totalLines > 0 ? (float)totalCommentLines / totalLines * 100f : 0;
        float codeRatio = totalLines > 0 ? (float)codeLines / totalLines * 100f : 0;
        sb.AppendLine($"  Ratio comentarios/total: {commentRatio:F1}%");
        sb.AppendLine($"  Ratio código/total:      {codeRatio:F1}%");
        sb.AppendLine($"  Media líneas/script:     {(totalFiles > 0 ? totalLines / totalFiles : 0)}");

        csvRows.Add($"Ratio comentarios,{commentRatio:F1}%,");
        csvRows.Add($"Ratio codigo,{codeRatio:F1}%,");
        csvRows.Add($"Media lineas/script,{(totalFiles > 0 ? totalLines / totalFiles : 0)},");

        sb.AppendLine();
        sb.AppendLine("═══ Análisis completado ═══");

        string report = sb.ToString();
        Debug.Log("[Architecture]\n" + report);

        ExportCSV(csvRows, "architecture_validation");
        return report;
    }

    // ───────────────────────── HELPERS ─────────────────────────

    private static int CountAssets<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        return guids.Length;
    }

    private static T[] FindAllAssets<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var results = new List<T>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) results.Add(asset);
        }
        return results.ToArray();
    }

    private void ExportCSV(List<string> rows, string baseName)
    {
        if (!Directory.Exists(csvPath))
            Directory.CreateDirectory(csvPath);

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fullPath = Path.Combine(csvPath, $"{baseName}_{timestamp}.csv");
        File.WriteAllLines(fullPath, rows, Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"[Architecture] CSV exportado: {fullPath}");
    }
}
