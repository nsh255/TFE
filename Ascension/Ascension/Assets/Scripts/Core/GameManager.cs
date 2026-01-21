using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager central que controla el flujo del juego completo.
/// Singleton persistente que maneja estados, pausas, victoria y derrota.
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
    
    [Header("Estado de Pausa")]
    private bool isPaused = false;
    
    // Eventos para que otros sistemas reaccionen
    public delegate void GameStateChanged(GameState newState);
    public static event GameStateChanged OnGameStateChanged;
    
    public delegate void RoomCleared(int roomNumber);
    public static event RoomCleared OnRoomCleared;
    
    public delegate void EnemyKilled(int totalKills);
    public static event EnemyKilled OnEnemyKilled;

    void Awake()
    {
        // Patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Inicializado como singleton persistente");
        }
        else
        {
            Debug.LogWarning("[GameManager] Ya existe una instancia. Destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Detectar escena inicial
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
    }

    void Update()
    {
        // Input de pausa (ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
            {
                TogglePause();
            }
            else if (currentState == GameState.Paused)
            {
                TogglePause();
            }
        }
    }

    #region Estado del Juego

    /// <summary>
    /// Cambia el estado del juego y notifica a todos los listeners
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;

        Debug.Log($"[GameManager] Estado cambiado: {previousState} → {newState}");

        // Aplicar efectos del estado
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
                Time.timeScale = 0f;
                break;

            case GameState.Victory:
                Time.timeScale = 0f;
                break;
        }

        // Notificar evento
        OnGameStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// Alterna entre pausa y jugando
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
        else if (currentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
    }

    /// <summary>
    /// Pausa el juego
    /// </summary>
    public void Pause()
    {
        if (currentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
    }

    /// <summary>
    /// Reanuda el juego
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
    /// Inicia una nueva run desde cero
    /// </summary>
    public void StartNewRun()
    {
        currentLevel = 1;
        roomsCleared = 0;
        enemiesKilled = 0;

        // Resetear sistemas
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        
        if (DamageBoostManager.Instance != null)
        {
            DamageBoostManager.Instance.ResetBoost();
        }

        ChangeState(GameState.Playing);
        Debug.Log("[GameManager] Nueva run iniciada");
    }

    /// <summary>
    /// Notifica que se ha limpiado una sala
    /// </summary>
    public void NotifyRoomCleared()
    {
        roomsCleared++;
        Debug.Log($"[GameManager] Sala {roomsCleared} limpiada");

        // Sumar puntos
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.Add(100); // 100 puntos por sala
        }

        OnRoomCleared?.Invoke(roomsCleared);

        // Verificar si es hora del boss
        if (roomsCleared >= roomsBeforeBoss)
        {
            Debug.Log("[GameManager] ¡Preparando sala del BOSS!");
            // Aquí se podría cargar sala especial de boss
        }
    }

    /// <summary>
    /// Notifica que se ha matado un enemigo
    /// </summary>
    public void NotifyEnemyKilled(int scoreValue = 10)
    {
        enemiesKilled++;

        // Sumar puntos
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.Add(scoreValue);
        }

        OnEnemyKilled?.Invoke(enemiesKilled);
    }

    /// <summary>
    /// El jugador ha derrotado al boss final
    /// </summary>
    public void Victory()
    {
        Debug.Log("[GameManager] ¡VICTORIA! El jugador ha completado la run");
        ChangeState(GameState.Victory);

        // Bonus de puntos por victoria
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.Add(1000);
        }

        // TODO: Mostrar pantalla de victoria
        // VictoryScreen.Show();
    }

    /// <summary>
    /// El jugador ha muerto (HP = 0)
    /// </summary>
    public void GameOver()
    {
        Debug.Log("[GameManager] GAME OVER - El jugador ha muerto");
        ChangeState(GameState.GameOver);

        // TODO: Mostrar pantalla de game over
        // GameOverScreen.Show();
    }

    #endregion

    #region Navegación de Escenas

    /// <summary>
    /// Carga el menú principal
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Resetear timeScale
        ChangeState(GameState.Menu);
        SceneManager.LoadScene(mainMenuScene);
    }

    /// <summary>
    /// Carga la selección de personaje
    /// </summary>
    public void LoadClassSelection()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.CharacterSelection);
        SceneManager.LoadScene(classSelectionScene);
    }

    /// <summary>
    /// Carga la escena de juego
    /// </summary>
    public void LoadGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
        StartNewRun();
    }

    /// <summary>
    /// Reinicia la run actual (recarga la escena de juego)
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
    /// Cierra el juego (funciona en build, no en editor)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[GameManager] Cerrando juego...");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}

/// <summary>
/// Estados posibles del juego
/// </summary>
public enum GameState
{
    Menu,                   // En el menú principal
    CharacterSelection,     // Seleccionando personaje
    Playing,                // Jugando activamente
    Paused,                 // Juego pausado
    GameOver,               // Jugador muerto
    Victory                 // Run completada con éxito
}
