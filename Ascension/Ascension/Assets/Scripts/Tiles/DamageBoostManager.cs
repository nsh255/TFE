using System.Collections;
using UnityEngine;

/// <summary>
/// Gestiona el boost de daño global (+1 daño) cuando el jugador pisa un tile PowerUp.
/// Afecta tanto al jugador como a todos los enemigos.
/// </summary>
public class DamageBoostManager : MonoBehaviour
{
    public static DamageBoostManager Instance { get; private set; }
    
    [Header("Boost de Daño")]
    [Tooltip("Daño adicional que se suma a todos los ataques")]
    public int globalDamageBoost = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Incrementa el boost de daño global
    /// </summary>
    public void AddDamageBoost(int amount)
    {
        globalDamageBoost += amount;
        Debug.Log($"[DamageBoostManager] Boost de daño aumentado a +{globalDamageBoost}");
    }

    /// <summary>
    /// Añade un boost de daño temporal por un periodo de tiempo específico.
    /// </summary>
    public void AddDamageBoostTimed(int amount, float duration)
    {
        if (duration <= 0f)
        {
            AddDamageBoost(amount);
            return;
        }

        AddDamageBoost(amount);
        StartCoroutine(RemoveDamageBoostAfterDelay(amount, duration));
    }

    private IEnumerator RemoveDamageBoostAfterDelay(int amount, float duration)
    {
        yield return new WaitForSeconds(duration);
        globalDamageBoost -= amount;
        if (globalDamageBoost < 0) globalDamageBoost = 0;
    }
    
    /// <summary>
    /// Resetea el boost (útil al cambiar de nivel o morir)
    /// </summary>
    public void ResetBoost()
    {
        globalDamageBoost = 0;
        Debug.Log("[DamageBoostManager] Boost de daño reseteado");
    }
    
    /// <summary>
    /// Obtiene el daño final sumando el boost
    /// </summary>
    public int GetBoostedDamage(int baseDamage)
    {
        return baseDamage + globalDamageBoost;
    }
}
