using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script que crea automáticamente los TileEffects necesarios
/// Solo funciona en el Editor
/// </summary>
public class TileEffectCreator : MonoBehaviour
{
    [ContextMenu("Crear TileEffects Automáticamente")]
    public void CreateTileEffects()
    {
#if UNITY_EDITOR
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║            CREANDO TILEEFFECTS AUTOMÁTICAMENTE                 ║");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝\n");

        CreateIceTileEffect();
        CreateMudTileEffect();
        CreateHealTileEffect();
        CreateDamageTileEffect();
        CreateSpeedUpTileEffect();
        CreateSpeedDownTileEffect();
        CreatePowerUpTileEffect();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("\n╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║              TILEEFFECTS CREADOS EXITOSAMENTE                  ║");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
        Debug.Log("\n✅ Ahora asigna estos TileEffects a tus VariantTiles en el Inspector");
#else
        Debug.LogError("Este script solo funciona en el Editor de Unity");
#endif
    }

#if UNITY_EDITOR
    void CreateIceTileEffect()
    {
        string path = "Assets/Data/TileEffects/IceTileEffect.asset";
        
        // Verificar si ya existe
        TileEffect existing = AssetDatabase.LoadAssetAtPath<TileEffect>(path);
        if (existing != null)
        {
            Debug.Log($"⚠️  IceTileEffect ya existe, actualizando...");
            ConfigureIceEffect(existing);
            EditorUtility.SetDirty(existing);
            return;
        }

        TileEffect effect = ScriptableObject.CreateInstance<TileEffect>();
        ConfigureIceEffect(effect);
        
        AssetDatabase.CreateAsset(effect, path);
        Debug.Log($"✅ IceTileEffect creado en: {path}");
    }

    void ConfigureIceEffect(TileEffect effect)
    {
        effect.tileName = "ice";
        effect.effectType = TileEffectType.Ice;
        effect.speedMultiplier = 1.5f;
        effect.healthChange = 0;
        effect.effectDuration = 0f;
        effect.continuous = false;
        effect.tickRate = 0f;
    }

    void CreateMudTileEffect()
    {
        string path = "Assets/Data/TileEffects/MudTileEffect.asset";
        
        TileEffect existing = AssetDatabase.LoadAssetAtPath<TileEffect>(path);
        if (existing != null)
        {
            Debug.Log($"⚠️  MudTileEffect ya existe, actualizando...");
            ConfigureMudEffect(existing);
            EditorUtility.SetDirty(existing);
            return;
        }

        TileEffect effect = ScriptableObject.CreateInstance<TileEffect>();
        ConfigureMudEffect(effect);
        
        AssetDatabase.CreateAsset(effect, path);
        Debug.Log($"✅ MudTileEffect creado en: {path}");
    }

    void ConfigureMudEffect(TileEffect effect)
    {
        effect.tileName = "mud";
        effect.effectType = TileEffectType.Mud;
        effect.speedMultiplier = 0.5f;
        effect.healthChange = 0;
        effect.effectDuration = 1f; // Sigue lento 1 segundo después de salir
        effect.continuous = false;
        effect.tickRate = 0f;
    }

    void CreateHealTileEffect()
    {
        string path = "Assets/Data/TileEffects/HealTileEffect.asset";
        
        TileEffect existing = AssetDatabase.LoadAssetAtPath<TileEffect>(path);
        if (existing != null)
        {
            Debug.Log($"⚠️  HealTileEffect ya existe, actualizando...");
            ConfigureHealEffect(existing);
            EditorUtility.SetDirty(existing);
            return;
        }

        TileEffect effect = ScriptableObject.CreateInstance<TileEffect>();
        ConfigureHealEffect(effect);
        
        AssetDatabase.CreateAsset(effect, path);
        Debug.Log($"✅ HealTileEffect creado en: {path}");
    }

    void ConfigureHealEffect(TileEffect effect)
    {
        effect.tileName = "heal";
        effect.effectType = TileEffectType.Heal;
        effect.speedMultiplier = 1f;
        effect.healthChange = 1;
        effect.effectDuration = 0f;
        effect.continuous = true; // Cura continuamente mientras estás sobre él
        effect.tickRate = 0.5f; // Cura cada 0.5 segundos
    }

    void CreateDamageTileEffect()
    {
        string path = "Assets/Data/TileEffects/DamageTileEffect.asset";
        
        TileEffect existing = AssetDatabase.LoadAssetAtPath<TileEffect>(path);
        if (existing != null)
        {
            Debug.Log($"⚠️  DamageTileEffect ya existe, actualizando...");
            ConfigureDamageEffect(existing);
            EditorUtility.SetDirty(existing);
            return;
        }

        TileEffect effect = ScriptableObject.CreateInstance<TileEffect>();
        ConfigureDamageEffect(effect);
        
        AssetDatabase.CreateAsset(effect, path);
        Debug.Log($"✅ DamageTileEffect creado en: {path}");
    }

    void ConfigureDamageEffect(TileEffect effect)
    {
        effect.tileName = "damage";
        effect.effectType = TileEffectType.Damage;
        effect.speedMultiplier = 1f;
        effect.healthChange = -1;
        effect.effectDuration = 0f;
        effect.continuous = true;
        effect.tickRate = 0.5f; // Daño cada 0.5 segundos
    }

    void CreateSpeedUpTileEffect()
    {
        string path = "Assets/Data/TileEffects/SpeedUpTileEffect.asset";
        
        TileEffect existing = AssetDatabase.LoadAssetAtPath<TileEffect>(path);
        if (existing != null)
        {
            Debug.Log($"⚠️  SpeedUpTileEffect ya existe, actualizando...");
            ConfigureSpeedUpEffect(existing);
            EditorUtility.SetDirty(existing);
            return;
        }

        TileEffect effect = ScriptableObject.CreateInstance<TileEffect>();
        ConfigureSpeedUpEffect(effect);
        
        AssetDatabase.CreateAsset(effect, path);
        Debug.Log($"✅ SpeedUpTileEffect creado en: {path}");
    }

    void ConfigureSpeedUpEffect(TileEffect effect)
    {
        effect.tileName = "speedup";
        effect.effectType = TileEffectType.SpeedUp;
        effect.speedMultiplier = 1.5f;
        effect.healthChange = 0;
        effect.effectDuration = 0f;
        effect.continuous = false;
        effect.tickRate = 0f;
    }

    void CreateSpeedDownTileEffect()
    {
        string path = "Assets/Data/TileEffects/SpeedDownTileEffect.asset";
        
        TileEffect existing = AssetDatabase.LoadAssetAtPath<TileEffect>(path);
        if (existing != null)
        {
            Debug.Log($"⚠️  SpeedDownTileEffect ya existe, actualizando...");
            ConfigureSpeedDownEffect(existing);
            EditorUtility.SetDirty(existing);
            return;
        }

        TileEffect effect = ScriptableObject.CreateInstance<TileEffect>();
        ConfigureSpeedDownEffect(effect);
        
        AssetDatabase.CreateAsset(effect, path);
        Debug.Log($"✅ SpeedDownTileEffect creado en: {path}");
    }

    void ConfigureSpeedDownEffect(TileEffect effect)
    {
        effect.tileName = "slowdown";
        effect.effectType = TileEffectType.SpeedDown;
        effect.speedMultiplier = 0.7f;
        effect.healthChange = 0;
        effect.effectDuration = 2f; // Sigue lento 2 segundos después de salir
        effect.continuous = false;
        effect.tickRate = 0f;
    }

    void CreatePowerUpTileEffect()
    {
        string path = "Assets/Data/TileEffects/PowerupTileEffect.asset";
        
        TileEffect existing = AssetDatabase.LoadAssetAtPath<TileEffect>(path);
        if (existing != null)
        {
            Debug.Log($"⚠️  PowerUpTileEffect ya existe, actualizando...");
            ConfigurePowerUpEffect(existing);
            EditorUtility.SetDirty(existing);
            return;
        }

        TileEffect effect = ScriptableObject.CreateInstance<TileEffect>();
        ConfigurePowerUpEffect(effect);
        
        AssetDatabase.CreateAsset(effect, path);
        Debug.Log($"✅ PowerUpTileEffect creado en: {path}");
    }

    void ConfigurePowerUpEffect(TileEffect effect)
    {
        effect.tileName = "powerup";
        effect.effectType = TileEffectType.PowerUp;
        effect.speedMultiplier = 1f;
        effect.healthChange = 0;
        effect.effectDuration = 0f;
        effect.continuous = false;
        effect.tickRate = 0f;
    }
#endif
}
