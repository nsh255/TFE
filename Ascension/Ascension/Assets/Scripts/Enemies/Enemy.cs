using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Configuración")]
    public EnemyData enemyData;
    
    [Header("Estado")]
    public int currentHealth;
    public bool isDead = false;
    
    protected Transform player;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;
    protected Animator animator;
    
    [Header("Daño al Jugador")]
    public float damageRate = 1f; // Daño por segundo al tocar al jugador
    private float lastDamageTime;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyData no asignado en {gameObject.name}");
            return;
        }
        
        currentHealth = enemyData.maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Configurar sprite inicial si no hay animación
        if (spriteRenderer != null && enemyData.sprite != null && animator == null)
        {
            spriteRenderer.sprite = enemyData.sprite;
        }
        
        Debug.Log($"[Enemy] {enemyData.enemyName} spawneado con {currentHealth} HP");
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        Debug.Log($"[Enemy] {enemyData.enemyName} recibió {amount} daño. HP: {currentHealth}/{enemyData.maxHealth}");
        
        // Efecto visual de daño (opcional)
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashDamage());
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log($"[Enemy] {enemyData.enemyName} murió");
        
        // Animación de muerte si existe
        if (animator != null)
        {
            animator.SetTrigger("Die");
            Destroy(gameObject, 0.5f); // Esperar a que termine la animación
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;
        
        // Movimiento y lógica en clases hijas
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            // Daño continuo al jugador
            if (Time.time >= lastDamageTime + damageRate)
            {
                PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
                if (ph != null && enemyData != null)
                {
                    ph.TakeDamage(enemyData.damage);
                    lastDamageTime = Time.time;
                    Debug.Log($"[Enemy] {enemyData.enemyName} dañó al jugador por {enemyData.damage}");
                }
            }
        }
    }
}
