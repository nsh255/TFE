using UnityEngine;
using UnityEngine.SceneManagement;

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
        EnsureGameManagerReady();

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

    private void EnsureGameManagerReady()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("[PlayerSpawner] GameManager no encontrado al iniciar partida. Creando uno.");
            new GameObject("GameManager").AddComponent<GameManager>();
            gm = GameManager.Instance;
        }

        if (gm != null && SceneManager.GetActiveScene().name == gm.gameScene)
        {
            gm.ChangeState(GameState.Playing);
            Debug.Log("[PlayerSpawner] GameManager listo en estado Playing.");
        }
    }
}
