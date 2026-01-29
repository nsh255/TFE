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
              .Append(" | Rooms ").Append(e.roomsCleared)
              .Append(" | Kills ").Append(e.enemiesKilled)
              .Append(" | ").Append(dt.ToString("yyyy-MM-dd HH:mm"))
              .AppendLine();
            rank++;
        }

        listText.text = sb.ToString();
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
            // Reservar espacio arriba para la "X" de cerrar
            rt.anchorMin = new Vector2(0.05f, 0.12f);
            rt.anchorMax = new Vector2(0.95f, 0.92f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Botones cerrar / limpiar
        if (closeButton == null)
        {
            closeButton = CreateButton(panel.transform, "CloseButton", "✕", new Vector2(44, 44));
            var rt = closeButton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-16f, -16f);

            // Estilo de la X
            var closeTmp = closeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (closeTmp != null)
            {
                closeTmp.fontSize = 28;
                closeTmp.color = Color.white;
                closeTmp.fontStyle = FontStyles.Bold;
                closeTmp.alignment = TextAlignmentOptions.Center;
            }

            var closeImg = closeButton.GetComponent<Image>();
            if (closeImg != null)
            {
                // Botón oscuro, sutil
                closeImg.color = new Color(1f, 1f, 1f, 0.08f);
            }
        }

        if (clearButton == null)
        {
            clearButton = CreateButton(panel.transform, "ClearButton", "Borrar", new Vector2(140, 36));
            var rt = clearButton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(20f, 20f);
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
