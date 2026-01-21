using UnityEngine;

/// <summary>
/// Slime Azul (Saltador): Dash rápido hacia el jugador en un juego top-down.
/// Simula un "salto" como movimiento rápido en 8 direcciones.
/// </summary>
public class SlimeBlue : Enemy
{
    [Header("Comportamiento Azul - Saltador")]
    [Tooltip("Velocidad del dash hacia el jugador")]
    public float dashSpeed = 12f;
    
    [Tooltip("Duración del dash en segundos")]
    public float dashDuration = 0.4f;
    
    [Tooltip("Tiempo de espera entre dashes")]
    public float dashCooldown = 2f;
    
    [Tooltip("Distancia mínima para intentar hacer dash")]
    public float minDashDistance = 3f;
    
    [Tooltip("Distancia máxima a la que hace dash")]
    public float maxDashDistance = 10f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float lastDashTime = -999f;
    private Vector2 dashDirection;

    protected override void Start()
    {
        base.Start();
        
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Valores por defecto
        if (dashSpeed == 0) dashSpeed = 12f;
        if (dashDuration == 0) dashDuration = 0.4f;
        if (dashCooldown == 0) dashCooldown = 2f;
        if (minDashDistance == 0) minDashDistance = 3f;
        if (maxDashDistance == 0) maxDashDistance = 10f;
        
        Debug.Log($"[SlimeBlue] Inicializado en {name} - DashSpeed: {dashSpeed}, Cooldown: {dashCooldown}s");
    }

    protected override void Update()
    {
        base.Update();
        
        if (isDead || player == null) return;

        // Cooldown al spawnear: evita dash instantáneo.
        if (!IsAIEnabled)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isDashing)
        {
            // Continuar el dash
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }
        else
        {
            // Comportamiento normal: verificar si debe hacer dash
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer >= minDashDistance && 
                distanceToPlayer <= maxDashDistance && 
                Time.time >= lastDashTime + dashCooldown)
            {
                StartDash();
            }
        }
        
        // Flip sprite según dirección
        FlipSprite();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            // Movimiento rápido durante el dash (en 8 direcciones para top-down)
            rb.linearVelocity = dashDirection * (dashSpeed * TileSpeedMultiplier);
        }
        else if (!isDead)
        {
            // Sin movimiento cuando no está dasheando
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void StartDash()
    {
        if (player == null) return;
        
        isDashing = true;
        dashTimer = dashDuration;
        lastDashTime = Time.time;
        
        // Calcular dirección hacia el jugador (normalizada para movimiento en 8 direcciones)
        dashDirection = (player.position - transform.position).normalized;
        
        // Trigger animator si existe (usaremos el trigger "Jump" para la animación de dash/salto)
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
        
        Debug.Log($"[SlimeBlue] Dash iniciado hacia {dashDirection} desde {transform.position}");
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
        
        Debug.Log($"[SlimeBlue] Dash finalizado en {transform.position}");
    }

    protected void FlipSprite()
    {
        // Flip horizontal basado en la dirección del dash
        if (spriteRenderer != null)
        {
            if (isDashing)
            {
                // Durante el dash, usar la dirección del dash
                if (dashDirection.x > 0.1f)
                    spriteRenderer.flipX = false;
                else if (dashDirection.x < -0.1f)
                    spriteRenderer.flipX = true;
            }
        }
    }

    // Dibujar gizmos para visualizar rangos en el editor
    private void OnDrawGizmosSelected()
    {
        // Visualizar rangos de dash
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDashDistance);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxDashDistance);
        
        // Visualizar dirección del dash si está activo
        if (isDashing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, dashDirection * 2f);
        }
    }
}
