using UnityEditor;
using UnityEngine;

/// <summary>
/// Ajusta todas las velocidades del juego para PPU=16 (divide por 16).
/// </summary>
public class FixSpeedsForPPU16 : EditorWindow
{
    [MenuItem("Ascension/Debug/Fix All Speeds for PPU=16")]
    /// <summary>
    /// Ejecuta el ajuste de velocidades para PPU igual a dieciséis.
    /// </summary>
    static void ShowWindow()
    {
        FixAllSpeeds();
    }

    /// <summary>
    /// Ajusta velocidades de clases de jugador y enemigos.
    /// </summary>
    private static void FixAllSpeeds()
    {
        int fixedCount = 0;

        // 1. Fix PlayerClass speeds
        string[] classPaths = {
            "Assets/Data/Classes/Swordsman.asset",
            "Assets/Data/Classes/Knight.asset",
            "Assets/Data/Classes/Mage.asset"
        };

        foreach (var path in classPaths)
        {
            var playerClass = AssetDatabase.LoadAssetAtPath<PlayerClass>(path);
            if (playerClass != null)
            {
                float oldSpeed = playerClass.speed;
                playerClass.speed = oldSpeed / 16f;
                EditorUtility.SetDirty(playerClass);
                fixedCount++;
            }
        }

        // 2. Fix EnemyData speeds
        string[] enemyPaths = {
            "Assets/Data/Enemies/ChaserEnemy.asset",
            "Assets/Data/Enemies/JumperEnemy.asset",
            "Assets/Data/Enemies/ShooterEnemy.asset",
            "Assets/Data/Enemies/Boss.asset"
        };

        foreach (var path in enemyPaths)
        {
            var enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (enemyData != null)
            {
                float oldSpeed = enemyData.speed;
                enemyData.speed = oldSpeed / 16f;
                
                // Ajustar también dashSpeed si existe (SlimeBlue)
                if (enemyData.name.Contains("Jumper"))
                {
                    // SlimeBlue tiene dashSpeed, ajustarlo también
                    Debug.LogWarning($"{enemyData.name} puede tener dashSpeed fijo en SlimeBlue.cs.");
                }
                
                EditorUtility.SetDirty(enemyData);
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();

        string message = $"Se han ajustado {fixedCount} velocidades.\n\n" +
                "Clases de jugador: velocidad dividida por dieciséis\n" +
                "Datos de enemigos: velocidad dividida por dieciséis\n\n" +
                "Nota: SlimeBlue mantiene dashSpeed en código.\n" +
                "Valores recomendados: speed = cero coma uno dos cinco, dashSpeed = cero coma siete cinco.";

        EditorUtility.DisplayDialog(
            "Velocidades Ajustadas",
            message,
            "OK"
        );
    }
}
