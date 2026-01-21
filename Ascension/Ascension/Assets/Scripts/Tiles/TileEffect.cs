using UnityEngine;

/// <summary>
/// Define el tipo de efecto que tiene un tile del suelo.
/// Usado por FloorTileManager para aplicar efectos al jugador.
/// </summary>
public enum TileEffectType
{
    None,           // Tile normal sin efecto
    Heal,           // Cura al jugador
    Damage,         // Daña al jugador
    SpeedUp,        // Aumenta velocidad
    SpeedDown,      // Reduce velocidad
    Ice,            // Resbaladizo (reduce fricción)
    Mud,            // Lento (reduce velocidad temporalmente)
    PowerUp         // Aumenta el daño del jugador
}

/// <summary>
/// ScriptableObject que define las propiedades de un tipo de tile.
/// Asigna uno de estos a cada Tile en el Inspector.
/// </summary>
[CreateAssetMenu(menuName = "Tiles/TileEffect")]
public class TileEffect : ScriptableObject
{
    [Header("Identificación")]
    public string tileName;
    public TileEffectType effectType = TileEffectType.None;
    
    [Header("Efectos Numéricos")]
    [Tooltip("Cantidad de HP que cura (positivo) o daña (negativo)")]
    public int healthChange = 0;
    
    [Tooltip("Multiplicador de velocidad mientras está sobre el tile (1 = normal, 1.5 = +50%, 0.5 = -50%)")]
    public float speedMultiplier = 1f;
    
    [Tooltip("Duración del efecto después de salir del tile (0 = solo mientras está encima)")]
    public float effectDuration = 0f;
    
    [Header("Aplicación")]
    [Tooltip("Tiempo entre aplicaciones del efecto (ej. daño cada 0.5s)")]
    public float tickRate = 1f;
    
    [Tooltip("Si es false, el efecto solo se aplica una vez al entrar")]
    public bool continuous = false;
    
    [Header("Visual")]
    public Color tintColor = Color.white;
    
    [Tooltip("Partículas opcionales al pisar el tile")]
    public GameObject vfxPrefab;
}
