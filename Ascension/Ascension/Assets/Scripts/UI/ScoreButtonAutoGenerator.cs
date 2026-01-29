using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Genera un botón "Scores" clonando el estilo/tamaño del botón "Jugar" del MainMenu.
/// Pensado para que puedas moverlo/ajustarlo a mano luego.
///
/// Uso:
/// - Añádelo a cualquier GameObject en la escena MainMenu.
/// - Asegúrate de que exista un Button con texto "Jugar" (o asigna templateButton manualmente).
/// - (Opcional) Asigna scoreboardUi para vincular el botón automáticamente.
/// </summary>
public class ScoreButtonAutoGenerator : MonoBehaviour
{
    [Header("Scope")]
    [SerializeField] private string onlyInScene = "MainMenu";

    [Header("Template")]
    [Tooltip("Si lo asignas, se clona este botón. Si no, intenta encontrar el botón cuyo texto sea 'Jugar'.")]
    [SerializeField] private Button templateButton;

    [Tooltip("Etiquetas aceptadas para detectar el botón de Jugar/Play.")]
    [SerializeField] private string[] templateLabels = new[] { "Jugar", "Play", "Start" };

    [Tooltip("Reintenta encontrar el botón plantilla durante un rato (útil si el menú se construye al inicio).")]
    [SerializeField, Min(0f)] private float retrySeconds = 1.5f;

    [Header("Output")]
    [SerializeField] private string newButtonName = "ScoresButton";
    [SerializeField] private string newButtonLabel = "Scores";

    [Tooltip("Offset respecto al botón Jugar (anchoredPosition). Ej: (0, -60) para ponerlo debajo.")]
    [SerializeField] private Vector2 anchoredOffsetFromTemplate = new Vector2(0f, -60f);

    [Tooltip("Si está activo, intenta vincular el botón al ScoreboardMenuUI si existe.")]
    [SerializeField] private bool autoBindToScoreboardUI = true;

    [SerializeField] private ScoreboardMenuUI scoreboardUi;

    private void Start()
    {
        StartCoroutine(EnsureButtonRoutine());
    }

    private System.Collections.IEnumerator EnsureButtonRoutine()
    {
        if (!string.IsNullOrWhiteSpace(onlyInScene))
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (!string.Equals(sceneName, onlyInScene, System.StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }
        }

        // Evitar duplicados
        if (GameObject.Find(newButtonName) != null)
            yield break;

        float start = Time.unscaledTime;
        while (templateButton == null && (Time.unscaledTime - start) < retrySeconds)
        {
            templateButton = FindTemplateButton();
            if (templateButton != null) break;
            yield return null;
        }

        if (templateButton == null)
        {
            Debug.LogWarning("[ScoreButtonAutoGenerator] No se encontró botón plantilla (Jugar/Play). Asigna templateButton en el inspector.");
            yield break;
        }

        var cloneGo = Instantiate(templateButton.gameObject, templateButton.transform.parent);
        cloneGo.name = newButtonName;

        // Texto (UI Text o TMP)
        var uiText = cloneGo.GetComponentInChildren<Text>(true);
        if (uiText != null) uiText.text = newButtonLabel;
        var tmpText = cloneGo.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null) tmpText.text = newButtonLabel;

        // Posición: mismo size/anchors que template, solo offset
        var templateRt = templateButton.GetComponent<RectTransform>();
        var cloneRt = cloneGo.GetComponent<RectTransform>();
        if (templateRt != null && cloneRt != null)
        {
            cloneRt.anchorMin = templateRt.anchorMin;
            cloneRt.anchorMax = templateRt.anchorMax;
            cloneRt.pivot = templateRt.pivot;
            cloneRt.sizeDelta = templateRt.sizeDelta;
            cloneRt.anchoredPosition = templateRt.anchoredPosition + anchoredOffsetFromTemplate;
            cloneRt.localRotation = templateRt.localRotation;
            cloneRt.localScale = templateRt.localScale;
        }

        // Acción: por defecto intenta llamar al MainMenuManager.ToggleScores.
        var btn = cloneGo.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();

            var menu = FindFirstObjectByType<MainMenuManager>();
            if (menu != null)
            {
                btn.onClick.AddListener(menu.ToggleScores);
            }
            else
            {
                // Fallback: buscar ScoreboardMenuUI y togglear.
                btn.onClick.AddListener(() =>
                {
                    var ui = FindFirstObjectByType<ScoreboardMenuUI>();
                    if (ui != null) ui.Toggle();
                });
            }
        }

        if (autoBindToScoreboardUI)
        {
            if (scoreboardUi == null) scoreboardUi = FindFirstObjectByType<ScoreboardMenuUI>();
            if (scoreboardUi != null && btn != null)
            {
                scoreboardUi.BindOpenButton(btn, disableAutoCreate: true);
            }
        }

        Debug.Log("[ScoreButtonAutoGenerator] Botón Scores generado.");
    }

    private Button FindTemplateButton()
    {
        // 1) Por texto (UI Text o TMP)
        if (templateLabels != null)
        {
            foreach (var label in templateLabels)
            {
                var b = FindButtonByLabel(label);
                if (b != null) return b;
            }
        }

        // 2) Fallback por nombre
        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            if (b == null) continue;
            string n = b.name ?? string.Empty;
            if (n.IndexOf("jugar", System.StringComparison.OrdinalIgnoreCase) >= 0) return b;
            if (n.IndexOf("play", System.StringComparison.OrdinalIgnoreCase) >= 0) return b;
            if (n.IndexOf("start", System.StringComparison.OrdinalIgnoreCase) >= 0) return b;
        }

        return null;
    }

    private static Button FindButtonByLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label)) return null;

        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            if (b == null) continue;

            var t = b.GetComponentInChildren<Text>(true);
            if (t != null && string.Equals(t.text?.Trim(), label, System.StringComparison.OrdinalIgnoreCase))
                return b;

            var tmp = b.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null && string.Equals(tmp.text?.Trim(), label, System.StringComparison.OrdinalIgnoreCase))
                return b;
        }
        return null;
    }
}
