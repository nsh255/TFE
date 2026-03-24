using UnityEngine;

/// <summary>
/// Componente de hitbox para armas cuerpo a cuerpo.
/// Detecta colisiones con enemigos y reenvía el daño al arma padre.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WeaponHitbox : MonoBehaviour
{
    private MeleeWeapon meleeWeapon;
    private Collider2D col;

    /// <summary>
    /// Busca el componente MeleeWeapon en el padre y configura el collider.
    /// </summary>
    void Awake()
    {
        meleeWeapon = GetComponentInParent<MeleeWeapon>();
        if (meleeWeapon == null)
        {
            Debug.LogError("[WeaponHitbox] No se encontró MeleeWeapon en el padre!");
        }

        col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (col == null)
        {
            col = GetComponent<Collider2D>();
        }

        if (col != null)
        {
            col.isTrigger = true;
            col.enabled = false;
        }
    }

    /// <summary>
    /// Detecta colisiones con enemigos y aplica daño cuando la hitbox está activa.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Validaciones básicas
        if (meleeWeapon == null)
        {
            return;
        }

        // La hitbox debe estar activa TANTO en MeleeWeapon como en el collider
        if (!meleeWeapon.IsSwinging || !meleeWeapon.IsHitboxActive)
        {
            return;
        }
        
        if (!col.enabled)
        {
            return;
        }
        
        // Si hay WeaponHitbox children, evitar double-hit
        if (meleeWeapon.GetComponentsInChildren<WeaponHitbox>(true).Length > 1)
        {
            return;
        }
        
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (meleeWeapon != null && meleeWeapon.weaponData != null)
                {
                    enemy.TakeDamage(meleeWeapon.weaponData.damage);
                }
            }
        }
    }

    /// <summary>
    /// Activa la hitbox para detectar colisiones.
    /// </summary>
    public void EnableHitbox()
    {
        if (col != null)
        {
            col.enabled = true;
        }
    }

    /// <summary>
    /// Desactiva la hitbox para dejar de detectar colisiones.
    /// </summary>
    public void DisableHitbox()
    {
        if (col != null)
        {
            col.enabled = false;
        }
    }
}
