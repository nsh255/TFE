using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public float speed = 10f;
    public float lifetime = 3f;
    [Header("Escala Pixel Consistente")]
    [Tooltip("Tamaño objetivo en píxeles que debe ocupar el proyectil (ancho) sobre su sprite base de 16x16")] public int desiredPixelSize = 8;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Aplicar boost de daño si existe
        if (DamageBoostManager.Instance != null)
        {
            damage = DamageBoostManager.Instance.GetBoostedDamage(damage);
        }
        
        // Ajustar escala del proyectil según el pixelScale de la cámara
        AdjustProjectileScale();
        
        // Hacer el proyectil más visible
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.yellow; // Amarillo brillante
            Debug.Log($"Projectile SpriteRenderer encontrado, color: {sr.color}, sprite: {sr.sprite?.name ?? "NULL"}");
        }
        else
        {
            Debug.LogWarning("Projectile no tiene SpriteRenderer!");
        }
        
        Debug.Log($"Projectile.Start() - Posición: {transform.position}, Speed: {speed}, Damage: {damage}, Escala: {transform.localScale}");
        
        if (rb != null)
        {
            // Mover el proyectil en la dirección que apunta
            // Usamos transform.up porque el sprite apunta hacia arriba (ajustado con offset -90°)
            rb.linearVelocity = transform.up * speed;
            Debug.Log($"Velocidad aplicada: {rb.linearVelocity}, Dirección: {transform.up}");
        }
        else
        {
            Debug.LogError("Projectile no tiene Rigidbody2D!");
        }

        // Destruir después del tiempo de vida
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar colisión con el jugador
        if (other.CompareTag("Player"))
        {
            return;
        }

        // Aplicar daño a enemigos
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Proyectil golpeó a {other.name} con {damage} de daño");
            }
        }

        // Destruir el proyectil al impactar con paredes o enemigos
        if (other.CompareTag("Wall") || other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Ajusta la escala del proyectil según el pixelScale de la cámara principal
    /// para que se mantenga proporcional al zoom del juego
    /// </summary>
    private void AdjustProjectileScale()
    {
        // Queremos que el proyectil ocupe desiredPixelSize píxeles sobre un sprite (normalmente 16x16).
        // WorldScale = desiredPixelSize / pixelsPerUnitSprite
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;
        float ppu = sr.sprite.pixelsPerUnit; // debería ser 16
        if (ppu > 64f) ppu = 16f; // Fallback para sprites importados con PPU gigante
        float targetScale = desiredPixelSize / ppu; // p.ej. 8 / 16 = 0.5
        // Clamp para evitar invisibilidad
        targetScale = Mathf.Max(targetScale, 0.0625f);
        transform.localScale = new Vector3(targetScale, targetScale, 1f);
        Debug.Log($"[Projectile] Escala fijada por pixels → {targetScale} (desiredPixelSize={desiredPixelSize}, ppu={ppu})");

        // Calcular tamaño aproximado en pantalla
        Camera cam = Camera.main;
        if (cam != null)
        {
            float worldVisibleHeight = cam.orthographicSize * 2f;
            float pixelsPerWorldUnit = Screen.height / worldVisibleHeight;
            float worldHeightUnits = (sr.sprite.rect.height / ppu) * targetScale; // normalmente (16/16)*scale = scale
            float onScreenPixelHeight = worldHeightUnits * pixelsPerWorldUnit;
            Debug.Log($"[ProjectileSizeDebug] sprite={sr.sprite.name}, targetScale={targetScale:F4}, screenPixels≈{onScreenPixelHeight:F1}");
        }
    }
}
