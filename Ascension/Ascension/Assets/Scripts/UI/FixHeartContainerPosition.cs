using UnityEngine;

/// <summary>
/// Asegura que el HeartContainer esté visible en la esquina superior izquierda.
/// Ejecuta esto una vez para corregir la posición.
/// </summary>
[ExecuteInEditMode]
public class FixHeartContainerPosition : MonoBehaviour
{
    [Header("Configuración de Posición")]
    [Tooltip("Margen desde el borde izquierdo")]
    public float leftMargin = 20f;
    
    [Tooltip("Margen desde el borde superior")]
    public float topMargin = 20f;

    void Start()
    {
        FixPosition();
    }

    [ContextMenu("Fix Position Now")]
    public void FixPosition()
    {
        RectTransform rect = GetComponent<RectTransform>();
        
        // Configurar anchors en top-left
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        
        // Posicionar con márgenes
        rect.anchoredPosition = new Vector2(leftMargin, -topMargin);
        
        Debug.Log($"[FixHeartContainer] Posición corregida: ({leftMargin}, {-topMargin})");
        Debug.Log($"Anchors: {rect.anchorMin} - {rect.anchorMax}");
        Debug.Log($"Pivot: {rect.pivot}");
    }
}
