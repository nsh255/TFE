# Ascension — Informe de Preparación TFG *roguelike*

Fecha: 2026-01-29

## Resumen Ejecutivo

Se dispone de un flujo jugable completo: menú principal, selección de clase, generación procedimental de una sala en una única escena con regeneración del suelo, combate, avance mediante escaleras y aparición periódica de jefes cada cinco salas, con victoria tras derrotar tres jefes o derrota. Se integran gestión de estados con `GameManager`, generación de sala con `RoomGenerator`, flujo con `RoomFlowController`, efectos de baldosas, armas cuerpo a cuerpo y a distancia, puntuación persistente y herramientas de Editor para automatización.

## Fortalezas

- se emplea una arquitectura modular con `GameManager` y objetos `ScriptableObject` para datos
- se mantiene un flujo continuo de salas con regeneración de suelo y control de avance por escaleras
- se ofrece variedad de enemigos con comportamientos diferenciados y jefes periódicos con escalado y multiplicadores
- se define una condición de victoria estable con tres jefes y una derrota con retorno automático al menú
- se dispone de un sistema de puntuación persistente con historial y panel de clasificación en el menú principal
- se incluye interfaz de usuario (IU) con pausa, panel de estado, pantallas de victoria y derrota
- se cuenta con herramientas de Editor para auto‑configuración y validación del contenido.

## Carencias Relevantes

- la recogida de armas en el mundo no equipa todavía el arma en el jugador y requiere integración en `PlayerController`
- el jefe dedicado en `BossController` está disponible pero no se usa en el flujo actual de jefes periódicos
- no existe `AudioManager` ni mezcla básica de música y efectos de sonido
- no hay un sistema genérico de caídas de objetos más allá de baldosas con efectos y `WeaponPickup`
- falta una pasada de balance de progresión y pesos de aparición.

## Escenas y Flujo

Escenas en configuración de compilación.
- [Assets/Scenes/MainMenu.unity](../../Assets/Scenes/MainMenu.unity)
- [Assets/Scenes/ClassSelection.unity](../../Assets/Scenes/ClassSelection.unity)
- [Assets/Scenes/GameScene.unity](../../Assets/Scenes/GameScene.unity)
- [Assets/Scenes/SampleScene.unity](../../Assets/Scenes/SampleScene.unity).

Flujo de juego.
- se accede a la selección de clase desde el menú principal y se carga `GameScene`
- se genera la sala con `RoomGenerator` y se inicia el flujo con `RoomFlowController`
- se instancian enemigos con `EnemyManager` por coste y se valida la sala limpia
- se avanza con la tecla de interacción en la escalera cuando la sala está limpia
- se activan jefes cada cinco salas y la victoria se dispara tras tres jefes
- la derrota se produce con `GameManager.GameOver()` y se retorna al menú.

## Arquitectura y Sistemas Clave

- gestión de estados y puntuación: `GameManager` coordina pausa, escenas, victoria y derrota, y `ScoreManager` registra historial persistente en PlayerPrefs en [Assets/Scripts/Core/GameManager.cs](../../Assets/Scripts/Core/GameManager.cs) y [Assets/Scripts/Core/ScoreManager.cs](../../Assets/Scripts/Core/ScoreManager.cs)
- interfaz de usuario: `PauseMenu`, `VictoryScreen`, `GameOverScreen`, `ScoreDisplay` y panel de clasificación con `ScoreboardMenuUI` en [Assets/Scripts/UI](../../Assets/Scripts/UI)
- generación y avance de salas: `RoomGenerator` crea suelo y muros y `RoomFlowController` inicia salas, alterna jefes y regenera el suelo en [Assets/Scripts/Tiles/RoomGenerator.cs](../../Assets/Scripts/Tiles/RoomGenerator.cs) y [Assets/Scripts/Core/RoomFlowController.cs](../../Assets/Scripts/Core/RoomFlowController.cs)
- interacción con escaleras y efectos: `FloorTileManager` gestiona baldosas, escaleras y mensajes de bloqueo en [Assets/Scripts/Tiles/FloorTileManager.cs](../../Assets/Scripts/Tiles/FloorTileManager.cs) y [Assets/Scripts/UI/HUDMessage.cs](../../Assets/Scripts/UI/HUDMessage.cs)
- enemigos y combate: `EnemyManager` selecciona prefabricados ponderados, aplica radio seguro y registra limpieza en [Assets/Scripts/Enemies/EnemyManager.cs](../../Assets/Scripts/Enemies/EnemyManager.cs)
- jefes: el flujo actual usa enemigos escalados con multiplicadores y secuencia fija opcional, mientras `BossController` queda disponible para un jefe dedicado en [Assets/Scripts/Enemy/BossController.cs](../../Assets/Scripts/Enemy/BossController.cs)
- armas y proyectiles: base `Weapon` con variantes `MeleeWeapon` y `RangedWeapon`, y `Projectile` con daño y escala en [Assets/Scripts/Weapons](../../Assets/Scripts/Weapons)
- jugador y salud: `PlayerController`, `PlayerHealth` y `PlayerSpawner` gestionan movimiento, rodar, invulnerabilidad y panel de corazones en [Assets/Scripts/Player](../../Assets/Scripts/Player) y [Assets/Scripts/PlayerSpawner.cs](../../Assets/Scripts/PlayerSpawner.cs)
- efectos de baldosas y daño: `TileEffect`, `DamageBoostManager` y receptores en enemigos en [Assets/Scripts/Tiles/TileEffect.cs](../../Assets/Scripts/Tiles/TileEffect.cs) y [Assets/Scripts/Tiles/DamageBoostManager.cs](../../Assets/Scripts/Tiles/DamageBoostManager.cs).

## Herramientas de Editor (Automatización)

Ubicación en [Assets/Scripts/Editor](../../Assets/Scripts/Editor).
- se incluyen utilidades de auto‑configuración de salas y mazmorras con `RoomAutoSetup` y `DungeonAutoBuilder`
- se dispone de editores de bases de datos para enemigos y armas con `EnemyDatabaseEditor` y `WeaponDatabaseEditor`
- se ofrece pintado de efectos con `TileEffectPainter` y validadores con `EnemyDataValidator`
- se incorporan correctores y utilidades como `FixEnemyPhysics`, `FixPlayerPrefab`, `FixWeaponColliders`, `ProjectSettingsAutoFix`, `CleanMissingScripts`, `AutoFixer`, etc.

## Datos y Prefabs

- enemigos: [Assets/Data/Enemies](../../Assets/Data/Enemies)
- armas: [Assets/Data/Weapons](../../Assets/Data/Weapons)
- clases: [Assets/Data/Classes](../../Assets/Data/Classes)
- efectos de baldosas: [Assets/Data/TileEffects](../../Assets/Data/TileEffects)
- prefabricados de enemigos en [Assets/Resources/Enemies](../../Assets/Resources/Enemies), usados por la carga automática de `EnemyManager`
- prefabricados de armas en [Assets/Prefabs/Weapons](../../Assets/Prefabs/Weapons)
- prefabricados de efectos visuales en [Assets/Prefabs/VFX](../../Assets/Prefabs/VFX).

## Criterios del TFG — Cobertura

- mazmorras lineales o procedurales: sí, con `RoomGenerator` y `RoomFlowController`
- enemigos con inteligencia artificial (IA) simple: sí, con base `Enemy` y variantes de comportamiento
- armas con estadísticas y proyectiles: sí, con `Weapon`, `MeleeWeapon`, `RangedWeapon` y `Projectile`
- clases seleccionables: sí, con `PlayerClass` y `ClassSelectionManager`
- jefes y condiciones de fin: sí, con jefes periódicos y `GameManager.Victory` o `GameManager.GameOver`
- caídas y potenciadores: parcial, con baldosas de efectos y `WeaponPickup` sin equipamiento completo
- menú de pausa y gestión de estado: sí, con `PauseMenu` y `GameManager`
- sistema de puntuación: sí, con historial persistente y panel de clasificación
- herramientas de Editor: sí, con utilidades de validación y automatización.

## Sugerencias de Mejora (Opcional)

- se recomienda integrar un `AudioManager` con mezcla básica y música por estado
- se recomienda completar la lógica de equipamiento en `WeaponPickup` y añadir un gestor de inventario sencillo
- se recomienda introducir un sistema de caídas con probabilidades para salud y mejoras temporales
- se recomienda integrar el jefe dedicado con `BossController` o ampliar la secuencia de jefes
- se recomienda ajustar los pesos de aparición en `EnemyManager` y el escalado de armas.

## Cómo Probar Rápido

- se inicia en `MainMenu` y se accede a la selección de clase
- se carga `GameScene` y se comprueba movimiento, rodar y ataque
- se limpia la sala y se avanza con la interacción en la escalera
- se verifica la aparición de jefes cada cinco salas y la victoria tras tres jefes
- se fuerza la derrota para validar la pantalla de derrota y el retorno automático al menú.

## Glosario de Rutas Clave

- ciclo de juego: [Assets/Scripts/Core/GameManager.cs](../../Assets/Scripts/Core/GameManager.cs)
- flujo de salas: [Assets/Scripts/Core/RoomFlowController.cs](../../Assets/Scripts/Core/RoomFlowController.cs)
- generación de salas: [Assets/Scripts/Tiles/RoomGenerator.cs](../../Assets/Scripts/Tiles/RoomGenerator.cs)
- control de escaleras y efectos: [Assets/Scripts/Tiles/FloorTileManager.cs](../../Assets/Scripts/Tiles/FloorTileManager.cs)
- control de sala con puertas: [Assets/Scripts/LevelGeneration/RoomController.cs](../../Assets/Scripts/LevelGeneration/RoomController.cs)
- enemigos base: [Assets/Scripts/Enemies/Enemy.cs](../../Assets/Scripts/Enemies/Enemy.cs)
- jefe dedicado: [Assets/Scripts/Enemy/BossController.cs](../../Assets/Scripts/Enemy/BossController.cs)
- armas: [Assets/Scripts/Weapons](../../Assets/Scripts/Weapons)
- interfaz de usuario y clasificación: [Assets/Scripts/UI](../../Assets/Scripts/UI)
- herramientas de Editor: [Assets/Scripts/Editor](../../Assets/Scripts/Editor).

---
Este documento resume el estado actual del proyecto Ascension para alimentación de una IA de documentación. Para más detalle técnico, revisar los archivos y carpetas vinculados arriba.
