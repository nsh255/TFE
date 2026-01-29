using UnityEngine;

/// <summary>
/// Clase base para todas las armas del juego.
/// Gestiona la rotación hacia el cursor, órbita alrededor del jugador y disparo básico.
/// </summary>
public class Weapon : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public WeaponData weaponData;
    public new Camera camera;

    private bool isInitialized = false;

    [Header("Attack Input")]
    [Tooltip("Si está activo, ataca mientras mantienes el click. Si no, ataca solo al hacer click")]
    [SerializeField] private bool attackWhileHeld = false;

    /// <summary>
    /// Inicializa componentes básicos del arma.
    /// </summary>
    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (camera == null)
        {
            camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("No se encontró ninguna cámara principal en la escena!");
            }
        }

        if (weaponData != null && !isInitialized)
        {
            Initialize();
        }
    }

    /// <summary>
    /// Inicializa el arma con los datos proporcionados.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (weaponData != null && weaponData.sprite != null)
        {
            spriteRenderer.sprite = weaponData.sprite;
        }
        
        isInitialized = true;
    }

    /// <summary>
    /// Actualiza la posición y rotación del arma cada frame y procesa la entrada de ataque.
    /// </summary>
    protected virtual void Update()
    {
        if (camera != null)
        {
            OrbitAroundPlayer();
            RotateTowardsMouse();
        }

        bool wantsAttack = attackWhileHeld ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        if (wantsAttack) Attack();
    }

    /// <summary>
    /// Ejecuta el ataque del arma. Debe ser sobrescrito en clases derivadas.
    /// </summary>
    public virtual void Attack()
    {
    }

    /// <summary>
    /// Posiciona el arma orbitando alrededor del jugador siguiendo la posición del cursor.
    /// </summary>
    private void OrbitAroundPlayer()
    {
        PlayerController player = transform.parent?.GetComponent<PlayerController>();
        if (player == null) return;

        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        float headOffset = 0.5f;
        Vector3 orbitCenter = transform.parent.position + new Vector3(0, headOffset, 0);

        Vector2 direction = (mouseWorldPosition - orbitCenter).normalized;
        float orbitDistance = player.weaponOffset.magnitude * 0.5f;
        
        Vector3 targetPosition = (Vector3)direction * orbitDistance + new Vector3(0, headOffset, 0);
        transform.localPosition = targetPosition;
    }

    /// <summary>
    /// Rota el arma para que apunte hacia la posición del cursor.
    /// </summary>
    private void RotateTowardsMouse()
    {
        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        Vector2 direction = (mouseWorldPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float spriteOrientationOffset = -90f;
        transform.rotation = Quaternion.Euler(0, 0, angle + spriteOrientationOffset);
    }
}
