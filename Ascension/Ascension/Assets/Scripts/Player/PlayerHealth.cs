using UnityEngine;
using UnityEngine.UI;

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
    
    private PlayerController playerController;
    private bool isInitialized = false;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine damageCoroutine;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        // NO inicializar aquí, esperar a que PlayerController llame a Initialize()
    }

    // Método público para inicializar después de que playerClass esté asignado
    public void Initialize()
    {
        Debug.Log("[PlayerHealth] Initialize() llamado");
        
        if (isInitialized) 
        {
            Debug.Log("[PlayerHealth] Ya estaba inicializado, saliendo...");
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
        
        Debug.Log($"[PlayerHealth] maxHealth={maxHealth}, heartDisplay={(heartDisplay != null ? "ASIGNADO" : "NULL")}");
        
        // Inicializar el nuevo sistema de HeartDisplay
        if (heartDisplay != null)
        {
            Debug.Log($"[PlayerHealth] Llamando a heartDisplay.InitializeHearts({maxHealth})");
            heartDisplay.InitializeHearts(maxHealth);
            heartDisplay.UpdateHearts(currentHealth);
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] No hay HeartDisplay asignado. Usando sistema antiguo.");
            UpdateHeartsOldSystem(); // Fallback al sistema antiguo
        }
        
        isInitialized = true;
        Debug.Log($"PlayerHealth inicializado: {currentHealth}/{maxHealth} HP");

        CacheSpriteRenderers();
    }

    private void CacheSpriteRenderers()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable || (playerController != null && playerController.IsInvulnerable()))
            return;
            
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Actualizar UI
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

        while (Time.time < end)
        {
            if (flashWhiteOnDamage && spriteRenderers != null)
            {
                if (Time.time >= nextToggle)
                {
                    nextToggle = Time.time + blinkPeriod;
                    white = !white;
                }

                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    var sr = spriteRenderers[i];
                    if (sr == null) continue;
                    var baseColor = (originalColors != null && i < originalColors.Length) ? originalColors[i] : sr.color;

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

        // Restaurar colores
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
            }
        }

        isInvulnerable = false;
        damageCoroutine = null;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();
        Debug.Log($"[PlayerHealth] Curado {amount} HP. HP actual: {currentHealth}/{maxHealth}");
    }

    void UpdateHearts()
    {
        // Sistema nuevo (preferido)
        if (heartDisplay != null)
        {
            heartDisplay.UpdateHearts(currentHealth);
        }
        else
        {
            // Fallback al sistema antiguo
            UpdateHeartsOldSystem();
        }
    }

    void UpdateHeartsOldSystem()
    {
        // Sistema antiguo compatible con el array de Image[]
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

    void Die()
    {
        Debug.Log("[PlayerHealth] Player Dead - Notificando a GameManager");
        
        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] GameManager no encontrado. Recargando escena como fallback.");
            // Fallback: recargar escena si no hay GameManager
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }
}
