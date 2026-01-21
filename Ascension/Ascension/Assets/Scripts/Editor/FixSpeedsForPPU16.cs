using UnityEditor;
using UnityEngine;

/// <summary>
/// Ajusta todas las velocidades del juego para PPU=16 (divide por 16).
/// </summary>
public class FixSpeedsForPPU16 : EditorWindow
{
    [MenuItem("Ascension/Debug/Fix All Speeds for PPU=16")]
    static void ShowWindow()
    {
        FixAllSpeeds();
    }

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
                Debug.Log($"✅ {playerClass.className}: speed {oldSpeed} → {playerClass.speed}");
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
                    Debug.Log($"   ⚠️ {enemyData.name} puede tener dashSpeed hardcoded en SlimeBlue.cs");
                }
                
                EditorUtility.SetDirty(enemyData);
                Debug.Log($"✅ {enemyData.name}: speed {oldSpeed} → {enemyData.speed}");
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();

        string message = $"✅ {fixedCount} velocidades ajustadas (divididas por 16)\n\n" +
                        "Player classes: speed / 16\n" +
                        "Enemy data: speed / 16\n\n" +
                        "NOTA: SlimeBlue dashSpeed está hardcoded en código (speed=2, dashSpeed=12).\n" +
                        "Cambiar a: speed=0.125, dashSpeed=0.75\n\n" +
                        "PRUEBA EL JUEGO AHORA.";

        Debug.Log(message);

        EditorUtility.DisplayDialog(
            "Velocidades Ajustadas",
            message,
            "OK"
        );
    }
}
