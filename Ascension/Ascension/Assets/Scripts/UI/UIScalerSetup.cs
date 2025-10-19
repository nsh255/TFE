using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Configura automáticamente el Canvas Scaler para pixel art en diferentes resoluciones.
/// Aplica la configuración correcta para que la UI se vea bien en fullscreen.
/// </summary>
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class UIScalerSetup : MonoBehaviour
{
    [Header("Configuración UI Pixel Art")]
    [Tooltip("Resolución de referencia (tu resolución de diseño original)")]
    public Vector2 referenceResolution = new Vector2(480, 270);
    
    [Tooltip("Modo de escalado: Scale With Screen Size para UI adaptable")]
    public CanvasScaler.ScaleMode scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    
    [Tooltip("Match: 0 = match width, 0.5 = match average, 1 = match height")]
    [Range(0f, 1f)]
    public float matchWidthOrHeight = 0f; // 0 = priorizar ancho (16:9)
    
    [Tooltip("Pixel Perfect para UI nítida")]
    public bool pixelPerfect = false;

    void Awake()
    {
        ConfigureCanvasScaler();
        EnsureCanvasIsVisible();
    }

    void ConfigureCanvasScaler()
    {
        Canvas canvas = GetComponent<Canvas>();
        CanvasScaler scaler = GetComponent<CanvasScaler>();

        // Configurar Canvas
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.LogWarning($"[UIScaler] Canvas '{gameObject.name}' está en WorldSpace, no se aplicará escalado automático");
            return;
        }

        // Si está en Overlay, intentar cambiar a Screen Space - Camera para mejor compatibilidad
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = mainCam;
                canvas.planeDistance = 10;
                Debug.Log($"[UIScaler] Canvas '{gameObject.name}' cambiado a Screen Space - Camera para mejor compatibilidad con pixel art");
            }
        }

        // Configurar CanvasScaler
        scaler.uiScaleMode = scaleMode;
        
        if (scaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
            scaler.referencePixelsPerUnit = 1; // Importante para PPU=1
        }

        // Pixel Perfect (opcional, puede causar pequeños desajustes pero más nítido)
        // Para pixel art, normalmente se deja en FALSE
        canvas.pixelPerfect = pixelPerfect;

        Debug.Log($"[UIScaler] Canvas '{gameObject.name}' configurado:");
        Debug.Log($"  - Reference Resolution: {referenceResolution}");
        Debug.Log($"  - Match: {matchWidthOrHeight} (0=width, 1=height)");
        Debug.Log($"  - Scale Mode: {scaleMode}");
        Debug.Log($"  - Pixel Perfect: {pixelPerfect}");
    }

    void EnsureCanvasIsVisible()
    {
        Canvas canvas = GetComponent<Canvas>();
        
        // Verificar que el Canvas tenga sorting order correcto
        if (canvas.sortingOrder < 0)
        {
            canvas.sortingOrder = 0;
            Debug.Log($"[UIScaler] Canvas sorting order ajustado a 0");
        }
        
        // IMPORTANTE: Para pixel art UI nítido
        // Pixel Perfect debe estar ACTIVADO para UI de pixel art
        canvas.pixelPerfect = true;
        
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            // Asegurar que el escalado sea en números enteros
            scaler.referencePixelsPerUnit = 1;
        }
    }

    /// <summary>
    /// Reconfigura el scaler en runtime (útil si cambias resolución)
    /// </summary>
    public void ReconfigureScaler()
    {
        ConfigureCanvasScaler();
    }

    /// <summary>
    /// Cambia la resolución de referencia en runtime
    /// </summary>
    public void SetReferenceResolution(float width, float height)
    {
        referenceResolution = new Vector2(width, height);
        ConfigureCanvasScaler();
    }
}
