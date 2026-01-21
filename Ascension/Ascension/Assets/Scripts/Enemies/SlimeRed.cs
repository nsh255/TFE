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

    protected override void Start()
    {
        base.Start();
        
        // Valores por defecto
        if (moveSpeed == 0) moveSpeed = enemyData != null ? enemyData.speed : 3f;
    }

    protected override void Update()
    {
        base.Update();
        
        if (isDead || player == null) return;

        // Cooldown al spawnear: evita persecución/daño instantáneo.
        if (!IsAIEnabled)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetBool("IsMoving", false);
            return;
        }

        // Calcular distancia al jugador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Perseguir al jugador si está lejos
        if (distanceToPlayer > minDistance)
        {
            ChasePlayer();
        }
        
        // Flip sprite según dirección
        FlipSprite();
    }

    private void ChasePlayer()
    {
        // Calcular dirección hacia el jugador
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Mover hacia el jugador
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
        
        // Animación de movimiento
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }

    private void FlipSprite()
    {
        if (spriteRenderer != null && player != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Parar movimiento al chocar
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
