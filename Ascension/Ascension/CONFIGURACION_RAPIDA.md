# 🎮 GUÍA RÁPIDA DE CONFIGURACIÓN - ASCENSION

## ✅ LO QUE ACABAS DE CONSEGUIR:

### 📜 Scripts Creados:
1. ✅ **TileEffectSystem.cs** - Sistema de efectos de terreno (hielo, lodo, daño, puertas)
2. ✅ **DoorIndicator.cs** - Indicador visual de puertas disponibles
3. ✅ **7 Room Templates** - Salas variadas listas para usar

### 🗺️ Room Templates Creadas:
- **Room_Start.asset** - Sala inicial sin enemigos
- **Room_Normal1.asset** - Sala con hielo (~) y obstáculos
- **Room_Normal2.asset** - Sala con lodo (M) 
- **Room_Normal3.asset** - Sala con pilares centrales
- **Room_Normal4.asset** - Sala mixta (hielo + lodo)
- **Room_Treasure.asset** - Sala de tesoro (T marca el centro)
- **Room_Boss.asset** - Sala de boss (B marca spawn del boss)

---

## 🔧 CONFIGURACIÓN EN UNITY (Paso a Paso)

### 1️⃣ CONFIGURAR TILES (Hazlo UNA vez)

#### A) Tile_Floor (Suelo Normal)
1. Abre `Assets/Tiles/Tile_Floor.asset`
2. Configura:
   - **Effect**: None
   - **Player Speed Mul**: 1.0
   - **Enemy Speed Mul**: 1.0
   - **Is Door**: false

#### B) Tile_Ice (Hielo - Resbala)
1. Abre `Assets/Tiles/Tile_Ice.asset`
2. Configura:
   - **Effect**: Ice
   - **Player Speed Mul**: 1.5 (más rápido)
   - **Enemy Speed Mul**: 1.5
   - **Slippery**: true (opcional)
   - **Is Door**: false

#### C) Tile_Mud (Lodo - Ralentiza)
1. Abre `Assets/Tiles/Tile_Mud.asset`
2. Configura:
   - **Effect**: Mud
   - **Player Speed Mul**: 0.5 (más lento)
   - **Enemy Speed Mul**: 0.5
   - **Is Door**: false

#### D) Tile_Door (Puerta)
1. Abre `Assets/Tiles/Tile_Door.asset`
2. Configura:
   - **Effect**: Door
   - **Player Speed Mul**: 1.0
   - **Enemy Speed Mul**: 1.0
   - **Is Door**: TRUE ⚠️ (IMPORTANTE)

---

### 2️⃣ CONFIGURAR ROOM TEMPLATES

Por cada Room Template (Room_Start, Room_Normal1, etc.):

1. Abre el asset en el Inspector
2. En **Symbols**, configura los mapeos:
   - `.` (punto) → Tile_Floor
   - `#` (almohadilla) → null (vacío, pared)
   - `D` (Door) → Tile_Door
   - `~` (tilde) → Tile_Ice
   - `M` (M mayúscula) → Tile_Mud
   - `T` (Treasure) → Tile_Floor (o un tile especial)
   - `B` (Boss) → Tile_Floor

**IMPORTANTE:** El símbolo se representa como un número ASCII:
- `.` = 46
- `#` = 35
- `D` = 68
- `~` = 126
- `M` = 77
- `T` = 84
- `B` = 66

---

### 3️⃣ CONFIGURAR GAMESCENE

#### A) Crear Jerarquía de GameObject:

```
GameScene
├─ Grid
│  └─ Ground (Tilemap)
├─ Main Camera
│  └─ RoomCamera (componente)
├─ Systems
│  ├─ TileEffectSystem
│  ├─ ScoreManager
│  └─ ProjectilePool (si existe)
├─ Level
│  ├─ RoomBuilder (componente)
│  ├─ RoomManager (componente)
│  └─ EnemyManager (componente)
├─ Canvas
│  ├─ HeartDisplay
│  ├─ ScoreText
│  └─ DoorIndicator (Panel con texto "Presiona E")
└─ EventSystem
```

#### B) Configurar Grid + Tilemap:

1. **Hierarchy** → Right Click → **2D Object → Tilemap → Rectangular**
2. Renombra el Tilemap a **"Ground"**
3. Selecciona **Grid**:
   - Cell Size: (1, 1, 0) - AUTO

#### C) Configurar Main Camera + RoomCamera:

1. Selecciona **Main Camera**
2. Add Component → **RoomCamera**
3. Configura:
   - **Room Width Tiles**: 20
   - **Room Height Tiles**: 15
   - **Pixels Per Unit**: 16
   - **Cam**: Arrastra Main Camera
   - **Grid**: Arrastra Grid

#### D) Configurar Systems:

1. Crea GameObject vacío **"Systems"**
2. Add Component → **TileEffectSystem**:
   - **Grid**: Arrastra Grid
   - **Ground Tilemap**: Arrastra Ground
3. Add Component → **ScoreManager** (si existe)

#### E) Configurar Level:

1. Crea GameObject vacío **"Level"**
2. Add Component → **RoomBuilder**:
   - **Grid**: Arrastra Grid
   - **Ground Tilemap**: Arrastra Ground
   - **Room Width Tiles**: 20
   - **Room Height Tiles**: 15

3. Add Component → **EnemyManager**:
   - **Enemies**: Configura lista:
     - Prefab: SlimeRed, Weight: 5
     - Prefab: SlimeBlue, Weight: 3
     - Prefab: SlimeGreen, Weight: 2

4. Add Component → **RoomManager**:
   - **Builder**: Arrastra RoomBuilder (mismo GO)
   - **Enemy Manager**: Arrastra EnemyManager (mismo GO)
   - **Ground Tilemap**: Arrastra Ground
   - **Start Template**: Room_Start
   - **Normal Templates** (Array de 4):
     - [0] Room_Normal1
     - [1] Room_Normal2
     - [2] Room_Normal3
     - [3] Room_Normal4
   - **Special Templates** (Array de 2):
     - [0] Room_Treasure
     - [1] Room_Boss
   - **Max Enemy Cost Per Room**: 10
   - **Special Room Chance**: 0.2 (20%)

#### F) Configurar UI (Canvas):

1. Si no existe, crea **Canvas** (UI → Canvas)
2. Dentro del Canvas, crea GameObject **"DoorIndicatorPanel"**
3. Añade componente **Text** o **TextMeshPro** con texto: **"Presiona E para avanzar"**
4. Desactiva el panel por defecto (checkbox en Inspector)
5. En Hierarchy, crea GameObject vacío **"DoorIndicatorManager"**
6. Add Component → **DoorIndicator**:
   - **Indicator UI**: Arrastra el panel "DoorIndicatorPanel"
   - **Detection Radius**: 2.0

---

### 4️⃣ CONFIGURAR PLAYER PREFAB

1. Abre **Player.prefab** (en Prefabs/)
2. Selecciona el prefab
3. Verifica que tenga:
   - **PlayerController**
   - **PlayerHealth**: 
     - **Heart Display**: Arrastra el HeartDisplay del Canvas
   - **Rigidbody2D** (Gravity Scale = 0, Linear Drag = 10)
   - **Collider2D** (CircleCollider o Capsule)

---

## 🎮 CÓMO JUGAR

1. **Play** en Unity
2. El jugador spawneará en Room_Start (sin enemigos)
3. Camina hacia la puerta marcada con `D`
4. Presiona **E** para avanzar a la siguiente sala
5. Mata todos los enemigos
6. Repite

---

## 🐛 TROUBLESHOOTING

### ❌ "NullReferenceException en TileEffectSystem"
- Asegúrate de que TileEffectSystem tenga asignado Grid y Tilemap

### ❌ "No se ven los tiles"
- Verifica que cada RoomTemplate tenga configurados los Symbols correctamente
- Asegúrate de que los tiles tengan sprites asignados

### ❌ "El jugador no se ralentiza en hielo/lodo"
- Verifica que Tile_Ice tenga playerSpeedMul configurado
- Verifica que TileEffectSystem esté activo en la escena

### ❌ "Las puertas no funcionan"
- Asegúrate de que Tile_Door tenga **isDoor = TRUE**
- Verifica que RoomManager esté detectando enemigos correctamente

### ❌ "Los enemigos no aparecen"
- Verifica que EnemyManager tenga prefabs asignados
- Verifica que las salas tengan maxEnemyCostPerRoom > 0

---

## 📝 PRÓXIMOS PASOS RECOMENDADOS

1. ✅ **Probar el juego** y verificar que todo funciona
2. 🎨 **Añadir sprites** a los tiles (ahora son placeholders)
3. 💥 **Añadir VFX** (partículas de ataque, muerte, transiciones)
4. 🔊 **Añadir SFX** (disparos, golpes, pasos)
5. ⚔️ **Sistema de drop de armas** aleatorias
6. 🏆 **Sistema de score** y guardado
7. 🎵 **Música de fondo** por bioma

---

## 💡 TIPS

- Usa **Ctrl + D** en Unity para duplicar Room Templates y crear variaciones
- Los tiles con efecto se pueden combinar (ej: lodo + daño)
- Ajusta `maxEnemyCostPerRoom` en RoomManager para balancear dificultad
- Los enemigos cuestan según su EnemyData (weight)

---

**¡Ahora tienes un sistema de salas procedurales funcional!** 🎉
