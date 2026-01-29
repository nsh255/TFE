using UnityEngine;

/// <summary>
/// Clase base para todos los enemigos del juego.
/// Gestiona la salud, movimiento, daño al jugador y efectos de tiles.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Configuración")]
    public EnemyData enemyData;
    
    [Header("Estado")]
    public int currentHealth;
    public bool isDead = false;
    
    protected Transform player;
    protected PlayerController playerController;
    protected SpriteRenderer spriteRenderer;
    protected SpriteRenderer[] allSpriteRenderers;
    protected Rigidbody2D rb;
    protected Animator animator;

    private float nextPlayerFindTime;
    private bool hasFacingFlip;
    private bool lastFlipX;
    private System.Collections.Generic.Dictionary<Transform, Vector3> mirroredPointBaseLocalPos;

    private bool hasAnimHorizontal;
    private bool hasAnimVertical;
    private bool hasAnimMoveX;
    private bool hasAnimMoveY;
    private bool hasAnimLookX;
    private bool hasAnimLookY;
    
    [Header("Daño al Jugador")]
    public float damageRate = 1f;
    private float lastDamageTime;

    [Header("Spawn Cooldown")]
    [Tooltip("Tiempo tras spawnear durante el cual no puede dañar al jugador")]
    [SerializeField] private float contactDamageGraceSeconds = 0.75f;
    [Tooltip("Tiempo tras spawnear durante el cual no debe hacer acciones agresivas")]
    [SerializeField] private float aiGraceSeconds = 0.75f;

    private float contactDamageEnabledTime;
    private float aiEnabledTime;

    protected bool IsAIEnabled => Time.time >= aiEnabledTime;

    [Header("Tile Effects")]
    [SerializeField, Min(0.1f)] private float tileSpeedMultiplier = 1f;
    public float TileSpeedMultiplier => tileSpeedMultiplier;

    /// <summary>
    /// Establece el multiplicador de velocidad por efectos de tiles.
    /// </summary>
    /// <param name="multiplier">Multiplicador de velocidad (limitado entre 0.1 y 5).</param>
    public void SetTileSpeedMultiplier(float multiplier)
    {
        tileSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 5f);
    }

    /// <summary>
    /// Restaura el multiplicador de velocidad al valor base.
    /// </summary>
    public void ResetTileSpeedMultiplier()
    {
        tileSpeedMultiplier = 1f;
    }

    /// <summary>
    /// Inicializa componentes y configura físicas básicas del enemigo.
    /// </summary>
    protected virtual void Awake()
    {
        allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null && allSpriteRenderers != null && allSpriteRenderers.Length > 0)
        {
            spriteRenderer = allSpriteRenderers[0];
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>(true);

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        int entitiesSortingLayerId = SortingLayer.NameToID("Entities");
        if (entitiesSortingLayerId != 0)
        {
            foreach (var sr in allSpriteRenderers)
            {
                if (sr == null) continue;
                if (sr.sortingLayerID == 0)
                {
                    sr.sortingLayerID = entitiesSortingLayerId;
                }
            }
        }

        if (animator != null)
        {
            foreach (var p in animator.parameters)
            {
                if (p.type != AnimatorControllerParameterType.Float) continue;
                if (p.name == "Horizontal") hasAnimHorizontal = true;
                else if (p.name == "Vertical") hasAnimVertical = true;
                else if (p.name == "MoveX") hasAnimMoveX = true;
                else if (p.name == "MoveY") hasAnimMoveY = true;
                else if (p.name == "LookX") hasAnimLookX = true;
                else if (p.name == "LookY") hasAnimLookY = true;
            }
        }
    }

    /// <summary>
    /// Inicializa el estado del enemigo y busca al jugador en la escena.
    /// </summary>
    protected virtual void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyData no asignado en {gameObject.name}");
            return;
        }
        
        currentHealth = enemyData.maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        float now = Time.time;
        contactDamageEnabledTime = now + Mathf.Max(0f, contactDamageGraceSeconds);
        aiEnabledTime = now + Mathf.Max(0f, aiGraceSeconds);

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
        
        bool animatorHasController = animator != null && animator.runtimeAnimatorController != null;
        if (spriteRenderer != null && enemyData.sprite != null && !animatorHasController)
        {
            spriteRenderer.sprite = enemyData.sprite;
        }
    }

    /// <summary>
    /// Aplica daño al enemigo y procesa su muerte si la salud llega a cero.
    /// </summary>
    /// <param name="amount">Cantidad de daño a recibir.</param>
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashDamage());
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Muestra un breve efecto visual de flash rojo al recibir daño.
    /// </summary>
    private System.Collections.IEnumerator FlashDamage()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    /// <summary>
    /// Procesa la muerte del enemigo, notifica al GameManager y destruye el objeto.
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if (GameManager.Instance != null)
        {
            int scoreValue = enemyData != null ? enemyData.damage * 5 : 10;
            GameManager.Instance.NotifyEnemyKilled(scoreValue);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
            Destroy(gameObject, 0.5f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Actualiza el estado del enemigo buscando al jugador y actualizando su orientación.
    /// </summary>
    protected virtual void Update()
    {
        if (isDead) return;

        if (player == null && Time.time >= nextPlayerFindTime)
        {
            nextPlayerFindTime = Time.time + 0.5f;
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
            }
        }

        UpdateFacingToPlayer();
    }

    /// <summary>
    /// Actualiza la orientación del sprite y parámetros del animator para mirar hacia el jugador.
    /// </summary>
    protected void UpdateFacingToPlayer()
    {
        if (player == null) return;

        Vector2 toPlayer = (player.position - transform.position);
        if (toPlayer.sqrMagnitude < 0.0001f) return;
        Vector2 dir = toPlayer.normalized;

        if (allSpriteRenderers != null && allSpriteRenderers.Length > 0)
        {
            bool flipX;
            const float deadzone = 0.05f;
            if (dir.x > deadzone) flipX = false;
            else if (dir.x < -deadzone) flipX = true;
            else flipX = hasFacingFlip ? lastFlipX : (player.position.x < transform.position.x);

            hasFacingFlip = true;
            lastFlipX = flipX;

            foreach (var sr in allSpriteRenderers)
            {
                if (sr == null) continue;
                sr.flipX = flipX;
            }

            MirrorNamedChildPoints(flipX);
        }

        if (animator != null)
        {
            if (hasAnimHorizontal) animator.SetFloat("Horizontal", dir.x);
            if (hasAnimVertical) animator.SetFloat("Vertical", dir.y);
            if (hasAnimMoveX) animator.SetFloat("MoveX", dir.x);
            if (hasAnimMoveY) animator.SetFloat("MoveY", dir.y);
            if (hasAnimLookX) animator.SetFloat("LookX", dir.x);
            if (hasAnimLookY) animator.SetFloat("LookY", dir.y);
        }
    }

    /// <summary>
    /// Espeja los puntos de spawn de proyectiles para que apunten correctamente al voltear el sprite.
    /// </summary>
    /// <param name="flipX">True si el sprite está volteado horizontalmente.</param>
    private void MirrorNamedChildPoints(bool flipX)
    {
        mirroredPointBaseLocalPos ??= new System.Collections.Generic.Dictionary<Transform, Vector3>();

        var points = GetComponentsInChildren<Transform>(true);
        if (points == null || points.Length == 0) return;

        float sign = flipX ? -1f : 1f;

        for (int i = 0; i < points.Length; i++)
        {
            var t = points[i];
            if (t == null || t == transform) continue;

            string n = t.name;
            if (string.IsNullOrEmpty(n)) continue;

            if (!n.Contains("ShootPoint") && !n.Contains("SpawnPoint") && !n.Contains("ProjectileSpawnPoint"))
                continue;

            if (!mirroredPointBaseLocalPos.TryGetValue(t, out var basePos))
            {
                basePos = t.localPosition;
                mirroredPointBaseLocalPos[t] = basePos;
            }

            float absX = Mathf.Abs(basePos.x);
            t.localPosition = new Vector3(absX * sign, basePos.y, basePos.z);
        }
    }

    /// <summary>
    /// Aplica daño continuo al jugador mientras permanece en contacto con el enemigo.
    /// </summary>
    /// <param name="collision">Información de la colisión.</param>
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time < contactDamageEnabledTime) return;

            if (Time.time >= lastDamageTime + damageRate)
            {
                PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
                if (ph != null && enemyData != null)
                {
                    ph.TakeDamage(enemyData.damage);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}
