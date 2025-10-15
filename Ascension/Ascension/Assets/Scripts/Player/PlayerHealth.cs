using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
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
        if (isInitialized) return;
        
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
        UpdateHearts();
        isInitialized = true;
        Debug.Log($"PlayerHealth inicializado: {currentHealth}/{maxHealth} HP");
    }

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
