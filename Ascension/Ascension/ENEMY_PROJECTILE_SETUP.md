# 🎯 Configuración de Proyectiles Enemigos

## ✅ **PASO 1: Ejecutar el Script de Creación**

En Unity, ve al menú superior:
```
Ascension → Setup → Create Enemy Projectile Prefab
```

Este script creará automáticamente:
- ✅ Textura circular verde (64x64 pixels @ PPU=16)
- ✅ Prefab `EnemyProjectile.prefab` en `Assets/Prefabs/Enemies/`
- ✅ Componentes configurados:
  - `SpriteRenderer` (círculo verde brillante)
  - `Rigidbody2D` (dynamic, gravityScale=0)
  - `CircleCollider2D` (trigger, radius=0.5)
  - `EnemyProjectile` script (damage=1, lifetime=5s)
- ✅ Asignación automática a prefabs `SlimeGreen` y `ShooterEnemy`

## 📐 **CARACTERÍSTICAS DEL PROYECTIL**

- **Tamaño**: 1/4 del enemigo (escala 0.25x0.25)
  - Enemigo radio: ~0.4 units
  - Proyectil radio: ~0.1 units
- **Color**: Verde brillante con borde oscuro
- **Velocidad**: 0.3125 units/sec (5/16 para PPU=16) - configurado en SlimeGreen
- **Daño**: 1 punto por impacto
- **Lifetime**: 5 segundos antes de auto-destruirse
- **Colisión**: Trigger, detecta Player y paredes

## 🎮 **COMPORTAMIENTO EN JUEGO**

### SlimeGreen (Francotirador):
1. **Distancia < 4 units** → Escapa del jugador (velocidad x1.5)
2. **Distancia 4-12 units** → Rango óptimo, dispara cada 2 segundos
3. **Distancia > 12 units** → Se acerca al jugador

### Sistema de Disparo:
- Detecta al jugador automáticamente
- Calcula dirección hacia el jugador
- Crea proyectil en `shootPoint` (0.5 units a la derecha)
- Aplica velocidad `direction * 0.3125`
- Proyectil viaja en línea recta
- Al impactar: daña al jugador y se destruye
- Al chocar con pared: se destruye

## 🧪 **PRUEBA**

1. Play → Elegir clase → Entrar a sala con SlimeGreen
2. Acércate al enemigo (< 4 units) → Debe escapar
3. Mantén distancia (4-12 units) → Debe disparar cada 2 segundos
4. Observa consola:
   ```
   [SlimeGreen] Disparó hacia el jugador
   [EnemyProjectile] Impactó al jugador por 1 de daño
   ```

## 🔧 **AJUSTES OPCIONALES**

Si quieres modificar el proyectil después de crearlo, edita el prefab `EnemyProjectile.prefab`:

- **Más rápido**: Aumenta `projectileSpeed` en SlimeGreen prefab (default: 0.3125)
- **Más daño**: Cambia `damage` en EnemyProjectile script (default: 1)
- **Más frecuencia**: Reduce `shootCooldown` en SlimeGreen prefab (default: 2s)
- **Más grande**: Aumenta `transform.localScale` en prefab (default: 0.25)
- **Otro color**: Edita la textura `Assets/Sprites/EnemyProjectile_Circle.png`

## ⚠️ **NOTAS IMPORTANTES**

- El proyectil usa `Rigidbody2D.linearVelocity` (Unity 6)
- La colisión es por **Trigger** (OnTriggerEnter2D)
- Tag del jugador debe ser "Player" (ya configurado)
- PPU del sprite es 16 (coherente con el resto del juego)
- Los proyectiles se autodestruyen tras 5s para no acumular en memoria

---

✅ **Sistema listo para usar** - Solo ejecuta el menú y prueba en juego.
