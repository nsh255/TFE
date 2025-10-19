using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Asegura que solo haya un EventSystem en la escena.
/// Útil cuando cargas múltiples escenas con UI.
/// </summary>
public class SingletonEventSystem : MonoBehaviour
{
    void Awake()
    {
        // Buscar todos los EventSystems en la escena
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        if (eventSystems.Length > 1)
        {
            // Si hay más de uno, destruir este
            Debug.LogWarning($"[SingletonEventSystem] Se encontraron {eventSystems.Length} EventSystems. Destruyendo el duplicado.");
            Destroy(gameObject);
        }
        else
        {
            // Mantener este vivo entre escenas
            DontDestroyOnLoad(gameObject);
            Debug.Log("[SingletonEventSystem] EventSystem único configurado");
        }
    }
}
