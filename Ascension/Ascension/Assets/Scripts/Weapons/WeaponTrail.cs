using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [Tooltip("Duración en segundos que permanece cada copia del rastro")]
    public float trailDuration = 0.15f;
    
    [Tooltip("Intervalo entre copias del rastro (menor = más denso)")]
    public float trailInterval = 0.05f;
    
    [Tooltip("Color del rastro (alfa determina transparencia)")]
    public Color trailColor = new Color(1f, 1f, 1f, 0.5f);
    
    [Tooltip("Escala del rastro respecto al sprite original")]
    public float trailScale = 1f;

    private SpriteRenderer spriteRenderer;
    private bool isTrailActive = false;
    private float lastTrailTime;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isTrailActive && Time.time >= lastTrailTime + trailInterval)
        {
            CreateTrailSprite();
            lastTrailTime = Time.time;
        }
    }

    public void EnableTrail()
    {
        isTrailActive = true;
        lastTrailTime = Time.time;
    }

    public void DisableTrail()
    {
        isTrailActive = false;
    }

    void CreateTrailSprite()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        // Crear objeto temporal para el rastro
        GameObject trailObj = new GameObject("WeaponTrail");
        trailObj.transform.position = transform.position;
        trailObj.transform.rotation = transform.rotation;
        trailObj.transform.localScale = transform.localScale * trailScale;

        // Añadir SpriteRenderer y copiar propiedades
        SpriteRenderer trailSR = trailObj.AddComponent<SpriteRenderer>();
        trailSR.sprite = spriteRenderer.sprite;
        trailSR.color = trailColor;
        trailSR.sortingLayerName = spriteRenderer.sortingLayerName;
        trailSR.sortingOrder = spriteRenderer.sortingOrder - 1; // Detrás del arma original

        // Añadir componente para destruir automáticamente
        TrailFade trailFade = trailObj.AddComponent<TrailFade>();
        trailFade.fadeDuration = trailDuration;
    }
}

// Componente auxiliar para desvanecer y destruir el rastro
public class TrailFade : MonoBehaviour
{
    public float fadeDuration = 0.15f;
    private SpriteRenderer spriteRenderer;
    private float startAlpha;
    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startAlpha = spriteRenderer.color.a;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / fadeDuration;

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
        else
        {
            // Fade out gradual
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(startAlpha, 0f, progress);
            spriteRenderer.color = color;
        }
    }
}
