using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Gestiona efectos de tiles para enemigos (velocidad, boost de daño, etc.)
/// Se adjunta a cada enemigo para detectar y aplicar efectos del suelo.
/// </summary>
public class EnemyTileEffectReceiver : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Tilemap del suelo (auto-detectado si está vacío)")]
    public Tilemap floorTilemap;
    
    private Enemy enemy;
    private Vector3Int currentTilePos;
    private TileEffect currentEffect;

    [Header("Persistencia")]
    [SerializeField, Min(0f)] private float persistEffectSeconds = 5f;

    [Header("VFX")]
    [SerializeField] private float vfxYOffset = 0.25f;
    [SerializeField] private float vfxSidePadding = 0.15f;

    private Coroutine speedPersistCoroutine;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        
        if (floorTilemap == null)
        {
            // Auto-buscar Tilemap de suelo
            foreach (var tm in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                if (tm.gameObject.name.ToLower().Contains("floor") || tm.gameObject.name.ToLower().Contains("ground"))
                {
                    floorTilemap = tm;
                    break;
                }
            }
        }
        
        // no mutamos EnemyData (es ScriptableObject compartido)
    }

    void Update()
    {
        if (floorTilemap == null || enemy == null) return;
        
        // Convertir posición del enemigo a coordenadas de tile
        Vector3Int tilePos = floorTilemap.WorldToCell(transform.position);
        
        // Si cambió de tile
        if (tilePos != currentTilePos)
        {
            OnExitTile();
            currentTilePos = tilePos;
            OnEnterTile();
        }
    }

    private void OnEnterTile()
    {
        TileBase currentTile = floorTilemap.GetTile(currentTilePos);
        if (currentTile == null)
        {
            currentEffect = null;
            return;
        }

        // Leer TileEffect usando reflexión (compatible con VariantTile)
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
            // Fallback por nombre
            currentEffect = GetTileEffectByName(currentTile.name);
        }
        
        if (currentEffect != null)
        {
            ApplyEffect(currentEffect);
        }
    }

    private void OnExitTile()
    {
        // No restaurar inmediatamente: se mantiene persistEffectSeconds.
        currentEffect = null;
    }

    private void ApplyEffect(TileEffect effect)
    {
        if (effect == null) return;

        // PowerUp: NO lo activan enemigos (evita boost global raro)
        if (!string.IsNullOrEmpty(effect.tileName) && effect.tileName.ToLower() == "powerup")
            return;

        // VFX al lado del enemigo
        if (effect.vfxPrefab != null)
        {
            var vfx = Instantiate(effect.vfxPrefab, GetVfxSpawnPosNearActor(transform), Quaternion.identity);
            if (vfx.GetComponentInChildren<Animator>(true) == null && vfx.GetComponentInChildren<PulseVFX>(true) == null)
            {
                vfx.AddComponent<PulseVFX>();
            }
            if (persistEffectSeconds > 0f)
            {
                Destroy(vfx, persistEffectSeconds);
                var pulse = vfx.GetComponentInChildren<PulseVFX>(true);
                if (pulse != null) pulse.SetLifetime(persistEffectSeconds);
            }
        }
        
        // Modificadores de velocidad
        switch (effect.effectType)
        {
            case TileEffectType.SpeedUp:
            case TileEffectType.SpeedDown:
            case TileEffectType.Ice:
            case TileEffectType.Mud:
                ApplySpeedModifier(effect.speedMultiplier);
                break;
            case TileEffectType.Damage:
                // Daño puntual al pisar (si quisieras DOT, marcarlo como continuous en el TileEffect)
                // TileEffect usa healthChange: para daño normalmente será negativo.
                enemy.TakeDamage(Mathf.Max(1, Mathf.Abs(effect.healthChange)));
                break;
            case TileEffectType.Heal:
                // Enemigos también pueden curarse (opcional)
                int heal = Mathf.Max(1, Mathf.Abs(effect.healthChange));
                int maxHp = enemy.enemyData != null ? enemy.enemyData.maxHealth : enemy.currentHealth + heal;
                enemy.currentHealth = Mathf.Min(enemy.currentHealth + heal, maxHp);
                break;
        }
    }

    private void ApplySpeedModifier(float multiplier)
    {
        if (enemy == null) return;

        enemy.SetTileSpeedMultiplier(multiplier);

        if (persistEffectSeconds > 0f)
        {
            if (speedPersistCoroutine != null) StopCoroutine(speedPersistCoroutine);
            speedPersistCoroutine = StartCoroutine(RemoveSpeedAfterDelay(persistEffectSeconds));
        }
    }

    private IEnumerator RemoveSpeedAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (enemy != null) enemy.ResetTileSpeedMultiplier();
        speedPersistCoroutine = null;
    }

    private Vector3 GetVfxSpawnPosNearActor(Transform actor)
    {
        if (actor == null) return transform.position;

        var sr = actor.GetComponentInChildren<SpriteRenderer>(true);
        var rb = actor.GetComponent<Rigidbody2D>();

        float side = 1f;
        if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
            side = Mathf.Sign(rb.linearVelocity.x);
        else
            side = actor.lossyScale.x >= 0f ? 1f : -1f;

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

    private TileEffect GetTileEffectByName(string tileName)
    {
        string cleanName = tileName.Replace("(Clone)", "").Trim();
        return Resources.Load<TileEffect>($"TileEffects/{cleanName}");
    }

    void OnDestroy()
    {
        if (enemy != null) enemy.ResetTileSpeedMultiplier();
    }
}
