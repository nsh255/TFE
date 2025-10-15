using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClassSelectionManager : MonoBehaviour
{
    public PlayerClass[] availableClasses;
    public string gameSceneName = "GameScene";
    public static PlayerClass SelectedClass;

    public void SelectClassAndStartGame(int index)
    {
        Debug.Log($"Intentando seleccionar clase en índice: {index}");
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
        Debug.Log($"Clase seleccionada: {SelectedClass.name}");
        Debug.Log($"Cargando escena: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    public void TestButton()
    {
        Debug.Log("¡El botón funciona!");
    }
}
