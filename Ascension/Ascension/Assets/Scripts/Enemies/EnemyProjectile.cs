using UnityEngine;

/// <summary>
/// Proyectil disparado por enemigos (SlimeGreen)
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("Configuración")]
    public int damage = 1;
    
    [Tooltip("Tiempo antes de destruirse automáticamente")]
    public float lifetime = 5f;
    
    [Tooltip("Capa del jugador para detectar colisiones")]
    public LayerMask playerLayer;

    private void Start()
    {
        // Autodestrucción después de lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si impacta al jugador
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"[EnemyProjectile] Impactó al jugador por {damage} de daño");
            }
            
            // Destruir proyectil
            Destroy(gameObject);
        }
        // Si impacta cualquier otra cosa (paredes, etc.)
        else if (!collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si impacta al jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"[EnemyProjectile] Impactó al jugador por {damage} de daño");
            }
            
            // Destruir proyectil
            Destroy(gameObject);
        }
        // Si impacta cualquier otra cosa
        else if (!collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
