using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramienta de editor para configurar automáticamente salas.
/// Detecta puertas, spawners, colliders y asigna scripts necesarios.
/// </summary>
public class RoomAutoSetup : EditorWindow
{
    private GameObject selectedRoom;
    private bool autoDetectDoors = true;
    private bool autoDetectSpawners = true;
    private bool autoAddCollider = true;
    private RoomType roomType = RoomType.Normal;

    [MenuItem("Tools/Ascension/Room Auto-Setup")]
    public static void ShowWindow()
    {
        GetWindow<RoomAutoSetup>("Room Auto-Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Room Auto-Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Selección de objeto
        selectedRoom = (GameObject)EditorGUILayout.ObjectField("Room GameObject", selectedRoom, typeof(GameObject), true);

        EditorGUILayout.Space();
        GUILayout.Label("Configuración", EditorStyles.boldLabel);

        roomType = (RoomType)EditorGUILayout.EnumPopup("Tipo de Sala", roomType);
        autoDetectDoors = EditorGUILayout.Toggle("Auto-detectar Puertas", autoDetectDoors);
        autoDetectSpawners = EditorGUILayout.Toggle("Auto-detectar Spawners", autoDetectSpawners);
        autoAddCollider = EditorGUILayout.Toggle("Añadir Collider si falta", autoAddCollider);

        EditorGUILayout.Space();

        // Botón de setup
        GUI.enabled = selectedRoom != null;
        if (GUILayout.Button("Setup Room", GUILayout.Height(40)))
        {
            SetupRoom();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Esta herramienta:\n" +
            "1. Añade RoomController al objeto\n" +
            "2. Detecta o crea puertas hijas\n" +
            "3. Detecta o crea spawners\n" +
            "4. Configura colliders para triggers",
            MessageType.Info
        );
    }

    private void SetupRoom()
    {
        if (selectedRoom == null)
        {
            EditorUtility.DisplayDialog("Error", "No hay sala seleccionada", "OK");
            return;
        }

        Undo.RegisterCompleteObjectUndo(selectedRoom, "Room Auto-Setup");

        // 1. Añadir o configurar RoomController
        RoomController roomController = selectedRoom.GetComponent<RoomController>();
        if (roomController == null)
        {
            roomController = Undo.AddComponent<RoomController>(selectedRoom);
            Debug.Log("[RoomAutoSetup] RoomController añadido");
        }

        // Usar reflection para setear el tipo de sala (campo privado)
        SerializedObject serializedRoom = new SerializedObject(roomController);
        SerializedProperty roomTypeProp = serializedRoom.FindProperty("roomType");
        if (roomTypeProp != null)
        {
            roomTypeProp.enumValueIndex = (int)roomType;
            serializedRoom.ApplyModifiedProperties();
        }

        // 2. Auto-detectar puertas
        if (autoDetectDoors)
        {
            Door[] doors = selectedRoom.GetComponentsInChildren<Door>();
            if (doors.Length == 0)
            {
                Debug.LogWarning("[RoomAutoSetup] No se encontraron puertas. Considera añadirlas manualmente.");
            }
            else
            {
                Debug.Log($"[RoomAutoSetup] {doors.Length} puertas detectadas");
            }
        }

        // 3. Auto-detectar spawners
        if (autoDetectSpawners)
        {
            EnemySpawner[] spawners = selectedRoom.GetComponentsInChildren<EnemySpawner>();
            if (spawners.Length == 0)
            {
                // Crear spawner automáticamente
                GameObject spawnerObj = new GameObject("EnemySpawner");
                spawnerObj.transform.SetParent(selectedRoom.transform);
                spawnerObj.transform.localPosition = Vector3.zero;
                
                EnemySpawner spawner = Undo.AddComponent<EnemySpawner>(spawnerObj);
                BoxCollider2D spawnerCollider = Undo.AddComponent<BoxCollider2D>(spawnerObj);
                spawnerCollider.isTrigger = true;
                spawnerCollider.size = new Vector2(20f, 10f);

                Debug.Log("[RoomAutoSetup] EnemySpawner creado automáticamente");
            }
            else
            {
                Debug.Log($"[RoomAutoSetup] {spawners.Length} spawners detectados");
            }
        }

        // 4. Configurar collider de la sala
        if (autoAddCollider)
        {
            Collider2D roomCollider = selectedRoom.GetComponent<Collider2D>();
            if (roomCollider == null)
            {
                BoxCollider2D newCollider = Undo.AddComponent<BoxCollider2D>(selectedRoom);
                newCollider.isTrigger = true;
                newCollider.size = new Vector2(20f, 10f);
                Debug.Log("[RoomAutoSetup] Collider añadido a la sala");
            }
            else
            {
                roomCollider.isTrigger = true;
                Debug.Log("[RoomAutoSetup] Collider existente configurado como trigger");
            }
        }

        EditorUtility.SetDirty(selectedRoom);
        EditorUtility.DisplayDialog("Success", "Sala configurada correctamente", "OK");
        Debug.Log("[RoomAutoSetup] Setup completado para: " + selectedRoom.name);
    }
}
