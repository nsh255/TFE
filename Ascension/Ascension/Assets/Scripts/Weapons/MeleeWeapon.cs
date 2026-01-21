using UnityEngine;

public class MeleeWeapon : Weapon
{
    // Collider que actúa como la "hitbox" del arma.
    public Collider2D hitbox;

    // Cooldown entre ataques
    public float attackCooldown = 0.5f;
    private float lastAttackTime = -999f;

    [Header("Swing Animation")]
    [Tooltip("Ángulo total del swing. Recomendado: 180° para barrido completo")]
    public float swingAngle = 0f; // Se inicializa en Awake si está en 0
    [Tooltip("Duración del swing en segundos. Recomendado: 0.3s")]
    public float swingDuration = 0f; // Se inicializa en Awake si está en 0
    
    private bool isSwinging = false;
    private float swingStartAngle;
    private float swingTimer;
    
    private WeaponTrail weaponTrail;

    public bool IsSwinging => isSwinging;

    void Awake()
    {
        // Aplicar valores por defecto si no están configurados en el prefab
        if (swingAngle == 0f)
        {
            swingAngle = 180f; // Barrido amplio
        }
        if (swingDuration == 0f)
        {
            swingDuration = 0.3f; // Duración media
        }
    }

    protected override void Start()
    {
        // IMPORTANTE: Llamar al Start() de la clase base para inicializar sprite, cámara, etc.
        base.Start();
        
        // Asegurarse de que el hitbox esté desactivado al inicio
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
        
        // Obtener o añadir componente de rastro
        weaponTrail = GetComponent<WeaponTrail>();
        if (weaponTrail == null)
        {
            weaponTrail = gameObject.AddComponent<WeaponTrail>();
            // Configurar valores por defecto del rastro
            weaponTrail.trailDuration = 0.15f;
            weaponTrail.trailInterval = 0.04f;
            weaponTrail.trailColor = new Color(1f, 1f, 1f, 0.5f); // Blanco semi-transparente
            weaponTrail.trailScale = 1f;
        }
    }

    protected override void Update()
    {
        // Llamar al Update de la clase base (rotación y input)
        base.Update();

        // Si el jugador NO está haciendo click, el arma no debe poder hacer daño.
        // Cancelamos el swing activo al soltar el botón para evitar hitbox persistente.
        if (!Input.GetMouseButton(0) && isSwinging)
        {
            isSwinging = false;
            DisableHitbox();

            if (weaponTrail != null)
            {
                weaponTrail.DisableTrail();
            }
        }
        
        // Manejar la animación del swing
        if (isSwinging)
        {
            swingTimer += Time.deltaTime;
            float progress = swingTimer / swingDuration;
            
            if (progress >= 1f)
            {
                // Swing terminado
                isSwinging = false;
                DisableHitbox();
                
                // Desactivar rastro
                if (weaponTrail != null)
                {
                    weaponTrail.DisableTrail();
                }
            }
            else
            {
                // Interpolar el ángulo del swing
                // Usamos una curva para que se vea más natural
                float smoothProgress = Mathf.Sin(progress * Mathf.PI);
                float currentSwingOffset = Mathf.Lerp(-swingAngle / 2, swingAngle / 2, smoothProgress);
                
                // Aplicar el offset del swing a la rotación actual
                Vector3 currentRotation = transform.localEulerAngles;
                currentRotation.z = swingStartAngle + currentSwingOffset;
                transform.localRotation = Quaternion.Euler(currentRotation);
            }
        }
        else
        {
            // Capa extra de seguridad: si no estamos swingueando, hitbox siempre off.
            DisableHitbox();
        }
    }

    // Se sobreescribe el método Attack() para que active la hitbox.
    public override void Attack()
    {
        // NO PERMITIR ATACAR SI EL JUGADOR ESTÁ EN ROLL
        PlayerController player = transform.parent?.GetComponent<PlayerController>();
        if (player != null && player.IsRolling)
        {
            return; // Bloqueado durante roll
        }
        
        // Verificar cooldown
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        if (isSwinging)
        {
            return; // No permitir atacar durante un swing
        }

        if (hitbox == null)
        {
            Debug.LogWarning("No hay hitbox asignada en MeleeWeapon!");
            return;
        }

        lastAttackTime = Time.time;
        
        // Iniciar el swing
        isSwinging = true;
        swingTimer = 0f;
        swingStartAngle = transform.localEulerAngles.z;
        
        // Activa la hitbox durante todo el swing
        hitbox.enabled = true;
        
        // Activar rastro visual
        if (weaponTrail != null)
        {
            weaponTrail.EnableTrail();
        }
        
        Debug.Log($"¡Swing iniciado! Daño: {weaponData?.damage ?? 0}");
    }

    private void DisableHitbox()
    {
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
    }

    // Método que se activa cuando la hitbox colisiona con algo.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Seguridad: si no estamos atacando (y click presionado), NO hacer daño.
        if (!isSwinging) return;
        if (!Input.GetMouseButton(0)) return;
        if (hitbox == null || !hitbox.enabled) return;

        // Aplicar daño al enemigo
        if (other.CompareTag("Enemy") && weaponData != null)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponData.damage);
                Debug.Log($"Golpeaste a {other.name} con {weaponData.damage} de daño");
            }
        }
    }
}
