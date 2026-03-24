using UnityEngine;

/// <summary>
/// Arma cuerpo a cuerpo que ejecuta ataques mediante swing con hitbox activa.
/// Incluye animación de barrido y sistema de rastro visual.
/// </summary>
public class MeleeWeapon : Weapon
{
    public Collider2D hitbox;
    private WeaponHitbox[] weaponHitboxes;
    private bool hitboxesActive;

    public float attackCooldown = 0.5f;
    private float lastAttackTime = -999f;

    [Header("Swing Animation")]
    [Tooltip("Ángulo total del swing. Recomendado: 180° para barrido completo")]
    public float swingAngle = 0f;
    [Tooltip("Duración del swing en segundos. Recomendado: 0.3s")]
    public float swingDuration = 0f;
    
    private bool isSwinging = false;
    private float swingStartAngle;
    private float swingTimer;
    
    private WeaponTrail weaponTrail;

    public bool IsSwinging => isSwinging;
    public bool IsHitboxActive => hitboxesActive;

    /// <summary>
    /// Inicializa valores por defecto de animación si no están configurados.
    /// </summary>
    void Awake()
    {
        weaponHitboxes = GetComponentsInChildren<WeaponHitbox>(true);

        if (swingAngle == 0f)
        {
            swingAngle = 180f;
        }
        if (swingDuration == 0f)
        {
            swingDuration = 0.3f;
        }
    }

    /// <summary>
    /// Inicializa el arma y desactiva la hitbox inicialmente.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        if (hitbox == null)
        {
            var selfCollider = GetComponent<Collider2D>();
            var childColliders = GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < childColliders.Length; i++)
            {
                var candidate = childColliders[i];
                if (candidate == null || candidate == selfCollider) continue;
                if (candidate.GetComponentInParent<WeaponHitbox>() != null)
                {
                    hitbox = candidate;
                    break;
                }
            }
        }
        
        DisableHitbox();
        
        weaponTrail = GetComponent<WeaponTrail>();
        if (weaponTrail == null)
        {
            weaponTrail = gameObject.AddComponent<WeaponTrail>();
            weaponTrail.trailDuration = 0.15f;
            weaponTrail.trailInterval = 0.04f;
            weaponTrail.trailColor = new Color(1f, 1f, 1f, 0.5f);
            weaponTrail.trailScale = 1f;
        }
    }

    /// <summary>
    /// Actualiza la animación del swing y controla la activación de la hitbox.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // Detener swing si sueltas el botón
        if (!Input.GetMouseButton(0) && isSwinging)
        {
            StopSwinging();
            return;
        }
        
        if (isSwinging)
        {
            swingTimer += Time.deltaTime;
            float progress = swingTimer / swingDuration;
            
            if (progress >= 1f)
            {
                // Swing completado
                StopSwinging();
            }
            else
            {
                // Animar el swing
                float smoothProgress = Mathf.Sin(progress * Mathf.PI);
                float currentSwingOffset = Mathf.Lerp(-swingAngle / 2, swingAngle / 2, smoothProgress);
                
                Vector3 currentRotation = transform.localEulerAngles;
                currentRotation.z = swingStartAngle + currentSwingOffset;
                transform.localRotation = Quaternion.Euler(currentRotation);
            }
        }
    }

    /// <summary>
    /// Detiene el swing y desactiva inmediatamente la hitbox.
    /// </summary>
    private void StopSwinging()
    {
        isSwinging = false;
        DisableHitbox();

        if (weaponTrail != null)
        {
            weaponTrail.DisableTrail();
        }
    }

    private void OnDisable()
    {
        isSwinging = false;
        DisableHitbox();

        if (weaponTrail != null)
        {
            weaponTrail.DisableTrail();
        }
    }

    /// <summary>
    /// Ejecuta un ataque de barrido activando la hitbox durante el swing.
    /// </summary>
    public override void Attack()
    {
        PlayerController player = transform.parent?.GetComponent<PlayerController>();
        if (player != null && player.IsRolling)
        {
            return;
        }
        
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        if (isSwinging)
        {
            return;
        }

        if (hitbox == null)
        {
            Debug.LogWarning("No hay hitbox asignada en MeleeWeapon!");
            return;
        }

        // Limpiar estado previo
        hitboxesActive = false;
        DisableHitbox();

        lastAttackTime = Time.time;
        
        isSwinging = true;
        swingTimer = 0f;
        swingStartAngle = transform.localEulerAngles.z;
        
        // Activar hitbox solo después de limpiar
        EnableHitbox();
        
        if (weaponTrail != null)
        {
            weaponTrail.EnableTrail();
        }
    }

    /// <summary>
    /// Desactiva la hitbox del arma.
    /// </summary>
    private void DisableHitbox()
    {
        hitboxesActive = false;

        // Desactivar collider principal
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }

        // Desactivar todos los WeaponHitbox children
        if (weaponHitboxes != null)
        {
            for (int i = 0; i < weaponHitboxes.Length; i++)
            {
                if (weaponHitboxes[i] == null) continue;
                weaponHitboxes[i].DisableHitbox();
            }
        }
        
        // DEBUG: Log para verificar que la hitbox se desactivó
        if (false) // Cambiar a true para debug
        {
            Debug.Log($"[MeleeWeapon] Hitbox desactivada. isSwinging={isSwinging}, hitboxesActive={hitboxesActive}");
        }
    }

    private void EnableHitbox()
    {
        hitboxesActive = true;

        if (hitbox != null)
        {
            hitbox.enabled = true;
        }

        if (weaponHitboxes != null)
        {
            for (int i = 0; i < weaponHitboxes.Length; i++)
            {
                if (weaponHitboxes[i] == null) continue;
                weaponHitboxes[i].EnableHitbox();
            }
        }
    }

    /// <summary>
    /// Detecta colisiones con enemigos y aplica daño si el swing está activo.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (weaponHitboxes != null && weaponHitboxes.Length > 0) return;
        if (!isSwinging || !hitboxesActive) return;
        if (hitbox == null || !hitbox.enabled) return;

        if (other.CompareTag("Enemy") && weaponData != null)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponData.damage);
            }
        }
    }
}
