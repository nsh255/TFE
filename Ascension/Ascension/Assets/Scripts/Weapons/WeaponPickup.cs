using UnityEngine;

/// <summary>
/// Permite al jugador recoger armas del suelo.
/// Se añade automáticamente a los drops de armas.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [Tooltip("Datos del arma a recoger")]
    public WeaponData weaponData;

    [Header("Pickup Settings")]
    [Tooltip("Tecla para recoger (por defecto E)")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    [Tooltip("Radio de detección del jugador")]
    [SerializeField] private float pickupRange = 1.5f;

    [Header("Visual Feedback")]
    [Tooltip("UI que muestra el prompt de recoger")]
    [SerializeField] private GameObject pickupPrompt;

    [Tooltip("Animación de flotación")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed = 2f;

    // Estado
    private bool playerNearby = false;
    private Transform playerTransform;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ocultar prompt al inicio
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // Animación de flotación
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // Detectar jugador cercano manualmente
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            playerNearby = distance <= pickupRange;

            // Mostrar/ocultar prompt
            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(playerNearby);
            }

            // Input para recoger
            if (playerNearby && Input.GetKeyDown(pickupKey))
            {
                PickupWeapon();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            playerTransform = other.transform;

            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Recoge el arma y la equipa al jugador
    /// </summary>
    private void PickupWeapon()
    {
        if (weaponData == null)
        {
            Debug.LogError("[WeaponPickup] No hay WeaponData asignada");
            return;
        }

        if (playerTransform == null) return;

        // Buscar el componente que equipa armas en el jugador
        // Nota: Asume que existe un PlayerWeaponManager o similar
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Intentar equipar el arma
            // NOTA: Esto requiere añadir método EquipWeapon en PlayerController
            // Por ahora, solo loggeamos
            Debug.Log($"[WeaponPickup] Jugador recogió: {weaponData.weaponName}");
            
            // TODO: Implementar lógica real de equipar arma
            // playerController.EquipWeapon(weaponData);
        }

        // Destruir el pickup
        Destroy(gameObject);
    }

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }

    #endregion
}
