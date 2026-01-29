using UnityEngine;

/// <summary>
/// Slime Rojo - Persigue al jugador para hacer daño por contacto
/// </summary>
public class SlimeRed : Enemy
{
    [Header("Comportamiento Rojo - Perseguidor")]
    [Tooltip("Velocidad de movimiento hacia el jugador")]
    public float moveSpeed = 3f;
    
    [Tooltip("Distancia mínima al jugador (para no pegarse demasiado)")]
    public float minDistance = 0.5f;

    /// <summary>
    /// Inicializa el slime rojo y establece valores por defecto.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        
        if (moveSpeed == 0) moveSpeed = enemyData != null ? enemyData.speed : 3f;
    }

    /// <summary>
    /// Actualiza el comportamiento de persecución cada frame.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        
        if (isDead || player == null) return;

        if (!IsAIEnabled)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetBool("IsMoving", false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > minDistance)
        {
            ChasePlayer();
        }
        
        FlipSprite();
    }

    /// <summary>
    /// Persigue al jugador aplicando velocidad hacia su posición.
    /// </summary>
    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * (moveSpeed * TileSpeedMultiplier);
        }
        else
        {
            transform.position = Vector2.MoveTowards(
                transform.position, 
                player.position, 
                (moveSpeed * TileSpeedMultiplier) * Time.deltaTime
            );
        }
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }

    /// <summary>
    /// Voltea el sprite según la posición del jugador.
    /// </summary>
    private void FlipSprite()
    {
        if (spriteRenderer != null && player != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }

    /// <summary>
    /// Detiene el movimiento al colisionar con el jugador.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb != null && collision.gameObject.CompareTag("Player"))
        {
            rb.linearVelocity = Vector2.zero;
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }
}
