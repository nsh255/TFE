using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    public int currentHealth;
    protected Transform player;
    private SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyData no asignado en {gameObject.name}");
            return;
        }
        currentHealth = enemyData.maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && enemyData.sprite != null)
        {
            spriteRenderer.sprite = enemyData.sprite;
        }
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    protected virtual void Update()
    {
        // Movimiento y lógica en hijos
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null && enemyData != null)
            {
                ph.TakeDamage(enemyData.damage);
            }
        }
    }
}
