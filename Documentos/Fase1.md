# Fase 1 – Base del juego

**Periodo:** Semana 1-3 (20 de abril - 12 de mayo)  
**Objetivo:** Sentar las bases del sistema de juego en Unity, con una escena funcional donde el jugador pueda moverse, detectar colisiones y preparar el terreno para sistemas más complejos.

## 1. Movimiento del jugador
Se implementa un sistema de movimiento en 8 direcciones para un personaje con sprite pixel art (12x16), usando un controlador personalizado de inputs.

- **Tecnología:** Unity (C#)
- **Detalle técnico:** Movimiento top-down con Rigidbody2D y normalización de vectores para mantener velocidad uniforme.
- **Validación:** Se probó en una sala de test, asegurando fluidez y control adecuado.

## 2. Colisiones con el entorno
El entorno se compone de tilemaps con colliders automáticos. El jugador tiene un BoxCollider2D que impide atravesar paredes u obstáculos.

- **Comprobación:** Se testea que el jugador no puede salir de los límites de la sala.
- **Herramientas usadas:** TilemapCollider2D + Rigidbody2D (dinámico para el jugador).

## 3. Sistema de input básico
Aunque se contempló el uso del nuevo Input System, se optó por un sistema propio más simple, con Input.GetAxis y Input.GetKey, suficiente para los controles actuales.

- **Acciones activas:** Movimiento.
- **Acciones pendientes:** Dash, ataque (planificados para Fase 2).

## 4. Cámara fija en grid
La cámara estática está posicionada manualmente para mostrar solo la sala actual sin seguir al jugador. Esto permite emular el sistema de salas separadas y prepara la base para una cámara por sala.

- **Beneficio:** Cada sala puede tener su propia composición visual y límites sin necesidad de recálculo dinámico.

## 5. Carga de mapas como escenas
Se diseñaron múltiples salas (ej. Sala1, Sala2…) como escenas independientes. Se implementó un sistema de cambio de sala mediante triggers (puertas) que cargan otra escena.

- **Sistema:** Script `Door.cs` que llama a `GameManager` para cargar otra escena.
- **Organización:** Cada sala se trata como una "habitación" de la torre.

## 6. Primer enemigo básico (placeholder)
Se creó un prefab de enemigo con sprite, colisionador y tag, pero sin lógica de comportamiento. Sirve para posicionar y testear colisiones o detecciones futuras.

- **Objetivo:** Tener una entidad en el mapa que represente un enemigo sin funcionalidad.

---

### Hito alcanzado
- El jugador se mueve con fluidez y tiene colisiones funcionales.
- El entorno está representado por tilemaps y colliders.
- Las salas se cargan como escenas separadas, conectadas mediante puertas.
- Se sienta la estructura para una torre modular por salas.
- El proyecto queda preparado para implementar enemigos, combate y progresión en la Fase 2.
