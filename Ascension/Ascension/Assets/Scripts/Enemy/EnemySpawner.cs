using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner de enemigos configurable por sala.
/// Alternativa más simple al EnemyManager existente.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    [Tooltip("Lista de enemigos a spawnear")]
    [SerializeField] private List<EnemySpawnData> enemiesToSpawn = new List<EnemySpawnData>();

    [Tooltip("Spawnear al iniciar la escena")]
    [SerializeField] private bool spawnOnStart = false;

    [Tooltip("Spawnear cuando el jugador entra en trigger")]
    [SerializeField] private bool spawnOnPlayerEnter = true;

    [Header("Área de Spawn")]
    [Tooltip("Centro del área de spawn (0,0 usa la posición de este objeto)")]
    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero;
    
    [Tooltip("Tamaño del área de spawn")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 10f);

    [Header("Estado")]
    [SerializeField] private bool hasSpawned = false;
    private List<Enemy> spawnedEnemies = new List<Enemy>();

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (spawnOnPlayerEnter && other.CompareTag("Player") && !hasSpawned)
        {
            Debug.Log("[EnemySpawner] Jugador detectado, spawneando enemigos...");
            SpawnEnemies();
        }
    }

    /// <summary>
    /// Spawnea todos los enemigos configurados
    /// </summary>
    public void SpawnEnemies()
    {
        if (hasSpawned)
        {
            Debug.LogWarning("[EnemySpawner] Ya se spawnearon enemigos aquí");
            return;
        }

        Vector2 center = (Vector2)transform.position + spawnAreaCenter;

        foreach (EnemySpawnData spawnData in enemiesToSpawn)
        {
            for (int i = 0; i < spawnData.count; i++)
            {
                SpawnEnemy(spawnData.enemyPrefab, center);
            }
        }

        hasSpawned = true;
        Debug.Log($"[EnemySpawner] {enemiesToSpawn.Count} tipos de enemigos spawneados");
    }

    /// <summary>
    /// Spawnea un enemigo individual en posición aleatoria del área
    /// </summary>
    private void SpawnEnemy(GameObject enemyPrefab, Vector2 center)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] Prefab de enemigo es null");
            return;
        }

        // Posición aleatoria dentro del área
        Vector2 randomOffset = new Vector2(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f)
        );

        Vector2 spawnPos = center + randomOffset;

        // Instanciar enemigo
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            spawnedEnemies.Add(enemy);
        }
    }

    /// <summary>
    /// Obtiene la lista de enemigos vivos spawneados por este spawner
    /// </summary>
    public List<Enemy> GetAliveEnemies()
    {
        spawnedEnemies.RemoveAll(e => e == null || e.isDead);
        return spawnedEnemies;
    }

    /// <summary>
    /// Verifica si todos los enemigos spawneados están muertos
    /// </summary>
    public bool AreAllEnemiesDead()
    {
        spawnedEnemies.RemoveAll(e => e == null || e.isDead);
        return spawnedEnemies.Count == 0 && hasSpawned;
    }

    /// <summary>
    /// Resetea el spawner para poder usarlo de nuevo
    /// </summary>
    [ContextMenu("Reset Spawner")]
    public void ResetSpawner()
    {
        hasSpawned = false;
        spawnedEnemies.Clear();
    }

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        Vector2 center = (Vector2)transform.position + spawnAreaCenter;
        
        Gizmos.color = hasSpawned ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(center, spawnAreaSize);

        // Dibujar cruz en el centro
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center + Vector2.left * 0.5f, center + Vector2.right * 0.5f);
        Gizmos.DrawLine(center + Vector2.up * 0.5f, center + Vector2.down * 0.5f);
    }

    #endregion
}

/// <summary>
/// Configuración de spawn para un tipo de enemigo
/// </summary>
[System.Serializable]
public class EnemySpawnData
{
    [Tooltip("Prefab del enemigo a spawnear")]
    public GameObject enemyPrefab;

    [Tooltip("Cantidad a spawnear")]
    [Min(1)]
    public int count = 1;
}
