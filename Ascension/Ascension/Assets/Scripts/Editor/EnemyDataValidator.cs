using UnityEditor;
using UnityEngine;

/// <summary>
/// Verifica y configura automáticamente los enemyCost en los EnemyData.
/// Asegura que SlimeRed=2, SlimeBlue=3, SlimeGreen=4.
/// </summary>
public class EnemyDataValidator : EditorWindow
{
    [MenuItem("Ascension/Setup/5. Verify EnemyData Costs")]
    /// <summary>
    /// Abre la ventana de validación de costes de enemigos.
    /// </summary>
    public static void ShowWindow()
    {
        GetWindow<EnemyDataValidator>("EnemyData Validator");
    }

    /// <summary>
    /// Dibuja la interfaz de validación en el editor.
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("EnemyData Cost Validator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Verifica y configura enemyCost:\n" +
            "SlimeRedData → enemyCost = dos\n" +
            "SlimeBlueData → enemyCost = tres\n" +
            "SlimeGreenData → enemyCost = cuatro\n\n" +
            "Estos valores se usan en EnemyManager.SpawnByCost()\n" +
            "para controlar cuántos enemigos caben en una sala.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Verificar y configurar costes", GUILayout.Height(40)))
        {
            VerifyAndConfigureCosts();
        }
    }

    /// <summary>
    /// Verifica los costes y actualiza EnemyData si es necesario.
    /// </summary>
    private static void VerifyAndConfigureCosts()
    {
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
                Debug.LogWarning($"{enemyType} tiene cost={data.enemyCost}, corrigiendo a {expectedCost}.");
                data.enemyCost = expectedCost;
                EditorUtility.SetDirty(data);
                allCorrect = false;
            }
        }

        AssetDatabase.SaveAssets();

                string message = allCorrect
                        ? "Los EnemyData tienen costes correctos:\n" +
                            "SlimeRed = dos\n" +
                            "SlimeBlue = tres\n" +
                            "SlimeGreen = cuatro."
                        : "Se han actualizado los costes de EnemyData:\n" +
                            "SlimeRed = dos\n" +
                            "SlimeBlue = tres\n" +
                            "SlimeGreen = cuatro.";

        EditorUtility.DisplayDialog(
            allCorrect ? "Verificación completada" : "Costes actualizados",
            message,
            "OK"
        );
    }
}
