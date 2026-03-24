using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestor central del flujo de juego que controla estados, progresión y transiciones de escena.
/// Implementa el patrón singleton persistente entre cambios de escena.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estado del Juego")]
    [SerializeField] private GameState currentState = GameState.Menu;
    
    [Header("Progresión")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int roomsCleared = 0;
    [SerializeField] private int enemiesKilled = 0;
    
    [Header("Configuración")]
    [Tooltip("Número de salas normales antes del boss")]
    public int roomsBeforeBoss = 5;
    
    [Tooltip("Tiempo de pausa antes de cargar siguiente nivel")]
    public float transitionDelay = 1.5f;
    
    [Header("Referencias de Escenas")]
    public string mainMenuScene = "MainMenu";
    public string classSelectionScene = "ClassSelection";
    public string gameScene = "GameScene";
    
    [Header("Fin de Partida")]
    [SerializeField] private bool returnToMainMenuOnGameOver = true;
    [SerializeField, Min(0f)] private float returnToMainMenuDelaySeconds = 0.75f;

    [Header("Debug")]
    [SerializeField] private bool debugPauseInput = true;

    [Header("UI Global")]
    [SerializeField] private bool enforceMainMenuFontEveryScene = true;
    [SerializeField] private string preferredMainMenuFontName = "VCR_OSD_MONO";

    private TMP_FontAsset cachedMainMenuTmpFont;
    private Font cachedMainMenuLegacyFont;

    private bool isPaused = false;
    private bool runResultRecorded;
    private Coroutine returnToMenuCoroutine;
    private GameObject deathOverlayInstance;
    private int lastPauseToggleFrame = -1;

    public delegate void GameStateChanged(GameState newState);
    public static event GameStateChanged OnGameStateChanged;
    
    public delegate void RoomCleared(int roomNumber);
    public static event RoomCleared OnRoomCleared;
    
    public delegate void EnemyKilled(int totalKills);
    public static event EnemyKilled OnEnemyKilled;

    /// <summary>
    /// Inicializa el singleton y asegura su persistencia entre escenas.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (ScoreManager.Instance == null)
            {
                new GameObject("ScoreManager").AddComponent<ScoreManager>();
            }
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[GameManager] Awake OK. object='{name}', activeScene='{SceneManager.GetActiveScene().name}', debugPauseInput={debugPauseInput}");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Retorna al menú principal tras un retraso en tiempo real.
    /// Utiliza tiempo real para funcionar correctamente con timeScale = 0.
    /// </summary>
    private System.Collections.IEnumerator ReturnToMenuAfterDelayRealtime()
    {
        float delay = Mathf.Max(0f, returnToMainMenuDelaySeconds);
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
        LoadMainMenu();
        returnToMenuCoroutine = null;
    }
    
    /// <summary>
    /// Detecta la escena inicial y establece el estado apropiado del juego.
    /// </summary>
    void Start()
    {
        if (currentState == GameState.GameOver || currentState == GameState.Victory)
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == mainMenuScene)
        {
            ChangeState(GameState.Menu);
        }
        else if (sceneName == classSelectionScene)
        {
            ChangeState(GameState.CharacterSelection);
        }
        else if (sceneName == gameScene)
        {
            ChangeState(GameState.Playing);
        }

        Debug.Log($"[GameManager] Start scene='{sceneName}' state={currentState} timeScale={Time.timeScale} debugPauseInput={debugPauseInput}");
        ApplyMainMenuFontToSceneUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMainMenuFontToSceneUI();
    }

    private void ApplyMainMenuFontToSceneUI()
    {
        if (!enforceMainMenuFontEveryScene) return;

        ResolvePreferredFonts();

        if (cachedMainMenuTmpFont != null)
        {
            var tmpTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            for (int i = 0; i < tmpTexts.Length; i++)
            {
                var t = tmpTexts[i];
                if (t == null) continue;
                if (t.font != cachedMainMenuTmpFont)
                {
                    t.font = cachedMainMenuTmpFont;
                }
            }
        }

        if (cachedMainMenuLegacyFont != null)
        {
            var uiTexts = FindObjectsByType<Text>(FindObjectsSortMode.None);
            for (int i = 0; i < uiTexts.Length; i++)
            {
                var t = uiTexts[i];
                if (t == null) continue;
                if (t.font != cachedMainMenuLegacyFont)
                {
                    t.font = cachedMainMenuLegacyFont;
                }
            }
        }
    }

    private void ResolvePreferredFonts()
    {
        if (cachedMainMenuTmpFont == null)
        {
            var tmpFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            for (int i = 0; i < tmpFonts.Length; i++)
            {
                var f = tmpFonts[i];
                if (f == null) continue;
                if (!string.IsNullOrEmpty(preferredMainMenuFontName) && f.name.IndexOf(preferredMainMenuFontName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    cachedMainMenuTmpFont = f;
                    break;
                }
            }

            if (cachedMainMenuTmpFont == null && tmpFonts.Length > 0)
            {
                cachedMainMenuTmpFont = tmpFonts[0];
            }
        }

        if (cachedMainMenuLegacyFont == null)
        {
            var fonts = Resources.FindObjectsOfTypeAll<Font>();
            for (int i = 0; i < fonts.Length; i++)
            {
                var f = fonts[i];
                if (f == null) continue;
                if (!string.IsNullOrEmpty(preferredMainMenuFontName) && f.name.IndexOf(preferredMainMenuFontName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    cachedMainMenuLegacyFont = f;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Procesa la entrada del jugador para pausar/reanudar el juego.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"[GameManager] ESC detectado. state={currentState}, isPaused={isPaused}, timeScale={Time.timeScale}, scene={SceneManager.GetActiveScene().name}");

            if (currentState == GameState.Playing || currentState == GameState.Paused)
            {
                TogglePause();
            }
            else
            {
                Debug.LogWarning($"[GameManager] ESC ignorado porque state={currentState} (solo Playing/Paused alternan pausa).");
            }
        }
    }

    #region Estado del Juego

    /// <summary>
    /// Cambia el estado del juego y notifica a los observadores registrados.
    /// </summary>
    /// <param name="newState">Nuevo estado del juego.</param>
    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                isPaused = false;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                isPaused = true;
                break;

            case GameState.GameOver:
            case GameState.Victory:
                Time.timeScale = 0f;
                break;
        }

        OnGameStateChanged?.Invoke(newState);
        SyncPauseMenuVisibility(newState);

        Debug.Log($"[GameManager] ChangeState {previousState} -> {newState} (timeScale={Time.timeScale})");
    }

    private void SyncPauseMenuVisibility(GameState state)
    {
        if (SceneManager.GetActiveScene().name != gameScene) return;

        var pauseMenu = FindFirstObjectByType<PauseMenu>();
        if (pauseMenu == null)
        {
            var go = new GameObject("PauseMenuRuntime");
            pauseMenu = go.AddComponent<PauseMenu>();
            pauseMenu.EnsureRuntimeUIIfMissing();
            Debug.LogWarning("[GameManager] PauseMenu no encontrado. Se creó uno en runtime.");
        }

        if (state == GameState.Paused)
        {
            pauseMenu.Show();
        }
        else
        {
            pauseMenu.Hide();
        }
    }

    /// <summary>
    /// Alterna entre el estado de pausa y el estado de juego activo.
    /// </summary>
    public void TogglePause()
    {
        if (lastPauseToggleFrame == Time.frameCount)
        {
            Debug.LogWarning("[GameManager] TogglePause duplicado en el mismo frame. Ignorado.");
            return;
        }

        lastPauseToggleFrame = Time.frameCount;
        Debug.Log($"[GameManager] TogglePause llamado desde state={currentState}");

        if (currentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
        else if (currentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
        else
        {
            Debug.LogWarning($"[GameManager] TogglePause ignorado en state={currentState}");
        }
    }

    /// <summary>
    /// Pausa el juego si está en ejecución.
    /// </summary>
    public void Pause()
    {
        if (currentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
    }

    /// <summary>
    /// Reanuda el juego si está pausado.
    /// </summary>
    public void Resume()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
    }

    #endregion

    #region Progresión del Juego

    /// <summary>
    /// Inicia una nueva partida reiniciando todos los contadores y sistemas relevantes.
    /// </summary>
    public void StartNewRun()
    {
        currentLevel = 1;
        roomsCleared = 0;
        enemiesKilled = 0;

        if (RoomFlowController.Instance != null)
        {
            RoomFlowController.Instance.ResetRunProgress();
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        
        if (DamageBoostManager.Instance != null)
        {
            DamageBoostManager.Instance.ResetBoost();
        }

        ChangeState(GameState.Playing);
    }

    /// <summary>
    /// Registra la limpieza de una sala y otorga puntos correspondientes.
    /// </summary>
    public void NotifyRoomCleared()
    {
        roomsCleared++;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.Add(100);
        }

        OnRoomCleared?.Invoke(roomsCleared);

        if (roomsCleared >= roomsBeforeBoss)
        {
            Debug.Log("[GameManager] ¡Preparando sala del BOSS!");
        }
    }

    /// <summary>
    /// Registra la eliminación de un enemigo y otorga puntos.
    /// </summary>
    /// <param name="scoreValue">Puntos otorgados por el enemigo eliminado.</param>
    public void NotifyEnemyKilled(int scoreValue = 10)
    {
        enemiesKilled++;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.Add(scoreValue);
        }

        OnEnemyKilled?.Invoke(enemiesKilled);
    }

    /// <summary>
    /// Procesa la victoria del jugador otorgando bonificación y registrando el resultado.
    /// </summary>
    public void Victory()
    {
        ChangeState(GameState.Victory);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.Add(1000);
        }

        if (!runResultRecorded && ScoreManager.Instance != null)
        {
            runResultRecorded = true;
            ScoreManager.Instance.RecordRunAndSave(
                result: "Victory",
                roomsCleared: roomsCleared,
                enemiesKilled: enemiesKilled,
                playerClass: TryGetPlayerClassName()
            );
        }
    }

    /// <summary>
    /// Procesa la muerte del jugador registrando el resultado y retornando al menú principal.
    /// </summary>
    public void GameOver()
    {
        ChangeState(GameState.GameOver);

        ShowDeathOverlay("Has muerto");

        if (!runResultRecorded && ScoreManager.Instance != null)
        {
            runResultRecorded = true;
            ScoreManager.Instance.RecordRunAndSave(
                result: "GameOver",
                roomsCleared: roomsCleared,
                enemiesKilled: enemiesKilled,
                playerClass: TryGetPlayerClassName()
            );
        }

        if (returnToMenuCoroutine != null)
        {
            StopCoroutine(returnToMenuCoroutine);
            returnToMenuCoroutine = null;
        }

        if (!returnToMainMenuOnGameOver)
        {
            Debug.LogWarning("[GameManager] returnToMainMenuOnGameOver está desactivado, pero se volverá al menú igualmente para evitar bloqueo.");
        }

        returnToMenuCoroutine = StartCoroutine(ReturnToMenuAfterDelayRealtime());
    }

    /// <summary>
    /// Muestra un overlay oscuro con mensaje de muerte en pantalla.
    /// </summary>
    /// <param name="message">Mensaje a mostrar al jugador.</param>
    private void ShowDeathOverlay(string message)
    {
        if (deathOverlayInstance != null) return;

        deathOverlayInstance = new GameObject("DeathOverlay");

        var canvas = deathOverlayInstance.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;

        var scaler = deathOverlayInstance.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        deathOverlayInstance.AddComponent<GraphicRaycaster>();

        var dimGo = new GameObject("Dim");
        dimGo.transform.SetParent(deathOverlayInstance.transform, false);
        var dim = dimGo.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.78f);
        dim.raycastTarget = false;
        var dimRt = dimGo.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;

        var textGo = new GameObject("Message");
        textGo.transform.SetParent(deathOverlayInstance.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = string.IsNullOrWhiteSpace(message) ? "Has muerto" : message;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.1f, 0.1f, 1f);
        tmp.fontSize = 72;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;

        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.pivot = new Vector2(0.5f, 0.5f);
        textRt.sizeDelta = new Vector2(1400f, 320f);
        textRt.anchoredPosition = Vector2.zero;
    }

    #endregion

    #region Navegación de Escenas

    /// <summary>
    /// Carga la escena del menú principal.
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.Menu);

        if (deathOverlayInstance != null)
        {
            Destroy(deathOverlayInstance);
            deathOverlayInstance = null;
        }

        SceneManager.LoadScene(mainMenuScene);
    }

    /// <summary>
    /// Carga la escena de selección de clase.
    /// </summary>
    public void LoadClassSelection()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.CharacterSelection);
        SceneManager.LoadScene(classSelectionScene);
    }

    /// <summary>
    /// Carga la escena de juego e inicia una nueva partida.
    /// </summary>
    public void LoadGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
        StartNewRun();
    }

    /// <summary>
    /// Reinicia la partida actual recargando la escena de juego.
    /// </summary>
    public void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
        StartNewRun();
    }

    #endregion

    #region Propiedades Públicas

    public GameState CurrentState => currentState;
    public bool IsPaused => isPaused;
    public int CurrentLevel => currentLevel;
    public int RoomsCleared => roomsCleared;
    public int EnemiesKilled => enemiesKilled;

    #endregion

    #region Métodos de Utilidad

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
    /// Obtiene el nombre de la clase del jugador actual.
    /// </summary>
    /// <returns>Nombre de la clase o "Unknown" si no se encuentra.</returns>
    private string TryGetPlayerClassName()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return "Unknown";

        var pc = playerObj.GetComponent<PlayerController>();
        if (pc != null && pc.playerClass != null)
        {
            return pc.playerClass.className;
        }

        return "Unknown";
    }

    #endregion
}

/// <summary>
/// Estados posibles del juego.
/// </summary>
public enum GameState
{
    Menu,
    CharacterSelection,
    Playing,
    Paused,
    GameOver,
    Victory
}
