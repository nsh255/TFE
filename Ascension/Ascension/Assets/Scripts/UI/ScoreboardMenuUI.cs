using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI simple para mostrar el historial de puntuaciones en el MainMenu.
/// - Si no se asignan referencias, intenta auto-crear un Canvas + botón + panel.
/// - Usa UnityEngine.UI.Text para evitar dependencias de TMP.
/// </summary>
public class ScoreboardMenuUI : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoCreateIfMissing = true;
    [SerializeField] private string onlyInScene = "MainMenu";

    [Header("UI Refs (opcional)")]
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI listText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform scrollViewport;
    [SerializeField] private RectTransform scrollContent;
    [SerializeField] private bool debugScoreboardUi = false;

    // Si ScoreboardMenuUI crea el botón, lo cableamos aquí.
    // Si el botón ya viene bakeado y llama a MainMenuManager.ToggleScores, no añadimos otro listener.
    private bool openButtonCreatedByAutoCreate;

    // Cuando el componente se crea en runtime desde el primer click, Toggle() puede ejecutarse
    // antes de Start(). En Start() no debemos cerrar el panel si ya se pidió abrir.
    private bool hasStarted;
    private bool requestedOpenBeforeStart;

    private void Start()
    {
        if (!string.IsNullOrWhiteSpace(onlyInScene))
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (!string.Equals(sceneName, onlyInScene, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        ResolveExistingOpenButtonIfAny();

        if (ScoreManager.Instance == null)
        {
            new GameObject("ScoreManager").AddComponent<ScoreManager>();
        }

        if ((panel == null || listText == null || openButton == null) && autoCreateIfMissing)
        {
            AutoCreateUI();
        }

        EnsureScrollableLayout();
        ApplyListTextStyle();

        // Por defecto el panel empieza cerrado, salvo que ya se haya pedido abrir desde Toggle()/Show()
        // antes de que Start() corra (caso: creado en runtime desde el primer click).
        if (panel != null)
        {
            if (requestedOpenBeforeStart)
            {
                panel.SetActive(true);
                Refresh();
            }
            else
            {
                panel.SetActive(false);
            }
        }

        RewireButtons();
        ApplyCloseButtonStyle();
        ApplyActionButtonSizing();

        hasStarted = true;
    }

    public void BindOpenButton(Button button, bool disableAutoCreate = true)
    {
        if (button == null) return;
        openButton = button;
        openButtonCreatedByAutoCreate = false;
        if (disableAutoCreate) autoCreateIfMissing = false;
        RewireButtons();
    }

    private void RewireButtons()
    {
        // Solo cableamos el openButton si lo creó este script.
        // Si el botón viene bakeado en escena, normalmente llama a MainMenuManager.ToggleScores,
        // y añadir otro listener aquí provocaría doble toggle (abrir y cerrar instantáneamente).
        if (openButton != null && openButtonCreatedByAutoCreate)
        {
            openButton.onClick.RemoveListener(Toggle);
            openButton.onClick.AddListener(Toggle);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(Clear);
            clearButton.onClick.AddListener(Clear);
        }
    }

    public void Toggle()
    {
        // Si se crea en runtime y se llama a Toggle() antes de Start(), aún no existe el panel.
        bool createdNow = false;
        if ((panel == null || listText == null) && autoCreateIfMissing)
        {
            createdNow = (panel == null);
            AutoCreateUI();
            RewireButtons();

            // Importante: los GameObjects nuevos empiezan activos.
            // Si acabamos de crear el panel, lo dejamos cerrado para que el primer click lo abra.
            if (createdNow && panel != null)
            {
                panel.SetActive(false);
            }
        }

        if (panel == null) return;

        bool show = !panel.activeSelf;

        if (!hasStarted)
        {
            requestedOpenBeforeStart = show;
        }

        panel.SetActive(show);
        if (show) Refresh();
    }

    public void Show()
    {
        if ((panel == null || listText == null) && autoCreateIfMissing)
        {
            AutoCreateUI();
            RewireButtons();
        }

        EnsureScrollableLayout();
        ApplyActionButtonSizing();

        if (!hasStarted)
        {
            requestedOpenBeforeStart = true;
        }

        if (panel == null) return;
        panel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }

    public void Refresh()
    {
        EnsureScrollableLayout();
        EnsureListTextExists();
        ApplyListTextStyle();
        if (listText == null) return;

        var sb = new StringBuilder();
        var entries = ScoreManager.Instance != null ? ScoreManager.Instance.GetHistory() : null;

        sb.AppendLine("SCORES");
        sb.AppendLine("------");

        if (entries == null || entries.Count == 0)
        {
            sb.AppendLine("(sin partidas guardadas)");
            listText.text = sb.ToString();
            return;
        }

        int rank = 1;
        foreach (var e in entries)
        {
            DateTime dt = new DateTime(e.utcTicks, DateTimeKind.Utc).ToLocalTime();
            sb.Append(rank).Append(". ")
              .Append(e.score).Append(" pts")
              .Append(" | ").Append(e.result)
              .Append(" | ").Append(e.playerClass)
                            .Append(" | R ").Append(e.roomsCleared)
                            .Append(" | K ").Append(e.enemiesKilled)
                            .Append(" | ").Append(dt.ToString("MM-dd HH:mm"))
              .AppendLine();
            rank++;
        }

        listText.text = sb.ToString();

        UpdateScrollContentSize();

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        if (debugScoreboardUi)
        {
            int count = entries != null ? entries.Count : 0;
            Debug.Log($"[ScoreboardMenuUI] Refresh -> entries={count}, textLength={listText.text?.Length ?? 0}, panelActive={(panel != null && panel.activeSelf)}");
        }
    }

    public void Clear()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ClearHistory();
        }
        Refresh();
    }

    private void AutoCreateUI()
    {
        // Buscar o crear Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        // Botón "Scores" (si ya existe en escena, reutilizarlo para evitar duplicados)
        ResolveExistingOpenButtonIfAny();
        if (openButton == null)
        {
            openButton = CreateButton(canvas.transform, "ScoresButton", "Scores", new Vector2(140, 40));
            openButtonCreatedByAutoCreate = true;
            var rt = openButton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 20f);
        }

        // Panel
        if (panel == null)
        {
            panel = new GameObject("ScoreboardPanel");
            panel.transform.SetParent(canvas.transform, false);
            var img = panel.AddComponent<Image>();
            // Más negro y más opaco
            img.color = new Color(0f, 0f, 0f, 0.9f);

            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.1f);
            rt.anchorMax = new Vector2(0.9f, 0.9f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Texto
        if (listText == null)
        {
            var textGo = new GameObject("ScoresText");
            textGo.transform.SetParent(panel.transform, false);
            listText = textGo.AddComponent<TextMeshProUGUI>();
            listText.fontSize = 18;
            listText.alignment = TextAlignmentOptions.TopLeft;
            listText.color = Color.white;
            listText.textWrappingMode = TextWrappingModes.Normal;
            listText.overflowMode = TextOverflowModes.Overflow;

            var rt = listText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var fitter = listText.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = listText.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // Botones cerrar / limpiar
        if (closeButton == null)
        {
            closeButton = CreateButton(panel.transform, "CloseButton", "X", new Vector2(22, 22));
            var rt = closeButton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-10f, -10f);

            // Estilo de la X
            var closeTmp = closeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (closeTmp != null)
            {
                closeTmp.fontSize = 16;
                closeTmp.color = Color.white;
                closeTmp.fontStyle = FontStyles.Bold;
                closeTmp.alignment = TextAlignmentOptions.Center;
            }

            var closeImg = closeButton.GetComponent<Image>();
            if (closeImg != null)
            {
                closeImg.color = new Color(0.86f, 0.14f, 0.14f, 0.98f);
            }
        }

        if (clearButton == null)
        {
            clearButton = CreateButton(panel.transform, "ClearButton", "Borrar", new Vector2(70, 18));
            var rt = clearButton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(10f, 10f);
        }
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.92f);

        var btn = go.AddComponent<Button>();

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        text.fontSize = 18;

        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return btn;
    }

    private void EnsureScrollableLayout()
    {
        if (panel == null) return;

        if (scrollRect == null)
        {
            scrollRect = panel.GetComponentInChildren<ScrollRect>(true);
        }

        if (scrollRect == null)
        {
            var scrollGo = new GameObject("ScoresScrollView");
            scrollGo.transform.SetParent(panel.transform, false);

            var rt = scrollGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.18f);
            rt.anchorMax = new Vector2(0.95f, 0.88f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = scrollGo.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0f);

            scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            scrollViewport = viewportGo.AddComponent<RectTransform>();
            scrollViewport.anchorMin = Vector2.zero;
            scrollViewport.anchorMax = Vector2.one;
            scrollViewport.offsetMin = Vector2.zero;
            scrollViewport.offsetMax = Vector2.zero;
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            scrollContent = contentGo.AddComponent<RectTransform>();
            scrollContent.anchorMin = new Vector2(0f, 1f);
            scrollContent.anchorMax = new Vector2(1f, 1f);
            scrollContent.pivot = new Vector2(0.5f, 1f);
            scrollContent.anchoredPosition = Vector2.zero;
            scrollContent.sizeDelta = Vector2.zero;

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            scrollRect.viewport = scrollViewport;
            scrollRect.content = scrollContent;

            scrollGo.transform.SetSiblingIndex(0);
        }
        else
        {
            if (scrollRect.viewport != null)
            {
                scrollViewport = scrollRect.viewport;
            }

            if (scrollRect.content != null)
            {
                scrollContent = scrollRect.content;
            }

            if (scrollViewport != null)
            {
                var oldMask = scrollViewport.GetComponent<Mask>();
                if (oldMask != null)
                {
                    oldMask.enabled = false;
                }

                if (scrollViewport.GetComponent<RectMask2D>() == null)
                {
                    scrollViewport.gameObject.AddComponent<RectMask2D>();
                }
            }
        }

        if (scrollContent == null && scrollViewport != null)
        {
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(scrollViewport, false);
            scrollContent = contentGo.AddComponent<RectTransform>();
            scrollContent.anchorMin = new Vector2(0f, 1f);
            scrollContent.anchorMax = new Vector2(1f, 1f);
            scrollContent.pivot = new Vector2(0.5f, 1f);
            scrollContent.anchoredPosition = Vector2.zero;
            scrollContent.sizeDelta = Vector2.zero;

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            scrollRect.content = scrollContent;
        }

        if (listText != null && scrollContent != null)
        {
            if (listText.transform.parent != scrollContent)
            {
                listText.transform.SetParent(scrollContent, false);
            }

            var textRt = listText.rectTransform;
            textRt.anchorMin = new Vector2(0f, 1f);
            textRt.anchorMax = new Vector2(1f, 1f);
            textRt.pivot = new Vector2(0.5f, 1f);
            textRt.anchoredPosition = Vector2.zero;
            textRt.sizeDelta = Vector2.zero;

            var fitter = listText.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = listText.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            listText.enableWordWrapping = false;
            listText.alignment = TextAlignmentOptions.TopLeft;
            listText.overflowMode = TextOverflowModes.Overflow;
            listText.raycastTarget = false;
        }

        ApplyListTextStyle();

        if (closeButton != null)
        {
            closeButton.transform.SetAsLastSibling();
        }

        if (clearButton != null)
        {
            clearButton.transform.SetAsLastSibling();
        }

        UpdateScrollContentSize();
    }

    private void UpdateScrollContentSize()
    {
        if (listText == null || scrollContent == null) return;

        Canvas.ForceUpdateCanvases();

        float contentHeight = Mathf.Max(60f, listText.preferredHeight + 16f);

        var textRt = listText.rectTransform;
        textRt.anchorMin = new Vector2(0f, 1f);
        textRt.anchorMax = new Vector2(1f, 1f);
        textRt.pivot = new Vector2(0.5f, 1f);
        textRt.anchoredPosition = new Vector2(0f, -8f);
        textRt.sizeDelta = new Vector2(0f, contentHeight);

        scrollContent.anchorMin = new Vector2(0f, 1f);
        scrollContent.anchorMax = new Vector2(1f, 1f);
        scrollContent.pivot = new Vector2(0.5f, 1f);
        scrollContent.anchoredPosition = Vector2.zero;
        scrollContent.sizeDelta = new Vector2(0f, contentHeight + 8f);

        if (debugScoreboardUi)
        {
            Debug.Log($"[ScoreboardMenuUI] UpdateScrollContentSize -> preferredHeight={listText.preferredHeight:F1}, contentHeight={contentHeight:F1}");
        }
    }

    private void EnsureListTextExists()
    {
        if (listText != null) return;

        if (panel == null) return;

        var existing = panel.GetComponentInChildren<TextMeshProUGUI>(true);
        if (existing != null)
        {
            listText = existing;
            if (debugScoreboardUi)
            {
                Debug.Log("[ScoreboardMenuUI] Reusing existing TextMeshProUGUI found in panel.");
            }
            return;
        }

        var parentForText = scrollContent != null ? (Transform)scrollContent : panel.transform;
        var textGo = new GameObject("ScoresText");
        textGo.transform.SetParent(parentForText, false);
        listText = textGo.AddComponent<TextMeshProUGUI>();
        listText.fontSize = 18;
        listText.alignment = TextAlignmentOptions.TopLeft;
        listText.color = Color.white;
        listText.textWrappingMode = TextWrappingModes.Normal;
        listText.overflowMode = TextOverflowModes.Overflow;
        listText.raycastTarget = false;

        var fitter = listText.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = listText.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (debugScoreboardUi)
        {
            Debug.Log("[ScoreboardMenuUI] Created fallback ScoresText (TMP). ");
        }
    }

    private void ApplyListTextStyle()
    {
        if (listText == null) return;

        listText.enableAutoSizing = true;
        listText.fontSizeMin = 10f;
        listText.fontSizeMax = 14f;
        listText.fontSize = 12f;
        listText.enableWordWrapping = true;
        listText.textWrappingMode = TextWrappingModes.Normal;
        listText.alignment = TextAlignmentOptions.TopLeft;
        listText.overflowMode = TextOverflowModes.Overflow;
        listText.raycastTarget = false;
    }

    private void ApplyCloseButtonStyle()
    {
        if (closeButton == null) return;

        var closeTmp = closeButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (closeTmp != null)
        {
            closeTmp.text = "X";
            closeTmp.fontSize = 16;
            closeTmp.color = Color.white;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.alignment = TextAlignmentOptions.Center;
        }

        var closeImg = closeButton.GetComponent<Image>();
        if (closeImg != null)
        {
            closeImg.color = new Color(0.86f, 0.14f, 0.14f, 0.98f);
        }
    }

    private void ApplyActionButtonSizing()
    {
        if (closeButton != null)
        {
            var closeRt = closeButton.GetComponent<RectTransform>();
            if (closeRt != null)
            {
                closeRt.sizeDelta = new Vector2(22f, 22f);
                closeRt.anchoredPosition = new Vector2(-10f, -10f);
            }

            var closeText = closeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (closeText != null)
            {
                closeText.fontSize = 16;
            }
        }

        if (clearButton != null)
        {
            var clearRt = clearButton.GetComponent<RectTransform>();
            if (clearRt != null)
            {
                clearRt.sizeDelta = new Vector2(70f, 18f);
                clearRt.anchoredPosition = new Vector2(10f, 10f);
            }

            var clearText = clearButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (clearText != null)
            {
                clearText.fontSize = 12;
                clearText.color = Color.black;
                clearText.alignment = TextAlignmentOptions.Center;
            }
        }
    }

    private void ResolveExistingOpenButtonIfAny()
    {
        if (openButton != null) return;

        var existing = GameObject.Find("ScoresButton");
        if (existing == null) return;

        var b = existing.GetComponent<Button>();
        if (b == null) return;

        openButton = b;
        openButtonCreatedByAutoCreate = false;
    }
}
