using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;

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
        
        // IMPORTANTE: Asignar la clase ANTES de que se ejecuten los componentes
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.playerClass = ClassSelectionManager.SelectedClass;
            // Inicializar manualmente después de asignar la clase
            controller.Initialize();
        }
        else
        {
            Debug.LogError("PlayerController no encontrado en el prefab del jugador!");
        }
    }
}
