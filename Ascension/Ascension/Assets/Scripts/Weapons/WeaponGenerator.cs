using UnityEngine;

/// <summary>
/// Generador procedural de armas con stats aleatorios.
/// Crea WeaponData con rareza y modificadores según la profundidad del run.
/// </summary>
public class WeaponGenerator : MonoBehaviour
{
    [Header("Base Weapons")]
    [Tooltip("Armas base desde las cuales generar variantes")]
    [SerializeField] private WeaponData[] baseWeapons;

    [Header("Rarities Configuration")]
    [SerializeField] private RarityConfig commonConfig = new RarityConfig
    {
        rarityName = "Común",
        color = Color.gray,
        damageMultiplier = 1f,
        attackSpeedMultiplier = 1f,
        dropChance = 0.6f
    };

    [SerializeField] private RarityConfig uncommonConfig = new RarityConfig
    {
        rarityName = "Poco Común",
        color = Color.green,
        damageMultiplier = 1.3f,
        attackSpeedMultiplier = 1.15f,
        dropChance = 0.25f
    };

    [SerializeField] private RarityConfig rareConfig = new RarityConfig
    {
        rarityName = "Raro",
        color = Color.blue,
        damageMultiplier = 1.6f,
        attackSpeedMultiplier = 1.3f,
        dropChance = 0.12f
    };

    [SerializeField] private RarityConfig legendaryConfig = new RarityConfig
    {
        rarityName = "Legendario",
        color = new Color(1f, 0.5f, 0f), // Naranja
        damageMultiplier = 2f,
        attackSpeedMultiplier = 1.5f,
        dropChance = 0.03f
    };

    /// <summary>
    /// Genera un arma aleatoria según la profundidad del run
    /// </summary>
    public WeaponData GenerateWeapon(int depth)
    {
        if (baseWeapons == null || baseWeapons.Length == 0)
        {
            Debug.LogError("[WeaponGenerator] No hay armas base configuradas");
            return null;
        }

        // Elegir arma base aleatoria
        WeaponData baseWeapon = baseWeapons[Random.Range(0, baseWeapons.Length)];

        // Determinar rareza según profundidad y probabilidades
        WeaponRarity rarity = DetermineRarity(depth);

        // Crear nueva instancia de WeaponData
        WeaponData generatedWeapon = ScriptableObject.CreateInstance<WeaponData>();
        
        // Obtener configuración de rareza
        RarityConfig rarityConfig = GetRarityConfig(rarity);

        // Aplicar stats base con modificadores de rareza y profundidad
        float depthMultiplier = 1f + (depth * 0.1f); // +10% por nivel

        generatedWeapon.weaponName = $"{rarityConfig.rarityName} {baseWeapon.weaponName}";
        generatedWeapon.damage = Mathf.RoundToInt(baseWeapon.damage * rarityConfig.damageMultiplier * depthMultiplier);
        generatedWeapon.atackSpeed = baseWeapon.atackSpeed * rarityConfig.attackSpeedMultiplier;
        generatedWeapon.sprite = baseWeapon.sprite;
        generatedWeapon.bulletPrefab = baseWeapon.bulletPrefab;
        generatedWeapon.weaponPrefab = baseWeapon.weaponPrefab;

        Debug.Log($"[WeaponGenerator] Generada: {generatedWeapon.weaponName} | Daño: {generatedWeapon.damage} | Vel: {generatedWeapon.atackSpeed}");

        return generatedWeapon;
    }

    /// <summary>
    /// Determina la rareza según profundidad y RNG
    /// </summary>
    private WeaponRarity DetermineRarity(int depth)
    {
        // Aumentar probabilidad de rarezas altas según profundidad
        float depthBonus = depth * 0.02f; // +2% por nivel

        float roll = Random.value;
        float cumulativeChance = 0f;

        // Legendario
        cumulativeChance += legendaryConfig.dropChance + depthBonus;
        if (roll <= cumulativeChance) return WeaponRarity.Legendary;

        // Raro
        cumulativeChance += rareConfig.dropChance + depthBonus;
        if (roll <= cumulativeChance) return WeaponRarity.Rare;

        // Poco común
        cumulativeChance += uncommonConfig.dropChance;
        if (roll <= cumulativeChance) return WeaponRarity.Uncommon;

        // Común (por defecto)
        return WeaponRarity.Common;
    }

    /// <summary>
    /// Obtiene la configuración de una rareza
    /// </summary>
    private RarityConfig GetRarityConfig(WeaponRarity rarity)
    {
        return rarity switch
        {
            WeaponRarity.Common => commonConfig,
            WeaponRarity.Uncommon => uncommonConfig,
            WeaponRarity.Rare => rareConfig,
            WeaponRarity.Legendary => legendaryConfig,
            _ => commonConfig
        };
    }

    /// <summary>
    /// Spawnea un arma en el mundo
    /// </summary>
    public void SpawnWeaponDrop(Vector2 position, int depth)
    {
        WeaponData weapon = GenerateWeapon(depth);
        if (weapon == null) return;

        // Crear un objeto visual para el drop
        GameObject dropObj = new GameObject($"WeaponDrop_{weapon.weaponName}");
        dropObj.transform.position = position;

        // Añadir sprite
        SpriteRenderer sprite = dropObj.AddComponent<SpriteRenderer>();
        sprite.sprite = weapon.sprite;
        sprite.sortingLayerName = "Items";

        // Añadir collider
        BoxCollider2D collider = dropObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        // Añadir componente de pickup
        WeaponPickup pickup = dropObj.AddComponent<WeaponPickup>();
        pickup.weaponData = weapon;

        Debug.Log($"[WeaponGenerator] Arma spawneada en {position}");
    }

    #region Testing

    [ContextMenu("Test Generate Common Weapon")]
    private void TestGenerateCommon()
    {
        WeaponData weapon = GenerateWeapon(1);
        Debug.Log($"Test: {weapon.weaponName}");
    }

    [ContextMenu("Test Generate High Level Weapon")]
    private void TestGenerateHighLevel()
    {
        WeaponData weapon = GenerateWeapon(10);
        Debug.Log($"Test: {weapon.weaponName} (Nivel 10)");
    }

    #endregion
}

/// <summary>
/// Configuración de rareza
/// </summary>
[System.Serializable]
public class RarityConfig
{
    public string rarityName;
    public Color color;
    [Range(1f, 3f)] public float damageMultiplier;
    [Range(1f, 2f)] public float attackSpeedMultiplier;
    [Range(0f, 1f)] public float dropChance;
}

/// <summary>
/// Niveles de rareza de armas
/// </summary>
public enum WeaponRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}
