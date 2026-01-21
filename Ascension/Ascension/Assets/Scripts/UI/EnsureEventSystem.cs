using UnityEngine;
using UnityEngine.EventSystems;

public static class EnsureEventSystem
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateIfMissing()
    {
        // Importante: si la escena ya trae EventSystem, NO crear otro.
        // Incluye inactivos para evitar duplicados durante transiciones.
        var existing = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (existing != null && existing.Length > 0)
            return;

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        Object.DontDestroyOnLoad(go);

        Debug.Log("[EnsureEventSystem] EventSystem creado automáticamente.");
    }
}
