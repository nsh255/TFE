using UnityEngine;

/// <summary>
/// Garantiza la persistencia del EventSystem de Unity entre cambios de escena.
/// Implementa un patrón singleton para evitar duplicados.
/// </summary>
public class PersistentEventSystem : MonoBehaviour
{
    private static PersistentEventSystem instance;

    /// <summary>
    /// Inicializa el singleton y marca el objeto como persistente entre escenas.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
