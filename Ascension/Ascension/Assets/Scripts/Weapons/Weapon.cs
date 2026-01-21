using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    // Conectamos los datos del arma a su objeto en el mundo.
    public WeaponData weaponData;

    // La cámara del jugador es necesaria para convertir la posición del ratón.
    public new Camera camera;

    private bool isInitialized = false;

    [Header("Attack Input")]
    [Tooltip("Si está activo, ataca mientras mantienes el click. Si no, ataca solo al hacer click.")]
    [SerializeField] private bool attackWhileHeld = false;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Auto-asignar la cámara principal si no está asignada
        if (camera == null)
        {
            camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("No se encontró ninguna cámara principal en la escena!");
            }
        }

        // Si weaponData ya está asignado, inicializar
        if (weaponData != null && !isInitialized)
        {
            Initialize();
        }
    }

    // Método público para inicializar después de asignar weaponData
    public void Initialize()
    {
        if (isInitialized) return;
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Si hay datos, se usa el sprite de ellos.
        if (weaponData != null && weaponData.sprite != null)
        {
            spriteRenderer.sprite = weaponData.sprite;
        }
        
        isInitialized = true;
        Debug.Log($"Arma inicializada: {weaponData?.weaponName ?? "Sin nombre"}");
    }

    protected virtual void Update()
    {
        if (camera != null)
        {
            OrbitAroundPlayer(); // PRIMERO orbita (posición)
            RotateTowardsMouse(); // DESPUÉS rota (orientación)
        }

        // Input de ataque: por defecto solo al click (evita "hitbox al apuntar").
        // Las armas concretas gestionan cooldown/animación/hitbox.
        bool wantsAttack = attackWhileHeld ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        if (wantsAttack) Attack();
    }

    // La lógica de ataque se manejará en scripts separados.
    public virtual void Attack()
    {
        // Este método se encargará de disparar o golpear.
        // Lo dejaremos vacío en la clase base.
        Debug.Log("Ataque base - Este método debe ser sobrescrito en las clases hijas");
    }

    private void OrbitAroundPlayer()
    {
        // El arma orbita alrededor del jugador siguiendo el mouse (estilo Tiny Rogues)
        PlayerController player = transform.parent?.GetComponent<PlayerController>();
        if (player == null) return;

        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        // Centro de órbita: posición del jugador + ligero offset hacia arriba (cabeza)
        float headOffset = 0.5f; // Ajusta este valor para subir/bajar el centro de órbita
        Vector3 orbitCenter = transform.parent.position + new Vector3(0, headOffset, 0);

        // Calcular dirección desde el centro de órbita (cabeza del player) hacia el mouse
        Vector2 direction = (mouseWorldPosition - orbitCenter).normalized;
        
        // Reducir la distancia de órbita para que el arma esté más cerca del player
        float orbitDistance = player.weaponOffset.magnitude * 0.5f; // Reducido a la mitad
        
        // Posicionar el arma en la dirección del mouse a la distancia de órbita (en localPosition)
        Vector3 targetPosition = (Vector3)direction * orbitDistance + new Vector3(0, headOffset, 0);
        transform.localPosition = targetPosition;
    }

    private void RotateTowardsMouse()
    {
        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        // Calcular dirección desde el arma hacia el mouse
        Vector2 direction = (mouseWorldPosition - transform.position).normalized;
        
        // Calcular ángulo en grados
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // AJUSTE: Si el sprite apunta hacia arriba en vez de a la derecha, resta 90°
        // Cambia este valor según la orientación de tu sprite:
        // - Sprite apunta DERECHA → offset = 0
        // - Sprite apunta ARRIBA → offset = -90
        // - Sprite apunta IZQUIERDA → offset = 180
        // - Sprite apunta ABAJO → offset = 90
        float spriteOrientationOffset = -90f; // Ajusta según tu sprite
        
        // Aplicar rotación para que la punta apunte al mouse
        transform.rotation = Quaternion.Euler(0, 0, angle + spriteOrientationOffset);
        
        // NO usar flip con sistema de órbita - la rotación maneja todo
    }
}
