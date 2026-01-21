using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla una sala individual: spawn de enemigos, puertas, y detección de limpieza.
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("Configuración de Sala")]
    [SerializeField] private RoomType roomType = RoomType.Normal;
    [SerializeField] private bool isCleared = false;
    
    [Header("Spawn de Enemigos")]
    [Tooltip("EnemyManager que spawnea los enemigos de esta sala")]
    [SerializeField] private EnemyManager enemyManager;
    
    [Tooltip("Área donde se spawnean enemigos (centrada en esta sala)")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(20f, 10f);
    
    [Tooltip("Número de enemigos a spawnear (si no se usa costo)")]
    [SerializeField] private int enemyCount = 5;
    
    [Tooltip("Usar sistema de costo en lugar de cantidad fija")]
    [SerializeField] private bool useEnemyCost = true;
    
    [Tooltip("Presupuesto de costo para enemigos")]
    [SerializeField] private int enemyCostBudget = 10;
    
    [Header("Puertas")]
    [SerializeField] private List<Door> doors = new List<Door>();
    
    [Header("Estado")]
    [SerializeField] private bool playerInside = false;
    [SerializeField] private bool enemiesSpawned = false;
    private List<Enemy> spawnedEnemies = new List<Enemy>();

    void Start()
    {
        // Auto-buscar EnemyManager si no está asignado
        if (enemyManager == null)
        {
            enemyManager = FindFirstObjectByType<EnemyManager>();
            if (enemyManager == null)
            {
                Debug.LogWarning("[RoomController] No se encontró EnemyManager en la escena");
            }
        }

        // Auto-buscar puertas si la lista está vacía
        if (doors.Count == 0)
        {
            Door[] foundDoors = GetComponentsInChildren<Door>();
            doors.AddRange(foundDoors);
            Debug.Log($"[RoomController] {foundDoors.Length} puertas encontradas automáticamente");
        }

        // Cerrar todas las puertas al inicio
        CloseAllDoors();
    }

    void Update()
    {
        // Si el jugador está dentro y no se han spawneado enemigos, spawnear
        if (playerInside && !enemiesSpawned && !isCleared)
        {
            SpawnEnemies();
        }

        // Si hay enemigos spawneados, verificar si todos están muertos
        if (enemiesSpawned && !isCleared)
        {
            CheckIfRoomCleared();
        }
    }

    /// <summary>
    /// Detecta cuando el jugador entra en la sala
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[RoomController] Jugador entró en la sala");
            playerInside = true;

            // Cerrar puertas al entrar (si no está limpia)
            if (!isCleared)
            {
                CloseAllDoors();
            }
        }
    }

    /// <summary>
    /// Detecta cuando el jugador sale de la sala
    /// </summary>
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[RoomController] Jugador salió de la sala");
            playerInside = false;
        }
    }

    /// <summary>
    /// Spawnea enemigos en la sala
    /// </summary>
    private void SpawnEnemies()
    {
        if (enemyManager == null)
        {
            Debug.LogError("[RoomController] No hay EnemyManager asignado. No se pueden spawnear enemigos.");
            enemiesSpawned = true; // Marcar como spawneados para no intentar de nuevo
            return;
        }

        Debug.Log($"[RoomController] Spawneando enemigos en sala tipo {roomType}");

        // Calcular área de spawn centrada en esta sala
        Rect spawnArea = new Rect(
            transform.position.x - spawnAreaSize.x / 2f,
            transform.position.y - spawnAreaSize.y / 2f,
            spawnAreaSize.x,
            spawnAreaSize.y
        );

        // Spawnear según configuración
        if (useEnemyCost)
        {
            // Sistema de costo
            int depth = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1;
            enemyManager.SpawnByCost(spawnArea, enemyCostBudget, depth);
        }
        else
        {
            // Sistema de cantidad fija
            int depth = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1;
            enemyManager.SpawnWave(spawnArea, depth);
        }

        // Encontrar todos los enemigos spawneados
        StartCoroutine(FindSpawnedEnemiesDelayed());

        enemiesSpawned = true;
    }

    /// <summary>
    /// Encuentra enemigos spawneados después de un pequeño delay
    /// (necesario porque Instantiate no es inmediato)
    /// </summary>
    private System.Collections.IEnumerator FindSpawnedEnemiesDelayed()
    {
        yield return new WaitForSeconds(0.1f);

        // Buscar todos los enemigos en el área de spawn
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        spawnedEnemies.Clear();

        Rect spawnArea = new Rect(
            transform.position.x - spawnAreaSize.x / 2f,
            transform.position.y - spawnAreaSize.y / 2f,
            spawnAreaSize.x,
            spawnAreaSize.y
        );

        foreach (Enemy enemy in allEnemies)
        {
            if (spawnArea.Contains(enemy.transform.position))
            {
                spawnedEnemies.Add(enemy);
            }
        }

        Debug.Log($"[RoomController] {spawnedEnemies.Count} enemigos encontrados en la sala");
    }

    /// <summary>
    /// Verifica si todos los enemigos están muertos
    /// </summary>
    private void CheckIfRoomCleared()
    {
        // Limpiar referencias nulas
        spawnedEnemies.RemoveAll(enemy => enemy == null || enemy.isDead);

        // Si no quedan enemigos vivos, la sala está limpia
        if (spawnedEnemies.Count == 0)
        {
            OnRoomCleared();
        }
    }

    /// <summary>
    /// Se llama cuando la sala ha sido limpiada de enemigos
    /// </summary>
    private void OnRoomCleared()
    {
        if (isCleared) return; // Ya estaba limpia

        isCleared = true;
        Debug.Log("[RoomController] ¡Sala limpiada! Abriendo puertas...");

        // Abrir todas las puertas
        OpenAllDoors();

        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyRoomCleared();
        }
    }

    #region Control de Puertas

    /// <summary>
    /// Cierra todas las puertas de la sala
    /// </summary>
    public void CloseAllDoors()
    {
        foreach (Door door in doors)
        {
            if (door != null)
            {
                door.Close();
            }
        }
    }

    /// <summary>
    /// Abre todas las puertas de la sala
    /// </summary>
    public void OpenAllDoors()
    {
        foreach (Door door in doors)
        {
            if (door != null)
            {
                door.Open();
            }
        }
    }

    /// <summary>
    /// Registra una puerta manualmente
    /// </summary>
    public void RegisterDoor(Door door)
    {
        if (!doors.Contains(door))
        {
            doors.Add(door);
        }
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Fuerza el spawn de enemigos (útil para testing)
    /// </summary>
    [ContextMenu("Force Spawn Enemies")]
    public void ForceSpawnEnemies()
    {
        if (!enemiesSpawned)
        {
            SpawnEnemies();
        }
    }

    /// <summary>
    /// Marca la sala como limpia manualmente
    /// </summary>
    [ContextMenu("Force Clear Room")]
    public void ForceClearRoom()
    {
        OnRoomCleared();
    }

    #endregion

    #region Gizmos para Visualización

    void OnDrawGizmosSelected()
    {
        // Dibujar área de spawn
        Gizmos.color = isCleared ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0));
        
        // Dibujar trigger de la sala
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }

    #endregion

    #region Propiedades Públicas

    public bool IsCleared => isCleared;
    public RoomType RoomType => roomType;

    #endregion
}

/// <summary>
/// Tipos de salas disponibles
/// </summary>
public enum RoomType
{
    Normal,     // Sala normal con enemigos
    Boss,       // Sala del jefe final
    Treasure,   // Sala de tesoro (sin enemigos)
    Shop,       // Sala de tienda
    Start       // Sala de inicio (sin enemigos)
}
