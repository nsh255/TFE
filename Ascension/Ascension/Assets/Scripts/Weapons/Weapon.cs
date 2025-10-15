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

    void Start()
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

    void Update()
    {
        if (camera != null)
        {
            RotateTowardsMouse();
        }

        // Detectar clic izquierdo para atacar
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    // La lógica de ataque se manejará en scripts separados.
    public virtual void Attack()
    {
        // Este método se encargará de disparar o golpear.
        // Lo dejaremos vacío en la clase base.
        Debug.Log("Ataque base - Este método debe ser sobrescrito en las clases hijas");
    }

    private void RotateTowardsMouse()
    {
        float angle = GetAngleTowardsMouse();

        transform.rotation = Quaternion.Euler(0, 0, angle);
        spriteRenderer.flipY = angle >= 90 && angle <= 270;
    }

    private float GetAngleTowardsMouse()
    {
        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);

        Vector3 mouseDirection = mouseWorldPosition - transform.position;
        mouseDirection.z = 0;

        float angle = (Vector3.SignedAngle(Vector3.right, mouseDirection, Vector3.forward) + 360) % 360;

        return angle;
    }
}
