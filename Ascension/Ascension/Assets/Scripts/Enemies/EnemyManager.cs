using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    [Range(0, 10)] public int weight = 1;
}

public class EnemyManager : MonoBehaviour
{
    [Header("Spawn List (Weighted)")] public List<EnemySpawnEntry> enemies = new();
    // [Header("Refs")] public GameBalance balance; // Comentado - clase eliminada

    [Header("Spawn Safety")]
    [Tooltip("Radio en unidades de mundo alrededor del jugador donde NO pueden spawnear enemigos.")]
    [SerializeField] private float safeSpawnRadius = 2.5f;

    [Tooltip("Número máximo de intentos para encontrar un punto seguro en el área.")]
    [SerializeField] private int safeSpawnAttempts = 25;

    public event Action OnRoomCleared;

    private readonly List<Enemy> trackedEnemies = new();
    private bool roomActive;
    private bool roomCleared;
    private bool suppressClearEvent;

    public bool IsRoomCleared
    {
        get
        {
            PruneTracked();
            // Importante: al quedar la sala limpia ponemos roomCleared=true y roomActive=false.
            // IsRoomCleared debe reflejar el estado de "sala ya limpiada", no solo "sala activa".
            return roomCleared;
        }
    }

    private void Awake()
    {
        AutoConfigureEnemyListIfEmpty();
    }

    private void Update()
    {
        if (!roomActive) return;

        PruneTracked();
        if (trackedEnemies.Count == 0)
        {
            roomActive = false;
            roomCleared = true;
            if (!suppressClearEvent)
            {
                OnRoomCleared?.Invoke();
            }
        }
    }

    public void BeginRoom()
    {
        trackedEnemies.Clear();
        roomActive = true;
        roomCleared = false;
        suppressClearEvent = false;
    }

    /// <summary>
    /// Registra un enemigo instanciado externamente (por ejemplo, un boss) para que cuente
    /// en el estado de sala limpia.
    /// </summary>
    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        if (!roomActive)
        {
            // Si alguien registra un enemigo fuera del flujo normal, activar sala.
            BeginRoom();
        }
        trackedEnemies.Add(enemy);
    }

    public void RegisterEnemy(GameObject instance)
    {
        if (instance == null) return;
        RegisterEnemy(instance.GetComponent<Enemy>());
    }

    public void ClearAll()
    {
        suppressClearEvent = true;
        foreach (var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None)) Destroy(e.gameObject);
        trackedEnemies.Clear();
        roomActive = false;
        roomCleared = false;
        suppressClearEvent = false;
    }

    public void SpawnWave(Rect area, int depth)
    {
        BeginRoom();
        int count = 3 + depth; // balance eliminado, usando valor por defecto
        int spawned = 0;
        for (int i = 0; i < count; i++)
        {
            var prefab = PickWeighted();
            if (prefab == null) continue;
            var pos = GetSafeSpawnPosition(area);
            var instance = Instantiate(prefab, pos, Quaternion.identity);
            TrackIfEnemy(instance);
            spawned++;
        }

        if (spawned == 0)
        {
            // No dejamos al jugador bloqueado.
            roomActive = false;
            roomCleared = true;
            Debug.LogWarning("[EnemyManager] SpawnWave no instanció enemigos; sala marcada como limpia.");
        }
    }

    /// <summary>
    /// Fill a room by total enemy cost (using EnemyData.enemyCost) up to maxCost.
    /// Chooses random weighted enemy prefabs; skips those whose cost would exceed remaining budget.
    /// </summary>
    public void SpawnByCost(Rect area, int maxCost, int depth)
    {
        BeginRoom();
        if (enemies.Count == 0)
        {
            roomActive = false;
            roomCleared = true;
            Debug.LogWarning("[EnemyManager] Lista de enemigos vacía; sala marcada como limpia.");
            return;
        }
        int remaining = maxCost;
        int safety = 200; // avoid infinite loop

        // Precompute costs
        var available = new List<(GameObject prefab, int cost, int weight)>();
        foreach (var e in enemies)
        {
            if (e.prefab == null) continue;
            var enemyComponent = e.prefab.GetComponent<Enemy>();
            int cost = 1;
            if (enemyComponent != null && enemyComponent.enemyData != null)
                cost = Mathf.Max(1, enemyComponent.enemyData.enemyCost);
            available.Add((e.prefab, cost, Mathf.Max(1, e.weight)));
        }
        if (available.Count == 0)
        {
            roomActive = false;
            roomCleared = true;
            Debug.LogWarning("[EnemyManager] Ningún prefab válido para spawnear; sala marcada como limpia.");
            return;
        }

        int minCost = int.MaxValue;
        foreach (var a in available) minCost = Mathf.Min(minCost, a.cost);

        int spawned = 0;
        while (remaining >= minCost && safety-- > 0)
        {
            // Build weighted selection filtered by remaining cost
            int totalWeight = 0;
            foreach (var a in available) if (a.cost <= remaining) totalWeight += a.weight;
            if (totalWeight == 0) break;
            int roll = Random.Range(0, totalWeight);
            GameObject chosen = null; int chosenCost = 0;
            foreach (var a in available)
            {
                if (a.cost > remaining) continue;
                roll -= a.weight;
                if (roll < 0)
                {
                    chosen = a.prefab; chosenCost = a.cost; break;
                }
            }
            if (chosen == null) break;
            var pos = GetSafeSpawnPosition(area);
            var instance = Instantiate(chosen, pos, Quaternion.identity);
            TrackIfEnemy(instance);
            remaining -= chosenCost;
            spawned++;
        }

        Debug.Log($"[EnemyManager] SpawnByCost: spawned={spawned}, maxCost={maxCost}, remaining={remaining}, depth={depth}, area={area}");

        if (spawned == 0)
        {
            // Si no spawneó nada, no bloqueamos la progresión.
            roomActive = false;
            roomCleared = true;
        }
    }

    private Vector2 GetSafeSpawnPosition(Rect area)
    {
        Vector2 fallback = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));

        if (safeSpawnRadius <= 0f || safeSpawnAttempts <= 0)
            return fallback;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return fallback;

        Vector2 playerPos = player.transform.position;
        float minDistSqr = safeSpawnRadius * safeSpawnRadius;

        for (int i = 0; i < safeSpawnAttempts; i++)
        {
            Vector2 candidate = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
            if ((candidate - playerPos).sqrMagnitude >= minDistSqr)
                return candidate;
        }

        // Fallback: no se encontró punto seguro; devolver el aleatorio inicial
        return fallback;
    }

    private void TrackIfEnemy(GameObject instance)
    {
        if (instance == null) return;
        var enemy = instance.GetComponent<Enemy>();
        if (enemy == null) return;
        trackedEnemies.Add(enemy);

        // Asegurar efectos de tiles en enemigos
        if (instance.GetComponent<EnemyTileEffectReceiver>() == null)
        {
            instance.AddComponent<EnemyTileEffectReceiver>();
        }
    }

    private void PruneTracked()
    {
        trackedEnemies.RemoveAll(e => e == null || e.isDead);
    }

    private void AutoConfigureEnemyListIfEmpty()
    {
        if (enemies == null) enemies = new List<EnemySpawnEntry>();
        if (enemies.Count > 0) return;

        var prefabs = Resources.LoadAll<GameObject>("Enemies");
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("[EnemyManager] No se encontraron prefabs en Resources/Enemies; no se spawnearán enemigos.");
            return;
        }

        foreach (var prefab in prefabs)
        {
            if (prefab == null) continue;
            if (prefab.name.IndexOf("boss", StringComparison.OrdinalIgnoreCase) >= 0) continue;
            if (prefab.GetComponent<Enemy>() == null) continue;
            enemies.Add(new EnemySpawnEntry { prefab = prefab, weight = 1 });
        }

        Debug.Log($"[EnemyManager] Auto-configurado: {enemies.Count} enemigos cargados desde Resources/Enemies.");
    }

    private GameObject PickWeighted()
    {
        int total = 0; foreach (var e in enemies) total += Mathf.Max(0, e.weight);
        if (total <= 0) return null;
        int roll = Random.Range(0, total);
        foreach (var e in enemies)
        {
            roll -= Mathf.Max(0, e.weight);
            if (roll < 0) return e.prefab;
        }
        return enemies.Count > 0 ? enemies[0].prefab : null;
    }
}
