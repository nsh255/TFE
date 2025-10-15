using UnityEngine;

public class ChaserEnemy : Enemy
{
    protected override void Update()
    {
        if (player != null && enemyData != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * enemyData.speed * Time.deltaTime;
        }
    }
}
