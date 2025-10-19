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
        
        // Flip sprite según dirección
        FlipSprite();
    }

    private void Escape()
    {
        // Huir en dirección opuesta al jugador
        Vector2 direction = (transform.position - player.position).normalized;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed * 1.5f; // Más rápido al escapar
        }
        else
        {
            transform.position += (Vector3)direction * moveSpeed * 1.5f * Time.deltaTime;
        }
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
        
        Debug.Log("[SlimeGreen] Escapando del jugador");
    }

    private void Approach()
    {
        // Acercarse al jugador
        Vector2 direction = (player.position - transform.position).normalized;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
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
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }
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

    private void FlipSprite()
    {
        if (spriteRenderer != null && player != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }
}
