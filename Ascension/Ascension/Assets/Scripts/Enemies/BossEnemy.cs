using UnityEngine;

public class BossEnemy : Enemy
{
    public float attackCooldown = 2f;
    private float attackTimer = 0f;
    public GameObject specialAttackPrefab;

    protected override void Start()
    {
        base.Start();
        // Puedes aumentar la vida del jefe aquí si quieres que sea más resistente
        if (enemyData != null)
        {
            currentHealth = Mathf.RoundToInt(enemyData.maxHealth * 3f); // 3 veces más vida que un enemigo normal
        }
    }

    protected override void Update()
    {
        base.Update();
        if (player != null)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                SpecialAttack();
                attackTimer = attackCooldown;
            }
        }
    }

    void SpecialAttack()
    {
        // Ejemplo: lanza un proyectil especial hacia el jugador
        if (specialAttackPrefab != null && player != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            GameObject attack = Instantiate(specialAttackPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = attack.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * (enemyData != null ? enemyData.speed : 5f);
            }
        }
        // Aquí puedes añadir más patrones: saltos, invocaciones, etc.
    }

    protected override void Die()
    {
        // Aquí puedes poner animación de muerte, drop especial, etc.
        base.Die();
    }
}
