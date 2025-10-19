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
    private float speed;

    [Header("Weapon")]
    [Tooltip("Offset del arma respecto al jugador. Se aplica automáticamente (5, 0, 0) si está en (0,0,0)")]
    public Vector3 weaponOffset = Vector3.zero; // Se inicializa en Awake

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
        // SIEMPRE aplicar valores si está en (0,0,0)
        if (weaponOffset == Vector3.zero)
        {
            // Radio ~5.0 con altura moderada para órbita tipo Tiny Rogues
            weaponOffset = new Vector3(4.5f, 2.5f, 0f); // Radio ≈ 5.15 unidades
            Debug.Log($"WeaponOffset inicializado a (4.5, 2.5, 0) - Radio: {weaponOffset.magnitude}");
        }
    }

    void Start()
    {
        // Solo inicializar componentes básicos
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        
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

        speed = playerClass.speed;

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

                // Aplicar el offset del arma
                weaponInstance.transform.localPosition = weaponOffset;

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
            playerHealth.Initialize();
        }

        isInitialized = true;
        Debug.Log($"PlayerController inicializado con clase: {playerClass.className}");
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
            rigidbody.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
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
}
