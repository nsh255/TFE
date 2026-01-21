using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Generador procedural de habitaciones con:
/// - Muros con colliders en los bordes
/// - Tiles de suelo aleatorios con efectos
/// 
/// Uso:
/// 1. Crea 2 Tilemaps en la escena: "WallTilemap" (con TilemapCollider2D) y "FloorTilemap"
/// 2. Crea Tiles en Assets/Tiles/ y asígnalos en el Inspector
/// 3. Llama a GenerateRoom() desde otro script o en Start
/// </summary>
public class RoomGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    [Tooltip("Tilemap para muros (debe tener TilemapCollider2D)")]
    public Tilemap wallTilemap;
    
    [Tooltip("Tilemap para tiles del suelo")]
    public Tilemap floorTilemap;
    
    [Header("Tiles de Muros")]
    public TileBase wallTile;
    
    [Header("Tiles de Suelo")]
    [Tooltip("VariantTiles normales sin efectos especiales (cada uno con múltiples sprites)")]
    public TileBase[] normalFloorTiles;
    
    [Tooltip("VariantTiles con efectos (heal, ice, mud, etc. - cada uno con múltiples sprites)")]
    public TileBase[] specialFloorTiles;

    [Header("Tile de Progresión")]
    [Tooltip("Tile de stairs (VariantTile) que permite avanzar a la siguiente sala")]
    [SerializeField] private TileBase stairsTile;

    [Tooltip("Padding desde los muros para colocar stairs")]
    [SerializeField] private int stairsPaddingTiles = 1;
    
    [Header("Configuración de Habitación")]
    [Tooltip("Ancho de la habitación en tiles")]
    public int roomWidth = 26;
    
    [Tooltip("Alto de la habitación en tiles")]
    public int roomHeight = 12;
    
    [Tooltip("Probabilidad (0-1) de que un tile sea especial en lugar de normal")]
    [Range(0f, 1f)]
    public float specialTileChance = 0.1f;

    [Header("Render / Sorting")]
    [Tooltip("Sorting Order del suelo. Debe ser menor que sprites. Ej: -20")]
    [SerializeField] private int floorSortingOrder = -20;
    [Tooltip("Sorting Order de muros. Debe ser menor que sprites, pero mayor que el suelo. Ej: -10")]
    [SerializeField] private int wallSortingOrder = -10;
    [Tooltip("Si está activo, fuerza ambos Tilemaps a la Sorting Layer más baja del proyecto (para que nunca tapen sprites).")]
    [SerializeField] private bool forceTilemapsToLowestSortingLayer = true;

    [Header("Muros")]
    [Tooltip("Grosor del borde de muros en tiles. 1 = anillo clásico; 2+ = más grueso")]
    [SerializeField, Min(1)] private int wallBorderThicknessTiles = 1;

    [Header("Distribución de Tiles Especiales")]
    [Tooltip("Si está activo, los tiles especiales se colocan con reglas (manchas/raridad) en vez de totalmente aleatorio.")]
    [SerializeField] private bool useClusteredSpecialTiles = true;

    [Tooltip("Multiplicador global para subir/bajar un poco la aparición de tiles raros (ice/mud/powerup/heal y specialTileChance en modo antiguo).")]
    [SerializeField, Range(0.5f, 3f)] private float rareTilesSpawnMultiplier = 1.15f;

    [Header("Heal (extremadamente raro)")]
    [SerializeField, Range(0f, 1f)] private float healSpawnChancePerRoom = 0.02f;
    [SerializeField] private int healMaxTilesPerRoom = 1;

    [Header("Speed Tiles (Ice/Mud) en manchas")]
    [SerializeField, Range(0f, 1f)] private float iceClusterChancePerRoom = 0.12f;
    [SerializeField, Range(0f, 1f)] private float mudClusterChancePerRoom = 0.12f;
    [Tooltip("Cantidad de tiles en la mancha (min-max)")]
    [SerializeField] private Vector2Int speedClusterSizeRange = new Vector2Int(4, 9);
    [Tooltip("Radio máximo (Manhattan) desde el centro de la mancha")]
    [SerializeField] private int speedClusterMaxRadius = 2;

    [Header("Damage Up (PowerUp)")]
    [SerializeField, Range(0f, 1f)] private float powerupSpawnChancePerRoom = 0.18f;
    [SerializeField] private Vector2Int powerupCountRange = new Vector2Int(1, 2);
    
    [Header("Posición Inicial")]
    [Tooltip("Offset para centrar la habitación (0,0 = esquina inferior izquierda en el centro)")]
    public Vector3Int roomOffset = new Vector3Int(-13, -7, 0);

    [Header("Colisión (fallback)")]
    [Tooltip("Si está activo, crea 4 BoxCollider2D en el perímetro como seguridad anti-bugs de TilemapCollider.")]
    [SerializeField] private bool forceBoundaryColliders = true;
    [Tooltip("Grosor del perímetro en tiles (1 = una celda).")]
    [SerializeField, Min(0.1f)] private float boundaryThicknessTiles = 1f;

    private Transform boundaryRoot;
    private BoxCollider2D boundaryLeft;
    private BoxCollider2D boundaryRight;
    private BoxCollider2D boundaryTop;
    private BoxCollider2D boundaryBottom;

    void Start()
    {
        // Auto-buscar Tilemaps si no están asignados
        if (wallTilemap == null || floorTilemap == null)
        {
            FindTilemaps();
        }

        EnsureWallCollisionSetup();
        EnsureBoundaryCollidersSetup();
        EnsureTilemapSorting();
        
        // Generar habitación al inicio (necesario para que GameScene funcione sin pre-pintado)
        GenerateRoom();
    }

    private void EnsureTilemapSorting()
    {
        int targetSortingLayerId = 0;
        if (forceTilemapsToLowestSortingLayer)
        {
            var layers = SortingLayer.layers;
            if (layers != null && layers.Length > 0)
            {
                int minValue = int.MaxValue;
                int minId = layers[0].id;
                for (int i = 0; i < layers.Length; i++)
                {
                    int v = layers[i].value;
                    if (v < minValue)
                    {
                        minValue = v;
                        minId = layers[i].id;
                    }
                }
                targetSortingLayerId = minId;
            }
        }

        // Empujar orders por debajo de cualquier SpriteRenderer encontrado.
        int minSpriteOrder = 0;
        var spriteRenderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                minSpriteOrder = Mathf.Min(minSpriteOrder, spriteRenderers[i].sortingOrder);
            }
        }

        int desiredWallOrder = Mathf.Min(wallSortingOrder, minSpriteOrder - 10);
        int desiredFloorOrder = Mathf.Min(floorSortingOrder, desiredWallOrder - 10);

        if (floorTilemap != null)
        {
            var r = floorTilemap.GetComponent<TilemapRenderer>();
            if (r != null)
            {
                if (forceTilemapsToLowestSortingLayer) r.sortingLayerID = targetSortingLayerId;
                r.sortingOrder = desiredFloorOrder;
            }
        }

        if (wallTilemap != null)
        {
            var r = wallTilemap.GetComponent<TilemapRenderer>();
            if (r != null)
            {
                if (forceTilemapsToLowestSortingLayer) r.sortingLayerID = targetSortingLayerId;
                r.sortingOrder = desiredWallOrder;
            }
        }
    }

    private void EnsureBoundaryCollidersSetup()
    {
        if (!forceBoundaryColliders) return;
        if (floorTilemap == null) return;

        if (boundaryRoot == null)
        {
            var rootGo = new GameObject("RoomBoundaryColliders");
            boundaryRoot = rootGo.transform;
            boundaryRoot.SetParent(transform, worldPositionStays: false);
        }

        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer >= 0) boundaryRoot.gameObject.layer = wallLayer;

        boundaryLeft ??= CreateBoundaryCollider("Left");
        boundaryRight ??= CreateBoundaryCollider("Right");
        boundaryTop ??= CreateBoundaryCollider("Top");
        boundaryBottom ??= CreateBoundaryCollider("Bottom");

        UpdateBoundaryColliders();
    }

    private BoxCollider2D CreateBoundaryCollider(string name)
    {
        var go = new GameObject($"Boundary_{name}");
        go.transform.SetParent(boundaryRoot, worldPositionStays: false);
        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer >= 0) go.layer = wallLayer;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        return col;
    }

    private void UpdateBoundaryColliders()
    {
        if (!forceBoundaryColliders) return;
        if (floorTilemap == null) return;
        if (boundaryLeft == null || boundaryRight == null || boundaryTop == null || boundaryBottom == null) return;

        // Calculamos el rect exterior usando CellToWorld (origen de celda) para obtener esquinas reales.
        var grid = floorTilemap.layoutGrid;
        Vector3 cellSizeLocal = grid != null ? grid.cellSize : Vector3.one;
        Vector3 cellSizeWorld = Vector3.Scale(cellSizeLocal, floorTilemap.transform.lossyScale);

        float thicknessX = Mathf.Max(0.01f, boundaryThicknessTiles) * Mathf.Max(0.01f, cellSizeWorld.x);
        float thicknessY = Mathf.Max(0.01f, boundaryThicknessTiles) * Mathf.Max(0.01f, cellSizeWorld.y);

        int wallT = Mathf.Max(1, wallBorderThicknessTiles);
        Vector3Int minCell = new Vector3Int(-wallT, -wallT, 0) + roomOffset;
        Vector3Int maxCellInclusive = new Vector3Int(roomWidth - 1 + wallT, roomHeight - 1 + wallT, 0) + roomOffset;

        // worldMin es esquina inferior izquierda del tile (-1,-1)
        Vector3 worldMin = floorTilemap.CellToWorld(minCell);
        // worldMax es esquina superior derecha del tile (roomWidth,roomHeight) -> sumamos (1,1) para la esquina final
        Vector3 worldMax = floorTilemap.CellToWorld(maxCellInclusive + new Vector3Int(1, 1, 0));

        float xMin = Mathf.Min(worldMin.x, worldMax.x);
        float xMax = Mathf.Max(worldMin.x, worldMax.x);
        float yMin = Mathf.Min(worldMin.y, worldMax.y);
        float yMax = Mathf.Max(worldMin.y, worldMax.y);

        float width = Mathf.Max(0.01f, xMax - xMin);
        float height = Mathf.Max(0.01f, yMax - yMin);
        float cx = (xMin + xMax) * 0.5f;
        float cy = (yMin + yMax) * 0.5f;

        // Colliders en world-space, así que colocamos los GO en world.
        boundaryLeft.transform.position = new Vector3(xMin + thicknessX * 0.5f, cy, 0f);
        boundaryLeft.size = new Vector2(thicknessX, height);

        boundaryRight.transform.position = new Vector3(xMax - thicknessX * 0.5f, cy, 0f);
        boundaryRight.size = new Vector2(thicknessX, height);

        boundaryBottom.transform.position = new Vector3(cx, yMin + thicknessY * 0.5f, 0f);
        boundaryBottom.size = new Vector2(width, thicknessY);

        boundaryTop.transform.position = new Vector3(cx, yMax - thicknessY * 0.5f, 0f);
        boundaryTop.size = new Vector2(width, thicknessY);
    }

    private void EnsureWallCollisionSetup()
    {
        if (wallTilemap == null) return;

        var go = wallTilemap.gameObject;

        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer >= 0) go.layer = wallLayer;

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;
        rb.gravityScale = 0f;

        var tilemapCollider = go.GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null) tilemapCollider = go.AddComponent<TilemapCollider2D>();
        tilemapCollider.isTrigger = false;
        tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        // Nota: usedByComposite está deprecado; compositeOperation controla la unión.

        var composite = go.GetComponent<CompositeCollider2D>();
        if (composite == null) composite = go.AddComponent<CompositeCollider2D>();
        composite.isTrigger = false;
        composite.usedByEffector = false;
        composite.geometryType = CompositeCollider2D.GeometryType.Outlines;
        composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
    }

    private void RefreshWallColliders()
    {
        if (wallTilemap == null) return;

        var go = wallTilemap.gameObject;
        var tilemapCollider = go.GetComponent<TilemapCollider2D>();
        var composite = go.GetComponent<CompositeCollider2D>();

        // Asegurar que la geometría física se recalcula al cambiar tiles en runtime.
        wallTilemap.RefreshAllTiles();

        if (tilemapCollider != null)
        {
            tilemapCollider.ProcessTilemapChanges();
        }

        if (composite != null)
        {
            composite.GenerateGeometry();
        }
    }

    [ContextMenu("Generate Room")]
    public void GenerateRoom()
    {
        GenerateRoom(clearWalls: true);
    }

    /// <summary>
    /// Regenera SOLO el suelo, manteniendo muros existentes.
    /// Útil para el loop de roguelike en la misma escena.
    /// </summary>
    public void RegenerateFloorKeepWalls()
    {
        GenerateRoom(clearWalls: false);
    }

    private void GenerateRoom(bool clearWalls)
    {
        if (wallTilemap == null || floorTilemap == null)
        {
            Debug.LogError("[RoomGenerator] Tilemaps no asignados. Asigna WallTilemap y FloorTilemap en el Inspector.");
            return;
        }
        
        if (wallTile == null)
        {
            Debug.LogError("[RoomGenerator] wallTile no asignado.");
            return;
        }
        
        if (normalFloorTiles == null || normalFloorTiles.Length == 0)
        {
            Debug.LogError("[RoomGenerator] normalFloorTiles vacío. Asigna al menos un VariantTile de suelo.");
            return;
        }
        
        // Limpiar tilemaps anteriores
        if (clearWalls)
        {
            wallTilemap.ClearAllTiles();
        }
        floorTilemap.ClearAllTiles();

        // Asegurar sorting incluso si se instanció por código o cambió renderer.
        EnsureTilemapSorting();
        
        Debug.Log($"[RoomGenerator] Generando habitación {roomWidth}x{roomHeight}...");
        
        if (!useClusteredSpecialTiles)
        {
            // Modo antiguo: especial totalmente aleatorio por probabilidad.
            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0) + roomOffset;
                    TileBase tile = GetRandomFloorTile();
                    floorTilemap.SetTile(pos, tile);
                }
            }

            PlaceStairsTile();
        }
        else
        {
            // Nuevo modo: base normal + colocación dirigida de especiales.
            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0) + roomOffset;
                    floorTilemap.SetTile(pos, GetRandomNormalFloorTile());
                }
            }

            PlaceSpecialTilesClustered();
            PlaceStairsTile();
        }
        
        if (!clearWalls)
        {
            Debug.Log("[RoomGenerator] Suelo regenerado (muros preservados).");
            return;
        }

        // Generar muros en los bordes (anillo con grosor configurable)
        int wallT = Mathf.Max(1, wallBorderThicknessTiles);

        // Muro inferior / superior (incluye esquinas)
        for (int t = 1; t <= wallT; t++)
        {
            int yBottom = -t;
            int yTop = roomHeight - 1 + t;
            for (int x = -wallT; x <= (roomWidth - 1 + wallT); x++)
            {
                wallTilemap.SetTile(new Vector3Int(x, yBottom, 0) + roomOffset, wallTile);
                wallTilemap.SetTile(new Vector3Int(x, yTop, 0) + roomOffset, wallTile);
            }
        }

        // Muro izquierdo / derecho (sin duplicar las filas ya pintadas arriba/abajo)
        for (int t = 1; t <= wallT; t++)
        {
            int xLeft = -t;
            int xRight = roomWidth - 1 + t;
            for (int y = 0; y < roomHeight; y++)
            {
                wallTilemap.SetTile(new Vector3Int(xLeft, y, 0) + roomOffset, wallTile);
                wallTilemap.SetTile(new Vector3Int(xRight, y, 0) + roomOffset, wallTile);
            }
        }

        RefreshWallColliders();
        EnsureBoundaryCollidersSetup();
        
        Debug.Log("[RoomGenerator] Habitación generada correctamente.");
    }

    private void PlaceStairsTile()
    {
        if (stairsTile == null)
        {
            Debug.LogError("[RoomGenerator] stairsTile no asignado. No se puede colocar stairs.");
            return;
        }

        int minX = Mathf.Clamp(stairsPaddingTiles, 0, roomWidth - 1);
        int maxX = Mathf.Clamp(roomWidth - 1 - stairsPaddingTiles, 0, roomWidth - 1);
        int minY = Mathf.Clamp(stairsPaddingTiles, 0, roomHeight - 1);
        int maxY = Mathf.Clamp(roomHeight - 1 - stairsPaddingTiles, 0, roomHeight - 1);

        if (minX > maxX || minY > maxY)
        {
            // Fallback: usar el centro de la habitación (sin padding) para garantizar 1 stairs.
            int cx = Mathf.Clamp(roomWidth / 2, 0, roomWidth - 1);
            int cy = Mathf.Clamp(roomHeight / 2, 0, roomHeight - 1);
            Vector3Int center = new Vector3Int(cx, cy, 0) + roomOffset;
            floorTilemap.SetTile(center, stairsTile);
            Debug.LogWarning("[RoomGenerator] Padding deja sin espacio interior; stairs colocado en el centro como fallback.");
            return;
        }

        int x = Random.Range(minX, maxX + 1);
        int y = Random.Range(minY, maxY + 1);
        Vector3Int pos = new Vector3Int(x, y, 0) + roomOffset;
        floorTilemap.SetTile(pos, stairsTile);
    }

    /// <summary>
    /// Rect interno en coordenadas de mundo para spawns (excluye muros con padding).
    /// </summary>
    public Rect GetInnerWorldRect(int paddingTiles = 1)
    {
        if (floorTilemap == null) return new Rect(0, 0, 0, 0);

        int minX = 0 + paddingTiles;
        int maxX = roomWidth - 1 - paddingTiles;
        int minY = 0 + paddingTiles;
        int maxY = roomHeight - 1 - paddingTiles;

        var cellMin = new Vector3Int(minX, minY, 0) + roomOffset;
        var cellMax = new Vector3Int(maxX, maxY, 0) + roomOffset;

        Vector3 wMin = floorTilemap.GetCellCenterWorld(cellMin);
        Vector3 wMax = floorTilemap.GetCellCenterWorld(cellMax);

        float half = 0.45f;
        float xMin = Mathf.Min(wMin.x, wMax.x) - half;
        float xMax = Mathf.Max(wMin.x, wMax.x) + half;
        float yMin = Mathf.Min(wMin.y, wMax.y) - half;
        float yMax = Mathf.Max(wMin.y, wMax.y) + half;
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private TileBase GetRandomFloorTile()
    {
        // Decidir si usar tile especial o normal
        bool useSpecial = Random.value < specialTileChance;
        
        TileBase[] pool = (useSpecial && specialFloorTiles != null && specialFloorTiles.Length > 0)
            ? specialFloorTiles
            : normalFloorTiles;

        // Garantía: stairs SOLO se coloca vía PlaceStairsTile (1 por sala).
        // Evitar que el random seleccione el mismo TileBase que stairsTile.
        const int maxAttempts = 8;
        for (int i = 0; i < maxAttempts; i++)
        {
            TileBase candidate = pool[Random.Range(0, pool.Length)];
            if (candidate != stairsTile) return candidate;
        }

        // Fallback si el pool está mal configurado y contiene solo stairs.
        // Devuelve el primero que no sea stairs; si no existe, devuelve el primero.
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != stairsTile) return pool[i];
        }
        return pool[0];
    }

    private TileBase GetRandomNormalFloorTile()
    {
        // Solo suelo normal; evita stairs por seguridad.
        TileBase[] pool = normalFloorTiles;
        if (pool == null || pool.Length == 0) return null;

        const int maxAttempts = 8;
        for (int i = 0; i < maxAttempts; i++)
        {
            TileBase candidate = pool[Random.Range(0, pool.Length)];
            if (candidate != stairsTile) return candidate;
        }
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != stairsTile) return pool[i];
        }
        return pool[0];
    }

    private void PlaceSpecialTilesClustered()
    {
        if (floorTilemap == null) return;

        var healTiles = new List<TileBase>();
        var iceTiles = new List<TileBase>();
        var mudTiles = new List<TileBase>();
        var powerupTiles = new List<TileBase>();

        CollectSpecialTiles(healTiles, iceTiles, mudTiles, powerupTiles);

        int padding = Mathf.Max(1, stairsPaddingTiles);
        var reserved = new HashSet<Vector2Int>();

        float Mult(float chance) => Mathf.Clamp01(chance * rareTilesSpawnMultiplier);

        // Ice/Mud en manchas (no demasiado grande)
        if (iceTiles.Count > 0 && Random.value < Mult(iceClusterChancePerRoom))
        {
            int count = Random.Range(speedClusterSizeRange.x, speedClusterSizeRange.y + 1);
            var center = PickRandomInteriorCell(padding + 1);
            PaintCluster(center, count, speedClusterMaxRadius, PickRandom(iceTiles), reserved, padding + 1);
        }

        if (mudTiles.Count > 0 && Random.value < Mult(mudClusterChancePerRoom))
        {
            int count = Random.Range(speedClusterSizeRange.x, speedClusterSizeRange.y + 1);
            var center = PickRandomInteriorCell(padding + 1);
            PaintCluster(center, count, speedClusterMaxRadius, PickRandom(mudTiles), reserved, padding + 1);
        }

        // PowerUp (damage up) poco común
        if (powerupTiles.Count > 0 && Random.value < Mult(powerupSpawnChancePerRoom))
        {
            int count = Random.Range(powerupCountRange.x, powerupCountRange.y + 1);
            for (int i = 0; i < count; i++)
            {
                var cell = PickRandomInteriorCell(padding);
                if (reserved.Add(cell))
                {
                    floorTilemap.SetTile(new Vector3Int(cell.x, cell.y, 0) + roomOffset, PickRandom(powerupTiles));
                }
            }
        }

        // Heal (extremadamente raro)
        if (healTiles.Count > 0 && Random.value < Mult(healSpawnChancePerRoom))
        {
            int count = Mathf.Max(1, healMaxTilesPerRoom);
            for (int i = 0; i < count; i++)
            {
                var cell = PickRandomInteriorCell(padding);
                if (reserved.Add(cell))
                {
                    floorTilemap.SetTile(new Vector3Int(cell.x, cell.y, 0) + roomOffset, PickRandom(healTiles));
                }
            }
        }
    }

    private void CollectSpecialTiles(List<TileBase> healTiles, List<TileBase> iceTiles, List<TileBase> mudTiles, List<TileBase> powerupTiles)
    {
        // Clasifica por TileEffect (o tileName) sin depender de que estén en Resources.
        void Consider(TileBase tile)
        {
            if (tile == null) return;
            if (tile == stairsTile) return;
            if (!TryGetTileEffect(tile, out var effect) || effect == null) return;

            string name = string.IsNullOrEmpty(effect.tileName) ? string.Empty : effect.tileName.ToLowerInvariant();

            // Heal
            if (effect.effectType == TileEffectType.Heal || name == "heal") { healTiles.Add(tile); return; }

            // Ice/Mud (speed)
            if (effect.effectType == TileEffectType.Ice || name == "ice" || effect.effectType == TileEffectType.SpeedUp || name == "speedup")
            {
                iceTiles.Add(tile);
                return;
            }
            if (effect.effectType == TileEffectType.Mud || name == "mud" || effect.effectType == TileEffectType.SpeedDown || name == "slowdown")
            {
                mudTiles.Add(tile);
                return;
            }

            // PowerUp (damage up)
            if (effect.effectType == TileEffectType.PowerUp || name == "powerup") { powerupTiles.Add(tile); }
        }

        if (normalFloorTiles != null)
        {
            foreach (var t in normalFloorTiles) Consider(t);
        }
        if (specialFloorTiles != null)
        {
            foreach (var t in specialFloorTiles) Consider(t);
        }
    }

    private bool TryGetTileEffect(TileBase tile, out TileEffect effect)
    {
        effect = null;
        if (tile == null) return false;

        // Fast path
        if (tile is VariantTile vt)
        {
            effect = vt.tileEffect;
            return true;
        }

        // Compatibility path (si alguna vez cambia el tipo)
        var tileType = tile.GetType();
        var effectField = tileType.GetField("tileEffect");
        if (effectField == null) return false;
        effect = effectField.GetValue(tile) as TileEffect;
        return true;
    }

    private TileBase PickRandom(List<TileBase> tiles)
    {
        if (tiles == null || tiles.Count == 0) return null;
        return tiles[Random.Range(0, tiles.Count)];
    }

    private Vector2Int PickRandomInteriorCell(int padding)
    {
        int minX = Mathf.Clamp(padding, 0, roomWidth - 1);
        int maxX = Mathf.Clamp(roomWidth - 1 - padding, 0, roomWidth - 1);
        int minY = Mathf.Clamp(padding, 0, roomHeight - 1);
        int maxY = Mathf.Clamp(roomHeight - 1 - padding, 0, roomHeight - 1);

        if (minX > maxX || minY > maxY)
        {
            int cx = Mathf.Clamp(roomWidth / 2, 0, roomWidth - 1);
            int cy = Mathf.Clamp(roomHeight / 2, 0, roomHeight - 1);
            return new Vector2Int(cx, cy);
        }

        return new Vector2Int(Random.Range(minX, maxX + 1), Random.Range(minY, maxY + 1));
    }

    private void PaintCluster(Vector2Int center, int targetCount, int maxRadius, TileBase tile, HashSet<Vector2Int> reserved, int padding)
    {
        if (tile == null || targetCount <= 0) return;
        if (reserved == null) return;

        int minX = padding;
        int maxX = roomWidth - 1 - padding;
        int minY = padding;
        int maxY = roomHeight - 1 - padding;
        if (minX > maxX || minY > maxY) return;

        bool InBounds(Vector2Int c) => c.x >= minX && c.x <= maxX && c.y >= minY && c.y <= maxY;
        int Manhattan(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        var painted = new List<Vector2Int>(targetCount);
        if (InBounds(center) && reserved.Add(center))
        {
            painted.Add(center);
            floorTilemap.SetTile(new Vector3Int(center.x, center.y, 0) + roomOffset, tile);
        }

        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        int safety = 200;
        while (painted.Count < targetCount && safety-- > 0)
        {
            if (painted.Count == 0) break;
            var baseCell = painted[Random.Range(0, painted.Count)];
            var dir = dirs[Random.Range(0, dirs.Length)];
            var next = baseCell + dir;
            if (!InBounds(next)) continue;
            if (Manhattan(next, center) > maxRadius) continue;
            if (!reserved.Add(next)) continue;

            painted.Add(next);
            floorTilemap.SetTile(new Vector3Int(next.x, next.y, 0) + roomOffset, tile);
        }
    }

    private void FindTilemaps()
    {
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        foreach (var tm in tilemaps)
        {
            string name = tm.gameObject.name.ToLower();
            if (name.Contains("wall"))
            {
                wallTilemap = tm;
                Debug.Log($"[RoomGenerator] WallTilemap auto-asignado: {tm.name}");
            }
            else if (name.Contains("floor") || name.Contains("ground"))
            {
                floorTilemap = tm;
                Debug.Log($"[RoomGenerator] FloorTilemap auto-asignado: {tm.name}");
            }
        }
    }

    /// <summary>
    /// Genera habitación con tamaño personalizado (útil para roguelike con habitaciones variables)
    /// </summary>
    public void GenerateRoom(int width, int height)
    {
        roomWidth = width;
        roomHeight = height;
        roomOffset = new Vector3Int(-width / 2, -height / 2, 0);
        GenerateRoom();
    }

    void OnDrawGizmosSelected()
    {
        // Dibujar preview de la habitación en el editor
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            roomOffset.x + roomWidth / 2f,
            roomOffset.y + roomHeight / 2f,
            0f
        );
        Vector3 size = new Vector3(roomWidth, roomHeight, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}
