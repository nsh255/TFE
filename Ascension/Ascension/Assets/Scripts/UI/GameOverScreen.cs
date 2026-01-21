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

    void Start()
    {
        // Ocultar panel al inicio
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Configurar botones
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Suscribirse a cambios de estado
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    /// <summary>
    /// Reacciona a cambios de estado del juego
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.GameOver)
        {
            Show();
        }
    }

    /// <summary>
    /// Muestra la pantalla de Game Over con estadísticas
    /// </summary>
    public void Show()
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        // Mostrar estadísticas
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

        Debug.Log("[GameOverScreen] Pantalla mostrada");
    }

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
        Debug.Log("[GameOverScreen] Reiniciando run...");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartRun();
        }
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("[GameOverScreen] Volviendo al menú principal...");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }

    #endregion
}
