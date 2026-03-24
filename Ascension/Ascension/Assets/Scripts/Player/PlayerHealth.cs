using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la salud del jugador, el sistema de daño e invulnerabilidad temporal.
/// Actualiza la interfaz de usuario de corazones y procesa la muerte del jugador.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    
    [Header("UI (Sistema Antiguo - Obsoleto)")]
    [Tooltip("Sistema antiguo de corazones - usar HeartDisplay en su lugar")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    
    [Header("UI (Sistema Nuevo - Recomendado)")]
    [Tooltip("Referencia al HeartDisplay en el Canvas")]
    public HeartDisplay heartDisplay;
    
    [Header("Invulnerabilidad")]
    public float invulnerableTime = 1f;
    private bool isInvulnerable = false;

    [Header("Feedback de Daño")]
    [Tooltip("Parpadeo blanco mientras es invulnerable")]
    [SerializeField] private bool flashWhiteOnDamage = true;
    [SerializeField, Min(1f)] private float blinkFrequencyHz = 12f;
    [Tooltip("Alterna visibilidad en cada pulso para que el parpadeo se note más")]
    [SerializeField] private bool visibilityBlinkOnDamage = true;
    
    private PlayerController playerController;
    private bool isInitialized = false;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine damageCoroutine;

    /// <summary>
    /// Obtiene la referencia al PlayerController.
    /// </summary>
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    /// <summary>
    /// Inicializa la salud del jugador basándose en la clase seleccionada y actualiza la interfaz.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) 
        {
            return;
        }
        
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (playerController.playerClass == null)
        {
            Debug.LogError("No se puede inicializar PlayerHealth sin playerClass!");
            return;
        }

        maxHealth = playerController.playerClass.maxHealth;
        currentHealth = maxHealth;
        
        if (heartDisplay != null)
        {
            heartDisplay.InitializeHearts(maxHealth);
            heartDisplay.UpdateHearts(currentHealth);
        }
        else
        {
            UpdateHeartsOldSystem();
        }
        
        isInitialized = true;
        CacheSpriteRenderers();
    }

    /// <summary>
    /// Almacena referencias a los SpriteRenderers para el efecto de parpadeo.
    /// </summary>
    private void CacheSpriteRenderers()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;
        }
    }

    /// <summary>
    /// Aplica daño al jugador y activa el periodo de invulnerabilidad.
    /// </summary>
    /// <param name="amount">Cantidad de daño a recibir.</param>
    public void TakeDamage(int amount)
    {
        if (isInvulnerable || (playerController != null && playerController.IsInvulnerable()))
            return;
            
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateHearts();
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
            damageCoroutine = StartCoroutine(DamageInvulnerabilityCoroutine());
        }
    }

    /// <summary>
    /// Gestiona el periodo de invulnerabilidad tras recibir daño con efecto visual de parpadeo.
    /// </summary>
    private System.Collections.IEnumerator DamageInvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            CacheSpriteRenderers();
        }

        float start = Time.time;
        float end = start + Mathf.Max(0f, invulnerableTime);
        float blinkPeriod = blinkFrequencyHz > 0f ? (1f / blinkFrequencyHz) : 0.1f;
        float nextToggle = 0f;
        bool white = true;
        bool visible = true;

        while (Time.time < end)
        {
            if (flashWhiteOnDamage && spriteRenderers != null)
            {
                if (Time.time >= nextToggle)
                {
                    nextToggle = Time.time + blinkPeriod;
                    white = !white;
                    visible = !visible;
                }

                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    var sr = spriteRenderers[i];
                    if (sr == null) continue;
                    var baseColor = (originalColors != null && i < originalColors.Length) ? originalColors[i] : sr.color;

                    sr.enabled = visibilityBlinkOnDamage ? visible : true;

                    if (white)
                    {
                        sr.color = new Color(1f, 1f, 1f, baseColor.a);
                    }
                    else
                    {
                        sr.color = baseColor;
                    }
                }
            }

            yield return null;
        }

        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                var sr = spriteRenderers[i];
                if (sr == null) continue;
                if (originalColors != null && i < originalColors.Length)
                {
                    sr.color = originalColors[i];
                }
                sr.enabled = true;
            }
        }

        isInvulnerable = false;
        damageCoroutine = null;
    }

    /// <summary>
    /// Restaura salud al jugador sin exceder el máximo.
    /// </summary>
    /// <param name="amount">Cantidad de salud a restaurar.</param>
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();
    }

    /// <summary>
    /// Actualiza la interfaz de usuario de salud.
    /// </summary>
    void UpdateHearts()
    {
        if (heartDisplay != null)
        {
            heartDisplay.UpdateHearts(currentHealth);
        }
        else
        {
            UpdateHeartsOldSystem();
        }
    }

    /// <summary>
    /// Actualiza la interfaz de corazones usando el sistema antiguo basado en arrays de imágenes.
    /// </summary>
    void UpdateHeartsOldSystem()
    {
        if (hearts == null || hearts.Length == 0) return;
        
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
                hearts[i].sprite = fullHeart;
            else
                hearts[i].sprite = emptyHeart;
            hearts[i].enabled = i < maxHealth;
        }
    }

    /// <summary>
    /// Procesa la muerte del jugador con parpadeo rojo antes de notificar al GameManager.
    /// </summary>
    void Die()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
        isInvulnerable = true;
        StartCoroutine(DeathBlinkCoroutine());
    }

    /// <summary>
    /// Parpadeo rojo rápido justo antes de morir para dar feedback visual de muerte.
    /// </summary>
    private System.Collections.IEnumerator DeathBlinkCoroutine()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            CacheSpriteRenderers();

        int blinks = 5;
        float interval = 0.08f;

        for (int i = 0; i < blinks; i++)
        {
            SetSpriteRenderersColor(Color.red);
            yield return new WaitForSeconds(interval);
            SetSpriteRenderersColor(Color.white);
            yield return new WaitForSeconds(interval);
        }

        // Restaurar colores originales
        RestoreOriginalColors();

        // Proceder con GameOver
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("[PlayerHealth] GameManager no encontrado. Creando uno en runtime...");
            new GameObject("GameManager").AddComponent<GameManager>();
            gm = GameManager.Instance;

            if (gm != null && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == gm.gameScene)
            {
                gm.ChangeState(GameState.Playing);
            }
        }

        if (gm != null)
        {
            gm.GameOver();
        }
        else
        {
            Debug.LogError("[PlayerHealth] No se pudo crear/encontrar GameManager. No se hará GameOver.");
        }
    }

    private void SetSpriteRenderersColor(Color color)
    {
        if (spriteRenderers == null) return;
        foreach (var sr in spriteRenderers)
        {
            if (sr != null) sr.color = color;
        }
    }

    private void RestoreOriginalColors()
    {
        if (spriteRenderers == null) return;
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null && originalColors != null && i < originalColors.Length)
                spriteRenderers[i].color = originalColors[i];
        }
    }
}
