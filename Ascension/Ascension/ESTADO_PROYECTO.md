# 🎮 ESTADO ACTUAL DEL PROYECTO ASCENSION - ROGUELIKE 2D

**Fecha:** 7 de diciembre de 2025  
**Repositorio:** nsh255/ascension  
**Engine:** Unity 2D  
**Género:** Roguelike procedural con combate en tiempo real

---

## 📊 RESUMEN EJECUTIVO

### ✅ LO QUE FUNCIONA (Sistemas Implementados)

#### 1. **Sistema de Personajes y Clases** ✅
- **3 clases jugables** completamente funcionales:
  - Knight (equilibrado)
  - Mage (mágico, arma de distancia)
  - Swordsman (espadachín ágil)
- Cada clase tiene stats únicos (HP, velocidad, arma inicial)
- Sistema de selección de clase en menú funcional
- ScriptableObjects configurables: `Assets/Data/Classes/`

#### 2. **Sistema de Movimiento del Jugador** ✅
- Movimiento WASD suave y responsive
- Sistema de dash/roll con cooldown (KeyCode.Space por defecto)
- **Roll con invulnerabilidad opcional**: `invulnerableDuringRoll = true`
- **Multiplicador de velocidad en roll**: `rollSpeedMultiplier = 2f`
- **Cooldown configurable**: `rollCooldown = 0.6f`
- Animaciones fluidas (Idle, Walk en 4 direcciones)
- Detección de dirección automática para las animaciones
- Configurado para pixel art (PPU 16)
- **Inicialización manual**: `Initialize()` debe llamarse después de asignar `playerClass`
- Sistema de `lastInputDirection` para recordar dirección del roll

#### 3. **Sistema de Combate - Armas** ✅
**Implementado:**
- ✅ Armas cuerpo a cuerpo (espada, espadón)
- ✅ Armas de distancia (bastón/staff con proyectiles)
- ✅ Sistema de daño a enemigos funcional
- ✅ Cooldown entre ataques
- ✅ Rotación del arma hacia el cursor del mouse
- ✅ **Sistema de órbita**: El arma orbita alrededor del jugador siguiendo el cursor (estilo Tiny Rogues)
- ✅ Scripts: `Weapon.cs`, `MeleeWeapon.cs`, `RangedWeapon.cs`, `Projectile.cs`, `WeaponHitbox.cs`, `WeaponTrail.cs`
- ✅ WeaponData ScriptableObjects: Sword, GreatSword, Staff

**Características:**
- Cada arma tiene: nombre, daño (`damage`), velocidad de ataque (`atackSpeed`), sprite, bulletPrefab, weaponPrefab
- Las armas de distancia disparan proyectiles con prefab configurable
- Sistema de hitbox para armas melee con `WeaponHitbox.cs` (GameObject hijo con Collider2D)
- **MeleeWeapon**: Sistema de swing animado con ángulo y duración configurables
  - `swingAngle = 180°` por defecto (barrido completo)
  - `swingDuration = 0.3s` por defecto
  - Trail visual durante el ataque (`WeaponTrail.cs`)
- **RangedWeapon**: Dispara proyectiles desde `spawner` Transform
  - Cooldown: `attackCooldown = 0.3f`
  - Los proyectiles se escalan automáticamente (20x para visibilidad)
- **Projectile**: 
  - Velocidad: `speed = 10f`
  - Lifetime: `lifetime = 3f`
  - Daño boosteado por `DamageBoostManager`
  - Auto-ajuste de escala según PPU de la cámara
- **Bloqueo durante roll**: Las armas no pueden atacar mientras el jugador hace roll
- **Input de ataque**: Click izquierdo del mouse (`GetMouseButtonDown(0)`)

#### 4. **Sistema de Vida del Jugador** ✅
- PlayerHealth con HP máxima por clase
- Métodos de daño y curación funcionando
- Sistema de invencibilidad temporal tras recibir daño
- **UI de corazones implementada**: `HeartDisplay.cs` muestra la vida visualmente

#### 5. **Sistema de Enemigos** ✅
**Enemigos implementados:**
- ✅ **SlimeBlue (Saltador/Dasher)**: Dash rápido hacia el jugador en 8 direcciones
  - `dashSpeed = 12f`, `dashDuration = 0.4s`, `dashCooldown = 2s`
  - Rango de dash: `minDashDistance = 3f` a `maxDashDistance = 10f`
  - Sin movimiento cuando no está dasheando
- ✅ **SlimeGreen (Francotirador)**: Dispara de lejos, escapa si te acercas
  - `optimalShootDistance = 8f` (distancia ideal para disparar)
  - `escapeDistance = 4f` (si el jugador se acerca más, huye)
  - `maxDistance = 12f` (si está más lejos, se acerca)
  - `shootCooldown = 2s`, `projectileSpeed = 5f`
  - Comportamiento adaptativo: Escape → Disparo → Acercamiento
- ✅ **SlimeRed**: (Archivo existe, implementación similar a base Enemy)
- ✅ **ShooterEnemy**: Placeholder con `EnemyProjectileSpawner`
- ✅ **JumperEnemy**: Placeholder
- ✅ **ChaserEnemy**: Placeholder  
- ✅ **BossEnemy**: Placeholder para jefe final

**Características:**
- Cada enemigo tiene `EnemyData` ScriptableObject:
  - `enemyName`, `maxHealth`, `damage`, `speed`, `sprite`
  - `enemyCost`: Valor para spawning basado en presupuesto
- **Sistema de daño al jugador**: Daño continuo por contacto (`damageRate = 1f` por segundo)
- **Flash visual**: Color rojo al recibir daño (0.1s)
- **Animación de muerte**: Trigger "Die" en Animator
- **IA modular**: Cada tipo hereda de `Enemy.cs` base
- Proyectiles de enemigos funcionales (`EnemyProjectile.cs`)
  - Lifetime: `lifetime = 5f`
  - Auto-escala según PPU
  - Detecta colisión con jugador y paredes
- **EnemyManager**: Sistema de spawn avanzado
  - `SpawnWave()`: Spawn por cantidad y profundidad
  - `SpawnByCost()`: Spawn basado en presupuesto de costo total
  - Sistema de pesos (`weight`) para rareza de enemigos
- Los enemigos reciben efectos de tiles: `EnemyTileEffectReceiver.cs`

#### 6. **Sistema de Tiles y Efectos** ✅ (RECIÉN ARREGLADO)
**Tipos de tiles implementados:**
- ✅ **Floor** (suelo normal) - 3 variantes (floor1, floor2, floor3)
- ✅ **Wall** (muro con collider)
- ✅ **Ice** (hielo) - 3 variantes (ice1, ice2, ice3)
  - Aumenta velocidad x1.5
  - Efecto inmediato sin duración
- ✅ **Mud** (barro) - 3 variantes (mud1, mud2, mud3)
  - Reduce velocidad x0.5
  - Efecto persiste 1s después de salir del tile
- ✅ **Heal** (curación)
  - Cura +1 HP cada 0.5s (continuo mientras estás sobre él)
  - `continuous = true`, `tickRate = 0.5f`
- ✅ **PowerUp** (power-up)
  - Aumenta daño +1 permanentemente vía `DamageBoostManager`
  - Efecto global afecta armas del jugador y proyectiles
- ✅ **Stairs** (escaleras para siguiente nivel - no implementado aún)

**Sistema:**
- **VariantTile.cs**: Sistema de tiles con múltiples sprites
  - Cada VariantTile puede tener array de sprites (`variants[]`)
  - Selección determinista por hash de posición o pseudo-aleatoria
  - `seed` configurable para variar distribución
  - Cada VariantTile tiene un `TileEffect` asignado directamente
- **TileEffect.cs**: ScriptableObject con propiedades de efecto
  - `tileName`, `effectType` (enum)
  - `healthChange`: HP que cura (+) o daña (-)
  - `speedMultiplier`: Multiplicador de velocidad (1 = normal)
  - `effectDuration`: Duración tras salir del tile
  - `continuous`: Si se aplica repetidamente
  - `tickRate`: Tiempo entre aplicaciones
  - `tintColor`, `vfxPrefab`: Efectos visuales opcionales
- **TileEffectCreator.cs**: Herramienta de editor
  - Context menu "Crear TileEffects Automáticamente"
  - Crea todos los TileEffects en `Assets/Data/TileEffects/`
  - Actualiza automáticamente si ya existen
- **FloorTileManager.cs**: Detecta tile bajo el jugador
  - Usa reflexión para obtener `TileEffect` de `VariantTile`
  - Aplica efectos cada frame según tipo
  - Modifica velocidad del jugador directamente en `playerClass.speed`
  - Restaura velocidad al salir del tile
  - Sistema de corrutinas para efectos con duración
- **Enum TileEffectType**: None, Heal, Damage, SpeedUp, SpeedDown, Ice, Mud, PowerUp
- **DamageBoostManager**: Singleton persistente para boost global de daño
  - `AddDamageBoost(int)`: Incrementa boost
  - `GetBoostedDamage(int)`: Devuelve daño + boost
  - `ResetBoost()`: Resetea a 0

#### 7. **Generación Procedural Básica** ⚠️ (PARCIALMENTE IMPLEMENTADO)
- ✅ **RoomGenerator.cs**: Genera habitaciones rectangulares con tilemaps
  - **Parámetros configurables**:
    - `roomWidth = 26`, `roomHeight = 12` (tamaño en tiles)
    - `specialTileChance = 0.1f` (10% probabilidad de tiles especiales)
    - `roomOffset = (-13, -7, 0)` para centrar habitación
  - **Referencias necesarias**:
    - `wallTilemap`: Tilemap con TilemapCollider2D para muros
    - `floorTilemap`: Tilemap para el suelo
    - `wallTile`: TileBase para muros
    - `normalFloorTiles[]`: Array de VariantTiles normales
    - `specialFloorTiles[]`: Array de VariantTiles especiales (heal, ice, mud, powerup)
  - **Funcionalidad**:
    - Genera muros en los 4 bordes (incluye esquinas)
    - Llena interior con tiles de suelo aleatorios
    - Auto-busca Tilemaps si no están asignados (por nombre)
    - Context menu "Generate Room" para testing en editor
    - Método `GenerateRoom(int width, int height)` para tamaños custom
    - Preview visual con Gizmos en Scene View
- ❌ **RoomController.cs**: **COMPLETAMENTE VACÍO** - Archivo existe pero sin código
  - Falta sistema de conexión entre salas
  - Falta control de estado de sala (limpia/sucia)
  - Falta spawn de enemigos al entrar
  - Falta detección de todos los enemigos muertos
- ❌ **No hay sistema de puertas**
- ❌ **No hay generación de dungeon completo** (múltiples salas conectadas)
- ❌ **No hay RoomData ScriptableObject**

#### 8. **UI Funcional** ✅
- **Menú principal** (`MainMenuManager.cs`)
  - `PlayGame()`: Carga escena ClassSelection
  - `QuitGame()`: Cierra aplicación
- **Selección de clase** (`ClassSelectionManager.cs`)
  - Array de `availableClasses[]` (PlayerClass ScriptableObjects)
  - Método `SelectClassAndStartGame(int index)` para botones
  - Variable estática `SelectedClass` compartida entre escenas
  - Carga escena "GameScene" tras selección
- **Display de corazones** (`HeartDisplay.cs`)
  - **Sistema nuevo y mejorado** que reemplaza al sistema antiguo
  - Sprites: `heartFull` y `heartEmpty`
  - Layout configurable: `spacing = 3f`, `heartScale = 0.1f` (pixel art)
  - `InitializeHearts(int maxHealth)`: Crea corazones dinámicamente
  - `UpdateHearts(int currentHealth)`: Actualiza sprites lleno/vacío
  - Puede usar prefab o generar corazones automáticamente
  - Posicionamiento horizontal automático con espaciado
- **Display de puntuación** (`ScoreDisplay.cs`)
  - Conectado a `ScoreManager` singleton
  - Actualiza Text UI automáticamente
- **ScoreManager** (`ScoreManager.cs`)
  - Singleton persistente con `DontDestroyOnLoad`
  - `Add(int amount)`: Suma puntos
  - `ResetScore()`: Resetea a 0
  - `CurrentScore`: Propiedad pública de solo lectura
- **EventSystem persistente** (`PersistentEventSystem.cs`, `SingletonEventSystem.cs`)
  - Previene múltiples EventSystems entre escenas
- **PlayerSpawner** (`PlayerSpawner.cs`)
  - Instancia prefab del jugador en `spawnPoint`
  - Asigna `playerClass` desde `ClassSelectionManager.SelectedClass`
  - Conecta `HeartDisplay` automáticamente antes de inicializar
  - Llama a `Initialize()` en orden correcto
- **Otros helpers**:
  - `UIScalerSetup.cs`: Configuración de Canvas Scaler
  - `FixHeartContainerPosition.cs`: Ayuda de posicionamiento

#### 9. **Sistemas Core** ✅
- **ScoreManager** (`ScoreManager.cs`): Sistema de puntuación singleton persistente
  - `CurrentScore` property
  - `Add(int)`, `ResetScore()`
  - TODO: Conectar a UI (comentado en código)
- **PixelPerfectSetup** (`PixelPerfectSetup.cs`): Configuración para pixel art
  - Ajusta cámara y settings para pixel art perfecto
  - PPU (Pixels Per Unit) = 16 para todos los sprites
- **PixelArtSettings** (`PixelArtSettings.cs`): ScriptableObject de configuración
  - Centraliza settings de pixel art
- **DamageBoostManager** (`DamageBoostManager.cs`): Sistema de power-ups de daño
  - Singleton persistente con `DontDestroyOnLoad`
  - `globalDamageBoost`: Daño adicional que se suma a todos los ataques
  - `AddDamageBoost(int)`: Incrementa boost
  - `GetBoostedDamage(int baseDamage)`: Devuelve daño + boost
  - `ResetBoost()`: Resetea a 0 (útil al morir o cambiar nivel)
  - Afecta tanto al jugador como a enemigos
- **Sistema de persistencia de jugador entre escenas**:
  - `ClassSelectionManager.SelectedClass` (variable estática)
  - `PlayerSpawner` lee la clase seleccionada y spawna jugador
  - Singletons con `DontDestroyOnLoad` para managers
- **SystemDiagnostics** (`SystemDiagnostics.cs`): Herramienta de diagnóstico
- **AutoFixer** (`AutoFixer.cs`): Herramienta de corrección automática

---

## 🏗️ ARQUITECTURA Y PATRONES IMPORTANTES

### Orden de Inicialización del Jugador
**CRÍTICO - Este orden debe respetarse:**

1. **Awake()** en `PlayerController`:
   - Inicializa referencias de componentes (Animator, Rigidbody2D)
   - Establece `weaponOffset` si está en (0,0,0)

2. **PlayerSpawner.Start()**:
   - Instancia prefab del jugador
   - **PRIMERO**: Conecta `heartDisplay` a `PlayerHealth`
   - **SEGUNDO**: Asigna `playerClass` a `PlayerController`
   - **TERCERO**: Llama a `controller.Initialize()`

3. **PlayerController.Initialize()**:
   - Verifica que `playerClass` no sea null
   - Copia `speed` desde `playerClass`
   - Instancia arma desde `playerClass.startingWeaponData`
   - Aplica `weaponOffset` con escalado opcional
   - Inicializa componente `Weapon`
   - **IMPORTANTE**: Llama a `playerHealth.Initialize()`

4. **PlayerHealth.Initialize()**:
   - Verifica que `playerClass` esté asignado
   - Copia `maxHealth` desde `playerClass`
   - Establece `currentHealth = maxHealth`
   - **Inicializa HeartDisplay** llamando a `InitializeHearts(maxHealth)`
   - Actualiza corazones con `UpdateHearts(currentHealth)`

**Problemas comunes si no se sigue este orden:**
- NullReferenceException al acceder a `playerClass`
- HP no inicializada correctamente
- Corazones no aparecen en UI
- Arma no instanciada

### Sistema de Singletons
**Clases con patrón Singleton + DontDestroyOnLoad:**
- `ScoreManager`: Puntuación global
- `DamageBoostManager`: Boost de daño global
- `PersistentEventSystem` / `SingletonEventSystem`: EventSystem único

**Patrón usado:**
```csharp
public static ClassName Instance { get; private set; }
void Awake() {
    if (Instance == null) {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    } else {
        Destroy(gameObject);
    }
}
```

### Sistema de Herencia de Enemigos
```
Enemy (base)
├── SlimeBlue (dasher)
├── SlimeGreen (shooter)
├── SlimeRed (básico)
├── ChaserEnemy (perseguidor)
├── JumperEnemy (saltador)
├── ShooterEnemy (tirador)
└── BossEnemy (jefe)
```

**Métodos virtuales importantes:**
- `Awake()`: Inicialización de componentes
- `Start()`: Setup de valores por defecto
- `Update()`: Lógica de comportamiento
- `TakeDamage(int)`: Recibe daño
- `Die()`: Muerte y cleanup

### Sistema de Reflexión para Tiles
`FloorTileManager` usa reflexión para obtener `TileEffect` de `VariantTile` sin dependencia dura:
```csharp
var tileType = currentTile.GetType();
if (tileType.Name == "VariantTile") {
    var effectField = tileType.GetField("tileEffect");
    if (effectField != null) {
        currentEffect = effectField.GetValue(currentTile) as TileEffect;
    }
}
```
Esto permite modularidad entre sistemas de tiles y efectos.

---

## ❌ LO QUE FALTA (Sistemas Pendientes)

### 🔴 CRÍTICO - Necesario para juego jugable

#### 1. **GameManager Global** ❌
**Falta:**
- No existe un GameManager central que controle el flujo del juego
- No hay gestión de estado de juego (en menú, jugando, pausado, game over)
- No hay sistema de victoria/derrota
- No hay sistema de progresión entre niveles

**Necesario:**
```csharp
// Crear GameManager.cs
- Singleton persistente
- Control de estados (Menu, Playing, Paused, GameOver, Victory)
- Transición entre niveles
- Gestión de victoria (derrotar jefe)
- Gestión de derrota (HP = 0)
- Sistema de puntuación global
```

#### 2. **Sistema de Generación de Dungeon Completo** ❌
**Lo que falta:**
- Generador de dungeon con múltiples salas conectadas
- Sistema de puertas que conectan habitaciones
- Sistema de "limpiar sala" (matar todos los enemigos para abrir puertas)
- Spawn procedural de enemigos por sala
- Diferentes tipos de salas (normal, tesoro, jefe, tienda opcional)
- Garantizar path al jefe final

**Archivos a crear:**
```
- DungeonGenerator.cs: Genera layout de X salas conectadas
- RoomData.cs: ScriptableObject con info de sala (tipo, enemigos, recompensas)
- Door.cs: Maneja transiciones entre salas
- RoomController.cs: Controla estado de sala individual (ACTUALMENTE VACÍO)
```

**Sugerencia de implementación:**
- Fase 1: 5-7 salas en línea recta (simple)
- Fase 2: Grid 3x3 con caminos aleatorios
- Fase 3: Generación BSP o similar (opcional)

#### 3. **Sistema de Drops y Pickups** ❌
**Lo que falta:**
- Los enemigos no dropean items al morir
- No hay sistema de recolección de armas/items
- No hay generador de armas aleatorias (variantes)

**Archivos a crear:**
```
- WeaponPickup.cs: Objeto que se puede recoger
- WeaponGenerator.cs: Genera armas aleatorias con stats variados
- ItemDrop.cs: Sistema de drop al matar enemigos
```

**WeaponData necesita expansión:**
```csharp
// Agregar a WeaponData.cs:
- WeaponType weaponType (enum: Sword, GreatSword, Staff)
- Rarity rarity (enum: Common, Rare, Epic, Legendary)
- Rangos de stats (minDamage-maxDamage)
- Prefijos/sufijos para nombres ("Espada Flamígera", "Gran Hacha del Hielo")
```

#### 4. **Jefe Final** ❌
**Estado actual:**
- Existe `BossEnemy.prefab` pero es un placeholder
- No tiene comportamiento especial implementado
- No hay sala especial para el jefe

**Necesario:**
```
- Script BossEnemy.cs con IA compleja:
  * Múltiples fases (cambio de comportamiento según HP)
  * Ataques especiales (patrones de proyectiles, AoE, etc.)
  * HP elevada (30-50)
  * Animaciones especiales
- Sala de jefe con diseño único
- Trigger de victoria al derrotar al jefe
```

#### 5. **Sistema de Pausa y Muerte** ❌
**Falta:**
- Menú de pausa (ESC)
- Pantalla de Game Over al morir
- Pantalla de Victoria al derrotar jefe
- Reiniciar run desde cero
- Volver al menú principal

### 🟡 IMPORTANTE - Mejora experiencia

#### 6. **Sistema de Mejoras/Upgrades** ❌
**Concepto roguelike estándar:**
- Al subir de nivel o limpiar salas, ofrecer mejoras
- Ejemplos: +HP, +Velocidad, +Daño, Arma nueva, etc.
- Pantalla de selección de upgrade entre salas

**Archivos a crear:**
```
- UpgradeData.cs: ScriptableObject con tipos de mejora
- UpgradeManager.cs: Controla upgrades aplicados
- UpgradeUI.cs: Muestra opciones al jugador
```

#### 7. **Variedad de Armas** ⚠️
**Estado actual:** 3 armas base (Sword, GreatSword, Staff)
**Objetivo:** Al menos 3 variantes por tipo = 9 armas totales

**Falta:**
- Implementar generación procedural de variantes
- Armas con efectos especiales (fuego, hielo, veneno, etc.)
- Armas legendarias únicas

#### 8. **Efectos Visuales y Audio** ⚠️
**Muy limitado:**
- Falta feedback visual al recibir daño (screen shake, flash, etc.)
- No hay partículas al matar enemigos
- No hay efectos de sonido (impactos, explosiones, música)
- Tiles tienen opción de VFX pero no hay prefabs asignados

**Archivos a crear:**
```
- CameraShake.cs
- HitEffect.cs
- Partículas para: muerte enemigo, curación, power-up, etc.
- AudioManager.cs (opcional pero recomendado)
```

#### 9. **Balance del Juego** ⚠️
**Necesita testing y ajuste:**
- Valores de daño de armas (actualmente básicos)
- HP de enemigos
- Velocidad de spawning
- Dificultad progresiva por nivel
- Probabilidad de drops
- Efectos de tiles (¿son muy fuertes/débiles?)

**Valores actuales para referencia:**
```
Jugador:
- HP: 5 (base)
- Velocidad: 32 unidades/s

Armas base:
- Sword: 2 damage, 0.5s cooldown
- GreatSword: 4 damage, 1.2s cooldown
- Staff: 3 damage, 0.8s cooldown (estimado)

Enemigos:
- Configurables vía EnemyData (HP, damage, speed)
- Actualmente sin balancear bien
```

### 🟢 OPCIONAL - Pulido final

#### 10. **Más Tipos de Enemigos**
**Sugerencias:**
- TankEnemy: Lento, mucho HP, mucho daño
- SwarmerEnemy: Rápido, poco HP, ataca en grupo
- SummonerEnemy: Invoca otros enemigos
- TeleporterEnemy: Se teletransporta cerca del jugador

#### 11. **Sistema de Meta-Progresión** (Muy opcional)
- Desbloqueo de clases adicionales
- Upgrades permanentes entre runs
- Sistema de logros

#### 12. **Tienda/Merchant** (Opcional)
- NPCs que venden items/mejoras
- Sistema de moneda (oro/puntos)

---

## 🗂️ ESTRUCTURA DEL PROYECTO

### Carpetas Principales

```
Assets/
├── Data/                              ✅ ScriptableObjects
│   ├── Classes/                       ✅ (Knight, Mage, Swordsman)
│   ├── Weapons/                       ✅ (Sword, GreatSword, Staff)
│   ├── Enemies/                       ✅ (Boss, Chaser, Jumper, Shooter)
│   ├── TileEffects/                   ✅ (Ice, Mud, Heal, PowerUp, Floor, Wall, Stairs)
│   └── Rooms/                         ❌ VACÍO - Crear RoomData aquí
│
├── Prefabs/
│   ├── Player.prefab                  ✅
│   ├── Enemies/                       ✅ (todos los enemigos + EnemyProjectile)
│   ├── Weapons/                       ✅ (MeleeWeaponPrefab, RangedWeaponPrefab, BulletPrefab)
│   └── EventSystem.prefab             ✅
│
├── Scripts/
│   ├── Player/                        ✅ (PlayerController, PlayerHealth)
│   ├── Weapons/                       ✅ (Weapon, MeleeWeapon, RangedWeapon, Projectile, WeaponHitbox, WeaponTrail)
│   ├── Enemies/                       ✅ (Enemy, tipos específicos, EnemyManager, EnemyProjectile, EnemyTileEffectReceiver)
│   ├── Tiles/                         ✅ (TileEffect, VariantTile, FloorTileManager, RoomGenerator, DamageBoostManager)
│   ├── LevelGeneration/               ⚠️ (RoomController VACÍO)
│   ├── UI/                            ✅ (HeartDisplay, ScoreDisplay, FixHeartContainerPosition, etc.)
│   ├── Core/                          ✅ (PixelArtSettings, PixelPerfectSetup, ScoreManager)
│   ├── Data/                          ✅ (PlayerClass, WeaponData, EnemyData)
│   ├── Editor/                        ✅ (muchos helpers automáticos)
│   │
│   ├── Managers/                      ❌ FALTA CREAR
│   │   └── GameManager.cs             ❌ CRÍTICO
│   │   └── UpgradeManager.cs          ❌ IMPORTANTE
│   │   └── AudioManager.cs            ❌ OPCIONAL
│   │
│   └── Items/                         ❌ FALTA CREAR
│       └── WeaponPickup.cs            ❌ IMPORTANTE
│       └── WeaponGenerator.cs         ❌ IMPORTANTE
│       └── ItemDrop.cs                ❌ IMPORTANTE
│
├── Sprites/
│   ├── Player/                        ✅ (animaciones completas)
│   ├── Weapons/                       ✅
│   ├── Enemies/                       ✅ (varios tipos)
│   ├── Tiles/                         ✅ (floor1-3, ice1-3, mud1-3, wall, heal, powerUp, stairs)
│   └── UI/                            ✅ (corazones, etc.)
│
├── Tiles/                             ✅ VariantTile assets
│   ├── FloorVariantTile.asset         ✅
│   ├── WallVariantTile.asset          ✅
│   ├── IceVariantTile.asset           ✅
│   ├── MudVariantTile.asset           ✅
│   ├── HealVariantTile.asset          ✅
│   ├── PowerupVariantTile.asset       ✅
│   └── StairsVariantTile.asset        ✅
│
├── Scenes/
│   ├── MainMenu.unity                 ✅
│   ├── ClassSelection.unity           ✅
│   ├── GameScene.unity                ✅ (escena de juego principal)
│   └── SampleScene.unity              ⚠️ (escena de prueba)
│
└── Animations/                        ✅ (Player animations completas)
```

---

## 🎯 ROADMAP RECOMENDADO PARA COMPLETAR EL JUEGO

### FASE 1: Funcionalidad Básica (CRÍTICA) - 10-15 horas
**Objetivo:** Juego jugable de inicio a fin

#### Sprint 1: GameManager y Flujo de Juego (3-4 horas)
1. Crear `GameManager.cs` singleton
   - Estados: Menu, Playing, Paused, GameOver, Victory
   - Método para reiniciar run
   - Método para volver a menú
   - Transición entre niveles

2. Implementar `PauseMenu.cs`
   - Tecla ESC para pausar
   - Opciones: Reanudar, Reiniciar, Menú Principal

3. Implementar pantallas finales:
   - `GameOverScreen.cs` (cuando HP = 0)
   - `VictoryScreen.cs` (cuando matas al jefe)

#### Sprint 2: Generación de Dungeon (5-6 horas)
1. Crear `DungeonGenerator.cs`
   - Generar 5-7 salas en línea/grid simple
   - Colocar puertas entre salas
   - Designar una sala como sala del jefe (última)

2. Crear `Door.cs`
   - Trigger para cambiar de sala
   - Estado: Cerrada/Abierta
   - Se abre al limpiar sala de enemigos

3. Mejorar `RoomController.cs`
   - Spawn de enemigos al entrar
   - Detectar cuando todos los enemigos mueren
   - Abrir puertas al limpiar sala

4. Crear `RoomData.cs` ScriptableObject
   - Tipo de sala (Normal, Boss)
   - Lista de enemigos a spawnear
   - Número de enemigos

#### Sprint 3: Sistema de Jefe y Victoria (2-3 horas)
1. Implementar comportamiento de `BossEnemy.cs`
   - IA más compleja (2-3 fases)
   - HP alta (30-50)
   - Ataques especiales

2. Integrar victoria:
   - Al matar al jefe → GameManager.Victory()
   - Mostrar pantalla de victoria
   - Opción de reiniciar o volver a menú

3. Integrar derrota:
   - PlayerHealth.Die() → GameManager.GameOver()
   - Mostrar pantalla de Game Over

---

### FASE 2: Contenido y Variedad (IMPORTANTE) - 8-10 horas
**Objetivo:** Hacer el juego más rejugable y divertido

#### Sprint 4: Sistema de Drops (3-4 horas)
1. Crear `WeaponGenerator.cs`
   - Generar armas aleatorias con stats variados
   - Implementar rareza (Común, Rara, Épica, Legendaria)
   - Nombres aleatorios con prefijos/sufijos

2. Expandir `WeaponData.cs`
   - Agregar `WeaponType` enum
   - Agregar `Rarity` enum
   - Rangos de stats

3. Crear `WeaponPickup.cs`
   - Objeto en el mundo que se puede recoger
   - Interacción con jugador (overlap o tecla E)
   - Reemplazar arma actual

4. Modificar `Enemy.cs`
   - Drop de armas al morir (probabilidad configurable)
   - Instanciar WeaponPickup en posición de muerte

#### Sprint 5: Sistema de Upgrades (3-4 horas)
1. Crear `UpgradeData.cs` ScriptableObject
   - Tipos: HP, Velocidad, Daño, Velocidad de ataque
   - Valor de mejora

2. Crear `UpgradeManager.cs`
   - Aplicar upgrades al jugador
   - Llevar registro de upgrades activos

3. Crear `UpgradeUI.cs`
   - Mostrar 3 opciones al limpiar sala (cada X salas)
   - Aplicar upgrade seleccionado
   - Pausar juego mientras se elige

#### Sprint 6: Más Variedad de Enemigos (2-3 horas)
1. Implementar 2-3 tipos nuevos de enemigos
   - TankEnemy (opcional)
   - SwarmerEnemy (opcional)

2. Balancear spawn de enemigos
   - Pools de enemigos por tipo de sala
   - Aumentar dificultad por nivel/sala

---

### FASE 3: Pulido y Balance (IMPORTANTE) - 5-8 horas
**Objetivo:** Que el juego se sienta bien y esté balanceado

#### Sprint 7: Efectos Visuales y Feedback (3-4 horas)
1. Implementar `CameraShake.cs`
   - Al recibir daño
   - Al matar enemigo grande
   - Al matar jefe

2. Crear partículas:
   - Muerte de enemigo
   - Curación (heal tile)
   - Power-up
   - Impactos de arma

3. Agregar efectos visuales de daño:
   - Flash blanco/rojo al recibir daño
   - Knockback al ser golpeado

4. Implementar trail visual para armas

#### Sprint 8: Balance del Juego (2-4 horas)
1. Testing exhaustivo:
   - Jugar runs completas múltiples veces
   - Anotar problemas de balance

2. Ajustar valores:
   - Daño de armas
   - HP de enemigos
   - Velocidades
   - Spawn rate de enemigos
   - Probabilidad de drops
   - Efectos de tiles

3. Ajustar dificultad progresiva
   - Más enemigos en salas posteriores
   - Enemigos más fuertes
   - Menos drops de curación

---

### FASE 4: Audio y Pulido Final (OPCIONAL) - 3-5 horas
**Objetivo:** Mejorar experiencia general

#### Sprint 9: Audio (2-3 horas)
1. Crear `AudioManager.cs`
2. Añadir efectos de sonido:
   - Ataque
   - Daño recibido
   - Muerte enemigo
   - Recoger item
   - Curación
3. Añadir música:
   - Menú
   - Gameplay
   - Jefe final

#### Sprint 10: Pulido (1-2 horas)
1. Mejorar menús
2. Añadir tutoriales in-game opcionales
3. Optimización de rendimiento
4. Arreglar bugs menores

---

## 🎓 RECOMENDACIONES PARA DESARROLLO

### Priorización
1. **Primero funcionalidad, luego contenido, finalmente pulido**
2. Enfocarse en tener un loop completo jugable antes de añadir variedad
3. Es mejor tener pocos sistemas bien hechos que muchos sin pulir

### Testing
- Jugar el juego cada 2-3 horas de desarrollo
- Anotar bugs y problemas de balance inmediatamente
- Pedir feedback a amigos/compañeros

### Documentación
- Tomar screenshots del antes/después para presentación
- Documentar decisiones de diseño importantes
- Usar Git con commits descriptivos

### Alcance realista para proyecto universitario
**Mínimo viable (40-50 horas desarrollo):**
- ✅ 3 clases
- ✅ 3 tipos de armas base (sin variantes complejas)
- ✅ 5 tipos de enemigos + 1 jefe
- ✅ Generación simple de 5-7 salas
- ✅ Sistema de drops básico
- ✅ Efectos de tiles funcionando
- ✅ Victoria/Derrota implementados

**Alcance extendido (60-80 horas):**
- Todo lo anterior +
- Sistema de upgrades entre salas
- Generación procedural de armas con variantes
- 8-12 salas con layouts variados
- Efectos visuales pulidos
- Audio completo
- Sistema de meta-progresión básico

---

## 🛠️ HERRAMIENTAS DE DESARROLLO Y SCRIPTS DE EDITOR

### Scripts de Editor Disponibles (Assets/Scripts/Editor/)
El proyecto tiene **numerosas herramientas automáticas** para agilizar desarrollo:

#### Generación Automática
1. **GenerateVariantTilesFromSprites.cs**
   - Genera VariantTiles automáticamente desde sprites
   - Crea TileEffects asociados

2. **AutoCreateEnemyProjectile.cs**
   - Crea prefabs de proyectiles enemigos automáticamente

3. **CreateEnemyProjectilePrefab.cs**
   - Helper para creación de proyectiles

#### Configuración Automática
4. **EnemyAnimationSystemGenerator.cs**
   - Genera sistemas de animación para enemigos

5. **EnemyPrefabUpdater.cs**
   - Actualiza prefabs de enemigos masivamente

6. **SetupMeleeWeapons.cs**
   - Configura armas melee automáticamente

#### Validación y Diagnóstico
7. **EnemyDataValidator.cs**
   - Valida configuración de EnemyData

8. **FullHDSetupValidator.cs**
   - Valida configuración para Full HD

9. **CleanMissingScripts.cs**
   - Limpia scripts faltantes de GameObjects

#### Corrección Automática (Auto-fixers)
10. **FullHDAutoFixer.cs** - Ajusta para resolución Full HD
11. **FixEnemyPhysics.cs** - Corrige física de enemigos
12. **FixPlayerPrefab.cs** - Corrige configuración del jugador
13. **FixProjectilePPUAndScale.cs** - Ajusta PPU y escala de proyectiles
14. **FixSpeedsForPPU16.cs** - Ajusta velocidades para PPU 16
15. **FixSpritePPU.cs** - Corrige PPU de sprites
16. **FixVFXAndTilesSprites.cs** - Corrige sprites de VFX y tiles
17. **FixWeaponColliders.cs** - Configura colliders de armas
18. **ProjectSettingsAutoFix.cs** - Ajusta Project Settings

#### Otros
19. **CreateEnemyTag.cs** - Crea tag "Enemy" si no existe
20. **TileEffectCreator.cs** - **[EN SCRIPTS RAÍZ]** Crea TileEffects con context menu

**Estos scripts demuestran que el proyecto tiene MUCHA infraestructura de automatización.**

### Context Menus Disponibles
- **TileEffectCreator**: Click derecho → "Crear TileEffects Automáticamente"
- **RoomGenerator**: Click derecho → "Generate Room"
- Múltiples herramientas de corrección en menú Unity

---

## 🐛 BUGS CONOCIDOS Y PROBLEMAS

### Bugs Críticos
- Ninguno conocido actualmente (se arreglaron los NullReference del sistema de tiles)

### Bugs Menores
- **Proyectiles a veces no visibles**: Se aplica escala 20x y color amarillo como workaround
- **WeaponData tiene typo**: Campo se llama `atackSpeed` en lugar de `attackSpeed`

### Limitaciones Actuales
1. **Una sola sala**: El juego actualmente solo funciona con una sala generada (RoomController vacío)
2. **Sin persistencia entre runs**: No se guarda progreso entre partidas
3. **Balance sin ajustar**: Los valores de stats no están balanceados
4. **Sin feedback visual de daño claro**: Falta shake y efectos (aunque hay flash rojo básico)
5. **Enemigos sin dropear items**: Sistema de drops no implementado
6. **Sin GameManager central**: No hay control de flujo del juego (pausa, game over, victoria)
7. **Stairs tile sin funcionalidad**: Existe el tile pero no hace nada
8. **EnemyManager tiene código comentado**: Referencia a `GameBalance` eliminada, usando valores hardcodeados
9. **ChaserEnemy, JumperEnemy, ShooterEnemy**: Existen como placeholders con EnemyData pero sin IA específica implementada (solo SlimeBlue, SlimeGreen, SlimeRed tienen comportamiento completo)

---

## 📦 ASSETS Y CONFIGURACIÓN DEL PROYECTO

### Sprites Disponibles
**Assets/Sprites/Tiles/:**
- floor1.png, floor2.png, floor3.png (3 variantes de suelo)
- ice1.png, ice2.png, ice3.png (3 variantes de hielo)
- mud1.png, mud2.png, mud3.png (3 variantes de barro)
- wall.png (muro)
- heal.png (curación)
- powerUp.png (power-up)
- stairs.png (escaleras)

**Todos los sprites configurados con PPU = 16 (Pixels Per Unit)**

### ScriptableObjects Creados
**Assets/Data/Classes/:**
- Knight.asset
- Mage.asset
- Swordsman.asset

**Assets/Data/Weapons/:**
- Sword.asset (espada)
- GreatSword.asset (espadón)
- Staff.asset (bastón mágico)

**Assets/Data/Enemies/:**
- Boss.asset
- ChaserEnemy.asset
- JumperEnemy.asset
- ShooterEnemy.asset

**Assets/Data/TileEffects/:**
- FloorTileEffect.asset
- WallTileEffect.asset
- IceTileEffect.asset
- MudTileEffect.asset
- HealTileEffect.asset
- PowerupTileEffect.asset
- StairsTileEffect.asset

### VariantTiles Creados (Assets/Tiles/)
- FloorVariantTile.asset
- WallVariantTile.asset
- IceVariantTile.asset
- MudVariantTile.asset
- HealVariantTile.asset
- PowerupVariantTile.asset
- StairsVariantTile.asset

### Prefabs
**Assets/Prefabs/:**
- Player.prefab
- EventSystem.prefab

**Assets/Prefabs/Enemies/:**
- SlimeBlue.prefab ✅ (IA completa)
- SlimeGreen.prefab ✅ (IA completa)
- SlimeRed.prefab ✅ (IA completa)
- ChaserEnemy.prefab ⚠️ (placeholder)
- JumperEnemy.prefab ⚠️ (placeholder)
- ShooterEnemy.prefab ⚠️ (placeholder)
- BossEnemy.prefab ⚠️ (placeholder)
- EnemyProjectile.prefab ✅

**Assets/Prefabs/Weapons/:**
- MeleeWeaponPrefab.prefab
- RangedWeaponPrefab.prefab
- BulletPrefab.prefab
- WeaponPrefab.prefab
- Spawner.prefab

### Escenas
**Assets/Scenes/:**
- MainMenu.unity ✅
- ClassSelection.unity ✅
- GameScene.unity ✅ (escena principal de juego)
- SampleScene.unity ⚠️ (escena de prueba/testing)

### Configuración del Proyecto
- **Unity Version**: (Detectar de ProjectSettings/ProjectVersion.txt)
- **Pixel Perfect**: PPU = 16 para todos los sprites
- **Tags**: Player, Enemy, Wall
- **Layers**: (Default, no layers custom aparentes en código)
- **Physics 2D**: Configurado para colisiones entre Player-Enemy, Projectile-Enemy, etc.

---

## 📋 CHECKLIST DE FUNCIONALIDAD MÍNIMA VIABLE

### Core Gameplay
- [x] Movimiento del jugador (WASD)
- [x] Roll/Dash
- [x] Sistema de vida
- [x] UI de vida visible (corazones)
- [x] Ataque cuerpo a cuerpo
- [x] Ataque a distancia
- [x] 3 armas funcionales

### Enemigos
- [x] Al menos 3 tipos de enemigos
- [x] IA básica (perseguir, saltar, disparar)
- [x] Enemigos hacen daño al jugador
- [ ] Enemigos dropean items al morir (código existe, falta conectar)

### Generación de Mundo
- [x] Generación de una sala con tiles
- [ ] Generación de múltiples salas conectadas
- [ ] Puertas funcionales
- [ ] Sistema de spawn de enemigos por sala
- [ ] Limpieza de sala (matar todos los enemigos)

### Progresión
- [ ] Jefe final funcional
- [ ] Condición de victoria (derrotar jefe)
- [ ] Condición de derrota (HP = 0)
- [ ] Reiniciar run
- [ ] Volver a menú

### Contenido Opcional pero Recomendado
- [ ] Sistema de upgrades entre salas
- [ ] 9+ armas con variantes
- [ ] Efectos visuales (partículas, shake)
- [ ] Audio (SFX y música)
- [ ] Balance ajustado

---

## 💬 NOTAS PARA LA IA ASISTENTE

### Contexto de uso
Este documento está diseñado para ser leído por otra IA que te dará directrices/tareas específicas para continuar el desarrollo del juego. El desarrollador te pasará esas directrices y tú las implementarás.

### Información importante para la implementación
1. **El código usa C# y Unity**
2. **Todas las rutas de archivos usan `\` (Windows)**
3. **Los archivos deben seguir las convenciones de Unity (PascalCase para clases, camelCase para variables)**
4. **Usar ScriptableObjects para configuración siempre que sea posible**
5. **El juego está configurado para pixel art con PPU 16**
6. **Ya existe infraestructura para tiles con efectos - ¡úsala!**
7. **Ya existe sistema de clases y stats - no reinventar**

### Sistemas prioritarios a implementar (en orden)
1. **GameManager** - CRÍTICO
2. **DungeonGenerator** - CRÍTICO
3. **Door system** - CRÍTICO
4. **Boss behavior** - CRÍTICO
5. **Victory/Defeat screens** - CRÍTICO
6. Weapon drops - IMPORTANTE
7. Weapon generator con variantes - IMPORTANTE
8. Upgrade system - IMPORTANTE
9. Visual effects - PULIDO
10. Audio - PULIDO

### Patrones de código a seguir
- Usar `[SerializeField]` para campos privados que se editan en Inspector
- Usar `[Header("Categoría")]` para organizar Inspector
- Usar `[Tooltip("Descripción")]` para ayuda en Inspector
- Crear `[ContextMenu("Acción")]` para funciones útiles en editor
- Comentar código en español (el proyecto está en español)
- Usar Debug.Log para tracking de eventos importantes
