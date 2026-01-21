using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el loop de "misma escena":
/// - Genera/regenera la sala (suelo)
/// - Spawnea enemigos
/// - Permite avanzar cuando la sala está limpia
/// </summary>
public class RoomFlowController : MonoBehaviour
{
    public static RoomFlowController Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private RoomGenerator roomGenerator;
    [SerializeField] private EnemyManager enemyManager;

    [Header("Boss")]
    [Tooltip("Candidatos a boss (se elige uno random). Si está vacío, se auto-carga desde Resources/Enemies (SlimeGreen/Red/Blue).")]
    [SerializeField] private System.Collections.Generic.List<GameObject> bossCandidates = new();

    [Tooltip("Cada cuántas salas aparece un boss (p.ej. 4 = boss en la 4, 8, 12...).")]
    [SerializeField] private int bossEveryRooms = 4;

    [Tooltip("Multiplicador de escala del boss provisional (slime grande).")]
    [SerializeField] private float bossScaleMultiplier = 2.4f;

    [Tooltip("Multiplicador de vida del boss provisional.")]
    [SerializeField] private float bossHealthMultiplier = 8f;

    [Tooltip("Activar bosses periódicos (sin necesidad de prefab/sprite nuevo).")]
    [SerializeField] private bool spawnPeriodicBosses = true;

    [Tooltip("Mensaje mostrado al iniciar una sala de boss.")]
    [SerializeField] private string bossMessage = "";

    [Header("Spawn")]
    [SerializeField] private int spawnPaddingTiles = 2;
    [SerializeField] private int baseSpawnCost = 5;
    [SerializeField] private int spawnCostPerRoom = 2;
    [SerializeField] private bool spawnOnStart = true;

    private int roomDepth = 1;
    private bool isBossRoom;

    private string lastBossName;

    private string lastSceneName;
    private bool startedRoomInCurrentScene;
    private bool startingRoomInCurrentScene;

    public bool CanAdvance => enemyManager != null && enemyManager.IsRoomCleared;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[RoomFlowController] Duplicado detectado. Destruyendo instancia extra.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ResolveRefs(logIfMissing: true);

        if (bossEveryRooms < 2)
        {
            Debug.LogWarning($"[RoomFlowController] bossEveryRooms estaba en {bossEveryRooms}. Forzando a 4 para evitar bosses demasiado frecuentes.");
            bossEveryRooms = 4;
        }

        if (bossCandidates == null) bossCandidates = new System.Collections.Generic.List<GameObject>();

        // Auto-cargar candidatos por defecto si faltan (3 bosses random).
        EnsureDefaultBossCandidates();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void EnsureDefaultBossCandidates()
    {
        var prefabs = Resources.LoadAll<GameObject>("Enemies");
        if (prefabs == null || prefabs.Length == 0) return;

        // 1) Preferencia: slimes (si existen). Intentar siempre para garantizar variedad.
        AddBossCandidateByName(prefabs, "SlimeGreen");
        AddBossCandidateByName(prefabs, "SlimeRed");
        AddBossCandidateByName(prefabs, "SlimeBlue");

        // 2) Si todavía no hay variedad, usar enemigos con comportamiento distinto como "boss provisional"
        if (bossCandidates.Count < 3)
        {
            AddBossCandidateByName(prefabs, "ShooterEnemy");
            AddBossCandidateByName(prefabs, "ChaserEnemy");
            AddBossCandidateByName(prefabs, "JumperEnemy");
        }

        // 3) Fallback final: BossEnemy si existe
        if (bossCandidates.Count < 3)
        {
            AddBossCandidateByName(prefabs, "BossEnemy");
        }

        if (bossCandidates.Count == 0)
        {
            Debug.LogWarning("[RoomFlowController] No se encontraron bossCandidates en Resources/Enemies.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al ser DontDestroyOnLoad, este controller se crea en MainMenu y debe re-enlazarse en GameScene.
        lastSceneName = scene.name;
        startedRoomInCurrentScene = false;

        ResolveRefs(logIfMissing: false);

        if (spawnOnStart)
        {
            StartCoroutine(TryStartRoomWhenReady());
        }
    }

    private void ResolveRefs(bool logIfMissing)
    {
        if (roomGenerator == null) roomGenerator = FindFirstObjectByType<RoomGenerator>();
        if (enemyManager == null) enemyManager = FindFirstObjectByType<EnemyManager>();
        if (enemyManager == null) enemyManager = new GameObject("EnemyManager").AddComponent<EnemyManager>();

        if (logIfMissing && roomGenerator == null)
        {
            Debug.LogWarning("[RoomFlowController] No se encontró RoomGenerator aún (probablemente estás en un menú). Se reintentará al cargar escena.");
        }
    }

    private System.Collections.IEnumerator TryStartRoomWhenReady()
    {
        if (startedRoomInCurrentScene || startingRoomInCurrentScene) yield break;
        startingRoomInCurrentScene = true;

        // Esperar a que exista RoomGenerator y haya generado tiles.
        float timeout = 3f;
        float start = Time.time;
        while (Time.time - start < timeout)
        {
            ResolveRefs(logIfMissing: false);

            if (roomGenerator != null && roomGenerator.floorTilemap != null)
            {
                // Cuando RoomGenerator.Start haya corrido, el suelo tendrá tiles.
                if (roomGenerator.floorTilemap.GetUsedTilesCount() > 0)
                {
                    break;
                }
            }
            yield return null;
        }

        ResolveRefs(logIfMissing: true);
        if (roomGenerator == null || enemyManager == null)
        {
            startingRoomInCurrentScene = false;
            Debug.LogError($"[RoomFlowController] No se pudo iniciar sala en escena '{lastSceneName}': faltan refs.");
            yield break;
        }

        startedRoomInCurrentScene = true;
        startingRoomInCurrentScene = false;
        Debug.Log($"[RoomFlowController] Iniciando sala automáticamente en escena '{lastSceneName}'.");
        StartRoom();
    }

    private void AddBossCandidateByName(GameObject[] prefabs, string name)
    {
        if (prefabs == null) return;
        foreach (var p in prefabs)
        {
            if (p == null) continue;
            if (!p.name.Contains(name)) continue;
            if (p.GetComponent<Enemy>() == null) continue;
            if (bossCandidates.Contains(p)) return;
            bossCandidates.Add(p);
            return;
        }
    }

    private void Start()
    {
        // Si el RoomFlow es de escena (no DontDestroy), puede que SceneManager.sceneLoaded
        // ya haya disparado antes de que este componente exista. En ese caso, Start()
        // debe arrancar el flujo esperando a que el RoomGenerator haya pintado tiles.

        lastSceneName = SceneManager.GetActiveScene().name;

        if (spawnOnStart)
        {
            Debug.Log("[RoomFlowController] spawnOnStart=true -> esperando RoomGenerator para iniciar sala");
            StartCoroutine(TryStartRoomWhenReady());
        }
    }

    public void StartRoom()
    {
        if (roomGenerator == null || enemyManager == null)
        {
            ResolveRefs(logIfMissing: true);
        }

        if (roomGenerator == null || enemyManager == null)
        {
            Debug.LogWarning("[RoomFlowController] StartRoom cancelado: faltan refs (RoomGenerator/EnemyManager).");
            return;
        }

        if (roomDepth < 1) roomDepth = 1;

        Rect area = roomGenerator.GetInnerWorldRect(spawnPaddingTiles);

        Debug.Log($"[RoomFlowController] StartRoom depth={roomDepth}, bossRoom?={ShouldSpawnBossRoom()}, area={area}");

        if (ShouldSpawnBossRoom())
        {
            SpawnBossRoom(area);
            return;
        }

        int cost = Mathf.Max(1, baseSpawnCost + (roomDepth - 1) * spawnCostPerRoom);
        enemyManager.SpawnByCost(area, cost, roomDepth);
    }

    public bool TryAdvanceToNextRoom()
    {
        if (roomGenerator == null) return false;
        if (enemyManager == null) return false;
        if (!enemyManager.IsRoomCleared) return false;

        // Progresión: contar sala superada al usar stairs
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyRoomCleared();
        }

        enemyManager.ClearAll();
        roomDepth++;
        roomGenerator.RegenerateFloorKeepWalls();
        StartRoom();
        return true;
    }

    private bool ShouldSpawnBossRoom()
    {
        if (!spawnPeriodicBosses) return false;
        if (bossCandidates == null || bossCandidates.Count == 0) return false;
        if (bossEveryRooms <= 0) return false;

        int safeDepth = Mathf.Max(1, roomDepth);
        if (safeDepth < bossEveryRooms) return false;

        // Boss en la 4, 8, 12... (roomDepth empieza en 1)
        return safeDepth % bossEveryRooms == 0;
    }

    private void SpawnBossRoom(Rect area)
    {
        if (bossCandidates == null || bossCandidates.Count == 0 || enemyManager == null)
            return;

        isBossRoom = true;
        enemyManager.BeginRoom();

        // Sin texto de boss (el usuario pidió quitarlo)

        Vector2 spawnPos = PickBossSpawn(area);

        // Elegir uno random intentando no repetir el anterior.
        GameObject chosen = null;
        int attempts = Mathf.Clamp(bossCandidates.Count * 2, 2, 10);
        for (int i = 0; i < attempts; i++)
        {
            var candidate = bossCandidates[Random.Range(0, bossCandidates.Count)];
            if (candidate == null) continue;
            if (!string.IsNullOrEmpty(lastBossName) && candidate.name == lastBossName && bossCandidates.Count > 1)
            {
                continue;
            }
            chosen = candidate;
            break;
        }

        if (chosen == null)
        {
            chosen = bossCandidates[0];
        }

        lastBossName = chosen != null ? chosen.name : null;
        Debug.Log($"[RoomFlowController] BossRoom depth={roomDepth}. Candidates={bossCandidates.Count}. Chosen={(chosen != null ? chosen.name : "null")}");
        GameObject bossObj = Instantiate(chosen, spawnPos, Quaternion.identity);
        var enemy = bossObj != null ? bossObj.GetComponent<Enemy>() : null;
        if (enemy != null)
        {
            enemyManager.RegisterEnemy(enemy);
            StartCoroutine(ConfigureProvisionalBoss(enemy));
        }
    }

    private System.Collections.IEnumerator ConfigureProvisionalBoss(Enemy enemy)
    {
        if (enemy == null) yield break;

        // Esperar a que Enemy.Start inicialice currentHealth.
        yield return null;

        // Hacerlo grande
        if (bossScaleMultiplier > 0f)
        {
            enemy.transform.localScale = enemy.transform.localScale * bossScaleMultiplier;
        }

        // Si tiene spawners de proyectil (p.ej. ShooterEnemy), acelerar un poco el proyectil acorde al tamaño.
        var spawners = enemy.GetComponentsInChildren<EnemyProjectileSpawner>(true);
        if (spawners != null && spawners.Length > 0)
        {
            // Factor suave: con scale 3.5 -> ~1.22
            float scaleFactor = Mathf.Max(1f, Mathf.Pow(Mathf.Max(1f, bossScaleMultiplier), 0.15f));
            float projectileFactor = 1.15f * scaleFactor;

            foreach (var s in spawners)
            {
                if (s == null) continue;
                s.projectileSpeed *= projectileFactor;
            }
        }

        // Darle mucha vida sin mutar el ScriptableObject (solo tocar instancia)
        if (bossHealthMultiplier > 1f)
        {
            int baseHp = Mathf.Max(1, enemy.currentHealth);
            enemy.currentHealth = Mathf.RoundToInt(baseHp * bossHealthMultiplier);
        }

        // (Opcional) evitar que salga disparado por físicas
        var rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.mass = Mathf.Max(rb.mass, 3f);
        }
    }

    private Vector2 PickBossSpawn(Rect area)
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
            return area.center;

        Vector2 playerPos = playerObj.transform.position;
        Vector2[] corners =
        {
            new Vector2(area.xMin, area.yMin),
            new Vector2(area.xMin, area.yMax),
            new Vector2(area.xMax, area.yMin),
            new Vector2(area.xMax, area.yMax),
        };

        Vector2 best = area.center;
        float bestDist = -1f;
        foreach (var c in corners)
        {
            float d = (c - playerPos).sqrMagnitude;
            if (d > bestDist)
            {
                bestDist = d;
                best = c;
            }
        }

        return best;
    }
}
