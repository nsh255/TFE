using UnityEditor;
using UnityEngine;

/// <summary>
/// Verifica y configura automáticamente los enemyCost en los EnemyData.
/// Asegura que SlimeRed=2, SlimeBlue=3, SlimeGreen=4.
/// </summary>
public class EnemyDataValidator : EditorWindow
{
    [MenuItem("Ascension/Setup/5. Verify EnemyData Costs")]
    public static void ShowWindow()
    {
        GetWindow<EnemyDataValidator>("EnemyData Validator");
    }

    private void OnGUI()
    {
        GUILayout.Label("EnemyData Cost Validator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Verifica y configura enemyCost:\n" +
            "✅ SlimeRedData → enemyCost = 2\n" +
            "✅ SlimeBlueData → enemyCost = 3\n" +
            "✅ SlimeGreenData → enemyCost = 4\n\n" +
            "Estos valores se usan en EnemyManager.SpawnByCost()\n" +
            "para controlar cuántos enemigos caben en una sala.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("🔍 VERIFICAR Y CONFIGURAR COSTS", GUILayout.Height(40)))
        {
            VerifyAndConfigureCosts();
        }
    }

    private static void VerifyAndConfigureCosts()
    {
        Debug.Log("[EnemyDataValidator] Verificando EnemyData...");

        bool allCorrect = true;

        // Buscar todos los EnemyData
        string[] guids = AssetDatabase.FindAssets("t:EnemyData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);

            if (data == null) continue;

            // Determinar cost esperado según nombre
            int expectedCost = -1;
            string enemyType = "";

            if (path.Contains("Red") || data.enemyName.Contains("Red") || data.enemyName.Contains("Chaser"))
            {
                expectedCost = 2;
                enemyType = "SlimeRed (Chaser)";
            }
            else if (path.Contains("Blue") || data.enemyName.Contains("Blue") || data.enemyName.Contains("Jumper"))
            {
                expectedCost = 3;
                enemyType = "SlimeBlue (Jumper)";
            }
            else if (path.Contains("Green") || data.enemyName.Contains("Green") || data.enemyName.Contains("Shooter"))
            {
                expectedCost = 4;
                enemyType = "SlimeGreen (Shooter)";
            }

            if (expectedCost == -1) continue;

            // Verificar y corregir
            if (data.enemyCost != expectedCost)
            {
                Debug.LogWarning($"[EnemyDataValidator] ⚠️ {enemyType} tiene cost={data.enemyCost}, corrigiendo a {expectedCost}");
                data.enemyCost = expectedCost;
                EditorUtility.SetDirty(data);
                allCorrect = false;
            }
            else
            {
                Debug.Log($"[EnemyDataValidator] ✅ {enemyType} cost={data.enemyCost} (correcto)");
            }
        }

        AssetDatabase.SaveAssets();

        string message = allCorrect
            ? "Todos los EnemyData tienen costs correctos:\n" +
              "✅ SlimeRed = 2\n" +
              "✅ SlimeBlue = 3\n" +
              "✅ SlimeGreen = 4\n\n" +
              "¡Configuración completa!\n" +
              "Ahora puedes jugar."
            : "EnemyData costs actualizados correctamente:\n" +
              "✅ SlimeRed = 2\n" +
              "✅ SlimeBlue = 3\n" +
              "✅ SlimeGreen = 4\n\n" +
              "¡Configuración completa!\n" +
              "Ahora puedes jugar.";

        EditorUtility.DisplayDialog(
            allCorrect ? "✅ Verificación Completa" : "✅ Costs Actualizados",
            message,
            "Jugar!"
        );

        Debug.Log("[EnemyDataValidator] ✅ Verificación completada.");
    }
}
