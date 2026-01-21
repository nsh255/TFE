using UnityEngine;
using UnityEngine.Tilemaps;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Script de diagnóstico completo para detectar problemas de configuración
/// Añádelo a cualquier GameObject y ejecuta el juego para ver el reporte
/// </summary>
public class SystemDiagnostics : MonoBehaviour
{
    [Header("Ejecutar Diagnóstico")]
    [Tooltip("Marcar para ejecutar diagnóstico automáticamente al iniciar")]
    public bool runOnStart = true;
    
    [Tooltip("Mostrar diagnóstico cada X segundos (0 = solo al inicio)")]
    public float repeatInterval = 0f;

    private StringBuilder report;
    private float lastCheckTime;

    void Start()
    {
        if (runOnStart)
        {
            RunFullDiagnostics();
        }
    }

    void Update()
    {
        if (repeatInterval > 0 && Time.time - lastCheckTime >= repeatInterval)
        {
            RunFullDiagnostics();
            lastCheckTime = Time.time;
        }
    }

    [ContextMenu("Ejecutar Diagnóstico Completo")]
    public void RunFullDiagnostics()
    {
        report = new StringBuilder();
        
        report.AppendLine("╔════════════════════════════════════════════════════════════════╗");
        report.AppendLine("║          DIAGNÓSTICO DEL SISTEMA - ASCENSION                   ║");
        report.AppendLine("╚════════════════════════════════════════════════════════════════╝\n");

        CheckPlayer();
        CheckWeapons();
        CheckTilemaps();
        CheckFloorEffects();
        CheckEnemies();
        CheckLayers();
        CheckPhysicsSettings();
        CheckRoomGeneration();
        
        report.AppendLine("\n╔════════════════════════════════════════════════════════════════╗");
        report.AppendLine("║                    FIN DEL DIAGNÓSTICO                         ║");
        report.AppendLine("╚════════════════════════════════════════════════════════════════╝");
        
        Debug.Log(report.ToString());
    }

    void CheckPlayer()
    {
        report.AppendLine("\n【 PLAYER 】");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            report.AppendLine("❌ ERROR: No se encuentra GameObject con tag 'Player'");
            report.AppendLine("   SOLUCIÓN: Asigna el tag 'Player' al GameObject del jugador");
            return;
        }

        report.AppendLine($"✅ Player encontrado: {player.name}");
        report.AppendLine($"   Layer: {LayerMask.LayerToName(player.layer)} ({player.layer})");
        report.AppendLine($"   Posición: {player.transform.position}");

        // Rigidbody2D
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            report.AppendLine("❌ ERROR: Player sin Rigidbody2D");
            report.AppendLine("   SOLUCIÓN: Add Component → Physics 2D → Rigidbody 2D");
        }
        else
        {
            report.AppendLine($"✅ Rigidbody2D: Body Type = {rb.bodyType}, Gravity Scale = {rb.gravityScale}");
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                report.AppendLine("⚠️  ADVERTENCIA: Body Type debería ser Dynamic");
            }
            if (rb.gravityScale != 0)
            {
                report.AppendLine("⚠️  ADVERTENCIA: Gravity Scale debería ser 0 para juegos top-down");
            }
            if (rb.constraints != RigidbodyConstraints2D.FreezeRotation)
            {
                report.AppendLine("⚠️  ADVERTENCIA: Se recomienda Freeze Rotation en Constraints");
            }
        }

        // Collider
        Collider2D col = player.GetComponent<Collider2D>();
        if (col == null)
        {
            report.AppendLine("❌ ERROR: Player sin Collider2D");
            report.AppendLine("   SOLUCIÓN: Add Component → Physics 2D → Box/Circle Collider 2D");
        }
        else
        {
            report.AppendLine($"✅ Collider: {col.GetType().Name}, IsTrigger = {col.isTrigger}");
        }

        // Scripts
        var controller = player.GetComponent<PlayerController>();
        if (controller == null)
        {
            report.AppendLine("⚠️  PlayerController no encontrado");
        }
        else
        {
            report.AppendLine($"✅ PlayerController presente");
        }

        var health = player.GetComponent<PlayerHealth>();
        if (health == null)
        {
            report.AppendLine("⚠️  PlayerHealth no encontrado");
        }
        else
        {
            report.AppendLine($"✅ PlayerHealth presente");
        }
    }

    void CheckWeapons()
    {
        report.AppendLine("\n【 ARMAS 】");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            report.AppendLine("⚠️  No se puede revisar armas sin Player");
            return;
        }

        // Buscar WeaponHolder
        Transform weaponHolder = player.transform.Find("WeaponHolder");
        if (weaponHolder == null)
        {
            report.AppendLine("❌ ERROR: No existe 'WeaponHolder' como hijo del Player");
            report.AppendLine("   SOLUCIÓN: Crear GameObject hijo del Player llamado 'WeaponHolder'");
            return;
        }

        report.AppendLine($"✅ WeaponHolder encontrado");
        report.AppendLine($"   Posición local: {weaponHolder.localPosition}");
        report.AppendLine($"   Hijos activos: {weaponHolder.childCount}");

        if (weaponHolder.childCount == 0)
        {
            report.AppendLine("❌ ERROR: WeaponHolder no tiene armas como hijos");
            report.AppendLine("   SOLUCIÓN: Arrastra el prefab del arma como hijo de WeaponHolder");
            return;
        }

        // Revisar cada arma
        for (int i = 0; i < weaponHolder.childCount; i++)
        {
            Transform weapon = weaponHolder.GetChild(i);
            report.AppendLine($"\n   Arma {i + 1}: {weapon.name}");
            report.AppendLine($"   - Activa: {weapon.gameObject.activeSelf}");
            report.AppendLine($"   - Layer: {LayerMask.LayerToName(weapon.gameObject.layer)}");
            
            SpriteRenderer sr = weapon.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                report.AppendLine("   ⚠️  Sin SpriteRenderer (arma invisible)");
            }
            else
            {
                report.AppendLine($"   - SpriteRenderer: Sorting Layer = {sr.sortingLayerName}, Order = {sr.sortingOrder}");
                report.AppendLine($"   - Sprite asignado: {(sr.sprite != null ? "✅" : "❌")}");
                if (sr.sortingLayerName == "Default")
                {
                    report.AppendLine("   ⚠️  ADVERTENCIA: Sorting Layer en 'Default', debería ser 'Entities' o superior");
                }
            }

            // Verificar script de arma
            var meleeWeapon = weapon.GetComponent<MeleeWeapon>();
            var rangedWeapon = weapon.GetComponent<RangedWeapon>();
            
            if (meleeWeapon != null)
            {
                report.AppendLine($"   ✅ Script de arma encontrado: MeleeWeapon");
            }
            else if (rangedWeapon != null)
            {
                report.AppendLine($"   ✅ Script de arma encontrado: RangedWeapon");
            }
            else
            {
                report.AppendLine("   ⚠️  No se encontró script de arma (MeleeWeapon/RangedWeapon)");
            }
        }
    }

    void CheckTilemaps()
    {
        report.AppendLine("\n【 TILEMAPS 】");
        
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        if (tilemaps.Length == 0)
        {
            report.AppendLine("❌ ERROR: No se encontraron Tilemaps en la escena");
            report.AppendLine("   SOLUCIÓN: Hierarchy → Click derecho → 2D Object → Tilemap → Rectangular");
            return;
        }

        report.AppendLine($"Total de Tilemaps encontrados: {tilemaps.Length}\n");

        Tilemap wallTilemap = null;
        Tilemap floorTilemap = null;

        foreach (Tilemap tm in tilemaps)
        {
            report.AppendLine($"   Tilemap: {tm.name}");
            report.AppendLine($"   - Layer: {LayerMask.LayerToName(tm.gameObject.layer)}");
            
            TilemapRenderer renderer = tm.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                report.AppendLine($"   - Sorting Layer: {renderer.sortingLayerName}, Order: {renderer.sortingOrder}");
            }

            TilemapCollider2D collider = tm.GetComponent<TilemapCollider2D>();
            if (collider != null)
            {
                bool usedByComposite = collider.compositeOperation != Collider2D.CompositeOperation.None;
                report.AppendLine($"   - TilemapCollider2D presente (UsedByComposite: {usedByComposite})");
            }

            CompositeCollider2D composite = tm.GetComponent<CompositeCollider2D>();
            if (composite != null)
            {
                report.AppendLine($"   - CompositeCollider2D presente (Geometry: {composite.geometryType})");
            }

            int tileCount = 0;
            foreach (var pos in tm.cellBounds.allPositionsWithin)
            {
                if (tm.HasTile(pos)) tileCount++;
            }
            report.AppendLine($"   - Tiles colocados: {tileCount}");

            if (tm.name.ToLower().Contains("wall"))
            {
                wallTilemap = tm;
                if (collider == null)
                {
                    report.AppendLine("   ❌ ERROR: Tilemap de muros sin TilemapCollider2D");
                    report.AppendLine("      SOLUCIÓN: Add Component → Tilemap Collider 2D");
                }
            }
            else if (tm.name.ToLower().Contains("floor"))
            {
                floorTilemap = tm;
            }

            report.AppendLine("");
        }

        if (wallTilemap == null)
        {
            report.AppendLine("⚠️  ADVERTENCIA: No se encontró Tilemap con 'wall' en el nombre");
        }
        if (floorTilemap == null)
        {
            report.AppendLine("⚠️  ADVERTENCIA: No se encontró Tilemap con 'floor' en el nombre");
        }
    }

    void CheckFloorEffects()
    {
        report.AppendLine("\n【 EFECTOS DE TILES 】");
        
        FloorTileManager manager = FindFirstObjectByType<FloorTileManager>();
        if (manager == null)
        {
            report.AppendLine("❌ ERROR: No se encontró FloorTileManager en la escena");
            report.AppendLine("   SOLUCIÓN: Añade el componente FloorTileManager al Player o a un Manager");
            return;
        }

        report.AppendLine("✅ FloorTileManager encontrado");
        
        // Usar reflexión para acceder a campos privados
        var floorTilemapField = typeof(FloorTileManager).GetField("floorTilemap", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (floorTilemapField != null)
        {
            Tilemap floorTilemap = floorTilemapField.GetValue(manager) as Tilemap;
            if (floorTilemap == null)
            {
                report.AppendLine("❌ ERROR: FloorTileManager.floorTilemap no está asignado");
                report.AppendLine("   SOLUCIÓN: En el Inspector del FloorTileManager, arrastra el FloorTilemap");
            }
            else
            {
                report.AppendLine($"✅ FloorTilemap asignado: {floorTilemap.name}");
            }
        }

        // Buscar TileEffects en Resources
        TileEffect[] effects = Resources.LoadAll<TileEffect>("TileEffects");
        report.AppendLine($"\nTileEffects en Resources/TileEffects/: {effects.Length}");
        
        if (effects.Length == 0)
        {
            report.AppendLine("⚠️  ADVERTENCIA: No se encontraron TileEffects en Resources/TileEffects/");
            report.AppendLine("   Asegúrate de que los TileEffects estén en Assets/Resources/TileEffects/");
        }
        else
        {
            foreach (var effect in effects)
            {
                report.AppendLine($"   - {effect.tileName}: {effect.effectType} (Speed: {effect.speedMultiplier}x)");
            }
        }
    }

    void CheckEnemies()
    {
        report.AppendLine("\n【 ENEMIGOS 】");
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        report.AppendLine($"Enemigos en escena: {enemies.Length}");

        foreach (GameObject enemy in enemies)
        {
            report.AppendLine($"\n   Enemigo: {enemy.name}");
            report.AppendLine($"   - Layer: {LayerMask.LayerToName(enemy.layer)}");
            
            Collider2D col = enemy.GetComponent<Collider2D>();
            if (col != null)
            {
                report.AppendLine($"   - Collider: {col.GetType().Name}, IsTrigger = {col.isTrigger}");
            }
            else
            {
                report.AppendLine("   ⚠️  Sin Collider2D");
            }
        }
    }

    void CheckLayers()
    {
        report.AppendLine("\n【 LAYERS Y TAGS 】");
        
        // Verificar tags importantes
        string[] requiredTags = { "Player", "Enemy", "Wall" };
        foreach (string tag in requiredTags)
        {
            try
            {
                GameObject.FindGameObjectWithTag(tag);
                report.AppendLine($"✅ Tag '{tag}' existe y está en uso");
            }
            catch
            {
                report.AppendLine($"⚠️  Tag '{tag}' no existe o no está asignado");
            }
        }

        // Verificar layers importantes
        report.AppendLine("\nLayers del proyecto:");
        string[] importantLayers = { "Player", "Enemy", "Wall", "Floor", "Entities" };
        foreach (string layerName in importantLayers)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
            {
                report.AppendLine($"⚠️  Layer '{layerName}' no existe");
            }
            else
            {
                report.AppendLine($"✅ Layer '{layerName}' = {layer}");
            }
        }

        // Verificar sorting layers
        report.AppendLine("\nSorting Layers:");
        string[] sortingLayers = { "Default", "Floor", "Entities", "Walls", "UI" };
        foreach (string sl in sortingLayers)
        {
            if (SortingLayer.IsValid(SortingLayer.NameToID(sl)))
            {
                report.AppendLine($"✅ Sorting Layer '{sl}' existe");
            }
            else
            {
                report.AppendLine($"⚠️  Sorting Layer '{sl}' no existe");
            }
        }
    }

    void CheckPhysicsSettings()
    {
        report.AppendLine("\n【 CONFIGURACIÓN DE FÍSICA 】");
        
        // No podemos acceder a la matriz de colisiones directamente en runtime
        // pero podemos probar colisiones específicas
        
        int playerLayer = LayerMask.NameToLayer("Player");
        int wallLayer = LayerMask.NameToLayer("Wall");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (playerLayer != -1 && wallLayer != -1)
        {
            bool canCollide = !Physics2D.GetIgnoreLayerCollision(playerLayer, wallLayer);
            report.AppendLine($"Player ↔ Wall: {(canCollide ? "✅ Colisiona" : "❌ NO colisiona")}");
        }

        if (playerLayer != -1 && enemyLayer != -1)
        {
            bool canCollide = !Physics2D.GetIgnoreLayerCollision(playerLayer, enemyLayer);
            report.AppendLine($"Player ↔ Enemy: {(canCollide ? "✅ Colisiona" : "❌ NO colisiona")}");
        }

        if (enemyLayer != -1 && wallLayer != -1)
        {
            bool canCollide = !Physics2D.GetIgnoreLayerCollision(enemyLayer, wallLayer);
            report.AppendLine($"Enemy ↔ Wall: {(canCollide ? "✅ Colisiona" : "❌ NO colisiona")}");
        }

        report.AppendLine($"\nGravity: {Physics2D.gravity}");
        if (Physics2D.gravity != Vector2.zero)
        {
            report.AppendLine("⚠️  ADVERTENCIA: Gravity no es (0,0), puede afectar juegos top-down");
        }
    }

    void CheckRoomGeneration()
    {
        report.AppendLine("\n【 GENERACIÓN DE HABITACIONES 】");
        
        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();
        if (generator == null)
        {
            report.AppendLine("⚠️  RoomGenerator no encontrado en la escena");
            return;
        }

        report.AppendLine("✅ RoomGenerator encontrado");
        
        // Aquí podrías añadir más checks sobre la configuración del RoomGenerator
        // usando reflexión si es necesario
    }
}
