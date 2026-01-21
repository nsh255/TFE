using UnityEngine;

public static class EnsureRoomFlowController
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateIfMissing()
    {
        // Solo tiene sentido en escenas donde exista un RoomGenerator (GameScene).
        // Evita crear un RoomFlowController en el menú y que se quede sin refs.
        var generator = Object.FindFirstObjectByType<RoomGenerator>();
        if (generator == null)
        {
            // Limpieza defensiva por si quedó alguno persistido de ejecuciones previas.
            var flows = Object.FindObjectsByType<RoomFlowController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (flows != null)
            {
                foreach (var f in flows)
                {
                    if (f != null) Object.Destroy(f.gameObject);
                }
            }
            return;
        }

        // Si hay duplicados en la escena de juego, limpiar extras.
        var existingFlows = Object.FindObjectsByType<RoomFlowController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (existingFlows != null && existingFlows.Length > 1)
        {
            bool keptOne = false;
            foreach (var f in existingFlows)
            {
                if (f == null) continue;
                if (!keptOne)
                {
                    keptOne = true;
                    continue;
                }
                Object.Destroy(f.gameObject);
            }
        }

        if (Object.FindFirstObjectByType<RoomFlowController>() != null)
            return;

        var go = new GameObject("RoomFlowController");
        go.AddComponent<RoomFlowController>();

        Debug.Log("[EnsureRoomFlowController] RoomFlowController creado automáticamente.");
    }
}
