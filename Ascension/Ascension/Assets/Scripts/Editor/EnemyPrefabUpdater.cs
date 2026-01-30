using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Herramienta para actualizar SOLO prefabs de enemigos (SlimeRed, SlimeBlue, SlimeGreen)
/// - Cambiar escala al 75% (0.75, 0.75, 1)
/// - Reasignar sprites correctos
/// - Verificar componentes necesarios
/// NO AFECTA: Player, armas, ni otros prefabs
/// </summary>
public class EnemyPrefabUpdater : EditorWindow
{
    private GameObject chaserPrefab;
    private GameObject jumperPrefab;
    private GameObject shooterPrefab;
    
    private Texture2D chaserSprite;
    private Texture2D jumperSprite;
    private Texture2D shooterSprite;
    
    private RuntimeAnimatorController chaserController;
    private RuntimeAnimatorController jumperController;
    private RuntimeAnimatorController shooterController;
    
    private float targetScale = 0.75f;

    [MenuItem("Tools/Enemies/Update Enemy Prefabs")]
    /// <summary>
    /// Abre la ventana de actualización de prefabs de enemigos.
    /// </summary>
    public static void ShowWindow()
    {
        EnemyPrefabUpdater window = GetWindow<EnemyPrefabUpdater>("Enemy Prefab Updater");
        window.Show();
    }

    /// <summary>
    /// Dibuja la interfaz de actualización en el editor.
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("Enemy Prefab Updater", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Actualiza los prefabs de enemigos:\n" +
            "- Escala al setenta y cinco por ciento\n" +
            "- Asignación de sprites\n" +
            "- Asignación de Animator Controllers\n" +
            "- Verificación de componentes necesarios",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        GUILayout.Label("Prefabs de Enemigos:", EditorStyles.boldLabel);
        chaserPrefab = (GameObject)EditorGUILayout.ObjectField("Chaser Prefab:", chaserPrefab, typeof(GameObject), false);
        jumperPrefab = (GameObject)EditorGUILayout.ObjectField("Jumper Prefab:", jumperPrefab, typeof(GameObject), false);
        shooterPrefab = (GameObject)EditorGUILayout.ObjectField("Shooter Prefab:", shooterPrefab, typeof(GameObject), false);
        
        EditorGUILayout.Space();
        
        GUILayout.Label("Spritesheets:", EditorStyles.boldLabel);
        chaserSprite = (Texture2D)EditorGUILayout.ObjectField("Chaser Sprite:", chaserSprite, typeof(Texture2D), false);
        jumperSprite = (Texture2D)EditorGUILayout.ObjectField("Jumper Sprite:", jumperSprite, typeof(Texture2D), false);
        shooterSprite = (Texture2D)EditorGUILayout.ObjectField("Shooter Sprite:", shooterSprite, typeof(Texture2D), false);
        
        EditorGUILayout.Space();
        
        GUILayout.Label("Animator Controllers:", EditorStyles.boldLabel);
        chaserController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Chaser Controller:", chaserController, typeof(RuntimeAnimatorController), false);
        jumperController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Jumper Controller:", jumperController, typeof(RuntimeAnimatorController), false);
        shooterController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Shooter Controller:", shooterController, typeof(RuntimeAnimatorController), false);
        
        EditorGUILayout.Space();
        
        targetScale = EditorGUILayout.Slider("Escala Objetivo:", targetScale, 0.1f, 2f);
        
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.cyan;
        
        if (GUILayout.Button("Actualizar todos los prefabs", GUILayout.Height(40)))
        {
            UpdateAllPrefabs();
        }
        
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Auto-detectar assets"))
        {
            AutoFindAssets();
        }
    }

    /// <summary>
    /// Intenta localizar prefabs, sprites y controllers automáticamente.
    /// </summary>
    private void AutoFindAssets()
    {
        int foundCount = 0;
        
        // Buscar prefabs SOLO de enemigos
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            // VALIDACIÓN: Solo prefabs con scripts de enemigos
            if (prefab.GetComponent<SlimeRed>() != null || 
                (prefab.name.Contains("Chaser") || prefab.name.Contains("Red") || prefab.name.Contains("Slime")))
            {
                if (!prefab.name.Contains("Player") && !prefab.name.Contains("Weapon"))
                {
                    chaserPrefab = prefab;
                    foundCount++;
                }
            }
            else if (prefab.GetComponent<SlimeBlue>() != null || 
                     (prefab.name.Contains("Jumper") || prefab.name.Contains("Blue")))
            {
                if (!prefab.name.Contains("Player") && !prefab.name.Contains("Weapon"))
                {
                    jumperPrefab = prefab;
                    foundCount++;
                }
            }
            else if (prefab.GetComponent<SlimeGreen>() != null || 
                     (prefab.name.Contains("Shooter") || prefab.name.Contains("Green")))
            {
                if (!prefab.name.Contains("Player") && !prefab.name.Contains("Weapon"))
                {
                    shooterPrefab = prefab;
                    foundCount++;
                }
            }
        }
        
        // Buscar spritesheets de enemigos
        string[] spriteGuids = AssetDatabase.FindAssets("t:Texture2D slime", new[] { "Assets/Sprites" });
        foreach (string guid in spriteGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            
            if (texture.name.ToLower().Contains("chaser") || texture.name.ToLower().Contains("red"))
            {
                chaserSprite = texture;
                foundCount++;
            }
            else if (texture.name.ToLower().Contains("jumper") || texture.name.ToLower().Contains("blue"))
            {
                jumperSprite = texture;
                foundCount++;
            }
            else if (texture.name.ToLower().Contains("shooter") || texture.name.ToLower().Contains("green"))
            {
                shooterSprite = texture;
                foundCount++;
            }
        }
        
        // Buscar controllers de enemigos
        string[] controllerGuids = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/Animations/Enemies" });
        foreach (string guid in controllerGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
            
            if (controller.name.Contains("Chaser"))
            {
                chaserController = controller;
                foundCount++;
            }
            else if (controller.name.Contains("Jumper"))
            {
                jumperController = controller;
                foundCount++;
            }
            else if (controller.name.Contains("Shooter"))
            {
                shooterController = controller;
                foundCount++;
            }
        }
        
        EditorUtility.DisplayDialog("Auto-Find", 
            $"Se encontraron {foundCount} assets de enemigos.\n\n" +
            "Se recomienda revisar los campos antes de actualizar.", 
            "OK");
        Repaint();
    }

    /// <summary>
    /// Actualiza los prefabs configurados en la ventana.
    /// </summary>
    private void UpdateAllPrefabs()
    {
        if (chaserPrefab != null && chaserSprite != null && chaserController != null)
        {
            UpdatePrefab(chaserPrefab, chaserSprite, chaserController, "Chaser");
        }
        
        if (jumperPrefab != null && jumperSprite != null && jumperController != null)
        {
            UpdatePrefab(jumperPrefab, jumperSprite, jumperController, "Jumper");
        }
        
        if (shooterPrefab != null && shooterSprite != null && shooterController != null)
        {
            UpdatePrefab(shooterPrefab, shooterSprite, shooterController, "Shooter");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Actualización completada", 
            "Prefabs actualizados:\n" +
            $"- Escala: ({targetScale}, {targetScale}, 1)\n" +
            "- Sprites reasignados\n" +
            "- Controllers asignados\n" +
            "- Componentes verificados.", 
            "OK");
    }

    private void UpdatePrefab(GameObject prefab, Texture2D spriteSheet, RuntimeAnimatorController controller, string enemyName)
    {
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        
        // VALIDACIÓN DE SEGURIDAD: Solo actualizar si es un prefab de enemigo
        if (prefabPath.Contains("Player") || prefabPath.Contains("Weapon"))
        {
            Debug.LogWarning($"{prefab.name} no es un prefab de enemigo. No se modificará.");
            return;
        }
        
        GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);
        
        // Verificar que tenga un script de enemigo (verificar la clase base Enemy y sus derivadas)
        Enemy enemyComponent = instance.GetComponent<Enemy>();
        bool hasEnemyScript = enemyComponent != null;
        
        // Si no tiene el componente Enemy, añadir el script correcto según el nombre
        if (!hasEnemyScript)
        {
            Debug.LogWarning($"{prefab.name} no tiene componente Enemy. Se intentará añadir el script correcto.");
            
            // Determinar qué script añadir según el nombre del enemigo
            if (enemyName.Contains("Chaser") || enemyName.Contains("Red"))
            {
                // Eliminar componentes MonoBehaviour rotos
                MonoBehaviour[] brokenScripts = instance.GetComponents<MonoBehaviour>();
                foreach (var script in brokenScripts)
                {
                    if (script == null)
                    {
                        DestroyImmediate(script, true);
                    }
                }
                
                instance.AddComponent<SlimeRed>();
                enemyComponent = instance.GetComponent<SlimeRed>();
            }
            else if (enemyName.Contains("Jumper") || enemyName.Contains("Blue"))
            {
                MonoBehaviour[] brokenScripts = instance.GetComponents<MonoBehaviour>();
                foreach (var script in brokenScripts)
                {
                    if (script == null)
                    {
                        DestroyImmediate(script, true);
                    }
                }
                
                instance.AddComponent<SlimeBlue>();
                enemyComponent = instance.GetComponent<SlimeBlue>();
            }
            else if (enemyName.Contains("Shooter") || enemyName.Contains("Green"))
            {
                MonoBehaviour[] brokenScripts = instance.GetComponents<MonoBehaviour>();
                foreach (var script in brokenScripts)
                {
                    if (script == null)
                    {
                        DestroyImmediate(script, true);
                    }
                }
                
                instance.AddComponent<SlimeGreen>();
                enemyComponent = instance.GetComponent<SlimeGreen>();
            }
            
            hasEnemyScript = enemyComponent != null;
        }
        
        if (!hasEnemyScript)
        {
            Debug.LogError($"No se pudo añadir script de enemigo a {prefab.name}. No se modificará.");
            PrefabUtility.UnloadPrefabContents(instance);
            return;
        }
        
        try
        {
            // 1. Cambiar escala al 75%
            instance.transform.localScale = new Vector3(targetScale, targetScale, 1f);
            
            // 2. Obtener primer sprite del spritesheet para el SpriteRenderer
            string spritePath = AssetDatabase.GetAssetPath(spriteSheet);
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath).OfType<Sprite>().ToArray();
            
            // 3. Asignar sprite inicial (primer Idle Down)
            SpriteRenderer spriteRenderer = instance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && sprites.Length > 0)
            {
                Sprite idleSprite = System.Array.Find(sprites, s => s.name.EndsWith("ID"));
                if (idleSprite != null)
                {
                    spriteRenderer.sprite = idleSprite;
                }
                else
                {
                    spriteRenderer.sprite = sprites[0];
                }
            }
            
            // 4. Asignar Animator Controller
            Animator animator = instance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = instance.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = controller;
            
            // 5. Verificar componentes necesarios
            Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = instance.AddComponent<Rigidbody2D>();
            }
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            if (instance.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D collider = instance.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1f, 1f);
            }
            
            // 6. Guardar prefab
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(instance);
        }
    }
}
