# Ascension — Estado Actual del Juego

Fecha: 2026-01-29

## Resumen

Se dispone de un flujo jugable completo con menú principal, selección de clase, generación procedimental de una sala, combate, avance con escaleras, jefes periódicos y condiciones de victoria y derrota, con puntuación persistente y herramientas de Editor para automatización.

## Fortalezas

- se emplea una arquitectura modular con `GameManager` y objetos `ScriptableObject`
- se mantiene un flujo continuo con regeneración de suelo y control de avance por escaleras
- se integra un sistema de enemigos con selección ponderada y radio seguro de aparición
- se ofrecen jefes periódicos con escalado y multiplicadores, con victoria tras tres jefes
- se dispone de armas cuerpo a cuerpo y a distancia con proyectiles y rarezas
- se incluye interfaz de usuario (IU) con pausa, panel de estado y pantallas de victoria y derrota
- se registra puntuación con historial persistente y panel de clasificación en el menú principal
- se cuenta con utilidades de Editor para auto‑configuración y validación.

## Carencias (mejoras recomendadas)

- la recogida de armas no equipa todavía el arma al jugador
- el jefe dedicado `BossController` no está integrado en el flujo actual de jefes
- no existe `AudioManager` ni mezcla básica de música y efectos de sonido
- no hay un sistema genérico de caídas con probabilidades
- falta una pasada de balance en pesos de aparición y progresión de armas.

## Flujo y Escenas

- escenas en configuración de compilación
  - [Assets/Scenes/MainMenu.unity](../../Assets/Scenes/MainMenu.unity)
  - [Assets/Scenes/ClassSelection.unity](../../Assets/Scenes/ClassSelection.unity)
  - [Assets/Scenes/GameScene.unity](../../Assets/Scenes/GameScene.unity)
  - [Assets/Scenes/SampleScene.unity](../../Assets/Scenes/SampleScene.unity)
- control del ciclo y estados en [Assets/Scripts/Core/GameManager.cs](../../Assets/Scripts/Core/GameManager.cs)
- pausa y navegación en [Assets/Scripts/UI/PauseMenu.cs](../../Assets/Scripts/UI/PauseMenu.cs)
- pantallas de fin de partida en [Assets/Scripts/UI/VictoryScreen.cs](../../Assets/Scripts/UI/VictoryScreen.cs) y [Assets/Scripts/UI/GameOverScreen.cs](../../Assets/Scripts/UI/GameOverScreen.cs).

## Generación de Salas y Flujo

- generación de mapa de baldosas, rect interno y regeneración de suelo en [Assets/Scripts/Tiles/RoomGenerator.cs](../../Assets/Scripts/Tiles/RoomGenerator.cs)
- flujo de salas con jefes periódicos y avance por escaleras en [Assets/Scripts/Core/RoomFlowController.cs](../../Assets/Scripts/Core/RoomFlowController.cs) y [Assets/Scripts/Tiles/FloorTileManager.cs](../../Assets/Scripts/Tiles/FloorTileManager.cs)
- control por sala con puertas y limpieza en [Assets/Scripts/LevelGeneration/RoomController.cs](../../Assets/Scripts/LevelGeneration/RoomController.cs)
- gestión de puertas en [Assets/Scripts/LevelGeneration/Door.cs](../../Assets/Scripts/LevelGeneration/Door.cs).

## Enemigos

- base de enemigo con daño por contacto, orientación y animación en [Assets/Scripts/Enemies/Enemy.cs](../../Assets/Scripts/Enemies/Enemy.cs)
- gestión de aparición ponderada y por coste con radio seguro en [Assets/Scripts/Enemies/EnemyManager.cs](../../Assets/Scripts/Enemies/EnemyManager.cs)
- prefabricados cargados desde [Assets/Resources/Enemies](../../Assets/Resources/Enemies)
- jefe dedicado disponible en [Assets/Scripts/Enemy/BossController.cs](../../Assets/Scripts/Enemy/BossController.cs)
- emisores de proyectiles en [Assets/Scripts/Enemies/EnemyProjectileSpawner.cs](../../Assets/Scripts/Enemies/EnemyProjectileSpawner.cs)
- aplicación de efectos de baldosas a enemigos en [Assets/Scripts/Enemies/EnemyTileEffectReceiver.cs](../../Assets/Scripts/Enemies/EnemyTileEffectReceiver.cs).

## Armas y Proyectiles

- base y variantes en [Assets/Scripts/Weapons/Weapon.cs](../../Assets/Scripts/Weapons/Weapon.cs), [Assets/Scripts/Weapons/MeleeWeapon.cs](../../Assets/Scripts/Weapons/MeleeWeapon.cs) y [Assets/Scripts/Weapons/RangedWeapon.cs](../../Assets/Scripts/Weapons/RangedWeapon.cs)
- proyectiles con daño y escala en [Assets/Scripts/Weapons/Projectile.cs](../../Assets/Scripts/Weapons/Projectile.cs)
- generación de armas con rarezas en [Assets/Scripts/Weapons/WeaponGenerator.cs](../../Assets/Scripts/Weapons/WeaponGenerator.cs)
- recogida de armas pendiente de equipamiento en [Assets/Scripts/Weapons/WeaponPickup.cs](../../Assets/Scripts/Weapons/WeaponPickup.cs)
- prefabricados en [Assets/Prefabs/Weapons](../../Assets/Prefabs/Weapons) y [Assets/Prefabs/VFX](../../Assets/Prefabs/VFX)
- datos de armas en [Assets/Data/Weapons](../../Assets/Data/Weapons).

## Jugador y UI

- control de jugador con movimiento, rodar e invulnerabilidad en [Assets/Scripts/Player/PlayerController.cs](../../Assets/Scripts/Player/PlayerController.cs)
- salud y corazones en [Assets/Scripts/Player/PlayerHealth.cs](../../Assets/Scripts/Player/PlayerHealth.cs) y [Assets/Scripts/UI/HeartDisplay.cs](../../Assets/Scripts/UI/HeartDisplay.cs)
- mensajes de bloqueo y utilidades de IU en [Assets/Scripts/UI/HUDMessage.cs](../../Assets/Scripts/UI/HUDMessage.cs)
- clasificación persistente con panel en menú principal mediante [Assets/Scripts/UI/ScoreboardMenuUI.cs](../../Assets/Scripts/UI/ScoreboardMenuUI.cs)
- control de evento de IU persistente en [Assets/Scripts/UI/SingletonEventSystem.cs](../../Assets/Scripts/UI/SingletonEventSystem.cs).

## Selección de Clase

- gestor en [Assets/Scripts/ClassSelectionManager.cs](../../Assets/Scripts/ClassSelectionManager.cs)
- datos de clases en [Assets/Data/Classes](../../Assets/Data/Classes).

## Efectos de Baldosas

- datos de efectos en [Assets/Data/TileEffects](../../Assets/Data/TileEffects)
- pintado de efectos en [Assets/Scripts/Editor/TileEffectPainter.cs](../../Assets/Scripts/Editor/TileEffectPainter.cs).

## Herramientas de Editor

- auto‑configuración de salas en [Assets/Scripts/Editor/RoomAutoSetup.cs](../../Assets/Scripts/Editor/RoomAutoSetup.cs)
- generador de mazmorras en [Assets/Scripts/Editor/DungeonAutoBuilder.cs](../../Assets/Scripts/Editor/DungeonAutoBuilder.cs)
- editores de bases de datos en [Assets/Scripts/Editor/EnemyDatabaseEditor.cs](../../Assets/Scripts/Editor/EnemyDatabaseEditor.cs) y [Assets/Scripts/Editor/WeaponDatabaseEditor.cs](../../Assets/Scripts/Editor/WeaponDatabaseEditor.cs)
- validadores y correctores en [Assets/Scripts/Editor](../../Assets/Scripts/Editor).

## Datos y Prefabs

- enemigos en [Assets/Data/Enemies](../../Assets/Data/Enemies)
- armas en [Assets/Data/Weapons](../../Assets/Data/Weapons)
- clases en [Assets/Data/Classes](../../Assets/Data/Classes)
- prefabricados en [Assets/Resources/Enemies](../../Assets/Resources/Enemies), [Assets/Prefabs/Weapons](../../Assets/Prefabs/Weapons) y [Assets/Prefabs/VFX](../../Assets/Prefabs/VFX).

## Balance y Rendimiento

- se aplica radio seguro de aparición y número máximo de intentos
- se ajustan costes y pesos en `EnemyManager` y el intervalo de jefes en `RoomFlowController`
- se define la victoria tras tres jefes y el avance por escaleras
- se mantienen físicas en vista superior con gravedad nula y rotación congelada
- se aplica multiplicador de velocidad por efectos de baldosas en enemigos.

## Conclusión y Próximos Pasos

Se cumple el núcleo funcional de un *roguelike* con victoria y derrota, con margen de mejora en audio, caídas y balance. Se recomiendan como siguientes pasos la integración de `AudioManager`, la finalización de la recogida de armas y la incorporación de telegráficos y efectos visuales en puertas y aparición de enemigos.

---
Este documento refleja el estado actual del proyecto y orienta mejoras incrementales sin comprometer la funcionalidad existente.