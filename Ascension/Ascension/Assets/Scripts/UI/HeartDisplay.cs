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
    /// Inicializa el display de corazones según la vida máxima del jugador.
    /// </summary>
    /// <param name="maxHealth">Vida máxima del jugador (número de corazones a mostrar)</param>
    public void InitializeHearts(int maxHealth)
    {
        ClearHearts();
        maxHearts = maxHealth;

        for (int i = 0; i < maxHearts; i++)
        {
            CreateHeart(i);
        }
    }

    /// <summary>
    /// Actualiza el display según la vida actual.
    /// </summary>
    /// <param name="currentHealth">Vida actual del jugador (corazones llenos)</param>
    public void UpdateHearts(int currentHealth)
    {
        if (heartImages.Count == 0)
        {
            Debug.LogWarning("[HeartDisplay] No hay corazones inicializados. Llama a InitializeHearts() primero.");
            return;
        }

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].sprite = heartFull;
                heartImages[i].enabled = true;
            }
            else
            {
                heartImages[i].sprite = heartEmpty;
                heartImages[i].enabled = true;
            }
        }
    }

    /// <summary>
    /// Crea un corazón individual en la posición especificada.
    /// </summary>
    /// <param name="index">Índice del corazón (determina su posición horizontal)</param>
    private void CreateHeart(int index)
    {
        GameObject heartObj;

        if (heartPrefab != null)
        {
            heartObj = Instantiate(heartPrefab, transform);
        }
        else
        {
            heartObj = new GameObject($"Heart_{index}");
            heartObj.transform.SetParent(transform);
            heartObj.AddComponent<Image>();
        }

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
        heartImage.raycastTarget = false;
        heartImage.type = Image.Type.Simple;
        heartImage.useSpriteMesh = false;

        RectTransform heartRect = heartObj.GetComponent<RectTransform>();
        heartRect.localScale = Vector3.one * heartScale;
        
        float xPos = Mathf.Round(index * spacing);
        heartRect.anchoredPosition = new Vector2(xPos, 0);
        
        heartRect.anchorMin = new Vector2(0, 0.5f);
        heartRect.anchorMax = new Vector2(0, 0.5f);
        heartRect.pivot = new Vector2(0, 0.5f);

        heartImages.Add(heartImage);
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

    /// <summary>
    /// Destruye todos los corazones existentes y limpia la lista.
    /// </summary>
    private void ClearHearts()
    {
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
    /// Cambia el número máximo de corazones (útil para power-ups de vida).
    /// </summary>
    /// <param name="newMaxHealth">Nueva vida máxima</param>
    /// <param name="currentHealth">Vida actual</param