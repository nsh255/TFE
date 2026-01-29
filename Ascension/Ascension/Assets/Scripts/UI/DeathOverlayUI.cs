// NOTE: Este script quedó DEPRECADO.
// El overlay de muerte final se crea desde GameManager.ShowDeathOverlay()
// para evitar problemas de referencias/ensamblados.
//
// Si en el futuro quieres volver a usarlo, elimina el #if false.

#if false
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Overlay simple de muerte: pantalla oscura + texto rojo centrado.
/// Se auto-crea en runtime y se destruye al cambiar de escena.
/// </summary>
public class DeathOverlayUI : MonoBehaviour
{
    private static DeathOverlayUI instance;

    [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.78f;
    [SerializeField] private string message = "Has muerto";
    [SerializeField] private int fontSize = 64;

    private Canvas canvas;
    private Image dimImage;
    private TMP_Text messageText;

    public static void Show(string messageOverride = null)
    {
        if (instance == null)
        {
            var go = new GameObject("DeathOverlayUI");
            instance = go.AddComponent<DeathOverlayUI>();
        }

        if (!string.IsNullOrWhiteSpace(messageOverride))
        {
            instance.message = messageOverride;
        }

        instance.BuildIfNeeded();
        instance.gameObject.SetActive(true);
        instance.ApplyValues();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // No queremos que se quede en menús u otras escenas.
        Destroy(gameObject);
    }

    private void BuildIfNeeded()
    {
        if (canvas != null && dimImage != null && messageText != null) return;

        canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        if (gameObject.GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Dim panel
        var dimGo = new GameObject("Dim");
        dimGo.transform.SetParent(transform, false);
        dimImage = dimGo.AddComponent<Image>();
        dimImage.raycastTarget = false;

        var dimRt = dimGo.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;

        // Text
        var textGo = new GameObject("Message");
        textGo.transform.SetParent(transform, false);
        messageText = textGo.AddComponent<TextMeshProUGUI>();
        messageText.raycastTarget = false;
        messageText.alignment = TextAlignmentOptions.Center;

        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.pivot = new Vector2(0.5f, 0.5f);
        textRt.sizeDelta = new Vector2(1200f, 300f);
        textRt.anchoredPosition = Vector2.zero;
    }

    private void ApplyValues()
    {
        if (dimImage != null) dimImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(dimAlpha));
        if (messageText != null)
        {
            messageText.text = string.IsNullOrWhiteSpace(message) ? "Has muerto" : message;
            messageText.fontSize = Mathf.Clamp(fontSize, 14, 200);
            messageText.color = new Color(1f, 0.1f, 0.1f, 1f);
        }
    }
}

#endif
