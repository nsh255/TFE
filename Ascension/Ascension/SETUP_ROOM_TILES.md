# Guía rápida: Setup de sala con tiles 16x16 y cámara estática

## 1. Preparar tus sprites de tiles (16x16)

1. Importa tus tiles en Unity (carpeta `Assets/Sprites/Tiles/`).
2. En el Inspector del sprite:
   - **Texture Type**: Sprite (2D and UI)
   - **Pixels Per Unit**: 16
   - **Filter Mode**: Point (no filter)
   - **Compression**: None
   - **Sprite Mode**: Multiple (si quieres slice múltiples tiles de una textura)
   - Apply.

## 2. Crear Tiles con efectos

1. Right click en Project → **Create → Ascension → Tiles → EffectTile**
2. Asigna el sprite del tile.
3. Configura propiedades:
   - **Effect**: Ice, Mud, Damage, Door, etc.
   - **Player Speed Mul**: 1.0 normal, < 1.0 ralentiza, > 1.0 acelera
   - **Enemy Speed Mul**: ídem para enemigos
   - **Is Door**: marca true si es tile de puerta
4. Repite para cada tipo de tile (suelo normal, hielo, fango, puerta, daño).

## 3. Crear Room Templates (ASCII art → tiles)

1. Right click → **Create → Ascension → Rooms → RoomTemplate**
2. En el Inspector:
   - **Rows**: pega tu diseño ASCII, ejemplo:
     ```
     ####################
     #..................#
     #...####....####...#
     #..................#
     #.........D........#
     #..................#
     ####################
     ```
     (cada línea = fila; primera línea = arriba)
   
3. En **Symbols**, mapea caracteres:
   - `.` → tile de suelo normal
   - `#` → tile de pared (o vacío)
   - `D` → tile de puerta (EffectTile con isDoor=true)
   - `~` → hielo
   - `M` → fango
4. Guarda templates: `Room_Start`, `Room_Normal1`, `Room_Treasure`, etc.

## 4. Configurar escena

### Grid y Tilemap
1. Hierarchy: Create → 2D Object → **Tilemap → Rectangular**
   - Esto crea: Grid → Tilemap
2. Selecciona **Grid**:
   - Cell Size: (1, 1, 0) — esto es automático si RoomCamera está bien configurado
3. Renombra Tilemap a "Ground"

### Cámara
1. Crea GameObject vacío "RoomCamera" (o añade componente a Main Camera)
2. Añade componente **RoomCamera** (script recién creado)
3. Configura:
   - **Room Width Tiles**: 20 (ancho de tu sala en tiles)
   - **Room Height Tiles**: 15 (alto de tu sala en tiles)
   - **Pixels Per Unit**: 16 (debe coincidir con tus sprites)
   - **Cam**: arrastra Main Camera
   - **Grid**: arrastra el Grid de la escena
4. Play: la cámara se centrará y ajustará ortho size automáticamente.

### Systems
1. Crea GameObject "Systems" y añade:
   - **TileEffectSystem**: asigna Grid y Tilemap (Ground)
   - **ScoreManager**
   - **ProjectilePool**: asigna prefab de proyectil e initial size (32)

### Level
1. Crea GameObject "Level" y añade:
   - **RoomBuilder**:
     - Grid: arrastra Grid
     - Ground Tilemap: arrastra Tilemap "Ground"
     - Room Width Tiles: 20
     - Room Height Tiles: 15
   - **RoomManager**:
     - Builder: arrastra RoomBuilder (mismo GO o referencia)
     - Enemy Manager: arrastra EnemyManager (ver abajo)
     - Ground Tilemap: arrastra Tilemap "Ground"
     - Start Template: arrastra tu RoomTemplate de inicio
     - Normal Templates: array con 2-3 templates normales
     - Special Templates: array con templates de tesoro/boss
     - Max Enemy Cost Per Room: 10 (ajusta según balance)
     - Special Room Chance: 0.2 (20% de salas especiales)

### Enemigos
1. Crea GameObject "EnemyManager" y añade componente **EnemyManager**
2. Configura:
   - **Enemies**: lista con entradas:
     - Prefab: SlimeRed prefab
     - Weight: 5
     - (repite para SlimeBlue weight=2, SlimeGreen weight=3)
   - **Balance**: arrastra tu GameBalance ScriptableObject (créalo si no existe)

## 5. Crear GameBalance (ScriptableObject)

1. Right click → **Create → Ascension → GameBalance**
2. Ajusta valores:
   - Base Health: 10
   - Base Move Speed: 5
   - IFrame Duration: 0.35
   - Enemy Spawn Weights (chaser: 5, shooter: 3, jumper: 2)
   - Enemy Count By Depth: curva que empieza en 3 y sube a 10
   - Weapon Drop Chance: 0.15
   - Heal Drop Chance: 0.10
   - Score values: kill=10, room=50, boss=500

## 6. Probar

1. Play.
2. Deberías ver la sala 0 (Start) dibujada centrada en pantalla.
3. Cámara estática muestra toda la sala.
4. Mata enemigos si hay.
5. Ve sobre un tile de puerta (símbolo 'D') y pulsa **E** → siguiente sala.
6. Tiles con efecto (hielo/fango) ralentizan (cuando integres efectos en Player).

## 7. Dimensiones típicas

- Sala pequeña: 15x12 tiles
- Sala mediana: 20x15 tiles
- Sala grande: 25x18 tiles

Asegúrate de que `RoomCamera.roomWidthTiles` y `RoomBuilder.roomWidthTiles` coincidan.

## 8. Siguiente paso

Integrar efectos de tiles en movimiento del Player y enemigos (llamo a `TileEffectSystem.ApplyContinuousEffects()` desde FixedUpdate).
