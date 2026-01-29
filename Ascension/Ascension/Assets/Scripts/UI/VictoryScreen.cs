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

    /// <summary>
    /// Inicializa la pantalla y configura los listeners de botones.
    /// </summary>
    void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        GameManager.OnGameStateChanged += OnGameStateChanged;
        runStartTime = Time.time;
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
        if (newState == GameState.Victory)
        {
            Show();
        }
        else if (newState == GameState.Playing)
        {
            runStartTime = Time.time;
        }
    }

    /// <summary>
    /// Muestra la pantalla de Victoria con estadísticas finales y tiempo de run.
    /// </summary>
    public void Show()
    {
        if (victoryPanel == null) return;

        victoryPanel.SetActive(true);

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
    }

    /// <summary>
    /// Oculta la pantalla de Victoria.
    /// </summary>
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
