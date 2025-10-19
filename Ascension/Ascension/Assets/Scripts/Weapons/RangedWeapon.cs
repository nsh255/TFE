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
        Debug.Log("RangedWeapon.Attack() llamado");
        
        // NO PERMITIR ATACAR SI EL JUGADOR ESTÁ EN ROLL
        PlayerController player = transform.parent?.GetComponent<PlayerController>();
        if (player != null && player.IsRolling)
        {
            return; // Bloqueado durante roll
        }
        
        // Verificar cooldown
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log("En cooldown, no se puede disparar todavía");
            return;
        }

        if (weaponData == null)
        {
            Debug.LogError("No hay weaponData asignado en RangedWeapon!");
            return;
        }

        Debug.Log($"WeaponData: {weaponData.weaponName}, BulletPrefab: {weaponData.bulletPrefab}");

        // Enlaza el bulletPrefab en el ScriptableObject de WeaponData.
        if (weaponData.bulletPrefab != null)
        {
            lastAttackTime = Time.time;
            
            // Usar spawner si está asignado, si no usar la posición del arma
            Vector3 spawnPosition = spawner != null ? spawner.position : transform.position;
            Quaternion spawnRotation = transform.rotation;
            
            Debug.Log($"Spawneando bala en: {spawnPosition}, rotación: {spawnRotation.eulerAngles}");
            
            GameObject bullet = Instantiate(weaponData.bulletPrefab, spawnPosition, spawnRotation);
            
            if (bullet == null)
            {
                Debug.LogError("¡No se pudo instanciar la bala!");
                return;
            }
            
            // Hacer la bala visible (20x el sprite original)
            bullet.transform.localScale = Vector3.one * 20f;
            
            // Cambiar el color para que sea visible
            SpriteRenderer bulletSprite = bullet.GetComponent<SpriteRenderer>();
            if (bulletSprite != null)
            {
                bulletSprite.color = Color.yellow; // Color amarillo brillante
            }
            
            Debug.Log($"Bala creada: {bullet.name}, Escala: {bullet.transform.localScale}");
            
            // Pasarle el daño a la bala si tiene un script de proyectil
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = weaponData.damage;
                projectileScript.speed = 10f; // Velocidad por defecto
                Debug.Log($"Proyectil configurado - Daño: {weaponData.damage}, Velocidad: 10");
            }
            else
            {
                Debug.LogWarning("La bala no tiene componente Projectile, intentando con Rigidbody2D");
                // Si no tiene script de proyectil, al menos darle velocidad con Rigidbody2D
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = transform.right * 10f;
                    Debug.Log("Velocidad aplicada vía Rigidbody2D");
                }
                else
                {
                    Debug.LogError("La bala no tiene ni Projectile ni Rigidbody2D!");
                }
            }
            
            Debug.Log($"¡Disparo exitoso! Daño: {weaponData.damage}");
        }
        else
        {
            Debug.LogError("No hay bulletPrefab asignado en el WeaponData! Verifica el ScriptableObject.");
        }
    }
}
