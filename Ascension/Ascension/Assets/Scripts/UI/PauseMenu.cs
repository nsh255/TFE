using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private bool debugPauseMenu = true;

    private bool listenersWired;
    private Canvas runtimePauseCanvas;

    /// <summary>
    /// Inicializa el menú y configura los listeners de botones.
    /// </summary>
    void Start()
    {
        EnsureRuntimeUIIfMissing();
        WireButtonsOnce();

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    /// <summary>
    /// Desuscribe eventos al destruirse.
    /// </summary>
    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        EnsureRuntimeUIIfMissing();
        WireButtonsOnce();

        if (debugPauseMenu)
        {
            Debug.Log($"[PauseMenu] ESC detectado. gmNull={(GameManager.Instance == null)} panelNull={(pauseMenuPanel == null)} panelActive={(pauseMenuPanel != null && pauseMenuPanel.activeSelf)}");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
            return;
        }

        if (pauseMenuPanel != null)
        {
            bool nextState = !pauseMenuPanel.activeSelf;
            pauseMenuPanel.SetActive(nextState);
            Time.timeScale = nextState ? 0f : 1f;

            if (debugPauseMenu)
            {
                Debug.LogWarning($"[PauseMenu] Fallback sin GameManager. Panel={(nextState ? "ON" : "OFF")}, timeScale={Time.timeScale}");
            }
        }
    }

    /// <summary>
    /// Reacciona a cambios de estado del juego mostrando/ocultando el panel.
    /// </summary>
    /// <param name="newState">Nuevo estado del juego</param>
    private void OnGameStateChanged(GameState newState)
    {
        EnsureRuntimeUIIfMissing();

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

    public void EnsureRuntimeUIIfMissing()
    {
        if (pauseMenuPanel != null) return;

        var canvasGo = new GameObject("PauseCanvasRuntime");
        runtimePauseCanvas = canvasGo.AddComponent<Canvas>();
        runtimePauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        runtimePauseCanvas.overrideSorting = true;
        runtimePauseCanvas.sortingOrder = 5000;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(480f, 270f);
        scaler.matchWidthOrHeight = 0f;
        canvasGo.AddComponent<GraphicRaycaster>();

        pauseMenuPanel = new GameObject("PauseMenuPanel");
        pauseMenuPanel.transform.SetParent(runtimePauseCanvas.transform, false);
        var panelImage = pauseMenuPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.55f);

        var panelRt = pauseMenuPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0f, 0f);
        panelRt.anchorMax = new Vector2(1f, 1f);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        var window = new GameObject("PauseWindow");
        window.transform.SetParent(pauseMenuPanel.transform, false);
        var windowImage = window.AddComponent<Image>();
        windowImage.color = new Color(0f, 0f, 0f, 0.82f);

        var windowRt = window.GetComponent<RectTransform>();
        windowRt.anchorMin = new Vector2(0.5f, 0.5f);
        windowRt.anchorMax = new Vector2(0.5f, 0.5f);
        windowRt.pivot = new Vector2(0.5f, 0.5f);
        windowRt.sizeDelta = new Vector2(250f, 150f);

        var titleObj = new GameObject("PauseTitle");
        titleObj.transform.SetParent(window.transform, false);
        var title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "PAUSA";
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 28;
        title.color = Color.white;
        ApplyMainMenuFontIfFound(title);

        var titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.1f, 0.70f);
        titleRt.anchorMax = new Vector2(0.9f, 0.95f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;

        resumeButton = CreateButton(window.transform, "ResumeButton", "Reanudar", new Vector2(0.5f, 0.48f));
        mainMenuButton = CreateButton(window.transform, "MainMenuButton", "Menu", new Vector2(0.5f, 0.27f));

        if (debugPauseMenu)
        {
            Debug.Log("[PauseMenu] UI de pausa creada automáticamente en runtime.");
        }
    }

    /// <summary>
    /// Muestra el menú de pausa manualmente.
    /// </summary>
    public void Show()
    {
        EnsureRuntimeUIIfMissing();

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

    private void WireButtonsOnce()
    {
        if (listenersWired) return;

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        listenersWired = true;
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 anchor)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.9f);

        var button = buttonObj.AddComponent<Button>();

        var rt = buttonObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(140f, 30f);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 20;
        text.color = Color.black;
        ApplyMainMenuFontIfFound(text);

        var textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return button;
    }

    private void ApplyMainMenuFontIfFound(TextMeshProUGUI text)
    {
        if (text == null) return;

        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        for (int i = 0; i < fonts.Length; i++)
        {
            var f = fonts[i];
            if (f == null) continue;
            if (f.name.IndexOf("VCR_OSD_MONO", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                text.font = f;
                return;
            }
        }
    }

    #endregion
}
