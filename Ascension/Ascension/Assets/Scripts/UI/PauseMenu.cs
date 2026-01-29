using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el menú de pausa.
/// Se muestra/oculta automáticamente según el estado del GameManager.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    /// <summary>
    /// Inicializa el menú y configura los listeners de botones.
    /// </summary>
    void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

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
    /// Detecta pulsación de ESC para pausar/despausar.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }

    /// <summary>
    /// Reacciona a cambios de estado del juego mostrando/ocultando el panel.
    /// </summary>
    /// <param name="newState">Nuevo estado del juego</param>
    private void OnGameStateChanged(GameState newState)
    {
        if (pauseMenuPanel == null) return;

        bool shouldShow = (newState == GameState.Paused);
        pauseMenuPanel.SetActive(shouldShow);
    }

    #region Callbacks de Botones

    private void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Resume();
        }
    }

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

    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Muestra el menú de pausa manualmente.
    /// </summary>
    public void Show()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta el menú de pausa manualmente.
    /// </summary>
    public void Hide()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    #endregion
}
