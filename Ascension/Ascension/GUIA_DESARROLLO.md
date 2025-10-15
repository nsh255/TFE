# 🎮 ASCENSION - GUÍA DE DESARROLLO ROGUELIKE

## ✅ ARREGLOS COMPLETADOS (Sesión actual)

### 1. Sistema de Carga del Jugador - ARREGLADO ✅
**Problema:** NullReferenceException al instanciar el jugador desde ClassSelection

**Solución implementada:**
- ✅ `PlayerSpawner.cs`: Ahora llama a `Initialize()` DESPUÉS de asignar `playerClass`
- ✅ `PlayerController.cs`: Nuevo método `Initialize()` que se ejecuta manualmente
- ✅ `PlayerHealth.cs`: Nuevo método `Initialize()` que espera a que `playerClass` esté asignado
- ✅ Validaciones de null agregadas en todos los scripts críticos

### 2. Sistema de Armas - MEJORADO ✅
**Cambios implementados:**
- ✅ `Weapon.cs`: Auto-asigna Camera.main si no está configurada
- ✅ `Weapon.cs`: Input de ataque con clic izquierdo (GetMouseButtonDown(0))
- ✅ `Weapon.cs`: Método `Initialize()` para configuración post-instanciación
- ✅ `MeleeWeapon.cs`: Sistema de daño a enemigos implementado con cooldown
- ✅ `RangedWeapon.cs`: Sistema de disparo mejorado con cooldown
- ✅ `Projectile.cs`: **NUEVO** - Script para balas/proyectiles

---

## 🎯 PRÓXIMOS PASOS PRIORITARIOS

### PASO 3: UI del Jugador - Corazones ❤️
**Lo que necesitas:**
1. Importar sprites de corazones (lleno y vacío) que ya tienes
2. Crear Canvas en GameScene
3. Configurar imágenes de corazones en UI

**Archivos a crear:**
- `HeartDisplay.cs` - Script para mostrar corazones en UI

**Tiempo estimado:** 30-45 minutos

---

### PASO 4: Sistema de Armas Mejorado 🗡️
**Tu visión:**
- 3 tipos base: Espada, Espadón, Bastón
- Cada tipo puede tener variantes aleatorias
- Drops aleatorios de enemigos

**Plan de implementación:**
1. **Crear `WeaponType` enum:**
   - Sword (rápida, poco daño)
   - GreatSword (lenta, mucho daño)
   - Staff (media, dispara proyectiles)

2. **Modificar `WeaponData.cs`:**
   - Agregar `WeaponType weaponType`
   - Agregar rangos de stats (minDamage-maxDamage, etc.)
   - Agregar prefijos/sufijos para nombres (ej: "Espada Flamígera")

3. **Crear `WeaponGenerator.cs`:**
   - Generar armas aleatorias basadas en tipo base
   - Sistema de rareza (Común, Rara, Épica, Legendaria)

**Archivos a modificar:**
- `WeaponData.cs` ✏️
- `Enemy.cs` ✏️ (agregar drop de armas al morir)

**Archivos nuevos:**
- `WeaponGenerator.cs` 🆕
- `WeaponPickup.cs` 🆕

**Tiempo estimado:** 1-2 horas

---

### PASO 5: Importar Enemigos Faltantes 👹
**Lo que necesitas hacer:**
1. Importar sprites de los 3 tipos de enemigos
2. Crear animaciones básicas (Idle, Walk, Attack, Death)
3. Configurar Animator Controller para cada enemigo
4. Crear prefabs y EnemyData ScriptableObjects

**Enemigos actuales:**
- ✅ ChaserEnemy (persigue al jugador)
- ✅ JumperEnemy (salta hacia el jugador)
- ✅ ShooterEnemy (dispara proyectiles)
- ✅ BossEnemy (placeholder)

**Tiempo estimado:** 2-3 horas (dependiendo de cuántas animaciones hagas)

---

### PASO 6: Generación Procedural con Tilemaps 🗺️
**Tu visión:**
- Salas generadas con Tilesets
- Diferentes biomas/temas
- Tiles con efectos (lava, hielo, etc.)

**Plan de implementación:**

1. **Estructura de datos:**
```csharp
// RoomData.cs
public class RoomData : ScriptableObject
{
    public Vector2Int roomSize;
    public TileBase[] floorTiles;
    public TileBase[] wallTiles;
    public RoomType roomType; // Normal, Boss, Shop, etc.
    public TileEffectType[] tileEffects;
}
```

2. **Sistema de generación:**
```csharp
// RoomController.cs
- Crear grid de habitaciones
- Generar conexiones entre habitaciones
- Instanciar Tilemaps
- Spawn de enemigos por habitación
- Puertas que se abren al limpiar la sala
```

3. **Efectos de tiles:**
```csharp
// TileEffect.cs
- Lava: Daño por segundo
- Hielo: Reduce velocidad
- Veneno: Daño gradual
- Agua: Ralentiza ligeramente
```

**Archivos a crear:**
- `RoomController.cs` ✏️ (actualmente vacío)
- `RoomData.cs` 🆕
- `DungeonGenerator.cs` 🆕
- `TileEffect.cs` 🆕
- `Door.cs` 🆕

**Tiempo estimado:** 3-5 horas

---

## 📊 ESTRUCTURA RECOMENDADA DE CARPETAS

```
Assets/
├── Data/
│   ├── Classes/          ✅ (Knight, Mage, Swordsman)
│   ├── Weapons/          ✅ (Sword, GreatSword, Staff)
│   ├── Enemies/          ✅ (varios enemigos)
│   └── Rooms/            🆕 (RoomData ScriptableObjects)
├── Prefabs/
│   ├── Player.prefab     ✅
│   ├── Weapons/          ✅ (MeleeWeaponPrefab, etc.)
│   ├── Enemies/          ✅ (varios enemigos)
│   ├── Projectiles/      🆕
│   ├── Pickups/          🆕 (WeaponPickup, HealthPickup, etc.)
│   └── Rooms/            🆕 (Prefabs de habitaciones)
├── Scripts/
│   ├── Player/           ✅ (PlayerController, PlayerHealth)
│   ├── Weapons/          ✅ (Weapon, MeleeWeapon, RangedWeapon, Projectile)
│   ├── Enemies/          ✅ (Enemy, ChaserEnemy, etc.)
│   ├── LevelGeneration/  ⚠️ (RoomController vacío)
│   ├── Items/            ❌ (vacío - necesita WeaponPickup, etc.)
│   ├── UI/               ❌ (vacío - necesita HeartDisplay, etc.)
│   ├── Managers/         ❌ (vacío - necesita GameManager, etc.)
│   └── Effects/          ❌ (vacío - necesita TileEffect, etc.)
├── Sprites/
│   ├── Player/           ✅
│   ├── Weapons/          ✅
│   ├── Enemies/          ⚠️ (faltan 3 tipos por importar)
│   ├── UI/               ⚠️ (corazones por importar)
│   └── Tiles/            🆕 (tilesets a crear/importar)
└── Animations/           ✅ (Player animations completas)
```

---

## 🎓 CONSEJOS PARA TU PROYECTO UNIVERSITARIO

### 1. **Documenta tu proceso**
- Toma screenshots del antes/después
- Guarda versiones en Git con commits descriptivos
- Crea un documento explicando decisiones de diseño

### 2. **Prioriza funcionalidad sobre contenido**
Es mejor tener:
- ✅ 3 tipos de enemigos funcionando bien
- ✅ Sistema de generación básico pero funcional
- ✅ Sistema de armas con 3 variantes por tipo

Que tener:
- ❌ 20 tipos de enemigos sin pulir
- ❌ Generación compleja que no funciona
- ❌ 100 armas diferentes sin balance

### 3. **Testing continuo**
Después de cada sistema implementado:
1. Juega 5-10 minutos
2. Anota bugs/problemas
3. Arregla lo crítico inmediatamente

### 4. **Balance es clave en roguelikes**
- Ajusta el daño de armas para que se sienta bien
- Enemigos no deben ser ni muy fáciles ni imposibles
- Generación de habitaciones debe ser justa pero desafiante

---

## 📝 CHECKLIST DE FUNCIONALIDAD MÍNIMA

Para que tu roguelike sea "jugable" necesitas:

- [x] Movimiento del jugador funcional
- [x] Sistema de roll/dash
- [x] Sistema de vida del jugador
- [x] Ataque con armas (cuerpo a cuerpo y distancia)
- [ ] UI de vida visible
- [ ] Al menos 3 tipos de enemigos
- [ ] Enemigos que hacen daño al jugador
- [ ] Generación de al menos 3-5 habitaciones
- [ ] Puertas que conectan habitaciones
- [ ] Sistema de spawn de enemigos por habitación
- [ ] Al menos 3 armas por tipo (9 total)
- [ ] Sistema de drops de armas
- [ ] Condición de victoria (derrotar jefe final)
- [ ] Condición de derrota (vida = 0)

---

## 🚀 PLAN DE SPRINT (RECOMENDADO)

### Semana 1: Base funcional
- [x] Día 1-2: Arreglar bugs críticos ✅ **COMPLETADO**
- [ ] Día 3: UI de corazones
- [ ] Día 4-5: Importar enemigos faltantes
- [ ] Día 6-7: Sistema básico de generación de salas

### Semana 2: Contenido y pulido
- [ ] Día 1-2: Sistema de drops de armas
- [ ] Día 3-4: Efectos de tiles
- [ ] Día 5: Balance y testing
- [ ] Día 6-7: Polish y efectos visuales

### Semana 3: Finalización
- [ ] Día 1-2: Jefe final
- [ ] Día 3-4: Menús mejorados
- [ ] Día 5: Testing final
- [ ] Día 6-7: Documentación y presentación

---

## 💡 RECURSOS ÚTILES

### Para Tilemaps:
- Unity Tilemap Palette
- Rule Tiles para transiciones automáticas
- Tilemap Collider 2D para colisiones

### Para Balance:
- Empieza con valores conservadores:
  - Espada: 2 daño, 0.5s cooldown
  - Espadón: 4 daño, 1.2s cooldown
  - Bastón: 3 daño, 0.8s cooldown
  
- Enemigos básicos: 3-5 HP
- Enemigos avanzados: 8-12 HP
- Jefes: 30-50 HP

### Para Generación:
- Empieza simple: 5 habitaciones en línea
- Luego evoluciona a: grid 3x3 con caminos aleatorios
- Finalmente: generación BSP o similar

---

## 🎯 META FINAL

**Un roguelike funcional con:**
- ✅ Mecánicas de movimiento y combate pulidas
- ✅ 3 clases jugables distintas
- ✅ 3 tipos de armas con variantes
- ✅ 3-5 tipos de enemigos diferentes
- ✅ Generación procedural de 8-12 habitaciones
- ✅ Sistema de progresión (mejoras/drops)
- ✅ Un jefe final desafiante
- ✅ UI clara y funcional

**Tiempo estimado total:** 20-30 horas de desarrollo

---

¡Ánimo con tu proyecto! Tienes una base sólida y con este plan deberías poder completarlo sin problemas. 🚀
