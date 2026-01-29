using UnityEngine;

/// <summary>
/// Arma a distancia que dispara proyectiles hacia la posición del cursor.
/// </summary>
public class RangedWeapon : Weapon
{
    public Transform spawner;

    public float attackCooldown = 0.3f;
    private float lastAttackTime = -999f;

    /// <summary>
    /// Dispara un proyectil desde el punto de spawn hacia la dirección del arma.
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

        if (weaponData == null)
        {
            Debug.LogError("No hay weaponData asignado en RangedWeapon!");
            return;
        }

        if (weaponData.bulletPrefab != null)
        {
            lastAttackTime = Time.time;
            
            Vector3 spawnPosition = spawner != null ? spawner.position : transform.position;
            Quaternion spawnRotation = transform.rotation;
            
            GameObject bullet = Instantiate(weaponData.bulletPrefab, spawnPosition, spawnRotation);
            
            if (bullet == null)
            {
                Debug.LogError("No se pudo instanciar la bala!");
                return;
            }
            
            bullet.transform.localScale = Vector3.one * 20f;
            
            SpriteRenderer bulletSprite = bullet.GetComponent<SpriteRenderer>();
            if (bulletSprite != null)
            {
                bulletSprite.color = Color.yellow;
            }
            
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = weaponData.damage;
                projectileScript.speed = 10f;
            }
            else
            {
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = transform.right * 10f;
                }
            }
        }
        else
        {
            Debug.LogError("No hay bulletPrefab asignado en el WeaponData!");
        }
    }
}
