using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public float speed = 10f;
    public float lifetime = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
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
}
