using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;
    
    [Header("UI References")]
    [Tooltip("Referencia al HeartDisplay en el Canvas de la escena")]
    public HeartDisplay heartDisplay;

    void Start()
    {
        // Verificar que se haya seleccionado una clase
        if (ClassSelectionManager.SelectedClass == null)
        {
            Debug.LogError("No se ha seleccionado ninguna clase. Volviendo al menú...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("ClassSelection");
            return;
        }

        // Instanciar el jugador
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        
        // Conectar el HeartDisplay PRIMERO (antes de inicializar)
        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && heartDisplay != null)
        {
            playerHealth.heartDisplay = heartDisplay;
            Debug.Log("[PlayerSpawner] HeartDisplay conectado automáticamente");
        }
        else if (heartDisplay == null)
        {
            Debug.LogWarning("[PlayerSpawner] No hay HeartDisplay asignado en PlayerSpawner. Asígnalo en el Inspector.");
        }
        
        // IMPORTANTE: Asignar la clase ANTES de que se ejecuten los componentes
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.playerClass = ClassSelectionManager.SelectedClass;
            // Inicializar manualmente después de asignar la clase (Y DESPUÉS de conectar HeartDisplay)
            controller.Initialize();
        }
        else
        {
            Debug.LogError("PlayerController no encontrado en el prefab del jugador!");
        }
    }
}
