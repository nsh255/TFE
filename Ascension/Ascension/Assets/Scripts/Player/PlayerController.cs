using System.Collections;
using UnityEngine;

/// <summary>
/// Controla el movimiento, animación y comportamiento del jugador.
/// Gestiona la inicialización basada en la clase seleccionada y el sistema de esquiva.
/// </summary>
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private new Rigidbody2D rigidbody;
    private Vector3 velocity;
    private bool isInvulnerable = false;

    public PlayerClass playerClass;

    [Header("Movimiento")]
    [SerializeField] private float speed;
    private float baseSpeed;

    public float BaseSpeed => baseSpeed > 0f ? baseSpeed : speed;
    public float CurrentSpeed => speed;

    [Header("Weapon")]
    [Tooltip("Offset del arma respecto al jugador")]
    public Vector3 weaponOffset = Vector3.zero;
    [Tooltip("Si está activo, el offset del arma se escala con el tamaño del jugador")]
    public bool scaleWeaponOffsetWithPlayer = true;
    [Tooltip("Multiplicador adicional para afinar el offset del arma")]
    public float weaponOffsetScale = 1f;

    [Header("Roll")]
    public float rollSpeedMultiplier = 2f;
    public float rollCooldown = 0.6f;
    public KeyCode rollKey = KeyCode.Space;
    public bool invulnerableDuringRoll = true;

    private bool isRolling = false;
    private bool canRoll = true;
    private Vector2 lastInputDirection = Vector2.up;
    
    public bool IsRolling => isRolling;

    private PlayerHealth playerHealth;
    private bool isInitialized = false;

    /// <summary>
    /// Inicializa componentes críticos y configuración de físicas.
    /// </summary>
    void Awake()
    {
        EnsureCriticalLayerCollisions();

        if (weaponOffset == Vector3.zero)
        {
            weaponOffset = new Vector3(0.8f, 0.5f, 0f);
        }
        
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();

        if (rigidbody != null)
        {
            rigidbody.gravityScale = 0f;
            rigidbody.freezeRotation = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    /// <summary>
    /// Asegura que las colisiones críticas entre capas estén habilitadas.
    /// </summary>
    private void EnsureCriticalLayerCollisions()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int wallLayer = LayerMask.NameToLayer("Wall");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int entitiesLayer = LayerMask.NameToLayer("Entities");

        if (playerLayer >= 0 && wallLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, wallLayer, false);

        if (entitiesLayer >= 0 && wallLayer >= 0)
            Physics2D.IgnoreLayerCollision(entitiesLayer, wallLayer, false);

        if (enemyLayer >= 0 && wallLayer >= 0)
            Physics2D.IgnoreLayerCollision(enemyLayer, wallLayer, false);
    }

    /// <summary>
    /// Inicializa el jugador si ya tiene una clase asignada.
    /// </summary>
    void Start()
    {
        if (playerClass != null && !isInitialized)
        {
            Initialize();
        }
    }

    /// <summary>
    /// Inicializa las estadísticas del jugador basadas en la clase seleccionada.
    /// Instancia el arma inicial y configura la salud.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;
        
        if (playerClass == null)
        {
            Debug.LogError("No se puede inicializar PlayerController sin una playerClass asignada!");
            return;
        }

        baseSpeed = playerClass.speed;
        speed = baseSpeed;

        if (playerClass.startingWeaponData != null)
        {
            if (playerClass.startingWeaponData.weaponPrefab != null)
            {
                GameObject weaponInstance = Instantiate(
                    playerClass.startingWeaponData.weaponPrefab,
                    transform.position,
                    Quaternion.identity,
                    transform
                );

                Vector3 finalOffset;
                if (scaleWeaponOffsetWithPlayer)
                {
                    float uniformScale = Mathf.Max(transform.localScale.x, transform.localScale.y);
                    finalOffset = weaponOffset * uniformScale * weaponOffsetScale;
                }
                else
                {
                    finalOffset = weaponOffset * weaponOffsetScale;
                }
                
                weaponInstance.transform.localPosition = finalOffset;

                Weapon weaponScript = weaponInstance.GetComponent<Weapon>();
                if (weaponScript != null)
                {
                    weaponScript.weaponData = playerClass.startingWeaponData;
                    weaponScript.Initialize();
                }

                var playerSr = GetComponent<SpriteRenderer>();
                if (playerSr != null)
                {
                    foreach (var sr in weaponInstance.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        sr.sortingLayerID = playerSr.sortingLayerID;
                        sr.sortingOrder = Mathf.Max(sr.sortingOrder, playerSr.sortingOrder);
                    }
                }
            }
        }

        if (playerHealth != null)
        {
            playerHealth.Initialize();
        }

        isInitialized = true;
        LogPlayerVisualSize();
    }

    /// <summary>
    /// Aplica un multiplicador de velocidad al movimiento base.
    /// </summary>
    /// <param name="multiplier">Factor multiplicador.</param>
    public void SetSpeedMultiplier(float multiplier)
    {
        speed = BaseSpeed * multiplier;
    }

    /// <summary>
    /// Restaura la velocidad al valor base de la clase.
    /// </summary>
    public void ResetSpeed()
    {
        speed = BaseSpeed;
    }

    /// <summary>
    /// Procesa la entrada del jugador y actualiza la animación de movimiento.
    /// </summary>
    void Update()
    {
        if (isRolling)
        {
            return;
        }

        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(hor, ver);

        if (input != Vector2.zero)
        {
            lastInputDirection = input.normalized;
            animator.SetFloat("Horizontal", input.x);
            animator.SetFloat("Vertical", input.y);
            animator.SetFloat("Speed", 1);
            Vector3 direction = (Vector3.up * input.y + Vector3.right * input.x).normalized;
            velocity = direction * speed;
        }
        else
        {
            animator.SetFloat("Speed", 0);
            velocity = Vector3.zero;
        }

        if (canRoll && Input.GetKeyDown(rollKey))
        {
            Vector2 rollDir = input != Vector2.zero ? input.normalized : lastInputDirection;
            StartCoroutine(DoRoll(rollDir));
        }
    }

    /// <summary>
    /// Aplica el movimiento del jugador mediante físicas.
    /// </summary>
    void FixedUpdate()
    {
        if (!isRolling)
        {
            if (rigidbody != null)
            {
                rigidbody.MovePosition(rigidbody.position + (Vector2)(velocity * Time.fixedDeltaTime));
            }
        }
    }
    
    /// <summary>
    /// Establece el estado de invulnerabilidad del jugador.
    /// </summary>
    /// <param name="value">True para activar invulnerabilidad, false para desactivarla.</param>
    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
    }
    
    /// <summary>
    /// Verifica si el jugador es actualmente invulnerable.
    /// </summary>
    /// <returns>True si el jugador es invulnerable.</returns>
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    /// <summary>
    /// Ejecuta la animación y movimiento de esquiva del jugador.
    /// </summary>
    /// <param name="direction">Dirección normalizada de la esquiva.</param>
    private IEnumerator DoRoll(Vector2 direction)
    {
        isRolling = true;
        canRoll = false;

        animator.SetBool("IsRolling", true);
        animator.SetFloat("RollHorizontal", direction.x);
        animator.SetFloat("RollVertical", direction.y);

        yield return null;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float clipLength = state.length;

        if (invulnerableDuringRoll)
        {
            SetInvulnerable(true);
        }

        // Desactivar colisiones con enemigos y proyectiles durante el roll
        EnablePhaseThrough(true);

        float elapsed = 0f;
        while (elapsed < clipLength)
        {
            Vector3 delta = (Vector3)(direction * speed * rollSpeedMultiplier) * Time.fixedDeltaTime;
            rigidbody.MovePosition(rigidbody.position + (Vector2)delta);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Reactivar colisiones
        EnablePhaseThrough(false);

        if (invulnerableDuringRoll)
        {
            SetInvulnerable(false);
        }

        animator.SetBool("IsRolling", false);
        isRolling = false;

        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    /// <summary>
    /// Activa o desactiva la capacidad de atravesar enemigos durante el roll.
    /// </summary>
    /// <param name="enable">True para atravesar enemigos, false para colisiones normales.</param>
    private void EnablePhaseThrough(bool enable)
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return;

        // Ignorar colisiones con todos los enemigos activos
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
                if (enemyCollider != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, enemyCollider, enable);
                }
            }
        }

        // Ignorar colisiones con proyectiles enemigos
        EnemyProjectile[] projectiles = FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
        foreach (EnemyProjectile projectile in projectiles)
        {
            if (projectile != null)
            {
                Collider2D projectileCollider = projectile.GetComponent<Collider2D>();
                if (projectileCollider != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, projectileCollider, enable);
                }
            }
        }
    }

    /// <summary>
    /// Calcula y registra el tamaño visual del jugador en píxeles en pantalla.
    /// Utilizado para depuración y ajustes de escala.
    /// </summary>
    private void LogPlayerVisualSize()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Camera mainCam = Camera.main;
        if (sr == null || sr.sprite == null || mainCam == null)
        {
            return;
        }
        float ppu = sr.sprite.pixelsPerUnit;
        float spritePixelHeight = sr.sprite.rect.height;
        float worldHeightUnits = (spritePixelHeight / ppu) * transform.localScale.y;

        float worldVisibleHeight = mainCam.orthographicSize * 2f;
        float pixelsPerWorldUnit = Screen.height / worldVisibleHeight;
        float onScreenPixelHeight = worldHeightUnits * pixelsPerWorldUnit;

        Debug.Log($"[PlayerSizeDebug] Sprite={sr.sprite.name}, localScale={transform.localScale}, worldHeightUnits={worldHeightUnits:F4}, screenPixels≈{onScreenPixelHeight:F1}");
    }
}
