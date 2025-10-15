using UnityEngine;

public class ShooterEnemy : Enemy
{
    public GameObject bulletPrefab;
    public float shootCooldown = 2f;
    private float shootTimer = 0f;

    protected override void Update()
    {
        base.Update();
        if (player != null && enemyData != null)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootCooldown;
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && player != null && enemyData != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * enemyData.speed;
            }
        }
    }
}
