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
    
    private float nextShootTime;

    private Vector2 lastMoveDirection;
    private float lastAppliedMoveSpeed;
    private bool isMoving;

    protected override void Start()
    {
        base.Start();
        
        nextShootTime = Time.time + shootCooldown;
        
        // Valores por defecto
        if (optimalShootDistance == 0) optimalShootDistance = 8f;
        if (escapeDistance == 0) escapeDistance = 4f;
        if (maxDistance == 0) maxDistance = 12f;
        if (moveSpeed == 0) moveSpeed = enemyData != null ? enemyData.speed : 2f;
        if (shootCooldown == 0) shootCooldown = 2f;
        if (projectileSpeed == 0) projectileSpeed = 5f;
        if (projectileDamage == 0) projectileDamage = 1;
        
        // Crear punto de disparo si no existe
        if (shootPoint == null)
        {
            GameObject sp = new GameObject("ShootPoint");
            sp.transform.SetParent(transform);
            sp.transform.localPosition = new Vector3(0.5f, 0, 0);
            shootPoint = sp.transform;
        }
    }

    protected override void Update()
    {
        base.Update();
        
        if (isDead || player == null) return;

        // Cooldown al spawnear: evita disparar instantáneo.
        if (!IsAIEnabled)
        {
            StopMoving();
            return;
        }

        // Calcular distancia al jugador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Decidir comportamiento según distancia
        if (distanceToPlayer < escapeDistance)
        {
            // MUY CERCA - Escapar
            Escape();
        }
        else if (distanceToPlayer > maxDistance)
        {
            // MUY LEJOS - Acercarse
            Approach();
        }
        else if (distanceToPlayer >= escapeDistance && distanceToPlayer <= maxDistance)
        {
            // EN RANGO ÓPTIMO - Disparar
            StopMoving();
            
            if (Time.time >= nextShootTime)
            {
                Shoot();
            }
        }
    }

    private void Escape()
    {
        // Huir en dirección opuesta al jugador
        Vector2 direction = (transform.position - player.position).normalized;

        // Velocidad de escape, pero nunca más rápido que el jugador
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

    private void Approach()
    {
        // Acercarse al jugador
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

    private float CapToPlayerSpeed(float desiredSpeed)
    {
        // Requisito: no puede ser más rápido que el player.
        if (playerController != null)
        {
            return Mathf.Min(desiredSpeed, playerController.CurrentSpeed);
        }
        return desiredSpeed;
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[SlimeGreen] No hay projectilePrefab asignado!");
            return;
        }
        
        // Calcular dirección hacia el jugador
        Vector2 direction = (player.position - shootPoint.position).normalized;
        
        // Crear proyectil
        GameObject projectile = Instantiate(
            projectilePrefab, 
            shootPoint.position, 
            Quaternion.identity
        );
        
        // Configurar proyectil
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * projectileSpeed;
        }
        
        // Configurar daño del proyectil
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.damage = projectileDamage;
        }
        
        // Trigger animación de ataque si existe
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        nextShootTime = Time.time + shootCooldown;
        
        Debug.Log($"[SlimeGreen] Disparó hacia el jugador");
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || player == null || !isMoving) return;
        if (collision.gameObject.CompareTag("Player")) return;

        if (!IsWallCollision(collision)) return;

        BounceFromWall(collision);
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        // Mantener daño al jugador del Enemy base
        base.OnCollisionStay2D(collision);

        if (isDead || player == null || !isMoving) return;
        if (collision.gameObject.CompareTag("Player")) return;
        if (!IsWallCollision(collision)) return;

        // Si seguimos empujando contra el muro, ajustar dirección otra vez
        if (rb != null && collision.contactCount > 0)
        {
            Vector2 normal = collision.GetContact(0).normal;
            if (Vector2.Dot(rb.linearVelocity, -normal) > 0.01f)
            {
                BounceFromWall(collision);
            }
        }
    }

    private bool IsWallCollision(Collision2D collision)
    {
        if (collision.collider == null) return false;
        if (collision.gameObject.layer == wallLayer) return true;
        // Fallback: si choca con TilemapCollider2D, asumir muro
        return collision.collider.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null;
    }

    private void BounceFromWall(Collision2D collision)
    {
        if (collision.contactCount == 0) return;

        Vector2 awayFromPlayer = (transform.position - player.position).normalized;
        Vector2 normal = collision.GetContact(0).normal;

        // Reflejar dirección actual y mezclar con "awayFromPlayer" para seguir escapando.
        Vector2 reflected = Vector2.Reflect(lastMoveDirection, normal).normalized;
        Vector2 candidate = (awayFromPlayer + reflected).normalized;
        if (candidate.sqrMagnitude < 0.0001f)
        {
            candidate = reflected.sqrMagnitude > 0.0001f ? reflected : awayFromPlayer;
        }

        // Reintentos: si el nuevo vector apunta claramente hacia el jugador, usar perpendicular del normal.
        for (int i = 0; i < wallBounceAttempts; i++)
        {
            Vector2 toPlayer = (player.position - transform.position).normalized;
            if (Vector2.Dot(candidate, toPlayer) < 0f) break; // ya va alejándose

            // Elegir una dirección tangencial al muro
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
