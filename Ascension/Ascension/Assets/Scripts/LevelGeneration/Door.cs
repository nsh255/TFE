using UnityEngine;

/// <summary>
/// Controla una puerta que conecta salas.
/// Se abre/cierra según el estado de la sala.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Door : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tipo de puerta")]
    [SerializeField] private DoorType doorType = DoorType.Normal;
    
    [Tooltip("Si es true, la puerta empieza abierta")]
    [SerializeField] private bool startOpen = false;

    [Header("Componentes")]
    [Tooltip("Sprite Renderer de la puerta")]
    [SerializeField] private SpriteRenderer doorSprite;
    
    [Tooltip("Collider que bloquea el paso")]
    [SerializeField] private Collider2D doorCollider;

    [Header("Estados Visuales")]
    [Tooltip("Sprite cuando la puerta está cerrada")]
    [SerializeField] private Sprite closedSprite;
    
    [Tooltip("Sprite cuando la puerta está abierta")]
    [SerializeField] private Sprite openSprite;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    // Estado
    private bool isOpen = false;

    void Awake()
    {
        // Auto-asignar componentes si no están configurados
        if (doorSprite == null)
        {
            doorSprite = GetComponent<SpriteRenderer>();
        }

        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider2D>();
        }

        // Aplicar estado inicial
        if (startOpen)
        {
            Open(false); // Sin animación/sonido al inicio
        }
        else
        {
            Close(false);
        }
    }

    /// <summary>
    /// Abre la puerta
    /// </summary>
    public void Open(bool playEffects = true)
    {
        if (isOpen) return;

        isOpen = true;

        // Desactivar collider para permitir paso
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        // Cambiar sprite
        if (doorSprite != null && openSprite != null)
        {
            doorSprite.sprite = openSprite;
        }

        // Reproducir sonido
        if (playEffects && openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }

        Debug.Log($"[Door] Puerta abierta en {transform.position}");
    }

    /// <summary>
    /// Cierra la puerta
    /// </summary>
    public void Close(bool playEffects = true)
    {
        if (!isOpen && !startOpen) return; // Ya estaba cerrada

        isOpen = false;

        // Activar collider para bloquear paso
        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }

        // Cambiar sprite
        if (doorSprite != null && closedSprite != null)
        {
            doorSprite.sprite = closedSprite;
        }

        // Reproducir sonido
        if (playEffects && closeSound != null)
        {
            AudioSource.PlayClipAtPoint(closeSound, transform.position);
        }

        Debug.Log($"[Door] Puerta cerrada en {transform.position}");
    }

    /// <summary>
    /// Alterna el estado de la puerta
    /// </summary>
    [ContextMenu("Toggle Door")]
    public void Toggle()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    #region Métodos para Testing

    [ContextMenu("Force Open")]
    private void ForceOpen()
    {
        Open(false);
    }

    [ContextMenu("Force Close")]
    private void ForceClose()
    {
        Close(false);
    }

    #endregion

    #region Propiedades Públicas

    public bool IsOpen => isOpen;
    public DoorType Type => doorType;

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
        Gizmos.color = isOpen ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }

    #endregion
}

/// <summary>
/// Tipos de puertas
/// </summary>
public enum DoorType
{
    Normal,     // Puerta normal que conecta salas
    Locked,     // Puerta bloqueada (requiere llave)
    Boss,       // Puerta del jefe (visual diferente)
    Exit        // Puerta de salida
}
