using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el display visual de corazones en la UI.
/// Muestra los corazones llenos/vacíos según la vida del jugador.
/// </summary>
public class HeartDisplay : MonoBehaviour
{
    [Header("Configuración de Corazones")]
    [Tooltip("Sprite del corazón lleno")]
    public Sprite heartFull;
    
    [Tooltip("Sprite del corazón vacío")]
    public Sprite heartEmpty;
    
    [Tooltip("Prefab del corazón (Image component)")]
    public GameObject heartPrefab;
    
    [Header("Layout")]
    [Tooltip("Espaciado entre corazones")]
    public float spacing = 3f; // Reducido de 10
    
    [Tooltip("Escala de los corazones")]
    public float heartScale = 0.1f; // Reducido de 1 para pixel art UI

    private List<Image> heartImages = new List<Image>();
    private int maxHearts = 0;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Inicializa el display de corazones según la vida máxima del jugador
    /// </summary>
    public void InitializeHearts(int maxHealth)
    {
        Debug.Log($"[HeartDisplay] InitializeHearts llamado con maxHealth={maxHealth}");
        Debug.Log($"[HeartDisplay] heartFull={heartFull?.name ?? "NULL"}, heartEmpty={heartEmpty?.name ?? "NULL"}");
        
        // Limpiar corazones existentes
        ClearHearts();

        maxHearts = maxHealth;

        // Crear corazones
        for (int i = 0; i < maxHearts; i++)
        {
            CreateHeart(i);
        }

        Debug.Log($"[HeartDisplay] {maxHearts} corazones inicializados. Total images creadas: {heartImages.Count}");
    }

    /// <summary>
    /// Actualiza el display según la vida actual
    /// </summary>
    public void UpdateHearts(int currentHealth)
    {
        if (heartImages.Count == 0)
        {
            Debug.LogWarning("[HeartDisplay] No hay corazones inicializados. Llama a InitializeHearts() primero.");
            return;
        }

        // Actualizar cada corazón
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHealth)
            {
                // Corazón lleno
                heartImages[i].sprite = heartFull;
                heartImages[i].enabled = true;
            }
            else
            {
                // Corazón vacío
                heartImages[i].sprite = heartEmpty;
                heartImages[i].enabled = true;
            }
        }

        Debug.Log($"[HeartDisplay] Corazones actualizados: {currentHealth}/{maxHearts}");
    }

    private void CreateHeart(int index)
    {
        Debug.Log($"[HeartDisplay] CreateHeart({index}) iniciando...");
        
        GameObject heartObj;

        // Crear corazón desde prefab o generar uno nuevo
        if (heartPrefab != null)
        {
            heartObj = Instantiate(heartPrefab, transform);
            Debug.Log($"[HeartDisplay] Corazón creado desde prefab");
        }
        else
        {
            // Crear corazón básico si no hay prefab
            heartObj = new GameObject($"Heart_{index}");
            heartObj.transform.SetParent(transform);
            heartObj.AddComponent<Image>();
            Debug.Log($"[HeartDisplay] Corazón creado proceduralmente (no hay prefab)");
        }

        // Configurar el Image component
        Image heartImage = heartObj.GetComponent<Image>();
        if (heartImage == null)
        {
            heartImage = heartObj.AddComponent<Image>();
        }

        if (heartFull == null)
        {
            Debug.LogError($"[HeartDisplay] heartFull sprite es NULL! No se puede mostrar el corazón {index}");
        }
        
        heartImage.sprite = heartFull;
        heartImage.preserveAspect = true;
        heartImage.raycastTarget = false; // Optimización
        
        // IMPORTANTE: Para pixel art nítido en UI
        heartImage.type = Image.Type.Simple;
        heartImage.useSpriteMesh = false;

        // Configurar RectTransform
        RectTransform heartRect = heartObj.GetComponent<RectTransform>();
        heartRect.localScale = Vector3.one * heartScale;
        
        // Posicionamiento horizontal (de izquierda a derecha)
        // IMPORTANTE: Redondear a píxeles para evitar distorsión
        float xPos = Mathf.Round(index * spacing);
        heartRect.anchoredPosition = new Vector2(xPos, 0);
        
        heartRect.anchorMin = new Vector2(0, 0.5f);
        heartRect.anchorMax = new Vector2(0, 0.5f);
        heartRect.pivot = new Vector2(0, 0.5f);

        // Añadir a la lista
        heartImages.Add(heartImage);
        
        Debug.Log($"[HeartDisplay] Corazón {index} creado en posición ({xPos}, 0)");
    }

    private void ClearHearts()
    {
        // Destruir todos los corazones existentes
        foreach (Image heart in heartImages)
        {
            if (heart != null)
            {
                Destroy(heart.gameObject);
            }
        }

        heartImages.Clear();
        maxHearts = 0;
    }

    /// <summary>
    /// Cambia el número máximo de corazones (útil para power-ups)
    /// </summary>
    public void SetMaxHearts(int newMaxHealth, int currentHealth)
    {
        InitializeHearts(newMaxHealth);
        UpdateHearts(currentHealth);
    }
}
