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
    [Header("Escala Pixel Consistente")]
    [Tooltip("Tamaño objetivo en píxeles para proyectil enemigo (ancho)")] public int desiredPixelSize = 6;

    private void Start()
    {
        // Aplicar boost de daño si existe
        if (DamageBoostManager.Instance != null)
        {
            damage = DamageBoostManager.Instance.GetBoostedDamage(damage);
        }
        
        // Ajustar escala del proyectil según el pixelScale de la cámara
        AdjustProjectileScale();
        
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

    /// <summary>
    /// Ajusta la escala del proyectil enemigo según el pixelScale de la cámara principal
    /// </summary>
    private void AdjustProjectileScale()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;
        float ppu = sr.sprite.pixelsPerUnit; // esperado 16
        if (ppu > 64f) ppu = 16f; // Fallback para PPU anómalos
        float targetScale = desiredPixelSize / ppu; // p.ej. 6 / 16 = 0.375
        targetScale = Mathf.Max(targetScale, 0.0625f);
        transform.localScale = new Vector3(targetScale, targetScale, 1f);
        Debug.Log($"[EnemyProjectile] Escala fijada por pixels → {targetScale} (desiredPixelSize={desiredPixelSize}, ppu={ppu})");

        Camera cam = Camera.main;
        if (cam != null)
        {
            float worldVisibleHeight = cam.orthographicSize * 2f;
            float pixelsPerWorldUnit = Screen.height / worldVisibleHeight;
            float worldHeightUnits = (sr.sprite.rect.height / ppu) * targetScale;
            float onScreenPixelHeight = worldHeightUnits * pixelsPerWorldUnit;
            Debug.Log($"[EnemyProjectileSizeDebug] sprite={sr.sprite.name}, targetScale={targetScale:F4}, screenPixels≈{onScreenPixelHeight:F1}");
        }
    }
}
