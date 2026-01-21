# Guía: Sistema de Tilemaps con Muros y Efectos

## Paso 1: Crear VariantTiles desde tus Sprites

### 1.1 Preparar Sprites
1. **Selecciona tus sprites de tiles** en `Assets/Sprites/Tiles/`
2. Para cada sprite:
   - Inspector → Sprite Mode: **Single** (ya debería estar así)
   - PPU: **16**
   - Filter Mode: **Point**

### 1.2 Crear VariantTiles (Tiles con Múltiples Variantes)

En lugar de crear un Tile por cada sprite, ahora usamos **VariantTile** para agrupar variantes visuales con el mismo efecto:

1. **Crear VariantTile para Muros**:
   - Click derecho en `Assets/Tiles/` → Create → Tiles → **VariantTile**
   - Nombra: `WallVariantTile`
   - Inspector:
     - **Variants**: Arrastra todos los sprites de muro (wall_1, wall_2, wall_3...)
     - **Deterministic**: ✅ (para consistencia entre sesiones)
     - **Seed**: 1337 (o cualquier número)
     - **Tile Effect**: Dejar vacío (muros no tienen efecto)

2. **Crear VariantTile para Suelo Normal**:
   - Create → Tiles → **VariantTile**
   - Nombra: `FloorVariantTile`
   - Inspector:
     - **Variants**: Arrastra todos los sprites de suelo normal (floor_1, floor_2, floor_3...)
     - **Deterministic**: ✅
     - **Tile Effect**: Dejar vacío

3. **Crear VariantTile para Tiles Especiales** (uno por cada tipo de efecto):
   - **IceVariantTile**:
     - **Variants**: ice_1, ice_2, ice_3...
     - **Tile Effect**: IceTileEffect (crear en Paso 3)
   - **MudVariantTile**:
     - **Variants**: mud_1, mud_2, mud_3...
     - **Tile Effect**: MudTileEffect
   - **HealVariantTile**:
     - **Variants**: heal_1, heal_2, heal_3...
     - **Tile Effect**: HealTileEffect

**Ventaja**: Ahora con 3 VariantTiles en lugar de 9 Tiles individuales, obtienes variedad visual automática + efectos unificados.

## Paso 2: Configurar Tilemaps en la Escena

1. **Crear Grid**:
   - Hierarchy → Click derecho → 2D Object → **Tilemap** → **Rectangular**
   - Esto crea: `Grid` (padre) → `Tilemap` (hijo)
   
2. **Crear segundo Tilemap para Muros**:
   - Click derecho en `Grid` → 2D Object → **Tilemap**
   - Renombrar los Tilemaps:
     - Primer Tilemap → `FloorTilemap`
     - Segundo Tilemap → `WallTilemap`

3. **Configurar WallTilemap** (muros colisionables):
   - Seleccionar `WallTilemap`
   - Add Component → **Tilemap Collider 2D**
   - Add Component → **Composite Collider 2D** (opcional, optimiza colliders)
   - Si añades Composite Collider:
     - Tilemap Collider 2D → marcar **Used By Composite**
     - Composite Collider 2D → Geometry Type: **Polygons**
   - Rigidbody2D se añade automáticamente → Body Type: **Static**

4. **Ordenar Sorting Layers**:
   - Edit → Project Settings → Tags and Layers → Sorting Layers
   - Añadir (si no existen):
     ```
     Default
     Floor      (Order in Layer: 0)
     Entities   (Order in Layer: 1)
     Walls      (Order in Layer: 2)
     UI         (Order in Layer: 10)
     ```
   - Asignar a cada Tilemap:
     - `FloorTilemap` → Tilemap Renderer → Sorting Layer: **Floor**
     - `WallTilemap` → Tilemap Renderer → Sorting Layer: **Walls**
   - Jugador y enemigos → SpriteRenderer → Sorting Layer: **Entities**

## Paso 3: Crear TileEffects (ScriptableObjects)

1. **Crear carpeta**: `Assets/Data/TileEffects/` (no necesita estar en Resources, ahora se referencian directamente)
2. **Crear TileEffects**:
   - Click derecho en la carpeta → Create → Tiles → **TileEffect**
   - Crear uno por cada tipo de tile especial:

### Ejemplo: HealTile
```
Tile Name: heal
Effect Type: Heal
Health Change: 1
Speed Multiplier: 1
Effect Duration: 0
Tick Rate: 0.5
Continuous: true
```

### Ejemplo: IceTile
```
Tile Name: ice
Effect Type: Ice
Speed Multiplier: 1.5
Effect Duration: 0
Continuous: false
```

### Ejemplo: MudTile
```
Tile Name: mud
Effect Type: Mud
Speed Multiplier: 0.5
Effect Duration: 1 (sigue lento 1s después de salir)
Continuous: false
```

3. **Asignar TileEffects a VariantTiles**:
   - Vuelve a cada VariantTile especial creado en Paso 1.2
   - En el Inspector → **Tile Effect**: arrastra el TileEffect correspondiente
   - Ejemplo: `IceVariantTile` → arrastra `IceTileEffect.asset`
   
**Nota**: Ya no es necesario que los nombres coincidan, el efecto se asigna directamente en el VariantTile.

## Paso 4: Configurar RoomGenerator

1. **Crear GameObject vacío** en la escena: `RoomManager`
2. Add Component → **RoomGenerator**
3. **Configurar en Inspector**:
   ```
   Wall Tilemap: [arrastrar WallTilemap]
   Floor Tilemap: [arrastrar FloorTilemap]
   Wall Tile: [arrastrar WallVariantTile]
   
   Normal Floor Tiles:
     - Element 0: FloorVariantTile
     (puedes añadir más VariantTiles de suelo si quieres diferentes "familias")
   
   Special Floor Tiles:
     - Element 0: IceVariantTile
     - Element 1: MudVariantTile
     - Element 2: HealVariantTile
   
   Room Width: 30
   Room Height: 20
   Special Tile Chance: 0.1 (10% de tiles especiales)
   ```
   
**Resultado**: Cada posición usará automáticamente un sprite aleatorio (pero determinista) de las variantes del VariantTile seleccionado.
4. **Generar habitación**:
   - Click derecho en RoomGenerator (Inspector) → **Generate Room**
   - O marca el checkbox en Start() del script para generarla automáticamente

## Paso 5: Configurar FloorTileManager

1. **Añadir al jugador o crear Manager**:
   - Opción A: Añadir FloorTileManager al GameObject del Player
   - Opción B: Crear GameObject `FloorManager` y añadir el componente
   
2. **Configurar en Inspector**:
   ```
   Floor Tilemap: [arrastrar FloorTilemap]
   Player Transform: [arrastrar Player] (o dejar vacío para auto-buscar)
   ```

## Paso 6: Configurar Layers y Collisiones

1. **Layers recomendados**:
   - Edit → Project Settings → Tags and Layers → Layers:
     ```
     Layer 8: Player
     Layer 9: Enemy
     Layer 10: Wall
     Layer 11: Floor (sin colliders)
     ```

2. **Asignar Layers**:
   - `WallTilemap` → Layer: **Wall**
   - `FloorTilemap` → Layer: **Floor**
   - Player → Layer: **Player**
   - Enemigos → Layer: **Enemy**

3. **Configurar Physics**:
   - Edit → Project Settings → Physics 2D → Layer Collision Matrix
   - Asegurar:
     - Player ↔ Wall: ✅ (colisiona)
     - Enemy ↔ Wall: ✅
     - Player ↔ Enemy: ✅ (o ❌ si usas triggers para daño)
     - Floor no colisiona con nada (solo visual + detección de efectos)

## Paso 7: Añadir Método Heal a PlayerHealth

Si `PlayerHealth` no tiene método `Heal()`, añádelo:

```csharp
public void Heal(int amount)
{
    currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    UpdateHeartDisplay();
    Debug.Log($"[PlayerHealth] Curado {amount} HP. HP actual: {currentHealth}/{maxHealth}");
}
```

## Testing

1. **Play**: La habitación debe generarse con muros en los bordes
2. **Mueve al jugador** sobre diferentes tiles
3. **Observa la Consola**:
   - Debe mostrar `[FloorTileManager] Jugador pisó tile: ...`
   - Efectos de velocidad/curación deben aplicarse
4. **Si no detecta tiles**:
   - Verifica que FloorTileManager.floorTilemap esté asignado
   - Verifica que cada VariantTile especial tenga su TileEffect asignado en el Inspector
   - Si usas tiles antiguos (sin VariantTile), asegúrate de tener TileEffects en `Resources/TileEffects/` con nombres coincidentes

## Próximos Pasos

- **VFX**: Asigna prefabs de partículas a `TileEffect.vfxPrefab` para efectos visuales
- **Generación procedural**: Modifica `RoomGenerator` para crear múltiples habitaciones conectadas
- **Enemigos**: Añade spawn points aleatorios usando `EnemyManager.SpawnWave()`
- **Puertas/Escaleras**: Crea tiles especiales que carguen nuevas escenas o habitaciones

---

**¿Problemas comunes?**
- *Muros no colisionan*: Verifica TilemapCollider2D en WallTilemap
- *Tiles sin efectos*: Revisa nombres y ruta Resources/TileEffects/
- *Jugador atraviesa muros*: Asegura que Player tenga Rigidbody2D + Collider2D
- *Velocidad no cambia*: Verifica que PlayerController.playerClass.speed sea accesible
