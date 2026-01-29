using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pantalla de Game Over que se muestra cuando el jugador muere.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI roomsClearedText;
    [SerializeField] private TextMeshProUGUI enemiesKilledText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    /// <summary>
    /// Inicializa la pantalla y configura los listeners de botones.
    /// </summary>
    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    /// <summary>
    /// Desuscribe eventos al destruirse.
    /// </summary>
    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    /// <summary>
    /// Reacciona a cambios de estado del juego.
    /// </summary>
    /// <param name="newState">Nuevo estado del juego</param>
    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.GameOver)
        {
            Show();
        }
    }

    /// <summary>
    /// Muestra la pantalla de Game Over con estadísticas finales.
    /// </summary>
    public void Show()
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        if (scoreText != null && ScoreManager.Instance != null)
        {
            scoreText.text = $"Puntuación: {ScoreManager.Instance.CurrentScore}";
        }

        if (roomsClearedText != null && GameManager.Instance != null)
        {
            roomsClearedText.text = $"Salas limpiadas: {GameManager.Instance.RoomsCleared}";
        }

        if (enemiesKilledText != null && GameManager.Instance != null)
        {
            enemiesKilledText.text = $"Enemigos eliminados: {GameManager.Instance.EnemiesKilled}";
        }
    }

    /// <summary>
    /// Oculta la pantalla de Game Over.
    /// </summary>
    public void Hide()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    #region Callbacks de Botones

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartRun();
        }
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }

    #endregion
}
