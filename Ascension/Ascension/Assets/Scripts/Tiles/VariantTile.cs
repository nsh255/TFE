using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Tile genérico con múltiples variantes de sprite y un efecto asociado.
/// Permite reducir la cantidad de Tile assets: un VariantTile = N sprites + 1 efecto.
/// </summary>
[CreateAssetMenu(menuName = "Tiles/VariantTile")]
public class VariantTile : TileBase
{
    [Header("Sprites Variantes")]
    [Tooltip("Lista de sprites que se usarán como variantes visuales")] public Sprite[] variants;
    [Tooltip("Si es verdadero, el índice se calcula de forma determinista (hash posición). Si es falso, se seleccionará un sprite pseudo-aleatorio cada vez que se pinte.")] public bool deterministic = true;
    [Tooltip("Semilla adicional para variar la distribución en distintas habitaciones.")] public int seed = 1337;

    [Header("Efecto del Tile")]
    [Tooltip("Efecto que se aplica al pisar este tile (opcional)")] public TileEffect tileEffect;

    [Header("Ajustes Visuales")]
    public Color tint = Color.white;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.color = tint;
        tileData.flags = TileFlags.LockTransform;
        tileData.colliderType = Tile.ColliderType.None; // Collider se maneja por Tilemap de muros, no aquí

        if (variants == null || variants.Length == 0)
        {
            tileData.sprite = null;
            return;
        }

        int index;
        if (deterministic)
        {
            // Hash simple de posición para selección estable
            unchecked
            {
                int hash = seed;
                hash = hash * 31 + position.x;
                hash = hash * 17 + position.y;
                if (position.x != 0) hash ^= position.x << 2;
                if (position.y != 0) hash ^= position.y << 5;
                if (hash < 0) hash = -hash;
                index = hash % variants.Length;
            }
        }
        else
        {
            // Pseudo-aleatorio. No recomendado si quieres consistencia entre sesiones.
            index = Random.Range(0, variants.Length);
        }

        tileData.sprite = variants[index];
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        // Permite re-pintar si se cambian las variantes en runtime (editor)
        base.RefreshTile(position, tilemap);
    }
}
