using UnityEngine;

/// <summary>
/// Spawner genérico de proyectiles para enemigos
/// Similar al sistema del Player Mago
/// </summary>
public class EnemyProjectileSpawner : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    [Tooltip("Prefab del proyectil a instanciar")]
    public GameObject projectilePrefab;
    
    [Tooltip("Velocidad del proyectil")]
    public float projectileSpeed = 5f;
    
    [Tooltip("Daño del proyectil")]
    public int projectileDamage = 1;
    
    [Tooltip("Tiempo de vida del proyectil")]
    public float projectileLifetime = 3f;
    
    [Header("Spawn Point")]
    [Tooltip("Punto desde donde se instancian los proyectiles (opcional, si no se asigna usa la posición del spawner)")]
    public Transform spawnPoint;
    
    private void Start()
    {
        // Si no hay spawn point asignado, usar la posición de este GameObject
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }
    
    /// <summary>
    /// Dispara un proyectil en la dirección especificada
    /// </summary>
    /// <param name="direction">Dirección normalizada del proyectil</param>
    /// <returns>El proyectil instanciado</returns>
    public GameObject SpawnProjectile(Vector2 direction)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"[EnemyProjectileSpawner] {gameObject.name}: No hay projectilePrefab asignado!");
            return null;
        }
        
        // Instanciar proyectil
        GameObject projectile = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.identity
        );
        
        // Configurar Rigidbody2D
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction.normalized * projectileSpeed;
        }
        
        // Configurar script del proyectil
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.damage = projectileDamage;
            projScript.lifetime = projectileLifetime;
        }
        
        // Rotar el proyectil para que apunte en la dirección del movimiento (opcional)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        return projectile;
    }
    
    /// <summary>
    /// Dispara un proyectil hacia un objetivo específico
    /// </summary>
    /// <param name="target">Transform del objetivo</param>
    /// <returns>El proyectil instanciado</returns>
    public GameObject SpawnProjectileTowards(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning($"[EnemyProjectileSpawner] {gameObject.name}: Target es null!");
            return null;
        }
        
        Vector2 direction = (target.position - spawnPoint.position).normalized;
        return SpawnProjectile(direction);
    }
    
    /// <summary>
    /// Dispara un proyectil hacia una posición específica
    /// </summary>
    /// <param name="targetPosition">Posición objetivo</param>
    /// <returns>El proyectil instanciado</returns>
    public GameObject SpawnProjectileTowards(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - spawnPoint.position).normalized;
        return SpawnProjectile(direction);
    }
}
