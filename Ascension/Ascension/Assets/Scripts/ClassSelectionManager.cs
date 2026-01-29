using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la selección de clase del jugador antes de iniciar la partida.
/// </summary>
public class ClassSelectionManager : MonoBehaviour
{
    public PlayerClass[] availableClasses;
    public string gameSceneName = "GameScene";
    public static PlayerClass SelectedClass;

    /// <summary>
    /// Selecciona una clase del array y carga la escena de juego.
    /// </summary>
    /// <param name="index">Índice de la clase en el array availableClasses.</param>
    public void SelectClassAndStartGame(int index)
    {
        if (availableClasses == null || availableClasses.Length == 0)
        {
            Debug.LogError("No hay clases disponibles en availableClasses.");
            return;
        }
        if (index < 0 || index >= availableClasses.Length)
        {
            Debug.LogError($"Índice fuera de rango: {index}");
            return;
        }
        SelectedClass = availableClasses[index];
        SceneManager.LoadScene(gameSceneName);
    }
}
