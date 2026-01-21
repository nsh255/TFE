using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

// Referencia al tipo VariantTile definido en el mismo namespace global
// (no requiere using adicional si está en Assets/Scripts/Tiles)

/// <summary>
/// Detecta en qué tile está parado el jugador y aplica efectos.
/// Coloca este script en el GameObject del jugador o en un Manager de escena.
/// </summary>
public class FloorTileManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Tilemap del suelo (no el de muros)")]
    public Tilemap floorTilemap;
    
    [Tooltip("Transform del jugador para detectar posición")]
    public Transform playerTransform;

    [Header("Interacción (Stairs)")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private RoomFlowController roomFlow;

    [Tooltip("Si no existe RoomFlowController en escena, lo crea como fallback (necesario para spawn de enemigos y stairs).")]
    [SerializeField] private bool autoCreateRoomFlowIfMissing = true;
    
    private PlayerController playerController;
    private PlayerHealth playerHealth;
    
    private Vector3Int currentTilePos;
    private TileBase currentTile;
    private TileEffect currentEffect;
    
    private float lastEffectTime;
    private Coroutine activeEffectCoroutine;
    private Coroutine speedPersistCoroutine;

    [Header("VFX (Auto)")]
    [Tooltip("Offset vertical para el VFX al pisar un tile")]
    [SerializeField] private float vfxYOffset = 0.25f;
    [Tooltip("Si está activo, intenta forzar el sorting del VFX para que se vea por encima del jugador")]
    [SerializeField] private bool forceVfxAbovePlayer = true;
    [SerializeField] private int vfxOrderOffset = 20;

    [Tooltip("Offset horizontal extra para el VFX respecto al borde del sprite.")]
    [SerializeField] private float vfxSidePadding = 0.15f;

    [Header("Persistencia de Efectos")]
    [Tooltip("Cuánto dura el efecto DESPUÉS de pisar el tile (para PowerUp/Speed/Ice/Mud).")]
    [SerializeField, Min(0f)] private float persistEffectSeconds = 5f;
    
    private float originalSpeed;
    private bool hasSpeedModifier = false;

    private float nextResolveTime;
    private float lastBlockedInteractLogTime;
    private float nextResolveRoomFlowTime;

    private bool hasInitialTileScan;

    void Start()
    {
        TryResolvePlayerRefs(force: true);
        
        if (floorTilemap == null)
        {
            Debug.LogWarning("[FloorTileManager] No se asignó floorTilemap. Buscando en la escena...");
            // Intentar encontrar el Tilemap de suelo (por nombre o tag)
            foreach (var tm in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                if (tm.gameObject.name.ToLower().Contains("floor") || tm.gameObject.name.ToLower().Contains("ground"))
                {
                    floorTilemap = tm;
                    Debug.Log($"[FloorTileManager] Tilemap encontrado: {tm.name}");
                    break;
                }
            }
        }
        
        if (playerController != null)
        {
            // Guardar velocidad original para restaurarla
            originalSpeed = playerController.BaseSpeed;
        }

        if (roomFlow == null)
        {
            TryResolveRoomFlow(canCreate: true);
        }

        // Nota: Aunque exista EnsureRoomFlowController, en algunas escenas/órdenes de ejecución
        // puede no haber creado aún el RoomFlow. Este script lo asegura como fallback.
    }

    void Update()
    {
        if (floorTilemap == null) return;

        // Si el jugador spawnea después, re-resolver referencias de forma throttled
        TryResolvePlayerRefs(force: false);
        if (playerTransform == null) return;

        // Importante: si el jugador spawnea justo en (0,0) (tilePos == default),
        // el primer scan no se dispara. Forzamos un scan 1 vez cuando ya tenemos playerTransform.
        if (!hasInitialTileScan)
        {
            currentTilePos = new Vector3Int(int.MinValue / 2, int.MinValue / 2, 0);
            hasInitialTileScan = true;
        }
        
        // Convertir posición del jugador a coordenadas de tile
        Vector3Int tilePos = floorTilemap.WorldToCell(playerTransform.position);
        
        // Si cambió de tile
        if (tilePos != currentTilePos)
        {
            OnExitTile();
            currentTilePos = tilePos;
            OnEnterTile();
        }
        
        // Aplicar efectos continuos
        if (currentEffect != null && currentEffect.continuous)
        {
            if (Time.time >= lastEffectTime + currentEffect.tickRate)
            {
                ApplyEffect(currentEffect);
                lastEffectTime = Time.time;
            }
        }

        // Asegurar que exista RoomFlowController (para que spawnee enemigos en la primera sala)
        if (roomFlow == null && Time.time >= nextResolveRoomFlowTime)
        {
            nextResolveRoomFlowTime = Time.time + 0.5f;
            TryResolveRoomFlow(canCreate: true);
        }

        // Interacción con stairs
        if (currentEffect != null && IsStairsEffect(currentEffect) && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"[FloorTileManager] Interact '{interactKey}' en stairs. roomFlow={(roomFlow != null ? roomFlow.name : "null")}, canAdvance={(roomFlow != null && roomFlow.CanAdvance)}");

            if (roomFlow == null)
            {
                TryResolveRoomFlow(canCreate: true);
            }

            if (roomFlow == null)
            {
                Debug.LogWarning("[FloorTileManager] No hay RoomFlowController; no se puede avanzar.");
                return;
            }

            if (!roomFlow.CanAdvance)
            {
                if (Time.time - lastBlockedInteractLogTime > 0.75f)
                {
                    HUDMessage.Show("Limpia la sala primero");
                    lastBlockedInteractLogTime = Time.time;
                }
                return;
            }

            if (!roomFlow.TryAdvanceToNextRoom())
            {
                HUDMessage.Show("No se pudo avanzar");
            }
        }
    }

    private void TryResolveRoomFlow(bool canCreate)
    {
        if (roomFlow != null) return;

        // Preferir singleton si existe
        if (RoomFlowController.Instance != null)
        {
            roomFlow = RoomFlowController.Instance;
            return;
        }

        roomFlow = FindFirstObjectByType<RoomFlowController>();
        if (roomFlow != null) return;

        if (!canCreate || !autoCreateRoomFlowIfMissing) return;

        // Solo crear si estamos en una escena con RoomGenerator
        var generator = FindFirstObjectByType<RoomGenerator>();
        if (generator == null) return;

        var go = new GameObject("RoomFlowController");
        roomFlow = go.AddComponent<RoomFlowController>();
        Debug.Log("[FloorTileManager] RoomFlowController creado automáticamente (fallback).");
    }

    private bool IsStairsEffect(TileEffect effect)
    {
        return effect != null && string.Equals(effect.tileName, "stairs", StringComparison.OrdinalIgnoreCase);
    }


    private void TryResolvePlayerRefs(bool force)
    {
        if (!force && Time.time < nextResolveTime) return;
        nextResolveTime = Time.time + 0.5f;

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (playerTransform != null)
        {
            if (playerController == null) playerController = playerTransform.GetComponent<PlayerController>();
            if (playerHealth == null) playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerController != null && originalSpeed <= 0f) originalSpeed = playerController.BaseSpeed;
        }
    }

    private void OnEnterTile()
    {
        currentTile = floorTilemap.GetTile(currentTilePos);
        if (currentTile == null)
        {
            currentEffect = null;
            return;
        }

        // Nuevo flujo: si es VariantTile, tomar directamente su TileEffect via reflexión (evita dependencia dura)
        var tileType = currentTile.GetType();
        if (tileType.Name == "VariantTile")
        {
            var effectField = tileType.GetField("tileEffect");
            if (effectField != null)
            {
                currentEffect = effectField.GetValue(currentTile) as TileEffect;
            }
        }

        if (currentEffect == null)
        {
            // Fallback antiguo por nombre (mantiene compatibilidad con tiles previos)
            currentEffect = GetTileEffectByName(currentTile.name);
        }
        
        if (currentEffect != null)
        {
            Debug.Log($"[FloorTileManager] Jugador pisó tile: {currentEffect.tileName} ({currentEffect.effectType})");
            
            // Aplicar efecto inmediato
            ApplyEffect(currentEffect);
            lastEffectTime = Time.time;
            
            // Spawnar VFX si existe
            if (currentEffect.vfxPrefab != null)
            {
                Vector3 spawnPos = GetVfxSpawnPosNearActor(playerTransform);

                var vfx = Instantiate(currentEffect.vfxPrefab, spawnPos, Quaternion.identity);

                // Si el prefab no trae Animator, le ponemos una animación simple de parpadeo.
                if (vfx.GetComponentInChildren<Animator>(true) == null && vfx.GetComponentInChildren<PulseVFX>(true) == null)
                {
                    vfx.AddComponent<PulseVFX>();
                }

                // Mantener el VFX vivo ~persistEffectSeconds para que se vea el efecto.
                if (persistEffectSeconds > 0f)
                {
                    Destroy(vfx, persistEffectSeconds);
                }

                // Si el VFX tiene PulseVFX, configurarlo para que dure igual.
                var pulse = vfx.GetComponentInChildren<PulseVFX>(true);
                if (pulse != null && persistEffectSeconds > 0f)
                {
                    pulse.SetLifetime(persistEffectSeconds);
                }

                if (forceVfxAbovePlayer && playerTransform != null)
                {
                    var playerSr = playerTransform.GetComponentInChildren<SpriteRenderer>(true);
                    if (playerSr != null)
                    {
                        foreach (var sr in vfx.GetComponentsInChildren<SpriteRenderer>(true))
                        {
                            sr.sortingLayerID = playerSr.sortingLayerID;
                            sr.sortingOrder = playerSr.sortingOrder + vfxOrderOffset;
                        }
                    }
                }
            }
            
            // Persistencia: para efectos no-continuos (speed/powerup), mantenemos 5s tras pisar.
            // Para Heal/Damage (continuous) se mantiene solo mientras estés encima.
            if (!currentEffect.continuous && persistEffectSeconds > 0f)
            {
                if (activeEffectCoroutine != null) StopCoroutine(activeEffectCoroutine);
                activeEffectCoroutine = StartCoroutine(RemoveEffectAfterDuration(persistEffectSeconds));
            }
        }
    }

    private Vector3 GetVfxSpawnPosNearActor(Transform actor)
    {
        if (actor == null)
        {
            var fallback = floorTilemap != null ? floorTilemap.GetCellCenterWorld(currentTilePos) : Vector3.zero;
            fallback.z = 0f;
            fallback.y += vfxYOffset;
            return fallback;
        }

        var sr = actor.GetComponentInChildren<SpriteRenderer>(true);
        var rb = actor.GetComponent<Rigidbody2D>();

        float side = 1f;
        if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
        {
            side = Mathf.Sign(rb.linearVelocity.x);
        }
        else
        {
            side = actor.lossyScale.x >= 0f ? 1f : -1f;
        }

        Vector3 basePos = actor.position;
        float halfWidth = 0.35f;
        float halfHeight = 0.45f;
        if (sr != null)
        {
            halfWidth = Mathf.Max(0.1f, sr.bounds.extents.x);
            halfHeight = Mathf.Max(0.1f, sr.bounds.extents.y);
            basePos = sr.bounds.center;
        }

        var pos = basePos + new Vector3(side * (halfWidth + vfxSidePadding), (-halfHeight * 0.1f) + vfxYOffset, 0f);
        pos.z = 0f;
        return pos;
    }

    private void OnExitTile()
    {
        if (currentEffect != null)
        {
            // Remover modificadores de velocidad inmediatamente al salir (excepto si tiene duración)
            // Ahora dejamos que la persistencia lo retire (persistEffectSeconds).
        }
        
        currentEffect = null;
    }

    private void ApplyEffect(TileEffect effect)
    {
        if (effect == null) return;
        
        switch (effect.effectType)
        {
            case TileEffectType.Heal:
                if (playerHealth != null && effect.healthChange > 0)
                {
                    playerHealth.Heal(effect.healthChange);
                    Debug.Log($"[FloorTileManager] Curado {effect.healthChange} HP");
                }
                break;
                
            case TileEffectType.Damage:
                if (playerHealth != null && effect.healthChange < 0)
                {
                    playerHealth.TakeDamage(Mathf.Abs(effect.healthChange));
                    Debug.Log($"[FloorTileManager] Dañado {Mathf.Abs(effect.healthChange)} HP");
                }
                break;
                
            case TileEffectType.SpeedUp:
            case TileEffectType.SpeedDown:
            case TileEffectType.Ice:
            case TileEffectType.Mud:
                ApplySpeedModifier(effect.speedMultiplier);

                // Persistir el speed a pesar de salir del tile.
                if (persistEffectSeconds > 0f)
                {
                    if (speedPersistCoroutine != null) StopCoroutine(speedPersistCoroutine);
                    speedPersistCoroutine = StartCoroutine(RemoveSpeedAfterDelay(persistEffectSeconds));
                }
                break;
                
            case TileEffectType.PowerUp:
                if (DamageBoostManager.Instance != null)
                {
                    if (persistEffectSeconds > 0f)
                    {
                        DamageBoostManager.Instance.AddDamageBoostTimed(1, persistEffectSeconds);
                        Debug.Log($"[FloorTileManager] ¡PowerUp! Daño +1 por {persistEffectSeconds:0.0}s");
                    }
                    else
                    {
                        DamageBoostManager.Instance.AddDamageBoost(1);
                        Debug.Log("[FloorTileManager] ¡PowerUp! Daño +1");
                    }
                }
                break;
        }
    }

    private IEnumerator RemoveSpeedAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        RestoreSpeed();
        speedPersistCoroutine = null;
    }

    private void ApplySpeedModifier(float multiplier)
    {
        if (playerController == null) return;

        if (!hasSpeedModifier)
        {
            originalSpeed = playerController.BaseSpeed;
        }

        playerController.SetSpeedMultiplier(multiplier);
        hasSpeedModifier = true;

        Debug.Log($"[FloorTileManager] Velocidad modificada: {originalSpeed} -> {playerController.CurrentSpeed} (x{multiplier})");
    }

    private void RestoreSpeed()
    {
        if (hasSpeedModifier && playerController != null)
        {
            playerController.ResetSpeed();
            hasSpeedModifier = false;
            Debug.Log($"[FloorTileManager] Velocidad restaurada a {originalSpeed}");
        }
    }

    private IEnumerator RemoveEffectAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        RestoreSpeed();
        activeEffectCoroutine = null;
    }

    /// <summary>
    /// Busca un TileEffect por nombre del tile.
    /// Los TileEffects deben estar asignados directamente a los VariantTiles en el Inspector.
    /// Este método es fallback para tiles antiguos.
    /// </summary>
    private TileEffect GetTileEffectByName(string tileName)
    {
        // Ya no necesitamos buscar en Resources porque los VariantTiles
        // tienen el TileEffect asignado directamente en el Inspector
        Debug.LogWarning($"[FloorTileManager] Tile '{tileName}' no tiene TileEffect asignado. Asígnalo en el VariantTile en el Inspector.");
        return null;
    }

    void OnDestroy()
    {
        RestoreSpeed();
    }
}
