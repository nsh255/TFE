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
    [SerializeField, Min(0.1f)] private float minProjectileBaseSpeed = 5f;
    
    [Tooltip("Tiempo entre disparos")]
    public float shootCooldown = 2f;
    
    [Tooltip("Daño del proyectil")]
    public int projectileDamage = 1;

    [Tooltip("Multiplicador de velocidad de las bolas (1.15 = +15%)")]
    [SerializeField] private float projectileSpeedMultiplier = 1.15f;
    [Tooltip("Multiplicador de escala de las bolas (0.8 = -20%)")]
    [SerializeField] private float projectileScaleMultiplier = 0.8f;
    [SerializeField] private bool debugCombat = false;

    [Header("Boss Tweaks")]
    [Tooltip("Escala extra aplicada al proyectil instanciado (solo afecta a esta instancia, útil para boss grande).")]
    [SerializeField] private float bossProjectileScale = 1f;
    
    private float nextShootTime;

    private Vector2 lastMoveDirection;
    private float lastAppliedMoveSpeed;
    private bool isMoving;
    private bool warnedMissingAnimatorController;
    private Transform visualTransform;
    private Vector3 baseVisualScale;
    private float fallbackShootPulseUntil;

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
        if (projectileSpeed <= 0f) projectileSpeed = 5f;
        if (projectileDamage == 0) projectileDamage = 1;

        if (projectileSpeed < minProjectileBaseSpeed)
        {
            if (debugCombat)
            {
                Debug.LogWarning($"[SlimeGreen] projectileSpeed demasiado baja ({projectileSpeed:F2}) en '{name}'. Se normaliza a {minProjectileBaseSpeed:F2}.");
            }
            projectileSpeed = minProjectileBaseSpeed;
        }
        
        if (shootPoint == null)
        {
            GameObject sp = new GameObject("ShootPoint");
            sp.transform.SetParent(transform);
            sp.transform.localPosition = new Vector3(0.5f, 0, 0);
            shootPoint = sp.transform;
        }

        if (animator != null && !HasAnimatorController && !warnedMissingAnimatorController)
        {
            warnedMissingAnimatorController = true;
            Debug.LogWarning($"[SlimeGreen] Animator sin AnimatorController en '{name}'. Usando animación fallback por código.");
        }

        visualTransform = (spriteRenderer != null) ? spriteRenderer.transform : transform;
        baseVisualScale = visualTransform.localScale;
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

        UpdateFallbackAnimation();
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
        
        SetAnimatorMoving(true);
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
        
        SetAnimatorMoving(true);
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
        
        SetAnimatorMoving(false);
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

        float finalScaleMultiplier = projectileScaleMultiplier * bossProjectileScale;
        projectile.transform.localScale = projectile.transform.localScale * finalScaleMultiplier;
        
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        float safeBaseSpeed = Mathf.Max(projectileSpeed, minProjectileBaseSpeed);
        float finalProjectileSpeed = safeBaseSpeed * projectileSpeedMultiplier;
        if (projRb != null)
        {
            projRb.linearVelocity = direction * finalProjectileSpeed;
        }
        
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.damage = projectileDamage;
            projScript.externalScaleMultiplier = finalScaleMultiplier;
        }

        SetAnimatorDirection(direction);
        
        TriggerShootAnimation();
        
        nextShootTime = Time.time + shootCooldown;
        
        if (debugCombat)
        {
            float speedNow = projRb != null ? projRb.linearVelocity.magnitude : -1f;
            Debug.Log($"[SlimeGreen] Shoot dir={direction} speed={finalProjectileSpeed:F2} (rb={speedNow:F2}) scaleMul={finalScaleMultiplier:F2} dist={Vector2.Distance(transform.position, player.position):F2}");
        }
    }

    private void SetAnimatorMoving(bool moving)
    {
        isMoving = moving;
    }

    private void TriggerShootAnimation()
    {
        if (HasAnimatorController && animator != null)
        {
            SetAnimatorAttacking(true);
            CancelInvoke(nameof(ClearAttackFlag));
            Invoke(nameof(ClearAttackFlag), 0.18f);
            return;
        }

        fallbackShootPulseUntil = Time.time + 0.12f;
    }

    private void UpdateFallbackAnimation()
    {
        if (HasAnimatorController || visualTransform == null) return;

        Vector3 targetScale = baseVisualScale;

        if (isMoving)
        {
            float wobble = Mathf.Sin(Time.time * 16f) * 0.06f;
            targetScale = new Vector3(
                baseVisualScale.x * (1f + wobble),
                baseVisualScale.y * (1f - wobble * 0.8f),
                baseVisualScale.z
            );
        }

        if (Time.time < fallbackShootPulseUntil)
        {
            targetScale = new Vector3(
                targetScale.x * 1.14f,
                targetScale.y * 1.14f,
                targetScale.z
            );
        }

        visualTransform.localScale = Vector3.Lerp(visualTransform.localScale, targetScale, 18f * Time.deltaTime);
    }

    private void ClearAttackFlag()
    {
        SetAnimatorAttacking(false);
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
