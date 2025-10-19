using UnityEngine;

/// <summary>
/// Configuración automática para pixel art perfecto en pantalla completa.
/// Este script ajusta la cámara para que los sprites 16x16 se vean nítidos y grandes.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PixelPerfectSetup : MonoBehaviour
{
    [Header("Configuración Pixel Art")]
    [Tooltip("Resolución base de diseño (tu resolución actual de trabajo)")]
    public Vector2Int referenceResolution = new Vector2Int(480, 270);
    
    [Tooltip("Factor de escala de píxeles (2 = cada pixel x2, 3 = cada pixel x3, etc.)")]
    [Range(1, 6)]
    public int pixelScale = 2;
    
    [Tooltip("Si está activo, ajusta automáticamente al cambiar la ventana")]
    public bool dynamicResolution = true;

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        if (!cam.orthographic)
        {
            Debug.LogWarning("¡La cámara debe ser ortográfica para pixel art!");
            cam.orthographic = true;
        }

        ApplyPixelPerfectSettings();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    void Update()
    {
        // Reajustar si cambia el tamaño de ventana
        if (dynamicResolution && (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight))
        {
            ApplyPixelPerfectSettings();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    void ApplyPixelPerfectSettings()
    {
        // Calcular el tamaño ortográfico correcto
        // La altura de la cámara en unidades de Unity debe ser la mitad de la resolución de referencia
        float targetHeight = referenceResolution.y / 2f;
        cam.orthographicSize = targetHeight;

        Debug.Log($"[PixelPerfect] Configuración aplicada:");
        Debug.Log($"  - Resolución base: {referenceResolution.x}x{referenceResolution.y}");
        Debug.Log($"  - Escala de píxeles: x{pixelScale}");
        Debug.Log($"  - Camera Size: {cam.orthographicSize}");
        Debug.Log($"  - Resolución pantalla: {Screen.width}x{Screen.height}");
        Debug.Log($"  - Factor de zoom efectivo: {(float)Screen.height / referenceResolution.y:F2}x");
    }

    /// <summary>
    /// Cambia el factor de escala en runtime
    /// </summary>
    public void SetPixelScale(int scale)
    {
        pixelScale = Mathf.Clamp(scale, 1, 6);
        ApplyPixelPerfectSettings();
    }

    /// <summary>
    /// Cambia la resolución de referencia en runtime
    /// </summary>
    public void SetReferenceResolution(int width, int height)
    {
        referenceResolution = new Vector2Int(width, height);
        ApplyPixelPerfectSettings();
    }
}
