using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Script que corrige automáticamente los problemas detectados por el diagnóstico
/// Ejecútalo una vez y luego elimínalo
/// </summary>
public class AutoFixer : MonoBehaviour
{
    [ContextMenu("Corregir Todos los Problemas")]
    public void FixAllProblems()
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║               INICIANDO CORRECCIÓN AUTOMÁTICA                  ║");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝\n");

        FixPlayerTag();
        FixWallTag();
        FixEntitiesLayer();
        FixGravity();
        FixWallCollider();
        CreateTileEffectsFolder();
        FixFloorTileManager();
        
        Debug.Log("\n╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║               CORRECCIÓN COMPLETADA                            ║");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
        Debug.Log("\n⚠️  IMPORTANTE: Ahora debes crear los TileEffects manualmente:");
        Debug.Log("   1. Click derecho en Assets/Resources/TileEffects/");
        Debug.Log("   2. Create → Tiles → TileEffect");
        Debug.Log("   3. Crea: IceTileEffect, MudTileEffect, HealTileEffect");
    }

    void FixPlayerTag()
    {
        Debug.Log("\n【 Corrigiendo Tag del Player 】");
        
        // Buscar por PlayerSpawner que crea el player
        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
        if (spawner != null)
        {
            Debug.Log("✅ PlayerSpawner encontrado - el Player se creará con tag correcto");
            return;
        }

        // Buscar GameObject que tenga PlayerController
        PlayerController[] controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        if (controllers.Length > 0)
        {
            foreach (var controller in controllers)
            {
                if (controller.gameObject.tag != "Player")
                {
                    controller.gameObject.tag = "Player";
                    Debug.Log($"✅ Tag 'Player' asignado a: {controller.gameObject.name}");
                }
            }
        }
        else
        {
            Debug.Log("⚠️  No se encontró PlayerController - el Player se creará dinámicamente");
        }
    }

    void FixWallTag()
    {
        Debug.Log("\n【 Corrigiendo Tag 'Wall' 】");
        
        // Buscar WallTilemap
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        bool wallFound = false;
        
        foreach (Tilemap tm in tilemaps)
        {
            if (tm.name.ToLower().Contains("wall"))
            {
                // Agregar tag si no existe (esto solo funciona si el tag ya está en TagManager)
                // Unity no permite crear tags por script en runtime, solo asignarlos
                try
                {
                    tm.gameObject.tag = "Wall";
                    Debug.Log($"✅ Tag 'Wall' asignado a: {tm.name}");
                    wallFound = true;
                }
                catch
                {
                    Debug.LogWarning("⚠️  Tag 'Wall' no existe en el proyecto");
                    Debug.LogWarning("   SOLUCIÓN MANUAL: Edit → Project Settings → Tags and Layers → Tags");
                    Debug.LogWarning("   Añade 'Wall' a la lista y vuelve a ejecutar este script");
                }
            }
        }
        
        if (!wallFound)
        {
            Debug.Log("⚠️  No se encontró WallTilemap");
        }
    }

    void FixEntitiesLayer()
    {
        Debug.Log("\n【 Verificando Layer 'Entities' 】");
        
        int entitiesLayer = LayerMask.NameToLayer("Entities");
        if (entitiesLayer == -1)
        {
            Debug.LogWarning("⚠️  Layer 'Entities' no existe");
            Debug.LogWarning("   SOLUCIÓN MANUAL: Edit → Project Settings → Tags and Layers → Layers");
            Debug.LogWarning("   Asigna 'Entities' a un User Layer (ej: Layer 12)");
        }
        else
        {
            Debug.Log($"✅ Layer 'Entities' existe (Layer {entitiesLayer})");
            
            // Asignar layer a player si existe
            PlayerController[] controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var controller in controllers)
            {
                controller.gameObject.layer = entitiesLayer;
                Debug.Log($"✅ Layer 'Entities' asignado a: {controller.gameObject.name}");
                
                // Asignar también a WeaponHolder y armas
                Transform weaponHolder = controller.transform.Find("WeaponHolder");
                if (weaponHolder != null)
                {
                    SetLayerRecursively(weaponHolder.gameObject, entitiesLayer);
                    Debug.Log("✅ Layer 'Entities' asignado a WeaponHolder y armas");
                }
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void FixGravity()
    {
        Debug.Log("\n【 Corrigiendo Gravedad 】");
        
        if (Physics2D.gravity != Vector2.zero)
        {
            // No podemos cambiar esto en runtime, pero podemos avisar
            Debug.LogWarning("⚠️  La gravedad no es (0,0)");
            Debug.LogWarning("   SOLUCIÓN MANUAL: Edit → Project Settings → Physics 2D");
            Debug.LogWarning("   Cambia Gravity Y de -9.81 a 0");
            Debug.LogWarning("   Valor actual: " + Physics2D.gravity);
        }
        else
        {
            Debug.Log("✅ Gravedad correctamente configurada en (0,0)");
        }
    }

    void FixWallCollider()
    {
        Debug.Log("\n【 Verificando Colliders de Muros 】");
        
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        bool hasChanges = false;
        
        foreach (Tilemap tm in tilemaps)
        {
            if (tm.name.ToLower().Contains("wall"))
            {
                TilemapCollider2D collider = tm.GetComponent<TilemapCollider2D>();
                if (collider == null)
                {
                    tm.gameObject.AddComponent<TilemapCollider2D>();
                    Debug.Log($"✅ TilemapCollider2D añadido a: {tm.name}");
                    hasChanges = true;
                }
                else
                {
                    Debug.Log($"✅ {tm.name} ya tiene TilemapCollider2D");
                }
                
                // Verificar/añadir CompositeCollider2D
                CompositeCollider2D composite = tm.GetComponent<CompositeCollider2D>();
                if (composite == null)
                {
                    composite = tm.gameObject.AddComponent<CompositeCollider2D>();
                    Debug.Log($"✅ CompositeCollider2D añadido a: {tm.name}");
                    
                    // Configurar TilemapCollider para usar Composite
                    collider = tm.GetComponent<TilemapCollider2D>();
                    if (collider != null)
                    {
                        collider.compositeOperation = Collider2D.CompositeOperation.Merge;
                        Debug.Log("✅ TilemapCollider configurado para usar Composite");
                    }
                    
                    // Asegurar que tiene Rigidbody2D estático
                    Rigidbody2D rb = tm.GetComponent<Rigidbody2D>();
                    if (rb == null)
                    {
                        rb = tm.gameObject.AddComponent<Rigidbody2D>();
                    }
                    rb.bodyType = RigidbodyType2D.Static;
                    Debug.Log("✅ Rigidbody2D estático configurado");
                }
            }
        }
        
        if (!hasChanges)
        {
            Debug.Log("ℹ️  No se realizaron cambios en los colliders");
        }
    }

    void CreateTileEffectsFolder()
    {
        Debug.Log("\n【 Creando Carpeta de TileEffects 】");
        
#if UNITY_EDITOR
        string path = "Assets/Resources/TileEffects";
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"✅ Carpeta creada: {path}");
            Debug.Log("   Ahora puedes crear TileEffects aquí: Click derecho → Create → Tiles → TileEffect");
        }
        else
        {
            Debug.Log($"✅ La carpeta ya existe: {path}");
        }
#else
        Debug.Log("⚠️  Solo se puede crear carpetas en el Editor");
#endif
    }

    void FixFloorTileManager()
    {
        Debug.Log("\n【 Verificando FloorTileManager 】");
        
        FloorTileManager manager = FindFirstObjectByType<FloorTileManager>();
        if (manager == null)
        {
            Debug.Log("⚠️  FloorTileManager no encontrado");
            
            // Intentar añadirlo al player
            PlayerController[] controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            if (controllers.Length > 0)
            {
                manager = controllers[0].gameObject.AddComponent<FloorTileManager>();
                Debug.Log($"✅ FloorTileManager añadido a: {controllers[0].gameObject.name}");
            }
            else
            {
                Debug.Log("⚠️  No se pudo añadir automáticamente (Player no encontrado)");
                return;
            }
        }
        
        // Verificar que tiene floorTilemap asignado
        var floorTilemapField = typeof(FloorTileManager).GetField("floorTilemap", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (floorTilemapField != null)
        {
            Tilemap floorTilemap = floorTilemapField.GetValue(manager) as Tilemap;
            if (floorTilemap == null)
            {
                // Buscar FloorTilemap
                Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
                foreach (Tilemap tm in tilemaps)
                {
                    if (tm.name.ToLower().Contains("floor"))
                    {
                        floorTilemapField.SetValue(manager, tm);
                        Debug.Log($"✅ FloorTilemap asignado automáticamente: {tm.name}");
                        break;
                    }
                }
            }
            else
            {
                Debug.Log($"✅ FloorTilemap ya está asignado: {floorTilemap.name}");
            }
        }
    }
}
