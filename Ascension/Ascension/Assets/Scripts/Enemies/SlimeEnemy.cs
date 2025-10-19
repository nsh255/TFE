using UnityEngine;

/// <summary>
/// Slime Azul - Salta hacia el jugador cuando está a cierta distancia
/// </summary>
public class SlimeBlue : Enemy
{
    [Header("Comportamiento Azul - Saltador")]
    [Tooltip("Fuerza del salto hacia el jugador")]
    public float jumpForce = 7f;
    
    [Tooltip("Tiempo entre saltos")]
    public float jumpCooldown = 2f;
    
    [Tooltip("Distancia mínima para saltar")]
    public float minJumpDistance = 3f;
    
    [Tooltip("Distancia máxima para saltar")]
    public float maxJumpDistance = 10f;
    
    private float nextJumpTime;
    private bool isGrounded = true;

    protected override void Start()
    {
        base.Start();
        nextJumpTime = Time.time + jumpCooldown;
        
        // Valores por defecto
        if (jumpForce == 0) jumpForce = 7f;
        if (jumpCooldown == 0) jumpCooldown = 2f;
        if (minJumpDistance == 0) minJumpDistance = 3f;
        if (maxJumpDistance == 0) maxJumpDistance = 10f;
    }

    protected override void Update()
    {
        base.Update();
        
        if (isDead || player == null) return;

        // Calcular distancia al jugador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Si está en rango y es tiempo de saltar
        if (distanceToPlayer >= minJumpDistance && 
            distanceToPlayer <= maxJumpDistance && 
            Time.time >= nextJumpTime && 
            isGrounded)
        {
            JumpTowardsPlayer();
        }
        
        // Flip sprite según dirección al jugador
        FlipSprite();
    }

    private void JumpTowardsPlayer()
    {
        if (rb == null || player == null) return;

        // Calcular dirección hacia el jugador
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Aplicar fuerza de salto (diagonal hacia el jugador)
        rb.linearVelocity = new Vector2(direction.x * jumpForce, jumpForce);
        
        // Trigger animación de salto si existe
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
        
        isGrounded = false;
        nextJumpTime = Time.time + jumpCooldown;
        
        Debug.Log($"[SlimeBlue] Saltó hacia el jugador con fuerza {jumpForce}");
    }

    private void FlipSprite()
    {
        if (spriteRenderer != null && player != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Detectar cuando aterriza
        if (!collision.gameObject.CompareTag("Player"))
        {
            isGrounded = true;
            
            if (animator != null)
            {
                animator.SetBool("IsGrounded", true);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Detectar cuando deja el suelo
        if (!collision.gameObject.CompareTag("Player"))
        {
            isGrounded = false;
            
            if (animator != null)
            {
                animator.SetBool("IsGrounded", false);
            }
        }
    }
}
