# ⚡ QUICK START - 5 Clicks to Play

## 🎯 EJECUTA ESTOS 5 SCRIPTS EN ORDEN

### En Unity, Menu superior:

```
1. Ascension → Setup → 1. Configure GameScene     [1 min]
2. Ascension → Setup → 2. Create Effect Tiles     [30 seg]
3. Ascension → Setup → 3. Create Room Templates   [30 seg]
4. Ascension → Setup → 4. Assign References       [30 seg]
5. Ascension → Setup → 5. Verify Enemy Costs      [30 seg]
```

**Tiempo total: 3-5 minutos**

---

## ✅ QUÉ HACEN

1. **Configure GameScene**
   - Crea Grid + Tilemap
   - Añade RoomCamera
   - Crea Systems (TileEffect, Score, Pool)
   - Crea LevelManager (Builder, RoomMgr, EnemyMgr)

2. **Create Effect Tiles**
   - Tile_Suelo (normal)
   - Tile_Hielo (slow)
   - Tile_Fango (slow)
   - Tile_Puerta (door)

3. **Create Room Templates**
   - Room_Start (inicial)
   - Room_Normal1 (con hielo)
   - Room_Normal2 (con fango)

4. **Assign References**
   - Conecta templates a RoomManager
   - Asigna prefabs a EnemyManager

5. **Verify Enemy Costs**
   - SlimeRed = 2
   - SlimeBlue = 3
   - SlimeGreen = 4

---

## 🎮 DESPUÉS: ScoreDisplay (Manual - 1 min)

```
Canvas → UI → Text - TextMeshPro
- Name: ScoreText
- Add Component: ScoreDisplay
- Asignar referencia
```

---

## 🚀 ¡JUGAR!

Press **Play** → Deberías ver:
- ✅ Sala dibujada
- ✅ Enemigos spawneados
- ✅ Score funcionando
- ✅ Puerta con E

---

**¿Problemas?** Lee `TUTORIAL.md` para detalles completos.

**Scripts creados:**
- GameSceneSetup.cs
- EffectTileCreator.cs
- RoomTemplateCreator.cs
- RoomManagerSetup.cs
- EnemyDataValidator.cs
