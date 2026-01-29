using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona las acciones del menú principal del juego.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    /// <summary>
    /// Inicia el flujo de juego cargando la escena de selección de clase.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("ClassSelection", LoadSceneMode.Single);
    }

    /// <summary>
    /// Cierra la aplicación o detiene el modo de juego en el editor.
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Alterna la visualización del tablero de puntuaciones.
    /// </summary>
    public void ToggleScores()
    {
        var ui = FindFirstObjectByType<ScoreboardMenuUI>();
        if (ui == null)
        {
            var go = new GameObject("ScoreboardMenuUI");
            ui = go.AddComponent<ScoreboardMenuUI>();
        }

        ui.Toggle();
    }
}
