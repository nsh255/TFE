using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador del jefe final con sistema de 2 fases.
/// Hereda de Enemy para reutilizar sistema de daño/muerte.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BossController : Enemy
{
    [Header("Boss Configuration")]
    [Tooltip("Vida total del boss")]
    [SerializeField] private int maxHealth = 100;
    
    [Tooltip("Umbral de vida para fase 2 (porcentaje)")]
    [SerializeField, Range(0f, 1f)] private float phase2Threshold = 0.5f;

    [Header("Phase 1 - Patrón Simple")]
    [Tooltip("Tiempo entre ataques en fase 1")]
    [SerializeField] private float phase1AttackInterval = 2f;
    
    [Tooltip("Velocidad de movimiento en fase 1")]
    [SerializeField] private float phase1MoveSpeed = 2f;
    
    [Tooltip("Proyectil para fase 1")]
    [SerializeField] private GameObject phase1Projectile;
    
    [Tooltip("Velocidad del proyectil fase 1")]
    [SerializeField] private float phase1ProjectileSpeed = 10f;

    [Header("Phase 2 - Patrón Intenso")]
    [Tooltip("Tiempo entre ataques en fase 2")]
    [SerializeField] private float phase2AttackInterval = 1f;
    
    [Tooltip("Velocidad de movimiento en fase 2")]
    [SerializeField] private float phase2MoveSpeed = 3.5f;
    
    [Tooltip("Proyectil para fase 2 (puede ser el mismo)")]
    [SerializeField] private GameObject phase2Projectile;
    
    [Tooltip("Velocidad del proyectil fase 2")]
    [SerializeField] private float phase2ProjectileSpeed = 13f;
    
    [Tooltip("Número de proyectiles en ráfaga (fase 2)")]
    [SerializeField] private int phase2BurstCount = 3;

    [Header("Puntos de Spawn de Proyectiles")]
    [Tooltip("Posición de spawn de proyectiles (relativa al boss)")]
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Estado")]
    [SerializeField] private BossPhase currentPhase = BossPhase.Phase1;
    [SerializeField] private bool isBossActive = false;

    // Referencias
    private Coroutine attackCoroutine;

    // Constantes
    private const float DETECTION_RANGE = 20f;

    protected override void Start()
    {
        base.Start();

        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        // Auto-crear spawn point si no existe
        if (projectileSpawnPoint == null)
        {
            GameObject spawnObj = new GameObject("ProjectileSpawnPoint");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = Vector3.zero;
            projectileSpawnPoint = spawnObj.transform;
        }

        // Buscar jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    protected override void Update()
    {
        if (isDead || player == null) return;

        // Activar boss si el jugador está cerca
        if (!isBossActive && Vector2.Distance(transform.position, player.position) < DETECTION_RANGE)
        {
            ActivateBoss();
        }

        if (isBossActive)
        {
            BossAI();
        }
    }

    /// <summary>
    /// Activa el boss al iniciar combate
    /// </summary>
    private void ActivateBoss()
    {
        isBossActive = true;
        Debug.Log("[BossController] ¡BOSS ACTIVADO!");

        // Iniciar rutina de ataque
        StartAttackPattern();
    }

    /// <summary>
    /// IA del boss según la fase actual
    /// </summary>
    private void BossAI()
    {
        float currentSpeed = currentPhase == BossPhase.Phase1 ? phase1MoveSpeed : phase2MoveSpeed;

        // Movimiento hacia el jugador
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * currentSpeed;

        // Rotar hacia el jugador
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    /// <summary>
    /// Inicia el patrón de ataque del boss
    /// </summary>
    private void StartAttackPattern()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        if (currentPhase == BossPhase.Phase1)
        {
            attackCoroutine = StartCoroutine(Phase1AttackPattern());
        }
        else
        {
            attackCoroutine = StartCoroutine(Phase2AttackPattern());
        }
    }

    /// <summary>
    /// Patrón de ataque fase 1: disparo simple
    /// </summary>
    private IEnumerator Phase1AttackPattern()
    {
        while (currentPhase == BossPhase.Phase1 && !isDead)
        {
            yield return new WaitForSeconds(phase1AttackInterval);

            if (player != null && phase1Projectile != null)
            {
                ShootProjectile(phase1Projectile, phase1ProjectileSpeed);
            }
        }
    }

    /// <summary>
    /// Patrón de ataque fase 2: ráfaga de proyectiles
    /// </summary>
    private IEnumerator Phase2AttackPattern()
    {
        while (currentPhase == BossPhase.Phase2 && !isDead)
        {
            yield return new WaitForSeconds(phase2AttackInterval);

            if (player != null && phase2Projectile != null)
            {
                // Disparar ráfaga
                for (int i = 0; i < phase2BurstCount; i++)
                {
                    ShootProjectile(phase2Projectile, phase2ProjectileSpeed);
                    yield return new WaitForSeconds(0.1f); // Pequeño delay entre disparos
                }
            }
        }
    }

    /// <summary>
    /// Dispara un proyectil hacia el jugador
    /// </summary>
    private void ShootProjectile(GameObject projectilePrefab, float speed)
    {
        if (projectilePrefab == null || player == null) return;

        Vector2 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        Vector2 direction = (player.position - (Vector3)spawnPos).normalized;

        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        
        // Configurar el proyectil
        Rigidbody2D projRb = projectileObj.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * speed;
        }
        
        // Configurar daño si tiene componente Projectile
        Projectile proj = projectileObj.GetComponent<Projectile>();
        if (proj != null && enemyData != null)
        {
            proj.damage = enemyData.damage;
        }
    }

    /// <summary>
    /// Recibe daño y verifica cambio de fase
    /// </summary>
    public override void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"[BossController] Boss recibió {damageAmount} daño. Vida: {currentHealth}/{maxHealth}");

        // Verificar cambio de fase
        if (currentPhase == BossPhase.Phase1 && currentHealth <= maxHealth * phase2Threshold)
        {
            EnterPhase2();
        }

        // Verificar muerte
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Entra en fase 2
    /// </summary>
    private void EnterPhase2()
    {
        Debug.Log("[BossController] ¡FASE 2 ACTIVADA!");
        currentPhase = BossPhase.Phase2;

        // Detener patrón de fase 1 e iniciar fase 2
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        StartAttackPattern();

        // Efecto visual opcional (cambiar color, animación, etc.)
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = Color.red;
        }
    }

    /// <summary>
    /// Muerte del boss
    /// </summary>
    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("[BossController] ¡BOSS DERROTADO!");

        // Detener ataques
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        rb.linearVelocity = Vector2.zero;

        // Animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Destruir después de un delay
        Destroy(gameObject, 2f);
    }

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        // Dibujar rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DETECTION_RANGE);

        // Dibujar punto de spawn de proyectiles
        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, 0.3f);
        }
    }

    #endregion

    #region Propiedades Públicas

    public BossPhase CurrentPhase => currentPhase;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsActive => isBossActive;

    #endregion
}

/// <summary>
/// Fases del boss
/// </summary>
public enum BossPhase
{
    Phase1,
    Phase2
}
