# Fase 2 – Núcleo roguelike

**Periodo:** Semana 4-5 (13 de mayo - 26 de mayo)  
**Objetivo:** Ampliar el esqueleto del juego hasta tener un prototipo jugable con enemigos, combate, recolección de objetos y muerte/reinicio.

## 1. Generación procedural de salas
Se implementa un sistema de generación de niveles basado en la combinación de salas individuales conectadas mediante puertas. Cada sala es una escena independiente (Sala1, Sala2...) y el sistema se encarga de mover al jugador entre ellas.

- **Estrategia:** Se utilizan triggers (puertas) con referencias a la escena destino.
- **Script principal:** `Door.cs` que notifica al `GameManager` para cargar la siguiente sala.
- **Gestor global:** `GameManager.cs` mantiene la persistencia entre escenas y controla la entrada/salida.

> **Nota:** En esta etapa no hay generación aleatoria total, pero la estructura modular permite sustituir las salas por versiones generadas en fases posteriores.

## 2. IA básica de enemigos
Se crea una lógica de enemigos que detectan al jugador y lo persiguen si entra en su rango de visión. Esta es la primera interacción activa con una entidad enemiga.

- **Comportamiento:** Movimiento hacia el jugador con `Vector2.MoveTowards()`.
- **Script:** `Enemy.cs`, asociado al prefab `Enemy.prefab`.
- **Validación:** Se testea en mapas con un solo enemigo para evitar problemas de colisiones o seguimientos infinitos.

## 3. Sistema de ataque y vida
Se introducen los sistemas básicos de combate y salud:

- **Jugador:** Puede atacar (en esta etapa se usa un placeholder como un proyectil o una animación simple).
- **Enemigos:** Tienen una vida que disminuye al ser alcanzados.
- **Script compartido:** `Health.cs`, utilizado tanto por enemigos como por el jugador.
- **Eventos:** Cuando una entidad llega a 0 de vida, se destruye.

## 4. Recolección de ítems simples
Se introducen objetos recogibles que otorgan efectos al jugador:

- **Ejemplos:** Pociones de vida, monedas.
- **Script:** `PickupItem.cs` que detecta la colisión y aplica un efecto.
- **Prefab:** Se crean elementos simples con collider y sprite visible.

## 5. Muerte y reinicio de partida
Se define el flujo de juego tras la muerte del jugador:

- **Condición:** Vida del jugador llega a 0.
- **Acción:** Mostrar pantalla de derrota (placeholder o mensaje temporal).
- **Reinicio:** Desde `GameManager`, se recarga la sala inicial o una escena de menú.

---

### Hito alcanzado
- El jugador puede explorar un conjunto de salas conectadas.
- Hay enemigos que lo persiguen y reciben daño.
- El jugador puede atacar y morir.
- Se pueden recoger objetos que afectan al estado del jugador.
- El juego puede reiniciarse al morir, cumpliendo las condiciones de jugabilidad básica de un roguelike.

> A partir de aquí, se empieza a trabajar en la rejugabilidad, la progresión y la variedad de enemigos (Fase 3).