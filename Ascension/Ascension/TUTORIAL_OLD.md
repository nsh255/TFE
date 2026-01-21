# Tutorial de Desarrollo - Ascension

Este documento contiene los pasos y configuraciones realizadas durante el desarrollo del juego.

## Configuración Inicial del Proyecto

### Sistema de Renderizado Pixel-Perfect
- **Resolución base**: 480x270 pixels
- **Pixels Per Unit (PPU)**: 1
- **Filter Mode**: Point (no filtering)
- **Compression**: None
- **Upscale Render Texture**: Habilitado
- **Pixel Snapping**: Habilitado

### Configuración de Fullscreen
Se implementó un sistema de fullscreen que mantiene el pixel-perfect rendering:
- Archivo: `Assets/Scripts/FullscreenManager.cs`
- Se ejecuta automáticamente al iniciar el juego
- Mantiene la calidad pixel art sin distorsión

## Sistema de Salud del Jugador

### UI con Corazones
- **Sistema**: 3 corazones = 6 puntos de vida (cada corazón = 2 HP)
- **Sprites**: 
  - `heart_full.png` - Corazón lleno (2 HP)
  - `heart_half.png` - Medio corazón (1 HP)
  - `heart_empty.png` - Corazón vacío (0 HP)
- **Script**: `Assets/Scripts/UI/HealthUI.cs`
- **Prefab**: Corazones en Canvas con `HeartContainer` componente

## Sistema de Enemigos

> ⚠️ **Nota importante**: Este es un juego **top-down** inspirado en Tiny Rogues. Todo se ve desde arriba y el movimiento es en **8 direcciones SIN gravedad** (Gravity Scale = 0 para todos).

### Tipos de Slimes Implementados

#### 1. Slime Rojo (Chaser)
- **Comportamiento**: Persigue al jugador directamente en movimiento continuo
- **Stats**:
  - Vida: 3 HP
  - Daño: 1
  - Velocidad: 3f
  - Distancia mínima: 0.5f
- **Script**: `Assets/Scripts/Enemies/SlimeRed.cs`
- **IA**: Movimiento directo hacia el jugador en 8 direcciones, se detiene al alcanzar distancia mínima

#### 2. Slime Azul (Dasher)
- **Comportamiento**: Hace dashes rápidos hacia el jugador (simula "saltos" en top-down)
- **Stats**:
  - Vida: 4 HP
  - Daño: 2
  - Velocidad de dash: 12f
  - Duración del dash: 0.4 segundos
  - Rango: 3-10 unidades
  - Cooldown: 2 segundos
- **Script**: `Assets/Scripts/Enemies/SlimeBlue.cs`
- **IA**: Espera estático, hace dash rápido en línea recta cuando el jugador está en rango, luego se detiene y espera cooldown

#### 3. Slime Verde (Shooter/Kiter)
- **Comportamiento**: Dispara proyectiles y huye cuando el jugador se acerca
- **Stats**:
  - Vida: 2 HP
  - Daño: 0 (solo proyectiles)
  - Velocidad: 2f
  - Distancia de disparo: 8f
  - Distancia de escape: 4f
  - Cooldown de disparo: 1.5 segundos
- **Script**: `Assets/Scripts/Enemies/SlimeGreen.cs`
- **IA**: 
  - Si el jugador está cerca (< 4 unidades): Huye en dirección opuesta
  - Si el jugador está en rango (< 8 unidades): Dispara proyectiles
  - Proyectiles: Velocidad 5f, 1 de daño, 3 segundos de vida

### Configuración de Sprites de Enemigos

Todos los slimes usan la misma estructura de sprites:

#### Estructura del Spritesheet (62 sprites total)
```
Fila 1 (sprites 0-5):   A1-A6   (Attack - 6 frames)
Fila 2 (sprites 6-11):  L1-L6   (Left - 6 frames)
Fila 3 (sprites 12-17): R1-R6   (Right - 6 frames)
Fila 4 (sprites 18-23): U1-U6   (Up - 6 frames)
Fila 5 (sprites 24-29): D1-D6   (Down - 6 frames)
Fila 6 (sprites 30-35): UL1-UL6 (Up-Left - 6 frames)
Fila 7 (sprites 36-41): UR1-UR6 (Up-Right - 6 frames)
Fila 8 (sprites 42-47): DL1-DL6 (Down-Left - 6 frames)
Fila 9 (sprites 48-53): DR1-DR6 (Down-Right - 6 frames)
Fila 10 (sprites 54-57): IL, IR, IU, ID (Idle - 4 direcciones)
Fila 11 (sprites 58-61): IUL, IUR, IDL, IDR (Idle diagonal - 4 direcciones)
```

#### Archivos de Sprites
- `chaser_slime.png` - Slime rojo (perseguidor)
- `jumper_slime.png` - Slime azul (saltador)
- `shooter_slime.png` - Slime verde (tirador)

**Nota**: Los tres spritesheets tienen la estructura idéntica para facilitar la creación de animaciones reutilizables.

## Sistema de Armas

### Arma Orbital (Orbital Weapon)
- **Script**: `Assets/Scripts/Weapons/OrbitalWeapon.cs`
- **Características**:
  - Número de proyectiles orbitales: Configurable
  - Radio de órbita: Configurable
  - Velocidad de rotación: Configurable
  - Daño por proyectil: Configurable
  - Los proyectiles rotan automáticamente alrededor del jugador
  - Detectan colisiones con enemigos mediante triggers

## Próximos Pasos

### Animaciones de Enemigos (Pendiente)
1. Crear animaciones para cada dirección (8 direcciones + idle)
2. Configurar Animator Controllers
3. Sistema de blend tree para movimiento fluido en 8 direcciones

### EnemyData ScriptableObjects (Pendiente)
Crear ScriptableObjects para configurar stats de enemigos:
- `SlimeRedData.asset`
- `SlimeBlueData.asset`
- `SlimeGreenData.asset`

### Prefabs de Enemigos (Pendiente)
1. Crear prefabs con todos los componentes necesarios
2. Asignar sprites y animadores
3. Configurar colliders y rigidbodies
4. Asignar EnemyData correspondiente

### Sistema de Proyectiles (Pendiente)
- Crear prefab de proyectil para SlimeGreen
- Visual y efectos de partículas
- Configurar daño y velocidad

---

## 🎮 Tutorial Completo: Configurar Enemigos en Unity

### 📋 **Índice**
1. Preparar Sprites
2. Crear Prefabs de Enemigos
3. Configurar Componentes
4. Crear Animaciones
5. Crear EnemyData (ScriptableObjects)
6. Crear Prefab de Proyectil

---

### 1️⃣ **Preparar Sprites**

#### **Importar sprites de slimes:**

1. **Arrastra tus carpetas de sprites** a Unity:
   ```
   Assets/Sprites/Enemies/
   ├── chaser_slime.png (Rojo - perseguidor)
   ├── jumper_slime.png (Azul - saltador)
   └── shooter_slime.png (Verde - francotirador)
   ```

2. **Configurar TODOS los sprites** (selecciona todos):
   - **Inspector → Texture Type:** `Sprite (2D and UI)`
   - **Sprite Mode:** `Multiple`
   - **Pixels Per Unit:** `1` (para pixel art)
   - **Filter Mode:** `Point (no filter)` ← IMPORTANTE para pixel art
   - **Compression:** `None`
   - **Max Size:** `2048` o `4096`
   - Click **Apply**

3. **Para spritesheets** (varios frames en 1 imagen):
   - **Sprite Mode:** `Multiple`
   - Click **Sprite Editor**
   - **Slice → Grid By Cell Count** o **Automatic**
   - **Apply**
   - Renombrar sprites según estructura: A1-A6, L1-L6, R1-R6, U1-U6, D1-D6, UL1-UL6, UR1-UR6, DL1-DL6, DR1-DR6, IL/IR/IU/ID, IUL/IUR/IDL/IDR

---

### 2️⃣ **Crear Prefabs de Enemigos**

#### **A) Slime Rojo (Perseguidor):**

1. **Hierarchy → Click derecho → Create Empty**
2. **Renombrar:** `SlimeRed`
3. **Reset Transform** (Position 0,0,0)

4. **Añadir componentes:**
   - **Add Component → Sprite Renderer**
     - **Sprite:** Selecciona primer sprite de idle del chaser_slime
     - **Material:** `Sprites-Default`
     - **Sorting Layer:** `Enemies` (créala si no existe)
     - **Order in Layer:** `0`

   - **Add Component → Circle Collider 2D**
     - **Radius:** `0.4` (ajusta según tu sprite)
     - **Is Trigger:** `NO` (queremos colisión física)

   - **Add Component → Rigidbody 2D**
     - **Body Type:** `Dynamic`
     - **Gravity Scale:** `0` ← **Top-down sin gravedad**
     - **Linear Drag:** `5` (para que no resbale)
     - **Freeze Rotation Z:** ✅ Activado (para que no rote)

   - **Add Component → Scripts → SlimeRed**

5. **Configurar SlimeRed script:**
   - **Move Speed:** `3`
   - **Min Distance:** `0.5`
   - **Damage Rate:** `1` (1 daño por segundo)

6. **Guardar como Prefab:**
   - Arrastra `SlimeRed` desde Hierarchy a `Assets/Prefabs/Enemies/`
   - Borra el objeto de la Hierarchy

---

#### **B) Slime Azul (Dasher):**

1. **Hierarchy → Create Empty → `SlimeBlue`**

2. **Añadir componentes:**
   - **Sprite Renderer:** Primer sprite de idle del jumper_slime
   - **Circle Collider 2D:** Radius `0.4`
   - **Rigidbody 2D:**
     - **Gravity Scale:** `0` ← **Top-down sin gravedad**
     - **Linear Drag:** `0.5`
     - **Freeze Rotation Z:** ✅
   - **Add Component → SlimeBlue script**

3. **Configurar SlimeBlue:**
   - **Dash Speed:** `12` (velocidad del dash)
   - **Dash Duration:** `0.4` (duración del movimiento rápido)
   - **Dash Cooldown:** `2` (tiempo entre dashes)
   - **Min Dash Distance:** `3`
   - **Max Dash Distance:** `10`
   - **Damage Rate:** `1`

4. **Guardar como Prefab:** `Assets/Prefabs/Enemies/SlimeBlue`

> 💡 **Nota**: El SlimeBlue hace un "dash" (movimiento rápido) en lugar de saltar verticalmente. Se mueve en línea recta hacia el jugador durante 0.4 segundos y luego se detiene, simulando un salto en el contexto top-down.

---

#### **C) Slime Verde (Francotirador):**

1. **Create Empty → `SlimeGreen`**

2. **Añadir componentes:**
   - **Sprite Renderer:** Primer sprite de idle del shooter_slime
   - **Circle Collider 2D:** Radius `0.4`
   - **Rigidbody 2D:**
     - **Gravity Scale:** `0` ← **Top-down sin gravedad**
     - **Linear Drag:** `5`
     - **Freeze Rotation Z:** ✅
   - **Add Component → SlimeGreen script**

3. **Crear ShootPoint:**
   - **Click derecho en SlimeGreen (Hierarchy) → Create Empty**
   - **Renombrar:** `ShootPoint`
   - **Position:** `(0.5, 0, 0)` (delante del slime)

4. **Configurar SlimeGreen:**
   - **Optimal Shoot Distance:** `8`
   - **Escape Distance:** `4`
   - **Max Distance:** `12`
   - **Move Speed:** `2`
   - **Shoot Point:** Arrastra `ShootPoint` aquí
   - **Projectile Prefab:** (lo crearemos después)
   - **Projectile Speed:** `5`
   - **Shoot Cooldown:** `2`
   - **Projectile Damage:** `1`
   - **Damage Rate:** `1`

5. **Guardar como Prefab:** `Assets/Prefabs/Enemies/SlimeGreen`

---

### 3️⃣ **Configurar Componentes**

#### **Verificar configuración de cada slime:**

> ⚠️ **Importante**: Todos los enemigos tienen **Gravity Scale = 0** porque el juego es **top-down** (vista desde arriba).

| Componente | SlimeRed | SlimeBlue | SlimeGreen |
|------------|----------|-----------|------------|
| **Gravity Scale** | 0 | 0 | 0 |
| **Linear Drag** | 5 | 0.5 | 5 |
| **Collider** | Circle 0.4 | Circle 0.4 | Circle 0.4 |
| **Script** | SlimeRed | SlimeBlue | SlimeGreen |
| **Comportamiento** | Persigue | Dash | Dispara/Huye |

---

### 4️⃣ **Crear Animaciones (Compartidas entre Slimes)**

> 💡 **Estrategia Optimizada**: Como los 3 slimes tienen spritesheets idénticos (mismos nombres de sprites), crearemos **1 Animator base** con **Animation Override Controllers** para cada color. Así solo hacemos las animaciones una vez.

#### **Paso 1: Crear Animator Base (Slime Master)**

1. **Project → Assets/Animations/Enemies → Crear carpeta `_Shared`**

2. **Crear GameObject temporal:**
   - Hierarchy → Create Empty → `SlimeMaster`
   - Add Component → Sprite Renderer
   - Sprite: Cualquier sprite idle de chaser_slime (ejemplo: `ID` - idle mirando abajo)

3. **Abrir Animation window:**
   - Menú superior → **Window → Animation → Animation**
   - Aparecerá la ventana "Animation" (anclala junto al Inspector)

4. **Crear animación Idle (paso por paso):**
   
   a. **Con `SlimeMaster` seleccionado en Hierarchy**, en la ventana Animation click **Create**
   
   b. **Guardar archivo:**
      - Navega a `Assets/Animations/Enemies/_Shared/`
      - Nombre: `Slime_Idle.anim`
      - Click **Save**
   
   c. **Unity creó automáticamente:**
      - `Slime_Idle.anim` (la animación)
      - `SlimeMaster.controller` (el Animator Controller)
   
   d. **Añadir el sprite idle:**
      - En la ventana Animation, click botón **Add Property**
      - Busca **Sprite Renderer → Sprite** → Click el **+** 
      - Verás una línea en el timeline que dice "Sprite Renderer.Sprite"
   
   e. **Configurar el sprite:**
      - En la timeline, verás un **rombo (keyframe)** en el tiempo 0:00
      - Click en ese rombo para seleccionarlo
      - En el Inspector (o en la misma ventana Animation), verás un campo de sprite
      - Arrastra el sprite **`ID`** (idle mirando abajo) desde el Project
      - Como el idle es **estático** (no tiene frames numerados), con 1 sprite es suficiente
   
   f. **Ajustar duración (opcional):**
      - El idle puede durar lo que quieras (1-2 segundos está bien)
      - Si quieres cambiar la duración, arrastra el final del clip en la timeline
   
   g. **Activar Loop:**
      - Selecciona el archivo `Slime_Idle.anim` en el Project
      - En Inspector, marca **Loop Time** ✅
      - Click **Apply**

   > 💡 **Nota**: El idle es estático (1 solo sprite). Si quieres animación idle con movimiento sutil, usa frames de movimiento lento (ejemplo: L1, L2 en loop).

5. **Crear animación Dash:**
   
   a. **Con `SlimeMaster` aún seleccionado**, en la ventana Animation:
      - Click el **dropdown** donde dice "Slime_Idle" (arriba a la izquierda)
      - Click **Create New Clip**
   
   b. **Guardar:**
      - Nombre: `Slime_Dash.anim`
      - Guardar en `Assets/Animations/Enemies/_Shared/`
   
   c. **Añadir frames de movimiento:**
      - Click **Add Property → Sprite Renderer → Sprite** (igual que antes)
      - Ahora vas a crear **múltiples keyframes** para la animación
   
   d. **Añadir sprites en la timeline:**
      - Click en la timeline en el tiempo **0:00**
      - En el campo de sprite (abajo en Animation window), arrastra el sprite **`D1`** (down frame 1)
      - Mueve la línea vertical roja (playhead) al tiempo **0:05** (aprox)
      - Click el botón **Record** (círculo rojo) para activar grabación
      - Arrastra el sprite **`D2`** al campo de sprite
      - Repite para **`D3, D4, D5, D6`** espaciándolos cada 0:05 segundos
      - Click **Record** de nuevo para desactivar grabación
   
   e. **Ajustar velocidad:**
      - En la ventana Animation, verás "Sample" (arriba derecha)
      - Cámbialo a **12-15** (FPS más rápido para efecto de dash)
   
   f. **NO activar Loop:**
      - Selecciona `Slime_Dash.anim` en Project
      - Inspector → **Loop Time** ❌ DESACTIVADO
   
   > 💡 **Truco**: Para dash en 8 direcciones, puedes crear clips separados (Dash_Down, Dash_Left, etc.) o usar Blend Trees (avanzado).

6. **Crear animación Shoot:**
   
   a. **Dropdown → Create New Clip → `Slime_Shoot.anim`**
   
   b. **Añadir frames de ataque:**
      - Add Property → Sprite Renderer → Sprite
      - Añade los sprites **`A1, A2, A3, A4, A5, A6`** en la timeline
      - Sample rate: **10 FPS**
   
   c. **NO activar Loop** (es animación única)

7. **Crear animación Die:**
   
   a. **Create New Clip → `Slime_Die.anim`**
   
   b. **Opciones para la animación de muerte:**
      - **Opción 1**: Usa los últimos frames de ataque invertidos
      - **Opción 2**: Crea un fade out (reducir alpha del Sprite Renderer)
      - **Opción 3**: Usa una animación de "squash" (escala Y reducida)
   
   c. **Ejemplo de fade out:**
      - Add Property → **Sprite Renderer → Color**
      - En tiempo 0:00, color blanco alpha 255
      - En tiempo 0:30, color blanco alpha 0
      - La animación hará fade out
   
   d. **NO activar Loop** (el objeto se destruye después)

8. **Resultado:**
   - Deberías tener 4 archivos `.anim` en `_Shared/`:
     - `Slime_Idle.anim`
     - `Slime_Dash.anim`
     - `Slime_Shoot.anim`
     - `Slime_Die.anim`
   - Y 1 archivo `SlimeMaster.controller`

---

#### **Paso 2: Configurar Animator Controller Base**

1. **Abre `SlimeMaster.controller`** (se creó automáticamente en `_Shared`)

2. **Window → Animation → Animator**

3. **Crear estados:**
   - **Idle** (naranja = default)
   - **Dash**
   - **Shoot**
   - **Die**

4. **Crear parámetros:**
   - **+ → Trigger → `Jump`**
   - **+ → Trigger → `Shoot`**
   - **+ → Trigger → `Die`**
   - **+ → Bool → `IsMoving`**

5. **Crear transiciones:**

   **Idle → Dash:**
   - Click derecho Idle → Make Transition → Dash
   - Conditions: `Jump` (trigger)
   - Has Exit Time: ❌
   - Transition Duration: `0`

   **Dash → Idle:**
   - Dash → Make Transition → Idle
   - Has Exit Time: ✅
   - Exit Time: `1`
   - Transition Duration: `0.1`

   **Idle → Shoot:**
   - Conditions: `Shoot` (trigger)
   - Has Exit Time: ❌
   - Transition Duration: `0`

   **Shoot → Idle:**
   - Has Exit Time: ✅
   - Exit Time: `1`
   - Transition Duration: `0.1`

   **Any State → Die:**
   - Click Any State → Make Transition → Die
   - Conditions: `Die` (trigger)
   - Has Exit Time: ❌
   - Transition Duration: `0`

6. **Borrar el GameObject `SlimeMaster`** de la Hierarchy (ya no lo necesitamos)

---

#### **Paso 3: Crear Animation Override Controllers**

Ahora crearemos versiones específicas para cada color:

1. **Project → Assets/Animations/Enemies → Crear carpetas:**
   - `SlimeRed/`
   - `SlimeBlue/`
   - `SlimeGreen/`

2. **Crear Override para Slime Rojo:**
   - Click derecho en `SlimeRed/` → Create → **Animator Override Controller**
   - Renombrar: `SlimeRed`
   - Inspector → **Controller:** Arrastra `SlimeMaster.controller` aquí
   - **Ahora verás la lista de animaciones**:
     - `Slime_Idle` → Click derecho → Selecciona sprites de **chaser_slime** (rojo)
     - `Slime_Dash` → Sprites de **chaser_slime**
     - `Slime_Shoot` → Sprites de **chaser_slime**
     - `Slime_Die` → Sprites de **chaser_slime**
   
   > 💡 **No necesitas crear nuevas animaciones**, solo reasignar los sprites rojos en el override.

3. **Crear Override para Slime Azul:**
   - En `SlimeBlue/` → Create → Animator Override Controller → `SlimeBlue`
   - Controller: `SlimeMaster.controller`
   - Reasignar con sprites de **jumper_slime** (azul)

4. **Crear Override para Slime Verde:**
   - En `SlimeGreen/` → Create → Animator Override Controller → `SlimeGreen`
   - Controller: `SlimeMaster.controller`
   - Reasignar con sprites de **shooter_slime** (verde)

---

#### **Paso 4: Asignar Overrides a los Prefabs**

1. **Selecciona prefab `SlimeRed`**
   - Inspector → **Add Component → Animator**
   - **Controller:** Arrastra `SlimeRed` (Animator Override Controller)

2. **Selecciona prefab `SlimeBlue`**
   - Add Component → Animator
   - Controller: `SlimeBlue` (Override)

3. **Selecciona prefab `SlimeGreen`**
   - Add Component → Animator
   - Controller: `SlimeGreen` (Override)

---

#### **✅ Ventajas de este método:**

- ✅ **Animaciones creadas una sola vez** (en SlimeMaster)
- ✅ **Lógica compartida** (transiciones, parámetros)
- ✅ **Fácil mantenimiento** (cambias en Master, se actualiza en todos)
- ✅ **Solo cambias sprites** en cada Override
- ✅ **Menos archivos** y más organizado

---

### 5️⃣ **Crear EnemyData (ScriptableObjects)**

#### **A) Crear los ScriptableObjects:**

1. **Project → Assets/Data/Enemies** (crea la carpeta si no existe)

2. **Click derecho → Create → Enemies → EnemyData**

3. **Renombrar:** `SlimeRedData`

4. **Configurar en Inspector:**
   ```
   Enemy Name: "Slime Rojo"
   Max Health: 3
   Damage: 1
   Speed: 3
   Sprite: Primer sprite idle del chaser_slime
   Enemy Cost: 10
   ```

5. **Repetir para SlimeBlue y SlimeGreen:**

**SlimeBlueData:**
```
Enemy Name: "Slime Azul"
Max Health: 4
Damage: 2
Speed: 2 (no se usa mucho, usa jumpForce)
Sprite: Primer sprite idle del jumper_slime
Enemy Cost: 15
```

**SlimeGreenData:**
```
Enemy Name: "Slime Verde"
Max Health: 2
Damage: 0 (daño por contacto, los proyectiles hacen 1)
Speed: 2
Sprite: Primer sprite idle del shooter_slime
Enemy Cost: 20
```

---

#### **B) Asignar EnemyData a los prefabs:**

1. **Selecciona prefab `SlimeRed`**
2. **Inspector → Enemy (Script) → Enemy Data:**
   - Arrastra `SlimeRedData` aquí

3. **Repetir para SlimeBlue y SlimeGreen**

---

### 6️⃣ **Crear Prefab de Proyectil**

#### **A) Crear sprite del proyectil:**

1. **Crea un sprite simple** (bola verde, 8x8 px) o usa uno de tus assets
2. **Importar con las mismas configuraciones** (Point filter, PPU=1)

#### **B) Crear prefab:**

1. **Hierarchy → Create Empty → `EnemyProjectile`**

2. **Añadir componentes:**
   - **Sprite Renderer:**
     - Sprite: Tu sprite de proyectil
     - Color: Verde claro
     - Sorting Layer: `Projectiles`
     - Order: `5`

   - **Circle Collider 2D:**
     - Radius: `0.2`
     - **Is Trigger:** ✅ Activado

   - **Rigidbody 2D:**
     - Body Type: `Dynamic`
     - **Gravity Scale:** `0` (vuela recto)
     - **Freeze Rotation Z:** ✅

   - **Add Component → EnemyProjectile script:**
     - Damage: `1`
     - Lifetime: `5`

3. **Guardar como Prefab:** `Assets/Prefabs/Projectiles/EnemyProjectile`

4. **Asignar al SlimeGreen:**
   - Selecciona prefab `SlimeGreen`
   - **Slime Green (Script) → Projectile Prefab:** Arrastra `EnemyProjectile` aquí

---

### ✅ **Checklist Final**

Antes de probar:

- [ ] **Sprites importados** (Point filter, PPU=1)
- [ ] **3 Prefabs creados** (SlimeRed, SlimeBlue, SlimeGreen)
- [ ] **Componentes configurados** (Colliders, Rigidbody, Scripts)
- [ ] **Animaciones creadas** (Idle, Jump/Shoot, Die)
- [ ] **Animator Controllers** configurados (transiciones + parámetros)
- [ ] **EnemyData ScriptableObjects** creados y asignados
- [ ] **Prefab de proyectil** creado y asignado a SlimeGreen
- [ ] **Tag "Player"** existe en el jugador
- [ ] **Sorting Layers creadas** (Enemies, Projectiles)

---

### 🎮 **Probar en escena:**

1. **Arrastra un prefab a la escena** (ej: SlimeRed)
2. **Asegúrate que el jugador tiene tag "Player"**
3. **Play** y observa:
   - ¿Se mueve hacia ti?
   - ¿Te hace daño al tocarte?
   - ¿Los corazones se actualizan?
   - ¿La animación funciona?

---

### 🐛 **Troubleshooting común:**

**"El slime no se mueve":**
- Verifica que `enemyData` esté asignado
- Verifica que haya un objeto con tag "Player"
- Mira la consola por errores

**"No hace daño":**
- Verifica que el jugador tenga tag "Player"
- Verifica que `PlayerHealth` esté en el jugador
- Verifica que `damageRate` no sea 0

**"Proyectiles no aparecen":**
- Verifica que `projectilePrefab` esté asignado en SlimeGreen
- Verifica que el prefab tenga Rigidbody2D

**"Animaciones no cambian":**
- Verifica que el Animator Controller esté asignado
- Verifica los parámetros (Jump, Shoot, Die)
- Verifica las transiciones

---

*Última actualización: 22 de octubre de 2025*
