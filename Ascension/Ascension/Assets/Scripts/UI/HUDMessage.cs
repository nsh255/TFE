using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Mensaje HUD simple para feedback rápido (ej. "Limpia la sala primero").
/// Se auto-crea en el primer Canvas encontrado (idealmente el HUD).
/// </summary>
public class HUDMessage : MonoBehaviour
{
    private static HUDMessage instance;

    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float defaultDuration = 1.25f;

    private Coroutine hideRoutine;

    /// <summary>
    /// Muestra un mensaje temporal en el HUD.
    /// </summary>
    /// <param name="message">Texto a mostrar</param>
    /// <param name="durationSeconds">Duración en segundos (-1 usa duración por defecto)</param>
    public static void Show(string message, float durationSeconds = -1f)
    {
        EnsureInstance();
        if (instance == null) return;

        if (durationSeconds <= 0f)
            durationSeconds = instance.defaultDuration;

        instance.InternalShow(message, durationSeconds);
    }

    /// <summary>
    /// Asegura que existe una instancia de HUDMessage en la escena.
    /// </summary>
    private static void EnsureInstance()
    {
        if (instance != null) return;

        var existing = FindFirstObjectByType<HUDMessage>();
        if (existing != null)
        {
            instance = existing;
            instance.TryEnsureLabel();
            return;
        }

        var canvas = FindHUDCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[HUDMessage] No se encontró Canvas; no se puede mostrar mensaje.");
            return;
        }

        var go = new GameObject("HUDMessage");
        go.transform.SetParent(canvas.transform, false);
        instance = go.AddComponent<HUDMessage>();
        instance.TryEnsureLabel();
    }

    /// <summary>
    /// Busca el Canvas del HUD en la jerarquía.
    /// </summary>
    /// <returns>Canvas encontrado o null</returns>
    private static Canvas FindHUDCanvas()
    {
        var hud = GameObject.Find("HUD");
        if (hud != null)
        {
            var c = hud.GetComponent<Canvas>();
            if (c != null) return c;
        }

        return FindFirstObjectByType<Canvas>();
    }

    /// <summary>
    /// Crea el componente TextMeshProUGUI si no existe.
    /// </summary>
    private void TryEnsureLabel()
    {
        if (label != null) return;

        var textGo = new GameObject("MessageText");
        textGo.transform.SetParent(transform, false);

        label = textGo.AddComponent<TextMeshProUGUI>();
        label.text = string.Empty;
        label.enabled = false;
        label.raycastTarget = false;
        label.alignment = TextAlignmentOptions.Top;
        label.fontSize = 24;

        var rt = label.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -60f);
        rt.sizeDelta = new Vector2(800f, 80f);
    }

    /// <summary>
    /// Muestra el mensaje internamente y programa su ocultación.
    /// </summary>
    private void InternalShow(string message, float durationSeconds)
    {
        TryEnsureLabel();
        if (label == null) return;

        label.text = message;
        label.enabled = true;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfter(durationSeconds));
    }

    /// <summary>
    /// Oculta el mensaje tras el tiempo especificado.
    /// </summary>
    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (label != null)
        {
            label.text = string.Empty;
            label.enabled = false;
        }
        hideRoutine = null;
    }
}
