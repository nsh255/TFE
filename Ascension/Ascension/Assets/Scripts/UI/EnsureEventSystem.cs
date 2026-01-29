using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Asegura que existe un EventSystem en la escena.
/// Se ejecuta automáticamente al cargar la escena.
/// </summary>
public static class EnsureEventSystem
{
    /// <summary>
    /// Crea un EventSystem si no existe ninguno en la escena.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateIfMissing()
    {
        var existing = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (existing != null && existing.Length > 0)
            return;

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        Object.DontDestroyOnLoad(go);
    }
}
