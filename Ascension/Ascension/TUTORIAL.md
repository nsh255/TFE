# 🚀 TUTORIAL AUTO-SETUP - Ascension

## ✅ SCRIPTS CREADOS

Se han creado **5 scripts de Editor** para automatizar toda la configuración:

### 📁 Assets/Scripts/Editor/
1. **GameSceneSetup.cs** - Configura GameScene completo
2. **EffectTileCreator.cs** - Crea 4 tiles con efectos
3. **RoomTemplateCreator.cs** - Crea 3 room templates
4. **RoomManagerSetup.cs** - Asigna referencias automáticamente
5. **EnemyDataValidator.cs** - Verifica costs de enemigos

---

## 🎯 ORDEN DE EJECUCIÓN (5 minutos total)

### **PASO 1: Configure GameScene** (1 min)
```
Menu: Ascension → Setup → 1. Configure GameScene
```

**Qué hace:**
- ✅ Crea Grid + Tilemap
- ✅ Añade RoomCamera a Main Camera
- ✅ Crea GameObject "Systems" con:
  - TileEffectSystem
  - ScoreManager
  - ProjectilePool
- ✅ Crea GameObject "LevelManager" con:
  - RoomBuilder
  - RoomManager
  - EnemyManager

**Resultado:** GameScene con toda la estructura base.

---

### **PASO 2: Create Effect Tiles** (30 seg)
```
Menu: Ascension → Setup → 2. Create Effect Tiles
```

**Qué hace:**
- ✅ Crea `Tile_Suelo.asset` (normal, 1x speed)
- ✅ Crea `Tile_Hielo.asset` (ice, 0.5x player, 0.3x enemy)
- ✅ Crea `Tile_Fango.asset` (mud, 0.6x speed)
- ✅ Crea `Tile_Puerta.asset` (door, isDoor=true)

**Ubicación:** `Assets/Tiles/`

**Nota:** Los tiles NO tienen sprites asignados (aparecerán blancos).

---

### **PASO 3: Create Room Templates** (30 seg)
```
Menu: Ascension → Setup → 3. Create Room Templates
```

**Qué hace:**
- ✅ Crea `Room_Start.asset` (sala inicial simple)
- ✅ Crea `Room_Normal1.asset` (sala con hielo)
- ✅ Crea `Room_Normal2.asset` (sala con fango)

**Ubicación:** `Assets/Data/Rooms/`

**Dimensiones:** 20x11 tiles con ASCII art

---

### **PASO 4: Assign Templates to RoomManager** (30 seg)
```
Menu: Ascension → Setup → 4. Assign Templates to RoomManager
```

**Qué hace:**
- ✅ Asigna `Room_Start` como startTemplate
- ✅ Asigna `Room_Normal1` y `Room_Normal2` como normalTemplates
- ✅ Asigna prefabs de enemigos al EnemyManager:
  - SlimeRed (weight=5, cost=2)
  - SlimeBlue (weight=2, cost=3)
  - SlimeGreen (weight=3, cost=4)

**Resultado:** RoomManager listo para spawnear salas y enemigos.

---

### **PASO 5: Verify EnemyData Costs** (30 seg)
```
Menu: Ascension → Setup → 5. Verify EnemyData Costs
```

**Qué hace:**
- ✅ Verifica SlimeRedData → enemyCost = 2
- ✅ Verifica SlimeBlueData → enemyCost = 3
- ✅ Verifica SlimeGreenData → enemyCost = 4
- ✅ Corrige automáticamente si están mal

**Resultado:** EnemyData configurados correctamente para spawning.

---

## 🎮 DESPUÉS: AÑADIR SCOREDISPLAY

**MANUAL (1 minuto):**

```
Canvas → UI → Text - TextMeshPro

Configurar:
- Name: "ScoreText"
- Text: "Score: 0"
- Font Size: 36
- Anchor: Top-Right
- Position: (-150, -50, 0)

Add Component → ScoreDisplay
- Score Text: [arrastrar TextMeshProUGUI]
```

---

## ✅ VERIFICACIÓN

Después de ejecutar los 5 scripts, deberías tener:

### **Jerarquía GameScene:**
```
GameScene
├─ Grid
│  └─ Ground (Tilemap)
├─ Main Camera (+ RoomCamera)
├─ Systems
│  ├─ TileEffectSystem ✅
│  ├─ ScoreManager ✅
│  └─ ProjectilePool ✅
├─ LevelManager
│  ├─ RoomBuilder ✅
│  ├─ RoomManager ✅
│  └─ EnemyManager ✅
└─ Canvas
   └─ ScoreText (+ ScoreDisplay) [manual]
```

### **Assets Creados:**
```
Assets/
├─ Tiles/
│  ├─ Tile_Suelo.asset ✅
│  ├─ Tile_Hielo.asset ✅
│  ├─ Tile_Fango.asset ✅
│  └─ Tile_Puerta.asset ✅
└─ Data/Rooms/
   ├─ Room_Start.asset ✅
   ├─ Room_Normal1.asset ✅
   └─ Room_Normal2.asset ✅
```

### **Referencias Asignadas:**
```
RoomManager:
- startTemplate: Room_Start ✅
- normalTemplates: [Room_Normal1, Room_Normal2] ✅
- enemyManager: EnemyManager ✅

EnemyManager:
- entries[0]: SlimeRed, weight=5 ✅
- entries[1]: SlimeBlue, weight=2 ✅
- entries[2]: SlimeGreen, weight=3 ✅

EnemyData:
- SlimeRedData.enemyCost = 2 ✅
- SlimeBlueData.enemyCost = 3 ✅
- SlimeGreenData.enemyCost = 4 ✅
```

---

## 🎨 OPCIONAL: Añadir Sprites a Tiles

**DESPUÉS de que todo funcione:**

1. Crea sprites 16x16 (Photoshop/Aseprite)
2. Import con PPU=16, Point filter
3. Selecciona cada tile en inspector
4. Asigna sprite correspondiente

---

## 🧪 TESTING

```
Play Mode:

1. Sala dibujada con tiles (blancos sin sprites) ✅
2. Enemigos spawneados según cost ✅
3. Matar enemigo → Score sube ✅
4. Caminar sobre tiles → Velocidad afectada ✅
5. Pararse en puerta + E → Nueva sala ✅
```

---

## 🚨 SI ALGO FALLA

### "No se encuentra GameScene"
→ Abre GameScene antes de ejecutar scripts

### "No se encuentran tiles"
→ Ejecuta scripts en orden: 1 → 2 → 3 → 4 → 5

### "Enemigos no spawnean"
→ Verifica que prefabs existan en `Assets/Prefabs/Enemies/`

### "Puerta no funciona"
→ Verifica Player tiene tag "Player"

---

## 📊 RESUMEN EJECUTIVO

```
ANTES:
- Configuración manual (30-60 minutos)
- Propenso a errores
- Muchos pasos manuales

AHORA:
- 5 clicks en menú (5 minutos)
- Auto-configuración completa
- Sin errores humanos

RESULTADO:
- GameScene funcional
- 4 Tiles configurados
- 3 Salas creadas
- Referencias asignadas
- EnemyData validados
```

---

## 🎯 PASOS A EJECUTAR AHORA

**En Unity, ejecuta en orden:**

1. `Ascension → Setup → 1. Configure GameScene`
2. `Ascension → Setup → 2. Create Effect Tiles`
3. `Ascension → Setup → 3. Create Room Templates`
4. `Ascension → Setup → 4. Assign Templates to RoomManager`
5. `Ascension → Setup → 5. Verify EnemyData Costs`
6. Añadir ScoreDisplay manualmente (1 min)
7. **¡JUGAR!** 🎮

---

**¡Automatización completa en 5 minutos!** 🚀
