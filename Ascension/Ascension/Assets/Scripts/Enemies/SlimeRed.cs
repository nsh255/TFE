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

    [Tooltip("Distancia para entrar en modo ataque")]
    [SerializeField] private float attackEnterDistance = 1.1f;

    [Tooltip("Distancia para salir del modo ataque (debe ser mayor o igual que Enter)")]
    [SerializeField] private float attackExitDistance = 1.4f;

    [SerializeField] private bool debugCombat = false;
    private bool inAttackMode;

    /// <summary>
    /// Inicializa el slime rojo y establece valores por defecto.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        
        if (moveSpeed == 0) moveSpeed = enemyData != null ? enemyData.speed : 3f;

        if (attackEnterDistance <= 0f)
        {
            attackEnterDistance = Mathf.Max(0.9f, minDistance);
        }

        if (attackExitDistance < attackEnterDistance)
        {
            attackExitDistance = attackEnterDistance + 0.25f;
        }
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
            SetAnimatorAttacking(false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 toPlayer = (player.position - transform.position).normalized;
        SetAnimatorDirection(toPlayer);

        bool shouldAttack = inAttackMode
            ? distanceToPlayer <= attackExitDistance
            : distanceToPlayer <= attackEnterDistance;

        if (!shouldAttack)
        {
            inAttackMode = false;
            ChasePlayer();
            SetAnimatorAttacking(false);
        }
        else
        {
            inAttackMode = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            SetAnimatorDirection(toPlayer);
            SetAnimatorAttacking(true);
            if (debugCombat)
            {
                Debug.Log($"[SlimeRed] Attacking dir={toPlayer} dist={distanceToPlayer:F2} enter={attackEnterDistance:F2} exit={attackExitDistance:F2}");
            }
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
        
        SetAnimatorAttacking(false);
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
            inAttackMode = true;
            SetAnimatorAttacking(true);
        }
    }
}
