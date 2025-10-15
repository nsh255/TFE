using UnityEngine;

public class MeleeWeapon : Weapon
{
    // Collider que actúa como la "hitbox" del arma.
    public Collider2D hitbox;

    // Cooldown entre ataques
    public float attackCooldown = 0.5f;
    private float lastAttackTime = -999f;

    void Start()
    {
        // Asegurarse de que el hitbox esté desactivado al inicio
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
    }

    // Se sobreescribe el método Attack() para que active la hitbox.
    public override void Attack()
    {
        // Verificar cooldown
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        if (hitbox == null)
        {
            Debug.LogWarning("No hay hitbox asignada en MeleeWeapon!");
            return;
        }

        lastAttackTime = Time.time;
        
        // Activa la hitbox para un solo frame.
        hitbox.enabled = true;
        
        // Llama a un método para desactivarla después de un tiempo.
        Invoke("DisableHitbox", 0.1f);
        
        Debug.Log($"¡Ataque cuerpo a cuerpo! Daño: {weaponData?.damage ?? 0}");
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
