using UnityEngine;

/// <summary>
/// Efecto visual de pulso para sprites 2D.
/// Anima la opacidad, posición y escala del sprite con efecto de pulsación.
/// Se autodestruye tras completar su tiempo de vida.
/// </summary>
public class PulseVFX : MonoBehaviour
{
    [SerializeField, Min(0.05f)] private float lifetimeSeconds = 0.9f;
    [SerializeField, Min(0f)] private float startDelaySeconds = 0f;

    [Header("Pulse")]
    [SerializeField, Min(0.1f)] private float pulseSpeed = 8f;
    [SerializeField, Range(0f, 1f)] private float alphaMin = 0.15f;
    [SerializeField, Range(0f, 1f)] private float alphaMax = 1f;

    [Header("Motion")]
    [SerializeField] private float bobAmplitude = 0.06f;
    [SerializeField] private float bobSpeed = 6f;
    [SerializeField] private float scalePulse = 0.08f;

    private SpriteRenderer[] spriteRenderers;
    private Color[] baseColors;
    private Vector3 baseLocalPos;
    private Vector3 baseLocalScale;
    private float startTime;

    /// <summary>
    /// Inicializa los componentes de renderizado y almacena los valores base.
    /// </summary>
    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        baseColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            baseColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;
        }

        baseLocalPos = transform.localPosition;
        baseLocalScale = transform.localScale;
    }

    /// <summary>
    /// Registra el tiempo de activación del efecto.
    /// </summary>
    private void OnEnable()
    {
        startTime = Time.time;
    }

    /// <summary>
    /// Establece el tiempo de vida del efecto.
    /// </summary>
    /// <param name="seconds">Duración en segundos.</param>
    public void SetLifetime(float seconds)
    {
        lifetimeSeconds = Mathf.Max(0.05f, seconds);
    }

    /// <summary>
    /// Actualiza la animación del pulso cada frame.
    /// </summary>
    private void Update()
    {
        float t = Time.time - startTime;
        if (t < startDelaySeconds)
        {
            SetAlpha(0f);
            return;
        }

        float animT = t - startDelaySeconds;

        float pulse = 0.5f + 0.5f * Mathf.Sin(animT * pulseSpeed * Mathf.PI * 2f);
        float alpha = Mathf.Lerp(alphaMin, alphaMax, pulse);
        SetAlpha(alpha);

        float bob = bobAmplitude > 0f ? Mathf.Sin(animT * bobSpeed * Mathf.PI * 2f) * bobAmplitude : 0f;
        transform.localPosition = baseLocalPos + new Vector3(0f, bob, 0f);

        float scale = 1f + (pulse - 0.5f) * 2f * scalePulse;
        transform.localScale = baseLocalScale * Mathf.Max(0.01f, scale);

        if (animT >= lifetimeSeconds)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Establece la opacidad de todos los SpriteRenderers.
    /// </summary>
    /// <param name="a">Valor de alpha entre 0 y 1.</param>
    private void SetAlpha(float a)
    {
        a = Mathf.Clamp01(a);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;
            Color c = baseColors[i];
            c.a = a;
            sr.color = c;
        }
    }
}
