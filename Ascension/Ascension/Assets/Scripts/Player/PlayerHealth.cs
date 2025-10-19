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
    
    private PlayerController playerController;
    private bool isInitialized = false;

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
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        isInvulnerable = false;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();
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
        // Aquí puedes poner la lógica de muerte del jugador
        Debug.Log("Player Dead");
        // Por ejemplo: SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
