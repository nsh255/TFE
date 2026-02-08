using UnityEngine;

/// <summary>
/// Slime Verde - Dispara de lejos, escapa si te acercas, se acerca si está muy lejos
/// </summary>
public class SlimeGreen : Enemy
{
    [Header("Comportamiento Verde - Francotirador")]
    [Tooltip("Distancia óptima para disparar")]
    public float optimalShootDistance = 8f;
    
    [Tooltip("Distancia mínima (si el jugador se acerca más, escapa)")]
    public float escapeDistance = 4f;
    
    [Tooltip("Distancia máxima (si está más lejos, se acerca)")]
    public float maxDistance = 12f;
    
    [Tooltip("Velocidad de movimiento")]
    public float moveSpeed = 2f;

    [Header("Colisión con muros")]
    [Tooltip("Layer de muros (en GameScene, WallTilemap está en layer 10)")]
    public int wallLayer = 10;
    [Tooltip("Reintentos para corregir dirección al chocar")]
    public int wallBounceAttempts = 2;
    
    [Header("Disparo")]
    [Tooltip("Prefab del proyectil")]
    public GameObject projectilePrefab;
    
    [Tooltip("Punto desde donde dispara")]
    public Transform shootPoint;
    
    [Tooltip("Velocidad del proyectil")]
    public float projectileSpeed = 5f;
    
    [Tooltip("Tiempo entre disparos")]
    public float shootCooldown = 2f;
    
    [Tooltip("Daño del proyectil")]
    public int projectileDamage = 1;

    [Header("Boss Tweaks")]
    [Tooltip("Escala extra aplicada al proyectil instanciado (solo afecta a esta instancia, útil para boss grande).")]
    [SerializeField] private float bossProjectileScale = 1f;
    
    private float nextShootTime;

    private Vector2 lastMoveDirection;
    private float lastAppliedMoveSpeed;
    private bool isMoving;

    /// <summary>
    /// Inicializa el slime verde y establece valores por defecto.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        
        nextShootTime = Time.time + shootCooldown;
        
        if (optimalShootDistance == 0) optimalShootDistance = 8f;
        if (escapeDistance == 0) escapeDistance = 4f;
        if (maxDistance == 0) maxDistance = 12f;
        if (moveSpeed == 0) moveSpeed = enemyData != null ? enemyData.speed : 2f;
        if (shootCooldown == 0) shootCooldown = 2f;
        if (projectileSpeed == 0) projectileSpeed = 5f;
        if (projectileDamage == 0) projectileDamage = 1;
        
        if (shootPoint == null)
        {
            GameObject sp = new GameObject("ShootPoint");
            sp.transform.SetParent(transform);
            sp.transform.localPosition = new Vector3(0.5f, 0, 0);
            shootPoint = sp.transform;
        }
    }

    /// <summary>
    /// Actualiza el comportamiento de distancia y disparo cada frame.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        
        if (isDead || player == null) return;

        if (!IsAIEnabled)
        {
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < escapeDistance)
        {
            Escape();
        }
        else if (distanceToPlayer > maxDistance)
        {
            Approach();
        }
        else if (distanceToPlayer >= escapeDistance && distanceToPlayer <= maxDistance)
        {
            StopMoving();
            
            if (Time.time >= nextShootTime)
            {
                Shoot();
            }
        }
    }

    /// <summary>
    /// Huye en dirección opuesta al jugador (nunca más rápido que el jugador).
    /// </summary>
    private void Escape()
    {
        Vector2 direction = (transform.position - player.position).normalized;
        float desiredSpeed = (moveSpeed * TileSpeedMultiplier) * 1.5f;
        float cappedSpeed = CapToPlayerSpeed(desiredSpeed);

        lastMoveDirection = direction;
        lastAppliedMoveSpeed = cappedSpeed;
        isMoving = true;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * cappedSpeed;
        }
        else
        {
            transform.position += (Vector3)direction * cappedSpeed * Time.deltaTime;
        }
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }

    /// <summary>
    /// Se acerca al jugador (nunca más rápido que el jugador).
    /// </summary>
    private void Approach()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float desiredSpeed = moveSpeed * TileSpeedMultiplier;
        float cappedSpeed = CapToPlayerSpeed(desiredSpeed);

        lastMoveDirection = direction;
        lastAppliedMoveSpeed = cappedSpeed;
        isMoving = true;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * cappedSpeed;
        }
        else
        {
            transform.position += (Vector3)direction * cappedSpeed * Time.deltaTime;
        }
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }

    /// <summary>
    /// Detiene el movimiento del slime.
    /// </summary>
    private void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        isMoving = false;
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }
    }

    /// <summary>
    /// Limita la velocidad para no exceder la del jugador.
    /// </summary>
    private float CapToPlayerSpeed(float desiredSpeed)
    {
        if (playerController != null)
        {
            float playerSpeed = playerController.CurrentSpeed;
            if (playerSpeed > 0f)
            {
                return Mathf.Min(desiredSpeed, playerSpeed);
            }
        }

        return desiredSpeed;
    }

    /// <summary>
    /// Dispara un proyectil hacia el jugador.
    /// </summary>
    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[SlimeGreen] No hay projectilePrefab asignado!");
            return;
        }

        Vector2 direction = (player.position - shootPoint.position).normalized;
        
        GameObject projectile = Instantiate(
            projectilePrefab, 
            shootPoint.position, 
            Quaternion.identity
        );

        if (bossProjectileScale != 1f)
        {
            projectile.transform.localScale = projectile.transform.localScale * bossProjectileScale;
        }
        
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * projectileSpeed;
        }
        
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.damage = projectileDamage;
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        nextShootTime = Time.time + shootCooldown;
        
        Debug.Log($"[SlimeGreen] Disparó hacia el jugador");
    }

    public float BossProjectileScale
    {
        get => bossProjectileScale;
        set => bossProjectileScale = Mathf.Clamp(value, 0.25f, 5f);
    }


    /// <summary>
    /// Maneja rebote contra muros al colisionar.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || player == null || !isMoving) return;
        if (collision.gameObject.CompareTag("Player")) return;

        if (!IsWallCollision(collision)) return;

        BounceFromWall(collision);
    }

    /// <summary>
    /// Ajusta dirección si sigue empujando contra el muro.
    /// </summary>
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        base.OnCollisionStay2D(collision);

        if (isDead || player == null || !isMoving) return;
        if (collision.gameObject.CompareTag("Player")) return;
        if (!IsWallCollision(collision)) return;

        if (rb != null && collision.contactCount > 0)
        {
            Vector2 normal = collision.GetContact(0).normal;
            if (Vector2.Dot(rb.linearVelocity, -normal) > 0.01f)
            {
                BounceFromWall(collision);
            }
        }
    }

    /// <summary>
    /// Determina si la colisión es con un muro.
    /// </summary>
    private bool IsWallCollision(Collision2D collision)
    {
        if (collision.collider == null) return false;
        if (collision.gameObject.layer == wallLayer) return true;
        return collision.collider.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null;
    }

    /// <summary>
    /// Rebota desde el muro eligiendo una nueva dirección de escape.
    /// </summary>
    private void BounceFromWall(Collision2D collision)
    {
        if (collision.contactCount == 0) return;

        Vector2 awayFromPlayer = (transform.position - player.position).normalized;
        Vector2 normal = collision.GetContact(0).normal;

        Vector2 reflected = Vector2.Reflect(lastMoveDirection, normal).normalized;
        Vector2 candidate = (awayFromPlayer + reflected).normalized;
        if (candidate.sqrMagnitude < 0.0001f)
        {
            candidate = reflected.sqrMagnitude > 0.0001f ? reflected : awayFromPlayer;
        }

        for (int i = 0; i < wallBounceAttempts; i++)
        {
            Vector2 toPlayer = (player.position - transform.position).normalized;
            if (Vector2.Dot(candidate, toPlayer) < 0f) break;

            candidate = new Vector2(-normal.y, normal.x).normalized;
            if (Vector2.Dot(candidate, awayFromPlayer) < 0f) candidate = -candidate;
        }

        lastMoveDirection = candidate;

        if (rb != null)
        {
            rb.linearVelocity = lastMoveDirection * lastAppliedMoveSpeed;
        }
    }
}
