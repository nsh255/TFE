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

    void Start()
    {
        // Ocultar menú al inicio
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Configurar botones
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Suscribirse a cambios de estado
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDestroy()
    {
        // Desuscribirse para evitar errores
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    void Update()
    {
        // Backup: detectar ESC si el GameManager no lo maneja
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }

    /// <summary>
    /// Reacciona a cambios de estado del juego
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        if (pauseMenuPanel == null) return;

        // Mostrar menú solo cuando esté pausado
        bool shouldShow = (newState == GameState.Paused);
        pauseMenuPanel.SetActive(shouldShow);

        Debug.Log($"[PauseMenu] Estado: {newState}, Mostrar: {shouldShow}");
    }

    #region Callbacks de Botones

    private void OnResumeClicked()
    {
        Debug.Log("[PauseMenu] Botón REANUDAR presionado");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Resume();
        }
    }

    private void OnRestartClicked()
    {
        Debug.Log("[PauseMenu] Botón REINICIAR presionado");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartRun();
        }
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("[PauseMenu] Botón MENÚ PRINCIPAL presionado");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }

    private void OnQuitClicked()
    {
        Debug.Log("[PauseMenu] Botón SALIR presionado");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }

    #endregion

    #region Métodos Públicos (por si se llaman desde otros scripts)

    public void Show()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    public void Hide()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    #endregion
}
