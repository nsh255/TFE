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
        
        if (rb != null)
        {
            // Mover el proyectil en la dirección que apunta
            rb.linearVelocity = transform.right * speed;
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
