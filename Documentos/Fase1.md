# Fase 1 – Base del juego

**Periodo:** Semana 1-3 (20 de abril - 12 de mayo)  
**Objetivo:** Sentar las bases del sistema de juego y establecer una escena interactiva funcional, con control básico del jugador y elementos estáticos para pruebas.

## 1. Movimiento del jugador
Se implementa un sistema de control de movimiento en dos dimensiones (top-down), utilizando el sistema de entradas de Unity (Input System). Se permite el desplazamiento en los ejes X e Y, con suavidad y coherencia en la velocidad.

- **Tecnología:** Unity (C#)
- **Detalle técnico:** Movimiento basado en Rigidbody2D y vectores normalizados para evitar velocidades mayores en movimientos diagonales.
- **Validación:** El jugador puede desplazarse libremente dentro del mapa sin errores de física.

## 2. Colisiones básicas
Se integran colisionadores (BoxCollider2D) para el entorno y el jugador, evitando el traspaso de paredes u obstáculos.

- **Comprobación:** Se testean distintas áreas cerradas y objetos para asegurar que el jugador no pueda atravesarlos.
- **Herramientas:** Layers de colisión y Rigidbody2D en modo cinemático para enemigos/objetos estáticos.

## 3. Sistema de input personalizable
Se utiliza el nuevo Input System de Unity para permitir mapeo de teclas y control más flexible, facilitando futuras ampliaciones (gamepad, remapeo, etc.).

- **Acciones definidas:** Movimiento, dash (preparado), ataque (placeholder).
- **Ventajas:** Separación entre acciones y teclas permite escalar el sistema sin refactorizar entradas directas.

## 4. Cámara que sigue al jugador
Se configura una cámara con sistema de seguimiento suave usando Cinemachine, centrada en el jugador pero con suavizado de movimiento para una mejor experiencia visual.

## 5. Carga de mapas básicos
Durante esta fase se trabaja con mapas estáticos, cargados como escenas o grids en Unity. Se usan tilemaps para el entorno (suelo, paredes).

- **Objetivo futuro:** Preparar la estructura para que en Fase 2 se reemplacen por generación procedural.

## 6. Sistema de entidades y componentes
Se establece la arquitectura básica del juego:

- **Jugador:** Controlado por input, con collider, sprite y sistema de salud inicial (placeholder).
- **Enemigos y objetos:** Representados por prefabs base sin funcionalidad aún, pero integrados en la jerarquía y con etiquetas para detección posterior.
- **Patrón de diseño:** Se adopta un enfoque orientado a componentes para facilitar la escalabilidad.

---

### Hito alcanzado
- El juego carga un mapa estático.
- El jugador se puede mover en las cuatro direcciones.
- Las colisiones con el entorno están activas y funcionales.
- La cámara sigue al jugador correctamente.
- Se sientan las bases para expandir el sistema a enemigos y objetos en la siguiente fase.
