using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pantalla de Victoria que se muestra cuando el jugador completa la run.
/// </summary>
public class VictoryScreen : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI roomsClearedText;
    [SerializeField] private TextMeshProUGUI enemiesKilledText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    private float runStartTime;

    void Start()
    {
        // Ocultar panel al inicio
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        // Configurar botones
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Suscribirse a cambios de estado
        GameManager.OnGameStateChanged += OnGameStateChanged;

        // Registrar tiempo de inicio
        runStartTime = Time.time;
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
        if (newState == GameState.Victory)
        {
            Show();
        }
        else if (newState == GameState.Playing)
        {
            // Registrar nuevo tiempo de inicio al empezar run
            runStartTime = Time.time;
        }
    }

    /// <summary>
    /// Muestra la pantalla de Victoria con estadísticas
    /// </summary>
    public void Show()
    {
        if (victoryPanel == null) return;

        victoryPanel.SetActive(true);

        // Mostrar estadísticas
        if (scoreText != null && ScoreManager.Instance != null)
        {
            scoreText.text = $"Puntuación Final: {ScoreManager.Instance.CurrentScore}";
        }

        if (roomsClearedText != null && GameManager.Instance != null)
        {
            roomsClearedText.text = $"Salas limpiadas: {GameManager.Instance.RoomsCleared}";
        }

        if (enemiesKilledText != null && GameManager.Instance != null)
        {
            enemiesKilledText.text = $"Enemigos eliminados: {GameManager.Instance.EnemiesKilled}";
        }

        if (timeText != null)
        {
            float runTime = Time.time - runStartTime;
            int minutes = Mathf.FloorToInt(runTime / 60f);
            int seconds = Mathf.FloorToInt(runTime % 60f);
            timeText.text = $"Tiempo: {minutes:00}:{seconds:00}";
        }

        Debug.Log("[VictoryScreen] ¡Victoria! Pantalla mostrada");
    }

    public void Hide()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    #region Callbacks de Botones

    private void OnPlayAgainClicked()
    {
        Debug.Log("[VictoryScreen] Iniciando nueva run...");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartRun();
        }
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("[VictoryScreen] Volviendo al menú principal...");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }

    #endregion
}
