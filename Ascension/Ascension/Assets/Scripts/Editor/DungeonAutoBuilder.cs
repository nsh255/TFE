using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramienta de editor para generar mazmorras procedurales automáticamente.
/// Crea salas conectadas con puertas y un boss al final.
/// </summary>
public class DungeonAutoBuilder : EditorWindow
{
    private int roomCount = 5;
    private float roomSpacing = 25f;
    private bool linearLayout = true;
    private GameObject roomPrefab;
    private GameObject doorPrefab;
    private GameObject bossPrefab;
    private string dungeonName = "GeneratedDungeon";

    [MenuItem("Tools/Ascension/Dungeon Auto-Builder")]
    public static void ShowWindow()
    {
        GetWindow<DungeonAutoBuilder>("Dungeon Builder");
    }

    void OnGUI()
    {
        GUILayout.Label("Dungeon Auto-Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUILayout.Label("Configuración de Mazmorra", EditorStyles.boldLabel);
        
        dungeonName = EditorGUILayout.TextField("Nombre de Mazmorra", dungeonName);
        roomCount = EditorGUILayout.IntSlider("Número de Salas", roomCount, 3, 20);
        roomSpacing = EditorGUILayout.Slider("Espaciado", roomSpacing, 15f, 50f);
        linearLayout = EditorGUILayout.Toggle("Layout Lineal", linearLayout);

        EditorGUILayout.Space();
        GUILayout.Label("Prefabs", EditorStyles.boldLabel);

        roomPrefab = (GameObject)EditorGUILayout.ObjectField("Room Prefab", roomPrefab, typeof(GameObject), false);
        doorPrefab = (GameObject)EditorGUILayout.ObjectField("Door Prefab", doorPrefab, typeof(GameObject), false);
        bossPrefab = (GameObject)EditorGUILayout.ObjectField("Boss Prefab", bossPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Dungeon", GUILayout.Height(40)))
        {
            GenerateDungeon();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Clear Current Dungeon", GUILayout.Height(30)))
        {
            ClearDungeon();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Esta herramienta genera una mazmorra completa:\n" +
            "- Salas conectadas linealmente o aleatoriamente\n" +
            "- Puertas entre salas\n" +
            "- Sala de boss al final\n" +
            "- Todo bajo un GameObject padre",
            MessageType.Info
        );
    }

    private void GenerateDungeon()
    {
        // Validaciones
        if (string.IsNullOrEmpty(dungeonName))
        {
            EditorUtility.DisplayDialog("Error", "El nombre de la mazmorra está vacío", "OK");
            return;
        }

        // Crear GameObject padre
        GameObject dungeonRoot = new GameObject(dungeonName);
        Undo.RegisterCreatedObjectUndo(dungeonRoot, "Generate Dungeon");

        if (linearLayout)
        {
            GenerateLinearDungeon(dungeonRoot);
        }
        else
        {
            GenerateRandomDungeon(dungeonRoot);
        }

        Debug.Log($"[DungeonAutoBuilder] Mazmorra '{dungeonName}' generada con {roomCount} salas");
        EditorUtility.DisplayDialog("Success", $"Mazmorra '{dungeonName}' generada correctamente", "OK");

        Selection.activeGameObject = dungeonRoot;
    }

    private void GenerateLinearDungeon(GameObject parent)
    {
        Vector3 currentPos = Vector3.zero;

        for (int i = 0; i < roomCount; i++)
        {
            // Crear sala
            GameObject room = CreateRoom(parent, currentPos, i);

            // Crear puerta a la derecha (excepto última sala)
            if (i < roomCount - 1)
            {
                Vector3 doorPos = currentPos + new Vector3(roomSpacing / 2f, 0, 0);
                CreateDoor(parent, doorPos, room);
            }

            // Avanzar posición
            currentPos.x += roomSpacing;
        }

        // Crear sala de boss al final
        CreateBossRoom(parent, currentPos);
    }

    private void GenerateRandomDungeon(GameObject parent)
    {
        // Implementación simplificada: grid con caminos aleatorios
        Vector3 currentPos = Vector3.zero;
        Vector3[] directions = { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
        
        for (int i = 0; i < roomCount; i++)
        {
            GameObject room = CreateRoom(parent, currentPos, i);

            if (i < roomCount - 1)
            {
                Vector3 direction = directions[Random.Range(0, 2)]; // Solo derecha o arriba para evitar solapamientos
                Vector3 doorPos = currentPos + direction * (roomSpacing / 2f);
                CreateDoor(parent, doorPos, room);
                currentPos += direction * roomSpacing;
            }
        }

        CreateBossRoom(parent, currentPos + Vector3.right * roomSpacing);
    }

    private GameObject CreateRoom(GameObject parent, Vector3 position, int index)
    {
        GameObject room;

        if (roomPrefab != null)
        {
            room = (GameObject)PrefabUtility.InstantiatePrefab(roomPrefab, parent.transform);
            room.transform.position = position;
        }
        else
        {
            // Crear sala básica si no hay prefab
            room = new GameObject($"Room_{index}");
            room.transform.SetParent(parent.transform);
            room.transform.position = position;

            // Añadir componentes básicos
            RoomController controller = Undo.AddComponent<RoomController>(room);
            BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(room);
            collider.isTrigger = true;
            collider.size = new Vector2(20f, 10f);

            // Añadir spawner
            GameObject spawner = new GameObject("EnemySpawner");
            spawner.transform.SetParent(room.transform);
            spawner.transform.localPosition = Vector3.zero;
            EnemySpawner enemySpawner = Undo.AddComponent<EnemySpawner>(spawner);
            BoxCollider2D spawnerCollider = Undo.AddComponent<BoxCollider2D>(spawner);
            spawnerCollider.isTrigger = true;
            spawnerCollider.size = new Vector2(20f, 10f);
        }

        room.name = $"Room_{index}";
        Undo.RegisterCreatedObjectUndo(room, "Create Room");
        return room;
    }

    private GameObject CreateDoor(GameObject parent, Vector3 position, GameObject connectedRoom)
    {
        GameObject door;

        if (doorPrefab != null)
        {
            door = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab, parent.transform);
            door.transform.position = position;
        }
        else
        {
            // Crear puerta básica
            door = new GameObject("Door");
            door.transform.SetParent(parent.transform);
            door.transform.position = position;

            Door doorScript = Undo.AddComponent<Door>(door);
            BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(door);
            collider.isTrigger = false;
            collider.size = new Vector2(2f, 2f);

            SpriteRenderer sprite = Undo.AddComponent<SpriteRenderer>(door);
            sprite.color = Color.gray;
        }

        Undo.RegisterCreatedObjectUndo(door, "Create Door");
        return door;
    }

    private GameObject CreateBossRoom(GameObject parent, Vector3 position)
    {
        GameObject bossRoom = new GameObject("BossRoom");
        bossRoom.transform.SetParent(parent.transform);
        bossRoom.transform.position = position;

        // Configurar como sala de boss
        RoomController controller = Undo.AddComponent<RoomController>(bossRoom);
        SerializedObject serializedRoom = new SerializedObject(controller);
        SerializedProperty roomTypeProp = serializedRoom.FindProperty("roomType");
        if (roomTypeProp != null)
        {
            roomTypeProp.enumValueIndex = (int)RoomType.Boss;
            serializedRoom.ApplyModifiedProperties();
        }

        BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(bossRoom);
        collider.isTrigger = true;
        collider.size = new Vector2(30f, 15f);

        // Spawnear boss si hay prefab
        if (bossPrefab != null)
        {
            GameObject boss = (GameObject)PrefabUtility.InstantiatePrefab(bossPrefab, bossRoom.transform);
            boss.transform.localPosition = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(boss, "Create Boss");
        }
        else
        {
            Debug.LogWarning("[DungeonAutoBuilder] No hay Boss Prefab configurado");
        }

        Undo.RegisterCreatedObjectUndo(bossRoom, "Create Boss Room");
        return bossRoom;
    }

    private void ClearDungeon()
    {
        GameObject existingDungeon = GameObject.Find(dungeonName);
        if (existingDungeon != null)
        {
            if (EditorUtility.DisplayDialog("Confirm", $"¿Eliminar '{dungeonName}'?", "Sí", "No"))
            {
                Undo.DestroyObjectImmediate(existingDungeon);
                Debug.Log($"[DungeonAutoBuilder] Mazmorra '{dungeonName}' eliminada");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Info", "No se encontró mazmorra con ese nombre", "OK");
        }
    }
}
