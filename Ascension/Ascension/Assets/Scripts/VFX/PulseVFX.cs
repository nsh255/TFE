using UnityEngine;

/// <summary>
/// Pequeña animación para VFX 2D basados en sprites:
/// - Hace "pulse" de alpha (aparece/desaparece)
/// - Optional: pequeño bob vertical y escala
/// - Se autodestruye tras un tiempo
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

    private void OnEnable()
    {
        startTime = Time.time;
    }

    public void SetLifetime(float seconds)
    {
        lifetimeSeconds = Mathf.Max(0.05f, seconds);
    }

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
