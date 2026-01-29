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
    
    /// <summary>
    /// Configura el singleton en Awake.
    /// </summary>
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
    /// Incrementa el boost de daño global de forma permanente.
    /// </summary>
    /// <param name="amount">Cantidad de daño adicional</param>
    public void AddDamageBoost(int amount)
    {
        globalDamageBoost += amount;
    }

    /// <summary>
    /// Añade un boost de daño temporal que expira tras la duración especificada.
    /// </summary>
    /// <param name="amount">Cantidad de daño adicional</param>
    /// <param name="duration">Duración en segundos (0 = permanente)</param>
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

    /// <summary>
    /// Remueve el boost tras el delay especificado.
    /// </summary>
    private IEnumerator RemoveDamageBoostAfterDelay(int amount, float duration)
    {
        yield return new WaitForSeconds(duration);
        globalDamageBoost -= amount;
        if (globalDamageBoost < 0) globalDamageBoost = 0;
    }
    
    /// <summary>
    /// Resetea el boost a cero (útil al cambiar de nivel o morir).
    /// </summary>
    public void ResetBoost()
    {
        globalDamageBoost = 0;
    }
    
    /// <summary>
    /// Calcula el daño final sumando el boost global.
    /// </summary>
    /// <param name="baseDamage">Daño base del ataque</param>
    /// <returns>Daño total con boost aplicado</returns>
    public int GetBoostedDamage(int baseDamage)
    {
        return baseDamage + globalDamageBoost;
    }
}
