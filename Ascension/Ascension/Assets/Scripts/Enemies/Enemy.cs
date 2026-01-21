using UnityEngine;

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
    public float damageRate = 1f; // Daño por segundo al tocar al jugador
    private float lastDamageTime;

    [Header("Spawn Cooldown")]
    [Tooltip("Tiempo tras spawnear durante el cual no puede dañar al jugador.")]
    [SerializeField] private float contactDamageGraceSeconds = 0.75f;
    [Tooltip("Tiempo tras spawnear durante el cual no debe hacer acciones agresivas (dash/disparo/persecución).")]
    [SerializeField] private float aiGraceSeconds = 0.75f;

    private float contactDamageEnabledTime;
    private float aiEnabledTime;

    protected bool IsAIEnabled => Time.time >= aiEnabledTime;

    [Header("Tile Effects")]
    [SerializeField, Min(0.1f)] private float tileSpeedMultiplier = 1f;
    public float TileSpeedMultiplier => tileSpeedMultiplier;

    public void SetTileSpeedMultiplier(float multiplier)
    {
        tileSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 5f);
    }

    public void ResetTileSpeedMultiplier()
    {
        tileSpeedMultiplier = 1f;
    }

    protected virtual void Awake()
    {
        // Algunos prefabs tienen SpriteRenderer/Animator en hijos.
        allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null && allSpriteRenderers != null && allSpriteRenderers.Length > 0)
        {
            spriteRenderer = allSpriteRenderers[0];
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>(true);

        // Top-down defaults: evitar que los enemigos caigan o roten por físicas.
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        // Si algún prefab viene con Sorting Layer "Default", queda por debajo del Tilemap de suelo.
        // Ajuste defensivo: solo toca renderers que estén en Default.
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
        
        // Configurar sprite inicial si:
        // - no hay Animator, o
        // - hay Animator pero NO tiene controller asignado (caso común en prefabs incompletos)
        bool animatorHasController = animator != null && animator.runtimeAnimatorController != null;
        if (spriteRenderer != null && enemyData.sprite != null && !animatorHasController)
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
        
        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            int scoreValue = enemyData != null ? enemyData.damage * 5 : 10; // Score basado en daño del enemigo
            GameManager.Instance.NotifyEnemyKilled(scoreValue);
        }
        
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

        // El Player puede spawnear después que los enemigos (orden de ejecución).
        // Reintentar encontrarlo de forma throttled para que la IA no se quede "ciega".
        if (player == null && Time.time >= nextPlayerFindTime)
        {
            nextPlayerFindTime = Time.time + 0.5f;
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
            }
        }

        // Por defecto, los enemigos miran hacia el jugador cuando lo tienen detectado.
        UpdateFacingToPlayer();
        
        // Movimiento y lógica en clases hijas
    }

    protected void UpdateFacingToPlayer()
    {
        if (player == null) return;

        Vector2 toPlayer = (player.position - transform.position);
        if (toPlayer.sqrMagnitude < 0.0001f) return;
        Vector2 dir = toPlayer.normalized;

        // Sprite flip básico (para sprites laterales). Aplicar a todos los renderers.
        if (allSpriteRenderers != null && allSpriteRenderers.Length > 0)
        {
            // Deadzone para evitar vibración cuando está alineado en X.
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

            // Espejar puntos de disparo/spawn para que el ataque salga del lado correcto.
            MirrorNamedChildPoints(flipX);
        }

        // Si el animator tiene parámetros de dirección, rellenarlos.
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

    private void MirrorNamedChildPoints(bool flipX)
    {
        // Cacheamos una vez las posiciones base.
        mirroredPointBaseLocalPos ??= new System.Collections.Generic.Dictionary<Transform, Vector3>();

        // Buscar puntos típicos usados por enemigos que disparan.
        var points = GetComponentsInChildren<Transform>(true);
        if (points == null || points.Length == 0) return;

        float sign = flipX ? -1f : 1f;

        for (int i = 0; i < points.Length; i++)
        {
            var t = points[i];
            if (t == null || t == transform) continue;

            string n = t.name;
            if (string.IsNullOrEmpty(n)) continue;

            // Solo nombres típicos para evitar tocar transforms sin querer.
            if (!n.Contains("ShootPoint") && !n.Contains("SpawnPoint") && !n.Contains("ProjectileSpawnPoint"))
                continue;

            if (!mirroredPointBaseLocalPos.TryGetValue(t, out var basePos))
            {
                basePos = t.localPosition;
                mirroredPointBaseLocalPos[t] = basePos;
            }

            // Espejar solo el eje X manteniendo magnitud.
            float absX = Mathf.Abs(basePos.x);
            t.localPosition = new Vector3(absX * sign, basePos.y, basePos.z);
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time < contactDamageEnabledTime) return;

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
