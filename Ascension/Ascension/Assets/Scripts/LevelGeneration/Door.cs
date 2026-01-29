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

    /// <summary>
    /// Inicializa la puerta y aplica el estado inicial.
    /// </summary>
    void Awake()
    {
        if (doorSprite == null)
        {
            doorSprite = GetComponent<SpriteRenderer>();
        }

        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider2D>();
        }

        if (startOpen)
        {
            Open(false);
        }
        else
        {
            Close(false);
        }
    }

    /// <summary>
    /// Abre la puerta permitiendo el paso del jugador.
    /// </summary>
    /// <param name="playEffects">Si debe reproducir sonido</param>
    public void Open(bool playEffects = true)
    {
        if (isOpen) return;

        isOpen = true;

        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        if (doorSprite != null && openSprite != null)
        {
            doorSprite.sprite = openSprite;
        }

        if (playEffects && openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }

    /// <summary>
    /// Cierra la puerta bloqueando el paso del jugador.
    /// </summary>
    /// <param name="playEffects">Si debe reproducir sonido</param>
    public void Close(bool playEffects = true)
    {
        if (!isOpen && !startOpen) return;

        isOpen = false;

        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }

        if (doorSprite != null && closedSprite != null)
        {
            doorSprite.sprite = closedSprite;
        }

        if (playEffects && closeSound != null)
        {
            AudioSource.PlayClipAtPoint(closeSound, transform.position);
        }
    }

    /// <summary>
    /// Alterna entre abrir y cerrar la puerta.
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
