using UnityEngine;

/// <summary>
/// Script que debe estar en el GameObject hijo que contiene el Collider2D de la hitbox.
/// Reenvía las colisiones al MeleeWeapon padre.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WeaponHitbox : MonoBehaviour
{
    private MeleeWeapon meleeWeapon;
    private Collider2D col;

    void Awake()
    {
        // Buscar el MeleeWeapon en el padre
        meleeWeapon = GetComponentInParent<MeleeWeapon>();
        if (meleeWeapon == null)
        {
            Debug.LogError("[WeaponHitbox] No se encontró MeleeWeapon en el padre!");
        }

        // Obtener el collider y asegurar que es trigger
        col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.enabled = false; // Desactivado por defecto
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[WeaponHitbox] ¡COLISIÓN DETECTADA! con {other.name}, Tag: {other.tag}, Collider enabled: {col.enabled}");

        // Gating duro: solo permitir daño durante un ataque real.
        if (meleeWeapon == null || !meleeWeapon.IsSwinging || !Input.GetMouseButton(0))
        {
            return;
        }
        
        if (!col.enabled)
        {
            Debug.LogWarning($"[WeaponHitbox] Colisión ignorada - collider desactivado");
            return; // Solo procesar si está activo
        }
        
        // Solo dañar enemigos
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"[WeaponHitbox] ✓ Tag 'Enemy' confirmado en {other.name}");
            
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (meleeWeapon != null && meleeWeapon.weaponData != null)
                {
                    Debug.Log($"[WeaponHitbox] >>> APLICANDO DAÑO: {meleeWeapon.weaponData.damage} a {other.name}");
                    enemy.TakeDamage(meleeWeapon.weaponData.damage);
                    Debug.Log($"[WeaponHitbox] Golpeaste a {other.name} con {meleeWeapon.weaponData.damage} de daño. HP restante: {enemy.currentHealth}");
                }
                else
                {
                    Debug.LogError($"[WeaponHitbox] meleeWeapon o weaponData es NULL! meleeWeapon: {meleeWeapon != null}, weaponData: {meleeWeapon?.weaponData != null}");
                }
            }
            else
            {
                Debug.LogError($"[WeaponHitbox] {other.name} tiene tag 'Enemy' pero no tiene componente Enemy!");
            }
        }
        else
        {
            Debug.Log($"[WeaponHitbox] Colisión con {other.name} ignorada - tag incorrecto (esperado 'Enemy', recibido '{other.tag}')");
        }
    }

    // Métodos públicos para que MeleeWeapon active/desactive la hitbox
    public void EnableHitbox()
    {
        if (col != null)
        {
            col.enabled = true;
            Debug.Log($"[WeaponHitbox] ✓✓✓ Hitbox ACTIVADA - isTrigger: {col.isTrigger}, bounds: {col.bounds}");
        }
        else
        {
            Debug.LogError("[WeaponHitbox] ¡ERROR! Collider es NULL, no se puede activar hitbox");
        }
    }

    public void DisableHitbox()
    {
        if (col != null)
        {
            col.enabled = false;
            Debug.Log("[WeaponHitbox] Hitbox desactivada");
        }
    }
}
