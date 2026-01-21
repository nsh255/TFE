using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("Offset del arma respecto al jugador. Se aplica automáticamente (5, 0, 0) si está en (0,0,0)")]
    public Vector3 weaponOffset = Vector3.zero; // Se inicializa en Awake
    [Tooltip("Si está activo, el offset del arma se escala con el tamaño del jugador (uniforme).")]
    public bool scaleWeaponOffsetWithPlayer = true;
    [Tooltip("Multiplicador adicional para afinar el offset del arma tras el escalado.")]
    public float weaponOffsetScale = 1f;

    [Header("Roll")]
    public float rollSpeedMultiplier = 2f;
    public float rollCooldown = 0.6f;
    public KeyCode rollKey = KeyCode.Space;
    public bool invulnerableDuringRoll = true;

    private bool isRolling = false;
    private bool canRoll = true;
    private Vector2 lastInputDirection = Vector2.up;
    
    // Propiedad pública para que las armas puedan verificar si estamos en roll
    public bool IsRolling => isRolling;

    private PlayerHealth playerHealth;
    private bool isInitialized = false;

    void Awake()
    {
        EnsureCriticalLayerCollisions();

        // SIEMPRE aplicar valores si está en (0,0,0)
        if (weaponOffset == Vector3.zero)
        {
            // Offset reducido para que rote cerca de la cabeza del jugador
            weaponOffset = new Vector3(0.8f, 0.5f, 0f);
        }
        
        // Inicializar referencias de componentes en Awake para que estén listas antes de Initialize()
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();

        // Hardening de físicas: evita atravesar colliders por tunneling.
        if (rigidbody != null)
        {
            rigidbody.gravityScale = 0f;
            rigidbody.freezeRotation = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    private void EnsureCriticalLayerCollisions()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int wallLayer = LayerMask.NameToLayer("Wall");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int entitiesLayer = LayerMask.NameToLayer("Entities");

        // Si el proyecto tiene capas configuradas, aseguramos que NO estén ignoradas.
        if (playerLayer >= 0 && wallLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, wallLayer, false);

        // Útil si en algún punto migramos player/enemies a Entities.
        if (entitiesLayer >= 0 && wallLayer >= 0)
            Physics2D.IgnoreLayerCollision(entitiesLayer, wallLayer, false);

        if (enemyLayer >= 0 && wallLayer >= 0)
            Physics2D.IgnoreLayerCollision(enemyLayer, wallLayer, false);
    }

    void Start()
    {
        // Si playerClass ya está asignado (para testing directo), inicializar
        if (playerClass != null && !isInitialized)
        {
            Initialize();
        }
    }

    // Método público para inicializar después de asignar playerClass
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

        // Instanciar el arma según el WeaponData de la clase
        if (playerClass.startingWeaponData != null)
        {
            if (playerClass.startingWeaponData.weaponPrefab != null)
            {
                GameObject weaponInstance = Instantiate(
                    playerClass.startingWeaponData.weaponPrefab,
                    transform.position,
                    Quaternion.identity,
                    transform // parent al jugador
                );

                // Aplicar el offset del arma, ajustando por el scale del jugador si corresponde
                Vector3 finalOffset;
                if (scaleWeaponOffsetWithPlayer)
                {
                    // Asumimos escala uniforme; tomamos el mayor eje para evitar distorsión
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
                else
                {
                    Debug.LogWarning("Weapon script not found on the weapon prefab.");
                }

                // Asegurar que el arma se renderiza por encima del suelo.
                var playerSr = GetComponent<SpriteRenderer>();
                if (playerSr != null)
                {
                    foreach (var sr in weaponInstance.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        // Si el prefab viene en Default, lo subimos a la capa de Entities.
                        sr.sortingLayerID = playerSr.sortingLayerID;
                        sr.sortingOrder = Mathf.Max(sr.sortingOrder, playerSr.sortingOrder);
                    }
                }
            }
            else
            {
                Debug.LogWarning("No weaponPrefab assigned in WeaponData for class: " + playerClass.className);
            }
        }
        else
        {
            Debug.LogWarning("No startingWeaponData assigned to PlayerClass: " + playerClass.className);
        }

        // Inicializar PlayerHealth después de que playerClass esté asignado
        if (playerHealth != null)
        {
            Debug.Log("[PlayerController] Inicializando PlayerHealth...");
            playerHealth.Initialize();
        }
        else
        {
            Debug.LogError("[PlayerController] playerHealth es NULL! No se puede inicializar. Buscando componente...");
            playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log("[PlayerController] PlayerHealth encontrado, inicializando...");
                playerHealth.Initialize();
            }
            else
            {
                Debug.LogError("[PlayerController] PlayerHealth NO existe en el prefab del jugador!");
            }
        }

        isInitialized = true;
        Debug.Log($"PlayerController inicializado con clase: {playerClass.className}");

        LogPlayerVisualSize();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speed = BaseSpeed * multiplier;
    }

    public void ResetSpeed()
    {
        speed = BaseSpeed;
    }


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
    
    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
    }
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

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

        float elapsed = 0f;
        while (elapsed < clipLength)
        {
            Vector3 delta = (Vector3)(direction * speed * rollSpeedMultiplier) * Time.fixedDeltaTime;
            rigidbody.MovePosition(rigidbody.position + (Vector2)delta);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

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
    /// Calcula y muestra en Debug el tamaño visual del jugador en píxeles en pantalla.
    /// Útil para ajustar pixelScale / escala del sprite.
    /// </summary>
    private void LogPlayerVisualSize()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Camera mainCam = Camera.main;
        if (sr == null || sr.sprite == null || mainCam == null)
        {
            Debug.LogWarning("[PlayerSizeDebug] Faltan referencias para calcular tamaño visual.");
            return;
        }
        float ppu = sr.sprite.pixelsPerUnit; // debería ser 16
        float spritePixelHeight = sr.sprite.rect.height; // normalmente 16
        float worldHeightUnits = (spritePixelHeight / ppu) * transform.localScale.y;

        float worldVisibleHeight = mainCam.orthographicSize * 2f; // unidades mundo visibles verticalmente
        float pixelsPerWorldUnit = Screen.height / worldVisibleHeight;
        float onScreenPixelHeight = worldHeightUnits * pixelsPerWorldUnit;

        Debug.Log($"[PlayerSizeDebug] Sprite={sr.sprite.name}, localScale={transform.localScale}, worldHeightUnits={worldHeightUnits:F4}, screenPixels≈{onScreenPixelHeight:F1}");
    }
}
