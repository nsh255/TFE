# Ascension — Estado Actual del Juego

Fecha: 2026-01-21

## Resumen

El juego está funcional y jugable de inicio a fin: menú principal → selección de clase → generación de salas con enemigos → bosses periódicos → victoria/derrota, con sistemas modulares y herramientas de Editor sólidas para crear y validar contenido.

## Fortalezas
- Arquitectura modular con `GameManager`, control de salas y uso extenso de ScriptableObjects.
- Generación de salas y flujo con progresión, spawn seguro y limpieza por sala.
- Variedad de enemigos (chaser/shooter/jumper) y jefe con patrón de ataque de 2 fases.
- Sistema de armas (melee/ranged/proyectiles), `WeaponGenerator` y recogidas (`WeaponPickup`).
- UI completa (pausa, HUD de corazones, mensajes, victoria/game over) y métricas de run.
- Herramientas de Editor para automatizar: setup de rooms, generar dungeons, bases de datos de enemigos/armas, painter de efectos de tiles, validadores.

## Carencias (mejoras recomendadas)
- Drops genéricos: falta un `DropController` para salud/buffs/arma con probabilidades (más allá de `WeaponPickup`).
- Audio: no hay `AudioManager` ni mezcla básica de música/SFX.
- Persistencia mínima: no se guarda high score ni clase seleccionada entre sesiones (aceptable para TFG, mejorable con `PlayerPrefs`).
- Balance fino y telegráficos: ajustar costes/pesos de spawn, añadir VFX/telegráficos al spawn de enemigos y puertas.

## Flujo y Escenas
- Escenas en Build Settings:
  - [Assets/Scenes/MainMenu.unity](../../Assets/Scenes/MainMenu.unity)
  - [Assets/Scenes/ClassSelection.unity](../../Assets/Scenes/ClassSelection.unity)
  - [Assets/Scenes/GameScene.unity](../../Assets/Scenes/GameScene.unity)
  - [Assets/Scenes/SampleScene.unity](../../Assets/Scenes/SampleScene.unity)
- Control del loop y estados: [Assets/Scripts/Core/GameManager.cs](../../Assets/Scripts/Core/GameManager.cs)
- Pausa y navegación: [Assets/Scripts/UI/PauseMenu.cs](../../Assets/Scripts/UI/PauseMenu.cs)
- Pantallas de fin de run: [Assets/Scripts/UI/VictoryScreen.cs](../../Assets/Scripts/UI/VictoryScreen.cs), [Assets/Scripts/UI/GameOverScreen.cs](../../Assets/Scripts/UI/GameOverScreen.cs)

## Generación de Salas y Flujo
- Generación de tilemaps, rect interno y regeneración: [Assets/Scripts/Tiles/RoomGenerator.cs](../../Assets/Scripts/Tiles/RoomGenerator.cs)
- Flujo de salas (boss periódico, avance cuando limpia): [Assets/Scripts/Core/RoomFlowController.cs](../../Assets/Scripts/Core/RoomFlowController.cs)
- Control por sala (puertas, spawn, limpieza): [Assets/Scripts/LevelGeneration/RoomController.cs](../../Assets/Scripts/LevelGeneration/RoomController.cs)
- Puertas: [Assets/Scripts/LevelGeneration/Door.cs](../../Assets/Scripts/LevelGeneration/Door.cs)

## Enemigos
- Base de enemigo, facing/anim, daño por contacto, efectos de tiles: [Assets/Scripts/Enemies/Enemy.cs](../../Assets/Scripts/Enemies/Enemy.cs)
- Gestión y spawn ponderado/por coste con radio seguro: [Assets/Scripts/Enemies/EnemyManager.cs](../../Assets/Scripts/Enemies/EnemyManager.cs)
- Tipos presentes (prefabs en Resources): [Assets/Resources/Enemies](../../Assets/Resources/Enemies)
  - `ChaserEnemy.prefab`, `ShooterEnemy.prefab`, `JumperEnemy.prefab`
  - `BossEnemy.prefab` (boss dedicado)
- Jefe final (2 fases, ráfaga de proyectiles, victoria al morir): [Assets/Scripts/Enemy/BossController.cs](../../Assets/Scripts/Enemy/BossController.cs)
- Spawners de proyectiles enemigos: [Assets/Scripts/Enemies/EnemyProjectileSpawner.cs](../../Assets/Scripts/Enemies/EnemyProjectileSpawner.cs)
- Efectos de tiles aplicados a enemigos: [Assets/Scripts/Enemies/EnemyTileEffectReceiver.cs](../../Assets/Scripts/Enemies/EnemyTileEffectReceiver.cs)

## Armas y Proyectiles
- Base y variantes: [Assets/Scripts/Weapons/Weapon.cs](../../Assets/Scripts/Weapons/Weapon.cs), [Assets/Scripts/Weapons/MeleeWeapon.cs](../../Assets/Scripts/Weapons/MeleeWeapon.cs), [Assets/Scripts/Weapons/RangedWeapon.cs](../../Assets/Scripts/Weapons/RangedWeapon.cs)
- Proyectil: [Assets/Scripts/Weapons/Projectile.cs](../../Assets/Scripts/Weapons/Projectile.cs)
- Generación por run (stats/rareza): [Assets/Scripts/Weapons/WeaponGenerator.cs](../../Assets/Scripts/Weapons/WeaponGenerator.cs)
- Recogida de armas: [Assets/Scripts/Weapons/WeaponPickup.cs](../../Assets/Scripts/Weapons/WeaponPickup.cs)
- Prefabs de armas/VFX: [Assets/Prefabs/Weapons](../../Assets/Prefabs/Weapons), [Assets/Prefabs/VFX](../../Assets/Prefabs/VFX)
- Datos de armas (SO): [Assets/Data/Weapons](../../Assets/Data/Weapons)

## Jugador y UI
- Control de jugador (movimiento 8 direcciones, rodar, mirar al ratón): [Assets/Scripts/Player/PlayerController.cs](../../Assets/Scripts/Player/PlayerController.cs)
- Salud, invulnerabilidad, flash de daño, HUD de corazones: [Assets/Scripts/Player/PlayerHealth.cs](../../Assets/Scripts/Player/PlayerHealth.cs), [Assets/Scripts/UI/HeartDisplay.cs](../../Assets/Scripts/UI/HeartDisplay.cs)
- Mensajes HUD, escalado UI y utilidades: [Assets/Scripts/UI/HUDMessage.cs](../../Assets/Scripts/UI/HUDMessage.cs), [Assets/Scripts/UI/UIScalerSetup.cs](../../Assets/Scripts/UI/UIScalerSetup.cs)
- Evento UI persistente: [Assets/Scripts/UI/SingletonEventSystem.cs](../../Assets/Scripts/UI/SingletonEventSystem.cs)

## Selección de Clase
- Gestor: [Assets/Scripts/ClassSelectionManager.cs](../../Assets/Scripts/ClassSelectionManager.cs)
- Datos de clases (SO): [Assets/Data/Classes](../../Assets/Data/Classes)

## Tile Effects
- Datos de efectos (SO): [Assets/Data/TileEffects](../../Assets/Data/TileEffects)
- Painter de efectos (Editor): [Assets/Scripts/Editor/TileEffectPainter.cs](../../Assets/Scripts/Editor/TileEffectPainter.cs)

## Herramientas de Editor
- Auto‑setup de salas: [Assets/Scripts/Editor/RoomAutoSetup.cs](../../Assets/Scripts/Editor/RoomAutoSetup.cs)
- Generador de dungeons: [Assets/Scripts/Editor/DungeonAutoBuilder.cs](../../Assets/Scripts/Editor/DungeonAutoBuilder.cs)
- Editor de base de datos de enemigos/armas: [Assets/Scripts/Editor/EnemyDatabaseEditor.cs](../../Assets/Scripts/Editor/EnemyDatabaseEditor.cs), [Assets/Scripts/Editor/WeaponDatabaseEditor.cs](../../Assets/Scripts/Editor/WeaponDatabaseEditor.cs)
- Validadores y fixers: `EnemyDataValidator.cs`, `FixEnemyPhysics.cs`, `FixPlayerPrefab.cs`, `FixWeaponColliders.cs`, `ProjectSettingsAutoFix.cs`, `CleanMissingScripts.cs` en [Assets/Scripts/Editor](../../Assets/Scripts/Editor)

## Datos y Prefabs
- Enemigos (SO): [Assets/Data/Enemies](../../Assets/Data/Enemies)
- Armas (SO): [Assets/Data/Weapons](../../Assets/Data/Weapons)
- Clases (SO): [Assets/Data/Classes](../../Assets/Data/Classes)
- Prefabs: [Assets/Resources/Enemies](../../Assets/Resources/Enemies), [Assets/Prefabs/Weapons](../../Assets/Prefabs/Weapons), [Assets/Prefabs/VFX](../../Assets/Prefabs/VFX)

## Balance y Rendimiento
- Spawn seguro con radio alrededor del jugador; intentos limitados para evitar bucles.
- Costes/weights ajustables en `EnemyManager`; bosses periódicos configurables en `RoomFlowController`.
- Físicas top‑down por defecto (sin gravedad, rotación congelada); ajustes de sorting layer a `Entities`.
- Multiplicador de velocidad por efectos de tiles en enemigos.

## Conclusión y Próximos Pasos
- El juego cumple los criterios del TFG rogue‑like y está presentable.
- Siguientes mejoras rápidas:
  1) `AudioManager` con música por estado y SFX clave.
  2) `DropController` genérico + base `Pickup` (salud/buff/arma).
  3) Telegráficos/VFX de spawn y feedback visual en puertas.
  4) Persistencia con `PlayerPrefs` para high score y clase.

---
Este documento refleja el estado actual del proyecto y orienta mejoras incrementales sin comprometer la funcionalidad existente.