using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Editor visual para gestionar la base de datos de enemigos.
/// Permite crear, editar y eliminar enemigos rápidamente.
/// </summary>
public class EnemyDatabaseEditor : EditorWindow
{
    private List<EnemyData> enemyDatabase = new List<EnemyData>();
    private Vector2 scrollPosition;
    private EnemyData selectedEnemy;
    private bool showCreatePanel = false;

    // Datos para crear nuevo enemigo
    private string newEnemyName = "New Enemy";
    private int newEnemyHealth = 10;
    private int newEnemyDamage = 5;
    private float newEnemyMoveSpeed = 2f;
    private Sprite newEnemySprite;
    private GameObject newEnemyPrefab;

    [MenuItem("Tools/Ascension/Enemy Database Editor")]
    /// <summary>
    /// Abre el editor de base de datos de enemigos.
    /// </summary>
    public static void ShowWindow()
    {
        GetWindow<EnemyDatabaseEditor>("Enemy Database");
    }

    /// <summary>
    /// Carga la base de datos al habilitar la ventana.
    /// </summary>
    void OnEnable()
    {
        LoadEnemyDatabase();
    }

    /// <summary>
    /// Dibuja la interfaz de edición en el inspector.
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Enemy Database Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Botones principales
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Database", GUILayout.Height(25)))
        {
            LoadEnemyDatabase();
        }
        if (GUILayout.Button("Create New Enemy", GUILayout.Height(25)))
        {
            showCreatePanel = !showCreatePanel;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Panel de creación
        if (showCreatePanel)
        {
            DrawCreatePanel();
        }

        EditorGUILayout.Space();
        GUILayout.Label($"Enemigos Registrados: {enemyDatabase.Count}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Lista de enemigos
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 0; i < enemyDatabase.Count; i++)
        {
            if (enemyDatabase[i] == null) continue;

            DrawEnemyEntry(enemyDatabase[i], i);
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Dibuja el panel de creación de nuevos EnemyData.
    /// </summary>
    private void DrawCreatePanel()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Create New Enemy", EditorStyles.boldLabel);

        newEnemyName = EditorGUILayout.TextField("Name", newEnemyName);
        newEnemyHealth = EditorGUILayout.IntField("Health", newEnemyHealth);
        newEnemyDamage = EditorGUILayout.IntField("Damage", newEnemyDamage);
        newEnemyMoveSpeed = EditorGUILayout.FloatField("Move Speed", newEnemyMoveSpeed);
        newEnemySprite = (Sprite)EditorGUILayout.ObjectField("Sprite", newEnemySprite, typeof(Sprite), false);
        newEnemyPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", newEnemyPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Enemy Data", GUILayout.Height(30)))
        {
            CreateNewEnemy();
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Dibuja una entrada de enemigo en la lista.
    /// </summary>
    private void DrawEnemyEntry(EnemyData enemy, int index)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        // Icono
        if (enemy.sprite != null)
        {
            Texture2D texture = AssetPreview.GetAssetPreview(enemy.sprite);
            if (texture != null)
            {
                GUILayout.Label(texture, GUILayout.Width(50), GUILayout.Height(50));
            }
        }

        // Info
        EditorGUILayout.BeginVertical();
        GUILayout.Label(enemy.enemyName, EditorStyles.boldLabel);
        GUILayout.Label($"HP: {enemy.maxHealth} | Damage: {enemy.damage} | Speed: {enemy.speed}");
        EditorGUILayout.EndVertical();

        // Botones
        EditorGUILayout.BeginVertical(GUILayout.Width(100));
        if (GUILayout.Button("Edit"))
        {
            Selection.activeObject = enemy;
        }
        if (GUILayout.Button("Delete"))
        {
            if (EditorUtility.DisplayDialog("Confirm", $"¿Eliminar {enemy.enemyName}?", "Sí", "No"))
            {
                DeleteEnemy(enemy);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Recarga todos los EnemyData presentes en el proyecto.
    /// </summary>
    private void LoadEnemyDatabase()
    {
        enemyDatabase.Clear();
        string[] guids = AssetDatabase.FindAssets("t:EnemyData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (enemy != null)
            {
                enemyDatabase.Add(enemy);
            }
        }

    }

    /// <summary>
    /// Crea un nuevo EnemyData a partir de los campos de la interfaz.
    /// </summary>
    private void CreateNewEnemy()
    {
        if (string.IsNullOrEmpty(newEnemyName))
        {
            EditorUtility.DisplayDialog("Error", "El nombre no puede estar vacío", "OK");
            return;
        }

        // Crear ScriptableObject
        EnemyData newEnemy = ScriptableObject.CreateInstance<EnemyData>();
        newEnemy.enemyName = newEnemyName;
        newEnemy.maxHealth = newEnemyHealth;
        newEnemy.damage = newEnemyDamage;
        newEnemy.speed = newEnemyMoveSpeed;
        newEnemy.sprite = newEnemySprite;
        // enemyPrefab no existe en EnemyData, se usa el prefab del enemy directamente

        // Guardar en Assets/Data/Enemies
        string path = $"Assets/Data/Enemies/{newEnemyName}.asset";
        
        // Crear directorio si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Data/Enemies"))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Enemies");
        }

        AssetDatabase.CreateAsset(newEnemy, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Éxito", $"Enemy '{newEnemyName}' creado correctamente.", "OK");

        // Resetear campos
        newEnemyName = "New Enemy";
        newEnemyHealth = 10;
        newEnemyDamage = 5;
        newEnemyMoveSpeed = 2f;
        newEnemySprite = null;
        newEnemyPrefab = null;

        LoadEnemyDatabase();
        showCreatePanel = false;
    }

    /// <summary>
    /// Elimina un EnemyData seleccionado.
    /// </summary>
    private void DeleteEnemy(EnemyData enemy)
    {
        string path = AssetDatabase.GetAssetPath(enemy);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        LoadEnemyDatabase();
    }
}
