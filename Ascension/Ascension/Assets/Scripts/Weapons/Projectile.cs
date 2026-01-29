using UnityEngine;

/// <summary>
/// Proyectil que se mueve en línea recta y daña enemigos al impactar.
/// Se destruye automáticamente tras su tiempo de vida o al colisionar con paredes o enemigos.
/// </summary>
public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public float speed = 10f;
    public float lifetime = 3f;
    
    [Header("Escala Pixel Consistente")]
    [Tooltip("Tamaño objetivo en píxeles que debe ocupar el proyectil sobre su sprite base de 16x16")]
    public int desiredPixelSize = 8;

    private Rigidbody2D rb;

    /// <summary>
    /// Inicializa el proyectil, ajusta su escala y aplica velocidad.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (DamageBoostManager.Instance != null)
        {
            damage = DamageBoostManager.Instance.GetBoostedDamage(damage);
        }
        
        AdjustProjectileScale();
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.yellow;
        }
        
        if (rb != null)
        {
            rb.linearVelocity = transform.up * speed;
        }

        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Detecta colisiones con enemigos y paredes, aplicando daño y destruyéndose.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        if (other.CompareTag("Wall") || other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Ajusta la escala del proyectil según el pixelScale de la cámara principal.
    /// </summary>
    private void AdjustProjectileScale()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;
        
        float ppu = sr.sprite.pixelsPerUnit;
        if (ppu > 64f) ppu = 16f;
        float targetScale = desiredPixelSize / ppu;
        targetScale = Mathf.Max(targetScale, 0.0625f);
        transform.localScale = new Vector3(targetScale, targetScale, 1f);
    }
}
