using UnityEngine;

/// <summary>
/// Instancia al jugador en el punto de spawn de la escena con la clase seleccionada previamente.
/// Conecta las dependencias necesarias del jugador con la interfaz de usuario.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;
    
    [Header("UI References")]
    [Tooltip("Referencia al HeartDisplay en el Canvas de la escena")]
    public HeartDisplay heartDisplay;

    /// <summary>
    /// Instancia al jugador con la clase seleccionada y conecta las referencias de UI.
    /// </summary>
    void Start()
    {
        if (ClassSelectionManager.SelectedClass == null)
        {
            Debug.LogError("No se ha seleccionado ninguna clase. Volviendo al menú...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("ClassSelection");
            return;
        }

        GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        
        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && heartDisplay != null)
        {
            playerHealth.heartDisplay = heartDisplay;
        }
        else if (heartDisplay == null)
        {
            Debug.LogWarning("[PlayerSpawner] No hay HeartDisplay asignado en PlayerSpawner.");
        }
        
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.playerClass = ClassSelectionManager.SelectedClass;
            controller.Initialize();
        }
        else
        {
            Debug.LogError("PlayerController no encontrado en el prefab del jugador!");
        }
    }
}
