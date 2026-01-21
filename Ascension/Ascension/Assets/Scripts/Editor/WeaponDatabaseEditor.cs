using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Editor visual para gestionar la base de datos de armas.
/// Permite crear, editar y eliminar armas base rápidamente.
/// </summary>
public class WeaponDatabaseEditor : EditorWindow
{
    private List<WeaponData> weaponDatabase = new List<WeaponData>();
    private Vector2 scrollPosition;
    private bool showCreatePanel = false;

    // Datos para crear nueva arma
    private string newWeaponName = "New Weapon";
    private int newWeaponDamage = 10;
    private float newWeaponAttackSpeed = 1f;
    private Sprite newWeaponSprite;
    private GameObject newWeaponBullet;
    private GameObject newWeaponPrefab;

    [MenuItem("Tools/Ascension/Weapon Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<WeaponDatabaseEditor>("Weapon Database");
    }

    void OnEnable()
    {
        LoadWeaponDatabase();
    }

    void OnGUI()
    {
        GUILayout.Label("Weapon Database Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Botones principales
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Database", GUILayout.Height(25)))
        {
            LoadWeaponDatabase();
        }
        if (GUILayout.Button("Create New Weapon", GUILayout.Height(25)))
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
        GUILayout.Label($"Armas Registradas: {weaponDatabase.Count}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Lista de armas
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 0; i < weaponDatabase.Count; i++)
        {
            if (weaponDatabase[i] == null) continue;

            DrawWeaponEntry(weaponDatabase[i], i);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawCreatePanel()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Create New Weapon", EditorStyles.boldLabel);

        newWeaponName = EditorGUILayout.TextField("Name", newWeaponName);
        newWeaponDamage = EditorGUILayout.IntField("Damage", newWeaponDamage);
        newWeaponAttackSpeed = EditorGUILayout.FloatField("Attack Speed", newWeaponAttackSpeed);
        newWeaponSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", newWeaponSprite, typeof(Sprite), false);
        newWeaponBullet = (GameObject)EditorGUILayout.ObjectField("Bullet Prefab", newWeaponBullet, typeof(GameObject), false);
        newWeaponPrefab = (GameObject)EditorGUILayout.ObjectField("Weapon Prefab", newWeaponPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Weapon Data", GUILayout.Height(30)))
        {
            CreateNewWeapon();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawWeaponEntry(WeaponData weapon, int index)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        // Icono
        if (weapon.sprite != null)
        {
            Texture2D texture = AssetPreview.GetAssetPreview(weapon.sprite);
            if (texture != null)
            {
                GUILayout.Label(texture, GUILayout.Width(50), GUILayout.Height(50));
            }
        }

        // Info
        EditorGUILayout.BeginVertical();
        GUILayout.Label(weapon.weaponName, EditorStyles.boldLabel);
        GUILayout.Label($"Damage: {weapon.damage} | Speed: {weapon.atackSpeed}");
        GUILayout.Label($"Bullet: {(weapon.bulletPrefab != null ? weapon.bulletPrefab.name : "None")}");
        EditorGUILayout.EndVertical();

        // Botones
        EditorGUILayout.BeginVertical(GUILayout.Width(100));
        if (GUILayout.Button("Edit"))
        {
            Selection.activeObject = weapon;
        }
        if (GUILayout.Button("Delete"))
        {
            if (EditorUtility.DisplayDialog("Confirm", $"¿Eliminar {weapon.weaponName}?", "Sí", "No"))
            {
                DeleteWeapon(weapon);
            }
        }
        if (GUILayout.Button("Duplicate"))
        {
            DuplicateWeapon(weapon);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void LoadWeaponDatabase()
    {
        weaponDatabase.Clear();
        string[] guids = AssetDatabase.FindAssets("t:WeaponData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (weapon != null)
            {
                weaponDatabase.Add(weapon);
            }
        }

        Debug.Log($"[WeaponDatabaseEditor] {weaponDatabase.Count} armas cargadas");
    }

    private void CreateNewWeapon()
    {
        if (string.IsNullOrEmpty(newWeaponName))
        {
            EditorUtility.DisplayDialog("Error", "El nombre no puede estar vacío", "OK");
            return;
        }

        // Crear ScriptableObject
        WeaponData newWeapon = ScriptableObject.CreateInstance<WeaponData>();
        newWeapon.weaponName = newWeaponName;
        newWeapon.damage = newWeaponDamage;
        newWeapon.atackSpeed = newWeaponAttackSpeed;
        newWeapon.sprite = newWeaponSprite;
        newWeapon.bulletPrefab = newWeaponBullet;
        newWeapon.weaponPrefab = newWeaponPrefab;

        // Guardar en Assets/Data/Weapons
        string path = $"Assets/Data/Weapons/{newWeaponName}.asset";
        
        // Crear directorio si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Data/Weapons"))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Weapons");
        }

        AssetDatabase.CreateAsset(newWeapon, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[WeaponDatabaseEditor] Weapon creado: {path}");
        EditorUtility.DisplayDialog("Success", $"Weapon '{newWeaponName}' creado correctamente", "OK");

        // Resetear campos
        newWeaponName = "New Weapon";
        newWeaponDamage = 10;
        newWeaponAttackSpeed = 1f;
        newWeaponSprite = null;
        newWeaponBullet = null;
        newWeaponPrefab = null;

        LoadWeaponDatabase();
        showCreatePanel = false;
    }

    private void DeleteWeapon(WeaponData weapon)
    {
        string path = AssetDatabase.GetAssetPath(weapon);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[WeaponDatabaseEditor] Weapon eliminado: {weapon.weaponName}");
        LoadWeaponDatabase();
    }

    private void DuplicateWeapon(WeaponData weapon)
    {
        WeaponData duplicate = Object.Instantiate(weapon);
        duplicate.weaponName = weapon.weaponName + " (Copy)";

        string path = $"Assets/Data/Weapons/{duplicate.weaponName}.asset";
        AssetDatabase.CreateAsset(duplicate, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[WeaponDatabaseEditor] Weapon duplicado: {path}");
        LoadWeaponDatabase();
    }
}
