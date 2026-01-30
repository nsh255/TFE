using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

/// <summary>
/// Generador automático de animaciones y Animator Controller para enemigos
/// Crea Blend Trees similares al Player para movimiento en 8 direcciones
/// </summary>
public class EnemyAnimationSystemGenerator : EditorWindow
{
    private Texture2D chaserSprite;
    private Texture2D jumperSprite;
    private Texture2D shooterSprite;
    
    private string basePath = "Assets/Animations/Enemies";
    private float idleFPS = 1f;
    private float walkFPS = 8f;
    private float attackFPS = 12f;
    
    [MenuItem("Tools/Enemies/Generate Enemy Animation System")]
    /// <summary>
    /// Abre el generador de animaciones de enemigos.
    /// </summary>
    public static void ShowWindow()
    {
        EnemyAnimationSystemGenerator window = GetWindow<EnemyAnimationSystemGenerator>("Enemy Animation System");
        window.Show();
    }

    /// <summary>
    /// Dibuja la interfaz de generación en el editor.
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("Enemy Animation System Generator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Genera animaciones y Animator Controller con Blend Trees para tres enemigos.\n" +
            "Sistema equivalente al del jugador: Idle, Walk y Attack en ocho direcciones.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        chaserSprite = (Texture2D)EditorGUILayout.ObjectField("Chaser Slime:", chaserSprite, typeof(Texture2D), false);
        jumperSprite = (Texture2D)EditorGUILayout.ObjectField("Jumper Slime:", jumperSprite, typeof(Texture2D), false);
        shooterSprite = (Texture2D)EditorGUILayout.ObjectField("Shooter Slime:", shooterSprite, typeof(Texture2D), false);
        
        EditorGUILayout.Space();
        basePath = EditorGUILayout.TextField("Base Path:", basePath);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("FPS Settings:", EditorStyles.boldLabel);
        idleFPS = EditorGUILayout.FloatField("Idle FPS:", idleFPS);
        walkFPS = EditorGUILayout.FloatField("Walk FPS:", walkFPS);
        attackFPS = EditorGUILayout.FloatField("Attack FPS:", attackFPS);
        
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        
        if (GUILayout.Button("Generar todo (animaciones y Blend Trees)", GUILayout.Height(40)))
        {
            if (chaserSprite == null || jumperSprite == null || shooterSprite == null)
            {
                EditorUtility.DisplayDialog("Error", "Se deben asignar los tres spritesheets.", "OK");
                return;
            }
            GenerateCompleteSystem();
        }
        
        GUI.backgroundColor = Color.white;
    }

    private void GenerateCompleteSystem()
    {
        try
        {
            EditorUtility.DisplayProgressBar("Generating", "Creating folders...", 0.1f);
            CreateFolderStructure();
            
            EditorUtility.DisplayProgressBar("Generating", "Creating animations...", 0.3f);
            
            // Generar animaciones para cada enemigo
            GenerateAnimationsForEnemy(chaserSprite, "Chaser");
            GenerateAnimationsForEnemy(jumperSprite, "Jumper");
            GenerateAnimationsForEnemy(shooterSprite, "Shooter");
            
            EditorUtility.DisplayProgressBar("Generating", "Creating Animator Controllers...", 0.7f);
            
            // Crear controllers para cada enemigo
            CreateAnimatorController("Chaser");
            CreateAnimatorController("Jumper");
            CreateAnimatorController("Shooter");
            
            EditorUtility.ClearProgressBar();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Proceso completado", 
                "Animaciones creadas (Idle, Walk y Attack en ocho direcciones).\n" +
                "Animator Controllers generados con Blend Trees.\n" +
                "Parámetros configurados (Horizontal, Vertical, Speed, IsAttacking).\n\n" +
                "Se recomienda asignar los Controllers a los prefabs correspondientes.", 
                "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Error", $"Error al generar: {e.Message}", "OK");
            Debug.LogError($"Error: {e}");
        }
    }

    private void CreateFolderStructure()
    {
        string[] folders = {
            basePath,
            $"{basePath}/Chaser",
            $"{basePath}/Jumper",
            $"{basePath}/Shooter"
        };

        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
    }

    private void GenerateAnimationsForEnemy(Texture2D spriteSheet, string enemyName)
    {
        string savePath = $"{basePath}/{enemyName}";
        string spritePath = AssetDatabase.GetAssetPath(spriteSheet);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath).OfType<Sprite>().ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError($"No se encontraron sprites en {spriteSheet.name}");
            return;
        }

        // Crear animaciones Idle (8 direcciones - 1 frame estático cada una)
        CreateIdleAnimations(sprites, savePath);
        
        // Crear animaciones Walk (8 direcciones - 6 frames cada una)
        CreateWalkAnimations(sprites, savePath);
        
        // Crear animaciones Attack (8 direcciones - 6 frames cada una)
        CreateAttackAnimations(sprites, savePath);
        
    }

    private void CreateIdleAnimations(Sprite[] sprites, string savePath)
    {
        var idleDirections = new[]
        {
            new { name = "Idle_Down", spriteName = "ID" },
            new { name = "Idle_DownLeft", spriteName = "IDL" },
            new { name = "Idle_DownRight", spriteName = "IDR" },
            new { name = "Idle_Left", spriteName = "IL" },
            new { name = "Idle_Right", spriteName = "IR" },
            new { name = "Idle_Up", spriteName = "IU" },
            new { name = "Idle_UpLeft", spriteName = "IUL" },
            new { name = "Idle_UpRight", spriteName = "IUR" }
        };

        foreach (var direction in idleDirections)
        {
            Sprite idleSprite = System.Array.Find(sprites, s => s.name.EndsWith(direction.spriteName));
            
            if (idleSprite == null)
            {
                Debug.LogWarning($"Sprite '{direction.spriteName}' no encontrado");
                continue;
            }

            AnimationClip clip = new AnimationClip();
            clip.frameRate = idleFPS;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[1];
            keyframes[0] = new ObjectReferenceKeyframe { time = 0f, value = idleSprite };

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            string animPath = $"{savePath}/{direction.name}.anim";
            AssetDatabase.CreateAsset(clip, animPath);
        }
    }

    private void CreateWalkAnimations(Sprite[] sprites, string savePath)
    {
        var walkDirections = new[]
        {
            new { name = "Walk_Down", prefix = "D" },
            new { name = "Walk_DownLeft", prefix = "DL" },
            new { name = "Walk_DownRight", prefix = "DR" },
            new { name = "Walk_Left", prefix = "L" },
            new { name = "Walk_Right", prefix = "R" },
            new { name = "Walk_Up", prefix = "U" },
            new { name = "Walk_UpLeft", prefix = "UL" },
            new { name = "Walk_UpRight", prefix = "UR" }
        };

        foreach (var direction in walkDirections)
        {
            AnimationClip clip = new AnimationClip();
            clip.frameRate = walkFPS;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[6];
            bool allFound = true;

            for (int i = 0; i < 6; i++)
            {
                string spriteName = $"{direction.prefix}{i + 1}";
                Sprite frameSprite = System.Array.Find(sprites, s => s.name.EndsWith(spriteName) && !s.name.Contains("A"));
                
                if (frameSprite == null)
                {
                    Debug.LogWarning($"Sprite '{spriteName}' no encontrado");
                    allFound = false;
                    break;
                }

                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / walkFPS,
                    value = frameSprite
                };
            }

            if (!allFound) continue;

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            string animPath = $"{savePath}/{direction.name}.anim";
            AssetDatabase.CreateAsset(clip, animPath);
        }
    }

    private void CreateAttackAnimations(Sprite[] sprites, string savePath)
    {
        var attackDirections = new[]
        {
            new { name = "Attack_Down", prefix = "AD" },
            new { name = "Attack_DownLeft", prefix = "ADL" },
            new { name = "Attack_DownRight", prefix = "ADR" },
            new { name = "Attack_Left", prefix = "AL" },
            new { name = "Attack_Right", prefix = "AR" },
            new { name = "Attack_Up", prefix = "AU" },
            new { name = "Attack_UpLeft", prefix = "AUL" },
            new { name = "Attack_UpRight", prefix = "AUR" }
        };

        foreach (var direction in attackDirections)
        {
            AnimationClip clip = new AnimationClip();
            clip.frameRate = attackFPS;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true; // Loop para que el ataque sea continuo
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[6];
            bool allFound = true;

            for (int i = 0; i < 6; i++)
            {
                string spriteName = $"{direction.prefix}{i + 1}";
                Sprite frameSprite = System.Array.Find(sprites, s => s.name.EndsWith(spriteName));
                
                if (frameSprite == null)
                {
                    Debug.LogWarning($"Sprite de ataque '{spriteName}' no encontrado");
                    allFound = false;
                    break;
                }

                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / attackFPS,
                    value = frameSprite
                };
            }

            if (!allFound) continue;

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            string animPath = $"{savePath}/{direction.name}.anim";
            AssetDatabase.CreateAsset(clip, animPath);
        }
    }

    private void CreateAnimatorController(string enemyName)
    {
        string controllerPath = $"{basePath}/{enemyName}/{enemyName}.controller";
        
        // Eliminar si existe
        if (File.Exists(controllerPath))
        {
            AssetDatabase.DeleteAsset(controllerPath);
        }
        
        // Crear controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        // Añadir parámetros (igual que el Player)
        controller.AddParameter("Horizontal", AnimatorControllerParameterType.Float);
        controller.AddParameter("Vertical", AnimatorControllerParameterType.Float);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
        
        // Obtener layer base
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine stateMachine = baseLayer.stateMachine;
        
        // Crear estados con Blend Trees
        AnimatorState idleState = CreateBlendTreeState(controller, stateMachine, "Idle", enemyName, new Vector3(300, 50, 0));
        AnimatorState walkState = CreateBlendTreeState(controller, stateMachine, "Walk", enemyName, new Vector3(300, 150, 0));
        AnimatorState attackState = CreateBlendTreeState(controller, stateMachine, "Attack", enemyName, new Vector3(300, 250, 0));
        
        // Establecer Idle como default
        stateMachine.defaultState = idleState;
        
        // Crear transiciones
        // Idle <-> Walk basado en Speed
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.1f;
        
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.1f;
        
        // Idle/Walk -> Attack cuando IsAttacking = true
        var idleToAttack = idleState.AddTransition(attackState);
        idleToAttack.AddCondition(AnimatorConditionMode.If, 0, "IsAttacking");
        idleToAttack.hasExitTime = false;
        idleToAttack.duration = 0.1f;
        
        var walkToAttack = walkState.AddTransition(attackState);
        walkToAttack.AddCondition(AnimatorConditionMode.If, 0, "IsAttacking");
        walkToAttack.hasExitTime = false;
        walkToAttack.duration = 0.1f;
        
        // Attack -> Idle cuando IsAttacking = false
        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        attackToIdle.hasExitTime = false;
        attackToIdle.duration = 0.1f;
        
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
    }

    private AnimatorState CreateBlendTreeState(AnimatorController controller, AnimatorStateMachine stateMachine, 
                                                string stateName, string enemyName, Vector3 position)
    {
        AnimatorState state = stateMachine.AddState(stateName, position);
        
        // Crear Blend Tree
        BlendTree blendTree = new BlendTree();
        blendTree.name = stateName;
        blendTree.blendType = BlendTreeType.SimpleDirectional2D;
        blendTree.blendParameter = "Horizontal";
        blendTree.blendParameterY = "Vertical";
        blendTree.useAutomaticThresholds = false;
        
        // Definir las 8 direcciones
        string[] directions = { "Down", "DownLeft", "DownRight", "Left", "Right", "Up", "UpLeft", "UpRight" };
        Vector2[] positions = {
            new Vector2(0, -1),   // Down
            new Vector2(-1, -1),  // DownLeft
            new Vector2(1, -1),   // DownRight
            new Vector2(-1, 0),   // Left
            new Vector2(1, 0),    // Right
            new Vector2(0, 1),    // Up
            new Vector2(-1, 1),   // UpLeft
            new Vector2(1, 1)     // UpRight
        };
        
        // Añadir las 8 animaciones al Blend Tree
        for (int i = 0; i < directions.Length; i++)
        {
            string animPath = $"{basePath}/{enemyName}/{stateName}_{directions[i]}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
            
            if (clip != null)
            {
                blendTree.AddChild(clip, positions[i]);
            }
            else
            {
                Debug.LogWarning($"No se encontró: {animPath}");
            }
        }
        
        // Asignar Blend Tree al estado
        state.motion = blendTree;
        
        // Guardar Blend Tree como subasset del controller
        AssetDatabase.AddObjectToAsset(blendTree, controller);
        
        return state;
    }
}
