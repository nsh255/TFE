using UnityEngine;

public class JumperEnemy : Enemy
{
    public float jumpForce = 5f;
    public float jumpCooldown = 2f;
    private float jumpTimer = 0f;
    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();
        if (player != null && enemyData != null)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0f)
            {
                JumpTowardsPlayer();
                jumpTimer = jumpCooldown;
            }
        }
    }

    void JumpTowardsPlayer()
    {
        if (rb != null && player != null && enemyData != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.AddForce(dir * enemyData.speed, ForceMode2D.Impulse);
        }
    }
}
