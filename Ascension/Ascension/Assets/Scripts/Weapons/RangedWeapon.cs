using UnityEngine;

// Este script hereda de la clase Weapon
public class RangedWeapon : Weapon
{
    // Este campo lo usarás en el Inspector para enlazar el spawner.
    public Transform spawner;

    // Cooldown entre disparos
    public float attackCooldown = 0.3f;
    private float lastAttackTime = -999f;

    // Se sobreescribe el método Attack() para que dispare.
    public override void Attack()
    {
        // Verificar cooldown
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        if (weaponData == null)
        {
            Debug.LogWarning("No hay weaponData asignado en RangedWeapon!");
            return;
        }

        // Enlaza el bulletPrefab en el ScriptableObject de WeaponData.
        if (weaponData.bulletPrefab != null)
        {
            lastAttackTime = Time.time;
            
            // Usar spawner si está asignado, si no usar la posición del arma
            Vector3 spawnPosition = spawner != null ? spawner.position : transform.position;
            
            GameObject bullet = Instantiate(weaponData.bulletPrefab, spawnPosition, transform.rotation);
            
            // Pasarle el daño a la bala si tiene un script de proyectil
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = weaponData.damage;
                projectileScript.speed = 10f; // Velocidad por defecto
            }
            else
            {
                // Si no tiene script de proyectil, al menos darle velocidad con Rigidbody2D
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = transform.right * 10f;
                }
            }
            
            Destroy(bullet, 3f); // Destruye la bala después de 3 segundos
            Debug.Log($"¡Disparo! Daño: {weaponData.damage}");
        }
        else
        {
            Debug.LogWarning("No hay bulletPrefab asignado en el WeaponData!");
        }
    }
}
