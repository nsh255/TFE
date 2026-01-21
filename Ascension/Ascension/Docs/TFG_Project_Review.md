# Ascension — Informe de Preparación TFG Rogue‑like

Fecha: 2026-01-21

## Resumen Ejecutivo

El proyecto está listo para ser presentado como TFG de un rogue‑like estilo Tiny Rogues. Incluye flujo completo (menú → selección de clase → salas procedurales → encuentros con enemigos → boss periódico → victoria/derrota), sistemas modulares (GameManager, generación de salas, IA de enemigos, armas y proyectiles, salud y UI), además de amplias herramientas de Editor para automatizar contenido.

Fortalezas:
- Arquitectura clara y modular, uso de ScriptableObjects para datos.
- Generación de salas y control de flujo con progreso y limpieza por sala.
- Varios tipos de enemigos con comportamientos distintos y boss con 2 fases/periodicidad.
- Sistemas de armas (melee/ranged/proyectiles) y pickups funcionales.
- UI completa: pausa, HUD, pantalla de victoria y game over.
- Herramientas de Editor robustas para creación/validación/auto‑setup.

Gaps menores (opcionales):
- Sistema genérico de drops (más allá de `WeaponPickup`).
- `AudioManager` y mezcla básica de SFX/música.
- Balance fino y persistencia mínima (p.ej. high score/clase seleccionada entre sesiones).

## Escenas y Flujo

Escenas en Build Settings:
- [Assets/Scenes/MainMenu.unity](../../Assets/Scenes/MainMenu.unity)
- [Assets/Scenes/ClassSelection.unity](../../Assets/Scenes/ClassSelection.unity)
- [Assets/Scenes/GameScene.unity](../../Assets/Scenes/GameScene.unity)
- [Assets/Scenes/SampleScene.unity](../../Assets/Scenes/SampleScene.unity)

Flujo de juego:
1. Menú principal → selección de clase → `GameScene`.
2. `RoomGenerator` genera suelo y muros; `RoomFlowController` calcula área jugable y decide spawns.
3. `EnemyManager` spawnea enemigos por coste/weights; limpieza de sala abre puertas/permite avanzar.
4. Boss periódico (p.ej. cada 4 salas) o boss dedicado; al morir, `GameManager.Victory()`.
5. Muerte del jugador → `GameManager.GameOver()`.

## Arquitectura y Sistemas Clave

- Game loop y estados:
  - `GameManager` (pausa, estados, transición de escenas, puntuación): [Assets/Scripts/Core/GameManager.cs](../../Assets/Scripts/Core/GameManager.cs)
  - UI de pausa: [Assets/Scripts/UI/PauseMenu.cs](../../Assets/Scripts/UI/PauseMenu.cs)
  - Pantallas de victoria/game over: [Assets/Scripts/UI/VictoryScreen.cs](../../Assets/Scripts/UI/VictoryScreen.cs), [Assets/Scripts/UI/GameOverScreen.cs](../../Assets/Scripts/UI/GameOverScreen.cs)
- Generación y flujo de salas:
  - `RoomGenerator` (tilemaps, rect interno, regeneración): [Assets/Scripts/Tiles/RoomGenerator.cs](../../Assets/Scripts/Tiles/RoomGenerator.cs)
  - `RoomFlowController` (spawn periódico de boss, avance de salas): [Assets/Scripts/Core/RoomFlowController.cs](../../Assets/Scripts/Core/RoomFlowController.cs)
  - `RoomController` (puertas, detección de limpieza, spawns): [Assets/Scripts/LevelGeneration/RoomController.cs](../../Assets/Scripts/LevelGeneration/RoomController.cs)
  - `Door` (abrir/cerrar): [Assets/Scripts/LevelGeneration/Door.cs](../../Assets/Scripts/LevelGeneration/Door.cs)
- Enemigos y boss:
  - Base `Enemy` (daño al jugador, facing/anim, tile effects): [Assets/Scripts/Enemies/Enemy.cs](../../Assets/Scripts/Enemies/Enemy.cs)
  - `EnemyManager` (spawn ponderado/por coste, safe spawn, tracking/clear): [Assets/Scripts/Enemies/EnemyManager.cs](../../Assets/Scripts/Enemies/EnemyManager.cs)
  - Tipos: `ChaserEnemy`, `ShooterEnemy`, `JumperEnemy` (prefabs y datos en Resources/Data).
  - Boss: `BossController` (2 fases, patrón de ataque, victoria al morir): [Assets/Scripts/Enemy/BossController.cs](../../Assets/Scripts/Enemy/BossController.cs)
- Armas y proyectiles:
  - Base `Weapon` + `MeleeWeapon` + `RangedWeapon` + `Projectile`: [Assets/Scripts/Weapons](../../Assets/Scripts/Weapons)
  - `WeaponGenerator` (stats y rareza por run): [Assets/Scripts/Weapons/WeaponGenerator.cs](../../Assets/Scripts/Weapons/WeaponGenerator.cs)
  - `WeaponPickup` (recogida en el mundo): [Assets/Scripts/Weapons/WeaponPickup.cs](../../Assets/Scripts/Weapons/WeaponPickup.cs)
- Jugador:
  - Movimiento y control: [Assets/Scripts/Player/PlayerController.cs](../../Assets/Scripts/Player/PlayerController.cs)
  - Salud y HUD: [Assets/Scripts/Player/PlayerHealth.cs](../../Assets/Scripts/Player/PlayerHealth.cs), `HeartDisplay` en [Assets/Scripts/UI/HeartDisplay.cs](../../Assets/Scripts/UI/HeartDisplay.cs)
  - Spawner: [Assets/Scripts/PlayerSpawner.cs](../../Assets/Scripts/PlayerSpawner.cs)
- Selección de personaje:
  - `ClassSelectionManager` con `PlayerClass` ScriptableObjects: [Assets/Scripts/ClassSelectionManager.cs](../../Assets/Scripts/ClassSelectionManager.cs), datos en [Assets/Data/Classes](../../Assets/Data/Classes)
- Tile effects:
  - `TileEffectPainter` (Editor), `EnemyTileEffectReceiver`, y efectos SO en [Assets/Data/TileEffects](../../Assets/Data/TileEffects)

## Herramientas de Editor (Automatización)

Ubicación: [Assets/Scripts/Editor](../../Assets/Scripts/Editor)
- Room Auto‑Setup: `RoomAutoSetup.cs`
- Dungeon Auto‑Builder: `DungeonAutoBuilder.cs`
- Enemy Database Editor: `EnemyDatabaseEditor.cs`
- Weapon Database Editor: `WeaponDatabaseEditor.cs`
- Tile Effect Painter: `TileEffectPainter.cs`
- Validaciones/fixers: `EnemyDataValidator.cs`, `FixEnemyPhysics.cs`, `FixPlayerPrefab.cs`, `FixWeaponColliders.cs`, `ProjectSettingsAutoFix.cs`, `CleanMissingScripts.cs`, etc.

## Datos y Prefabs

- Enemigos (SO): [Assets/Data/Enemies](../../Assets/Data/Enemies)
- Armas (SO): [Assets/Data/Weapons](../../Assets/Data/Weapons)
- Clases (SO): [Assets/Data/Classes](../../Assets/Data/Classes)
- Tile Effects (SO): [Assets/Data/TileEffects](../../Assets/Data/TileEffects)
- Prefabs:
  - Enemigos: [Assets/Resources/Enemies](../../Assets/Resources/Enemies) (incluye `BossEnemy` y enemigos base para auto‑descubrimiento)
  - Armas: [Assets/Prefabs/Weapons](../../Assets/Prefabs/Weapons)
  - VFX: [Assets/Prefabs/VFX](../../Assets/Prefabs/VFX)

## Criterios del TFG — Cobertura

- Mazmorras lineales/procedurales: Sí (`RoomGenerator`, `RoomFlowController`).
- Enemigos con IA simple: Sí (chaser/shooter/jumper con base `Enemy`).
- Armas con stats y proyectiles: Sí (melee/ranged/proyectiles, `WeaponGenerator`).
- Clases seleccionables: Sí (3 clases en SO + `ClassSelectionManager`).
- Boss + victoria/derrota: Sí (`BossController` + `GameManager.Victory`/`GameOver`).
- Drops/powerups: Sí (tile powerups y `WeaponPickup`; se recomienda `DropController` genérico como mejora).
- Menú de pausa y GameManager: Sí (`PauseMenu`, `GameManager`).
- Editor Tools: Sí (varias herramientas para contenido y validación).

## Sugerencias de Mejora (Opcional)

- Audio: `AudioManager` (singleton, mixer groups), música por estado (menú/salas/boss), SFX (ataques, daño, pickup, puertas).
- Drops generales: `DropController` con probabilidades (health shard, buff temporal, cambio de arma); base `Pickup` con variantes.
- UX/VFX: telegráficos de spawn, flash de puertas al abrir, pequeñas partículas en daño.
- Balance: ajustar `EnemyManager` (costes/pesos) y progresión de armas.
- Persistencia mínima: `PlayerPrefs` para high score y clase seleccionada.

## Cómo Probar Rápido

1. Abrir `MainMenu` y pulsar Play.
2. Seleccionar clase en `ClassSelection`.
3. En `GameScene`, moverse/rodar/disparar; limpiar salas para abrir puertas; avanzar.
4. Ver boss periódico; al morir, aparece pantalla de victoria.
5. Morir para comprobar pantalla de game over.

## Glosario de Rutas Clave

- Game loop: [Assets/Scripts/Core/GameManager.cs](../../Assets/Scripts/Core/GameManager.cs)
- Flujo de salas: [Assets/Scripts/Core/RoomFlowController.cs](../../Assets/Scripts/Core/RoomFlowController.cs)
- Generación de salas: [Assets/Scripts/Tiles/RoomGenerator.cs](../../Assets/Scripts/Tiles/RoomGenerator.cs)
- Control de sala: [Assets/Scripts/LevelGeneration/RoomController.cs](../../Assets/Scripts/LevelGeneration/RoomController.cs)
- Enemigos base: [Assets/Scripts/Enemies/Enemy.cs](../../Assets/Scripts/Enemies/Enemy.cs)
- Boss: [Assets/Scripts/Enemy/BossController.cs](../../Assets/Scripts/Enemy/BossController.cs)
- Armas: [Assets/Scripts/Weapons](../../Assets/Scripts/Weapons)
- UI: pausa/victoria/game over en [Assets/Scripts/UI](../../Assets/Scripts/UI)
- Editor Tools: [Assets/Scripts/Editor](../../Assets/Scripts/Editor)

---
Este documento resume el estado actual del proyecto Ascension para alimentación de una IA de documentación. Para más detalle técnico, revisar los archivos y carpetas vinculados arriba.
